namespace Application.DTOs;

public class AnalysisResultDto
{
    public string DocumentType { get; set; } = string.Empty; // "Invoice" or "Information"
    public InvoiceData? InvoiceData { get; set; }
    public InformationData? InformationData { get; set; }
}

public class InvoiceData
{
    public string ClientName { get; set; } = string.Empty;
    public string ClientAddress { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderAddress { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public decimal Total { get; set; }
    public List<ProductDto> Products { get; set; } = new();
}

public class ProductDto
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}

public class InformationData
{
    public string Description { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Sentiment { get; set; } = string.Empty; // "Positive", "Negative", "Neutral"
}
