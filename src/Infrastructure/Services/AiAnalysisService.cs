using Application.DTOs;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UglyToad.PdfPig;

namespace Infrastructure.Services;

public class AiAnalysisService(
    IEventLogService eventLogService,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory) : IAiAnalysisService
{
    public async Task<AnalysisResultDto> AnalyzeDocumentAsync(Stream fileStream, string fileName)
    {
        var apiKey = configuration["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            await eventLogService.LogEventAsync("AI Analysis Warning", "Gemini API Key not found. Using mock.");
            return await MockAnalyzeAsync(fileName);
        }

        try
        {
            var extension = Path.GetExtension(fileName).ToLower();
            var isImage = IsImage(extension);

            // Prepare content for Gemini
            var parts = new List<object>();

            if (isImage)
            {
                fileStream.Position = 0;
                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                var base64Image = Convert.ToBase64String(imageBytes);
                var mimeType = extension == ".png" ? "image/png" : "image/jpeg";

                parts.Add(new { inline_data = new { mime_type = mimeType, data = base64Image } });
                parts.Add(new { text = "Analyze this image document." });
            }
            else
            {
                string extractedText = ExtractContent(fileStream, fileName);
                if (string.IsNullOrWhiteSpace(extractedText)) throw new Exception("Could not extract text.");
                if (extractedText.Length > 30000)
                    extractedText = extractedText.Substring(0, 30000); // Gemini has large context but let's be safe

                parts.Add(new { text = $"Analyze this document content:\n\n{extractedText}" });
            }

            // System Prompt (Gemini 1.5 supports system instructions, but we can also put it in user prompt for simplicity in v1beta)
            var systemPrompt = @"
You are an expert document analyzer. Analyze the provided document (text or image) and determine if it is an 'Invoice' or 'Information'.
Return ONLY a valid JSON object matching this structure:
{
    ""documentType"": ""Invoice"" | ""Information"",
    ""invoiceData"": {
        ""clientName"": ""string"",
        ""clientAddress"": ""string"",
        ""providerName"": ""string"",
        ""providerAddress"": ""string"",
        ""invoiceNumber"": ""string"",
        ""date"": ""YYYY-MM-DD"",
        ""total"": 0.00,
        ""products"": [
            { ""name"": ""string"", ""quantity"": 0, ""unitPrice"": 0.00, ""total"": 0.00 }
        ]
    },
    ""informationData"": {
        ""description"": ""string"",
        ""summary"": ""string"",
        ""sentiment"": ""Positive"" | ""Negative"" | ""Neutral""
    }
}
If it is an Invoice, populate 'invoiceData' and leave 'informationData' null.
If it is Information, populate 'informationData' and leave 'invoiceData' null.
Ensure dates are valid ISO 8601 strings. Do not use markdown code blocks in response, just raw JSON.
";
            // Prepend system prompt to parts for simplicity
            parts.Insert(0, new { text = systemPrompt });

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = parts.ToArray() }
                },
                generationConfig = new
                {
                    response_mime_type = "application/json"
                }
            };

            var client = httpClientFactory.CreateClient();
            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            var response = await client.PostAsJsonAsync(url, requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Gemini API Error: {response.StatusCode} - {error}");
            }

            var jsonResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            var textResult = jsonResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            if (string.IsNullOrEmpty(textResult)) throw new Exception("Empty response from Gemini.");

            // Clean markdown if present (sometimes Gemini adds ```json ... ``` despite instructions)
            textResult = textResult.Replace("```json", "").Replace("```", "").Trim();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<AnalysisResultDto>(textResult, options);

            if (result == null) throw new Exception("Failed to deserialize AI response.");

            var logSummary = result.DocumentType == "Invoice"
                ? $"Invoice {result.InvoiceData?.InvoiceNumber} for {result.InvoiceData?.Total:C}"
                : $"Info: {result.InformationData?.Summary.Substring(0, Math.Min(50, result.InformationData?.Summary?.Length ?? 0))}...";

            await eventLogService.LogEventAsync("AI Analysis", $"Analyzed {fileName}: {logSummary}");

            return result;
        }
        catch (Exception ex)
        {
            await eventLogService.LogEventAsync("AI Analysis Error", $"Failed to analyze {fileName}: {ex.Message}");
            return new AnalysisResultDto
            {
                DocumentType = "Information",
                InformationData = new InformationData
                {
                    Description = "Analysis Failed",
                    Summary = $"Error: {ex.Message}",
                    Sentiment = "Neutral"
                }
            };
        }
    }

    private bool IsImage(string extension) => extension == ".jpg" || extension == ".jpeg" || extension == ".png";

    private string ExtractContent(Stream stream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        stream.Position = 0;

        if (extension == ".pdf")
        {
            try
            {
                using var document = PdfDocument.Open(stream);
                return string.Join(" ", document.GetPages().Select(p => p.Text));
            }
            catch
            {
                return "PDF content could not be extracted.";
            }
        }
        else if (extension == ".txt" || extension == ".csv" || extension == ".json" || extension == ".md")
        {
            using var reader = new StreamReader(stream, leaveOpen: true);
            return reader.ReadToEnd();
        }

        return $"[File: {fileName}]";
    }

    private async Task<AnalysisResultDto> MockAnalyzeAsync(string fileName)
    {
        await Task.Delay(1500);
        var result = new AnalysisResultDto();
        if (fileName.ToLower().Contains("invoice"))
        {
            result.DocumentType = "Invoice";
            result.InvoiceData = new InvoiceData
            {
                ClientName = "Tech Solutions Inc.",
                ClientAddress = "123 Innovation Dr",
                ProviderName = "Cloud Services LLC",
                ProviderAddress = "456 Server Ave",
                InvoiceNumber = "INV-MOCK-001",
                Date = DateTime.Now,
                Total = 1500.00m,
                Products = new List<ProductDto>
                    { new() { Name = "Mock Service", Quantity = 1, UnitPrice = 1500m, Total = 1500m } }
            };
        }
        else
        {
            result.DocumentType = "Information";
            result.InformationData = new InformationData
                { Description = "Mock Info", Summary = "This is a mock response.", Sentiment = "Neutral" };
        }

        return result;
    }

    // Gemini Response Classes
    private class GeminiResponse
    {
        public Candidate[]? Candidates { get; set; }
    }

    private class Candidate
    {
        public Content? Content { get; set; }
    }

    private class Content
    {
        public Part[]? Parts { get; set; }
    }

    private class Part
    {
        public string? Text { get; set; }
    }
}
