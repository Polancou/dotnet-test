from pydantic import BaseModel, Field
from typing import List, Optional
from datetime import datetime

# -----------------------------------------------------------------------------
# Data Transfer Objects (DTOs) for AI Document Analysis Results
#
# These classes define the structure of the parsed and analyzed data returned 
# by the document AI analysis endpoints. Responses include invoice details, 
# freeform information analysis, and product-level information.
# -----------------------------------------------------------------------------

class ProductDto(BaseModel):
    """
    Represents a single product extracted from an invoice in an analyzed document.
    """
    name: str  # Name/description of the product
    quantity: int  # Quantity of the product
    unit_price: float = Field(alias="unitPrice")  # Price per unit, uses JSON alias 'unitPrice'
    total: float  # Total cost for this product

    class Config:
        populate_by_name = True  # Allow population by both field and alias names

class InvoiceData(BaseModel):
    """
    Captures structured invoice details parsed from a document,
    including client/provider info and line items.
    """
    client_name: str = Field(alias="clientName")  # Name of the invoice client
    client_address: str = Field(alias="clientAddress")  # Address of the invoice client
    provider_name: str = Field(alias="providerName")  # Name of the invoice provider
    provider_address: str = Field(alias="providerAddress")  # Address of the provider
    invoice_number: str = Field(alias="invoiceNumber")  # Invoice number identifier
    date: Optional[datetime]  # Date of the invoice (may be None if not found)
    total: float  # Invoice grand total
    products: List[ProductDto] = []  # List of product items included in the invoice

    class Config:
        populate_by_name = True  # Allow population by both field and alias names

class InformationData(BaseModel):
    """
    Contains general information or interpretations extracted from a document,
    such as descriptive summaries and sentiment analysis.
    """
    description: str  # Freeform description or main content summary
    summary: str      # Summarized version of the content
    sentiment: str    # Sentiment classification (e.g., "positive", "neutral", "negative")

class AnalysisResultDto(BaseModel):
    """
    Aggregates the overall analysis result of a document, including its
    type, structured invoice data (if present), and informational analysis.
    """
    document_type: str = Field(alias="documentType")  # Type/classification of the document
    invoice_data: Optional[InvoiceData] = Field(default=None, alias="invoiceData")  # Detailed invoice data (if applicable)
    information_data: Optional[InformationData] = Field(default=None, alias="informationData")  # Additional analysis (may be None)

    class Config:
        populate_by_name = True  # Enable population by field or JSON alias names

class AnalyzeDocumentResponse(BaseModel):
    """
    Defines the complete response for a document analysis request,
    linking the document identifier with its AI-derived analysis.
    """
    document_id: int = Field(alias="documentId")  # Reference ID for the analyzed document
    analysis: AnalysisResultDto  # Full analysis result DTO

    class Config:
        populate_by_name = True  # Allow population by both field and alias names
