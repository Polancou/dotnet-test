from sqlalchemy import Column, String, Integer, BigInteger, Boolean, ForeignKey
from sqlalchemy.orm import relationship
from src.domain.common.base_entity import BaseEntity

class Document(BaseEntity):
    """
    Document entity/model representing a file uploaded by a user.

    Inherits from BaseEntity, providing:
    - Standard id, is_active, and audit timestamps.

    This entity captures:
    - File metadata (name, storage path, type, size)
    - Processing state and result
    - Foreign key to uploading user

    Relationships:
    - uploaded_by_user: Reference to the User entity responsible for the upload.
    """

    __tablename__ = "documents"

    # Name of the file on upload (example: 'report.pdf'). Cannot be empty.
    file_name = Column(String, nullable=False)

    # Path where the file is stored (example: '/files/f239jk.pdf'). Cannot be empty.
    storage_path = Column(String, nullable=False)

    # MIME content type (example: 'application/pdf'). Cannot be empty.
    content_type = Column(String, nullable=False)

    # Size of the file in bytes.
    file_size = Column(BigInteger, nullable=False)

    # Whether the document has been analyzed/processed.
    is_processed = Column(Boolean, default=False, nullable=False)

    # Result from content analysis, classification, etc; can be null if not processed or result is not available.
    analysis_result = Column(String, nullable=True)

    # Foreign key referencing the User that uploaded the document.
    uploaded_by_user_id = Column(Integer, ForeignKey("users.id"), nullable=False)

    # SQLAlchemy relationship to the User entity.
    uploaded_by_user = relationship("User")

    def __init__(
        self,
        file_name: str,
        storage_path: str,
        content_type: str,
        file_size: int,
        uploaded_by_user_id: int
    ):
        """
        Initialize a new Document entity with file metadata and uploader.

        Args:
            file_name (str): Name of the uploaded file. Cannot be empty.
            storage_path (str): Path to where the file is stored. Cannot be empty.
            content_type (str): File MIME type.
            file_size (int): File size in bytes.
            uploaded_by_user_id (int): Foreign key of the uploading user.

        Raises:
            ValueError: If file_name or storage_path is empty.
        """
        # Validate file_name
        if not file_name:
            raise ValueError("File name cannot be empty")
        # Validate storage_path
        if not storage_path:
            raise ValueError("Storage path cannot be empty")

        # Set entity attributes
        self.file_name = file_name
        self.storage_path = storage_path
        self.content_type = content_type
        self.file_size = file_size
        self.uploaded_by_user_id = uploaded_by_user_id

        # Initially mark as not processed. Processing status should be updated via mark_as_processed().
        self.is_processed = False

        # Initialize base entity (ensures is_active is set to True by default)
        super().__init__(is_active=True)

    def mark_as_processed(self, result: str):
        """
        Mark the document as processed and store analysis/classification result.

        Args:
            result (str): The result of document analysis, e.g., classification output.

        This method updates:
            - is_processed to True
            - analysis_result to the given result value
            - modification_date timestamp (via update_modification_date())
        """
        self.is_processed = True
        self.analysis_result = result
        self.update_modification_date()
