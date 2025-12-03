using Application.Interfaces;

namespace Infrastructure.Services;

public class AiAnalysisService(IEventLogService eventLogService) : IAiAnalysisService
{
    public async Task<string> AnalyzeDocumentAsync(Stream fileStream, string fileName)
    {
        // Simulate processing delay
        await Task.Delay(1000);

        var extension = Path.GetExtension(fileName).ToLower();
        string result = "General Content";
        
        if (fileName.ToLower().Contains("invoice"))
        {
            result = "Invoice";
        }
        else if (extension == ".pdf")
        {
            result = "Document (PDF)";
        }
        else if (extension == ".jpg" || extension == ".png" || extension == ".jpeg")
        {
            result = "Image";
        }

        // We don't have userId here easily unless passed. 
        // For now, we'll log without userId or we need to update the interface.
        // The controller has the user ID. 
        // Let's update the interface to accept userId? 
        // Or just log with null userId for now as per interface definition.
        // Wait, the controller calls this. The controller can pass the userId.
        // But I don't want to change the interface signature right now if I can avoid it.
        // Actually, the requirement says "Registra: ID del evento, Tipo, Descripción, Fecha y hora".
        // UserId is not explicitly required in the display, but good for filtering.
        // Let's check if I can pass userId.
        // The interface is: Task<string> AnalyzeDocumentAsync(Stream fileStream, string fileName);
        // I should update it to include userId.
        
        await eventLogService.LogEventAsync("AI Analysis", $"Analyzed {fileName}: {result}");

        return result;
    }
}
