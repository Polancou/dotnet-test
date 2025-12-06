namespace Application.DTOs;

/// <summary>
/// Represents the result of an AI document analysis.
/// Depending on the DocumentType, one of InvoiceData or InformationData should be populated.
/// </summary>
public class AnalysisResultDto
{
    /// <summary>
    /// The type of document analyzed. Example values: "Invoice" or "Information".
    /// Determines which data object will be populated.
    /// </summary>
    public string DocumentType { get; set; } = string.Empty; // "Invoice" or "Information"

    /// <summary>
    /// Detailed invoice data, present if DocumentType is "Invoice".
    /// Null otherwise.
    /// </summary>
    public InvoiceData? InvoiceData { get; set; }

    /// <summary>
    /// Detailed information data, present if DocumentType is "Information".
    /// Null otherwise.
    /// </summary>
    public InformationData? InformationData { get; set; }
}

/// <summary>
/// Contains the extracted data for an invoice document.
/// </summary>
public class InvoiceData
{
    /// <summary>
    /// Name of the client being billed.
    /// </summary>
    public string ClientName { get; set; } = string.Empty;

    /// <summary>
    /// The address of the client.
    /// </summary>
    public string ClientAddress { get; set; } = string.Empty;

    /// <summary>
    /// Name of the provider/supplier issuing the invoice.
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Address of the provider/supplier.
    /// </summary>
    public string ProviderAddress { get; set; } = string.Empty;

    /// <summary>
    /// Invoice number/identifier.
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Date the invoice was issued. Can be null if missing.
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// Total amount for the invoice.
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// List of products or line items included on the invoice.
    /// </summary>
    public List<ProductDto> Products { get; set; } = new();
}

/// <summary>
/// Represents a specific product or line item on an invoice.
/// </summary>
public class ProductDto
{
    /// <summary>
    /// Name or description of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Quantity purchased.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Price per unit of the product.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Total price for this line item (typically Quantity * UnitPrice).
    /// </summary>
    public decimal Total { get; set; }
}

/// <summary>
/// Contains data extracted from a general informational document.
/// </summary>
public class InformationData
{
    /// <summary>
    /// Extracted main description or body text from the document.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Extracted summary or abstract of the document content.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Sentiment detected in the document. Can be "Positive", "Negative", or "Neutral".
    /// </summary>
    public string Sentiment { get; set; } = string.Empty; // "Positive", "Negative", "Neutral"
}
