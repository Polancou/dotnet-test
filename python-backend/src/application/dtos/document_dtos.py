from pydantic import BaseModel, Field
from datetime import datetime
from typing import List, Optional

# -----------------------------------------------------------------------------
# Data Transfer Object (DTO) for Document Metadata/API Response
#
# This DTO is used to define the structure of document records that are 
# returned to the client via API responses. It provides both file metadata and 
# processing results, serving as the authoritative schema for document-related 
# exchanges in the system.
# -----------------------------------------------------------------------------
class DocumentResponse(BaseModel):
    """
    Represents the API response structure for a document, including uploaded file 
    metadata, processing status, any extracted analysis, and validation results.

    Attributes:
        id (int): Unique identifier for the document.
        file_name (str): Name of the uploaded file (serialization alias: "fileName").
        content_type (str): MIME type of the file (serialization alias: "contentType").
        file_size (int): File size in bytes (serialization alias: "fileSize").
        is_processed (bool): Whether this document has been processed/analyzed 
            (serialization alias: "isProcessed").
        analysis_result (Optional[str]): The textual or JSON result of document 
            analysis (nullable, serialization alias: "analysisResult").
        creation_date (datetime): Date/time when the document was uploaded 
            (serialization alias: "creationDate").
        validation_errors (Optional[List[str]]): Any errors encountered during
            validation or processing, if applicable (nullable, serialization alias: "validationErrors").
    """
    id: int  # Unique identifier for the document (primary key)
    file_name: str = Field(serialization_alias="fileName")  # Name of the uploaded file
    content_type: str = Field(serialization_alias="contentType")  # MIME type of file
    file_size: int = Field(serialization_alias="fileSize")  # Size of file in bytes
    is_processed: bool = Field(serialization_alias="isProcessed")  # True if document has been processed
    analysis_result: Optional[str] = Field(default=None, serialization_alias="analysisResult")  # Analysis or parsing output, if available
    creation_date: datetime = Field(serialization_alias="creationDate")  # Upload timestamp
    validation_errors: Optional[List[str]] = Field(default=None, serialization_alias="validationErrors")  # Validation or business rule errors, if any

    class Config:
        # Allow population of model fields from ORM object attributes
        from_attributes = True
        # Allow population by both field names and serialization aliases for flexibility
        populate_by_name = True
