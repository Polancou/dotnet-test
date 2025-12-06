from typing import List, Optional, Any
from src.application.interfaces.interfaces import (
    IDocumentService,
    IDocumentRepository,
    IUnitOfWork,
    IFileStorageService,
    ICsvService,
    IEventLogService,
)
from src.domain.entities.document import Document
from src.domain.enums.user_role import UserRole
from src.application.dtos.document_dtos import DocumentResponse

class DocumentService(IDocumentService):
    """
    Handles document-related operations including upload, retrieval, processing, and event logging.

    This service coordinates with the Repository, Unit of Work, File Storage, Bulk CSV processing,
    and Event Log services to implement all business logic involving documents, especially around
    secure file handling, validation, and processing workflows.
    """

    def __init__(
        self,
        document_repository: IDocumentRepository,
        unit_of_work: IUnitOfWork,
        file_storage_service: IFileStorageService,
        csv_service: ICsvService,
        event_log_service: IEventLogService,
    ):
        """
        Initializes the DocumentService with its component dependencies.

        Args:
            document_repository (IDocumentRepository): Manages persistence of Document entities.
            unit_of_work (IUnitOfWork): Handles transaction commit/rollback.
            file_storage_service (IFileStorageService): Stores uploaded files to disk or cloud.
            csv_service (ICsvService): Processes bulk user-upload CSVs.
            event_log_service (IEventLogService): Audits document actions/events.
        """
        self.document_repository = document_repository
        self.unit_of_work = unit_of_work
        self.file_storage_service = file_storage_service
        self.csv_service = csv_service
        self.event_log_service = event_log_service

    def upload_document(
        self,
        file_name: str,
        content: bytes,
        content_type: str,
        user_id: int,
        role: UserRole,
        process_type: Optional[str] = None,
    ) -> DocumentResponse:
        """
        Handles an uploaded document: stores file, instantiates Document, optionally processes bulk CSV.

        For typical uploads, simply records the document and stores the file.
        For user bulk uploads ("UserBulk" process_type), delegates CSV content to CsvService for validation and ingestion.

        Args:
            file_name (str): Name of the uploaded file.
            content (bytes): File content bytes.
            content_type (str): MIME content type.
            user_id (int): ID of the uploading user.
            role (UserRole): Role of the uploading user (for access checks).
            process_type (Optional[str]): Indicates special processing (e.g. "UserBulk").

        Returns:
            DocumentResponse: DTO for API/client, summarizing the saved document (and any validation errors).

        Raises:
            Exception: If non-admin attempts user bulk upload.
        """
        # Save the file using the FileStorageService, get storage path (disk or cloud).
        storage_path = self.file_storage_service.save_file(content, file_name, content_type)

        # Create a Document entity but do not yet persist.
        document = Document(file_name, storage_path, content_type, len(content), user_id)

        validation_errors = None  # Collects validation errors if "UserBulk" processing, else None.

        # If processing a user bulk upload (special import CSV):
        if process_type == "UserBulk" and content_type in ["text/csv", "application/vnd.ms-excel", "text/plain"]:
            # Restrict user bulk uploads to admins only.
            if role != UserRole.ADMIN:
                raise Exception("Only admins can perform bulk user uploads.")

            # Delegate CSV parsing, validation, and user import to CsvService.
            result = self.csv_service.process_user_bulk_upload(content)
            result_message = f"Processed: {result['success_count']} success, {result['failure_count']} failed."
            # Mark document as processed, store import results in Document.
            document.mark_as_processed(result_message)
            # Capture per-row validation or parsing errors to report.
            validation_errors = result['errors']

        # Persist the new/updated Document entity.
        self.document_repository.add(document)
        # Commit transaction (write changes atomically).
        self.unit_of_work.commit()

        # Log this document upload event for traceability/auditing.
        self.event_log_service.log_event("Document Upload", f"User uploaded {file_name}", user_id)

        # Prepare API/client response (serializable DTO), including any process or validation outcome.
        return DocumentResponse(
            id=document.id,
            file_name=document.file_name,
            content_type=document.content_type,
            file_size=document.file_size,
            is_processed=document.is_processed,
            analysis_result=document.analysis_result,
            creation_date=document.creation_date,
            validation_errors=validation_errors,
        )

    def get_user_documents(self, user_id: int) -> List[Document]:
        """
        Retrieve all documents belonging to the given user.

        Args:
            user_id (int): User identifier.

        Returns:
            List[Document]: All matching documents for this user.
        """
        # Query the repository for documents by user ID.
        return self.document_repository.get_by_user_id(user_id)

    def update_analysis_result(self, document_id: int, analysis_result: str):
        """
        Records the result of processing or analyzing a document file.

        Marks the document as processed, attaches the supplied analysis result string,
        and persists the change.

        Args:
            document_id (int): Document's unique identifier.
            analysis_result (str): Extracted or computed result (could be JSON, summary, etc).
        """
        # Retrieve the Document entity by primary key.
        document = self.document_repository.get_by_id(document_id)
        if document:
            # Set analysis result and mark as processed.
            document.mark_as_processed(analysis_result)
            # Commit the update.
            self.unit_of_work.commit()
