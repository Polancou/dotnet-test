using Application.DTOs;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;
using UglyToad.PdfPig;

namespace Infrastructure.Services;

/// <summary>
/// Service for AI-based document analysis using the Gemini API. Supports both image and text-based files,
/// extracting the relevant content and submitting it to Gemini for classification and data extraction.
/// Fakes a response if the API Key is not configured.
/// </summary>
public class AiAnalysisService(
    IEventLogService eventLogService,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory) : IAiAnalysisService
{
    /// <summary>
    /// Analyzes a document stream (image or text) with Gemini or mock, returns structured analysis result.
    /// Logs all operations and errors for auditing.
    /// </param>
    /// <param name="fileStream">Input document as stream.</param>
    /// <param name="fileName">Original file name to determine type.</param>
    /// <returns>AnalysisResultDto structed result with either Invoice or Information data filled.</returns>
    public async Task<AnalysisResultDto> AnalyzeDocumentAsync(Stream fileStream, string fileName)
    {
        // Try to get the Gemini API key from configuration
        var apiKey = configuration["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            // Log warning and use mock analyzer if API key is missing
            await eventLogService.LogEventAsync("AI Analysis Warning", "Gemini API Key not found. Using mock.");
            return await MockAnalyzeAsync(fileName);
        }

        try
        {
            // Determine file type (image or not)
            var extension = Path.GetExtension(fileName).ToLower();
            var isImage = IsImage(extension);

            // List of Gemini message parts for the request (images and/or prompts)
            var parts = new List<object>();

            if (isImage)
            {
                // Reset stream and copy entire image to memory for encoding
                fileStream.Position = 0;
                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                var base64Image = Convert.ToBase64String(imageBytes);

                // Determine MIME type for Gemini
                var mimeType = extension == ".png" ? "image/png" : "image/jpeg";

                // Add image as inline_data and a user prompt for analysis
                parts.Add(new { inline_data = new { mime_type = mimeType, data = base64Image } });
                parts.Add(new { text = "Analyze this image document." });
            }
            else
            {
                // Try to extract all possible text from the document
                string extractedText = ExtractContent(fileStream, fileName);
                if (string.IsNullOrWhiteSpace(extractedText))
                    throw new Exception("Could not extract text.");

                // Truncate text if very large (Gemini has limits)
                if (extractedText.Length > 30000)
                    extractedText = extractedText.Substring(0, 30000);

                // Add extracted document text for Gemini to analyze
                parts.Add(new { text = $"Analyze this document content:\n\n{extractedText}" });
            }

            // Compose the system prompt with strict JSON instructions for Gemini output structure
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
            // Insert system prompt at the top of the message parts for Gemini (v1beta doesn't use system prompt field)
            parts.Insert(0, new { text = systemPrompt });

            // Create request body in Gemini v1beta model format
            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = parts.ToArray() }
                },
                generationConfig = new
                {
                    response_mime_type = "application/json" // Explicitly request JSON output
                }
            };

            // Call Gemini API with constructed request and API key
            var client = httpClientFactory.CreateClient();
            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            var response = await client.PostAsJsonAsync(url, requestBody);

            // Handle HTTP errors with full message for logging
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Gemini API Error: {response.StatusCode} - {error}");
            }

            // Parse GeminiResponse from returned JSON
            var jsonResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            var textResult = jsonResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            if (string.IsNullOrEmpty(textResult))
                throw new Exception("Empty response from Gemini.");

            // Sometimes Gemini ignores prompt and puts JSON in markdown code blocks; clean these
            textResult = textResult.Replace("```json", "").Replace("```", "").Trim();

            // Deserialize the returned JSON string to the expected DTO, with casing tolerant to Gemini
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<AnalysisResultDto>(textResult, options);

            if (result == null) throw new Exception("Failed to deserialize AI response.");

            // Prepare summary for log event, with info depending on document type
            var logSummary = result.DocumentType == "Invoice"
                ? $"Invoice {result.InvoiceData?.InvoiceNumber} for {result.InvoiceData?.Total:C}"
                : $"Info: {result.InformationData?.Summary?.Substring(0, Math.Min(50, result.InformationData?.Summary?.Length ?? 0))}...";

            await eventLogService.LogEventAsync("AI Analysis", $"Analyzed {fileName}: {logSummary}");

            return result;
        }
        catch (Exception ex)
        {
            // On failure, log the error and return default "analysis failed" result
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

    /// <summary>
    /// Checks if a given file extension corresponds to a supported image format (jpg, jpeg, png).
    /// </summary>
    /// <param name="extension">File extension including leading dot, case insensitive.</param>
    /// <returns>True if supported image extension; false otherwise.</returns>
    private bool IsImage(string extension) => extension == ".jpg" || extension == ".jpeg" || extension == ".png";

    /// <summary>
    /// Extracts readable textual content from the provided stream depending on file type.
    /// Handles PDFs via PdfPig, reads directly for .txt/.csv/.md/.json, or returns file info fallback.
    /// </summary>
    /// <param name="stream">Input file stream (position reset to 0).</param>
    /// <param name="fileName">Filename for extension and fallback.</param>
    /// <returns>Extracted text content or a fallback string if unextractable.</returns>
    private string ExtractContent(Stream stream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        stream.Position = 0;

        if (extension == ".pdf")
        {
            try
            {
                // Extract all text from all pages using PdfPig
                using var document = PdfDocument.Open(stream);
                return string.Join(" ", document.GetPages().Select(p => p.Text));
            }
            catch
            {
                // Return message if unable to extract PDF content
                return "PDF content could not be extracted.";
            }
        }
        else if (extension == ".txt" || extension == ".csv" || extension == ".json" || extension == ".md")
        {
            // Read entire text file into string
            using var reader = new StreamReader(stream, leaveOpen: true);
            return reader.ReadToEnd();
        }

        // Fallback for unsupported types
        return $"[File: {fileName}]";
    }

    /// <summary>
    /// Provides a mock analysis result if Gemini API key is missing or for test purposes.
    /// </summary>
    /// <param name="fileName">The document file name to determine mock logic.</param>
    /// <returns>AnalysisResultDto with either Invoice data or Information data.</returns>
    private async Task<AnalysisResultDto> MockAnalyzeAsync(string fileName)
    {
        await Task.Delay(1500); // Simulate processing delay

        var result = new AnalysisResultDto();

        // File name heuristic: if "invoice" in file name, provide mock invoice data
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
                {
                    new() { Name = "Mock Service", Quantity = 1, UnitPrice = 1500m, Total = 1500m }
                }
            };
        }
        else
        {
            // Otherwise, provide mock information data
            result.DocumentType = "Information";
            result.InformationData = new InformationData
            {
                Description = "Mock Info",
                Summary = "This is a mock response.",
                Sentiment = "Neutral"
            };
        }

        return result;
    }

    // ==== Gemini API Internal Response Classes ====

    /// <summary>
    /// Represents the root of the Gemini API JSON response
    /// </summary>
    private class GeminiResponse
    {
        public Candidate[]? Candidates { get; set; }
    }

    /// <summary>
    /// Represents a single response candidate returned by Gemini
    /// </summary>
    private class Candidate
    {
        public Content? Content { get; set; }
    }

    /// <summary>
    /// Represents the content payload given by Gemini for a candidate
    /// </summary>
    private class Content
    {
        public Part[]? Parts { get; set; }
    }

    /// <summary>
    /// Represents a part of Gemini's content payload (typically a text result)
    /// </summary>
    private class Part
    {
        public string? Text { get; set; }
    }
}
