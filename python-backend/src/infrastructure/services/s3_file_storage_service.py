import uuid
import boto3
from botocore.exceptions import ClientError, BotoCoreError
from typing import Optional
from src.application.interfaces.interfaces import IFileStorageService
from src.config import settings


class S3FileStorageService(IFileStorageService):
    """
    Service responsible for handling file storage operations on AWS S3.

    This implementation stores uploaded files in an S3 bucket configured via environment variables.
    It ensures that every saved file has a unique name by prefixing the original filename with a random UUID.
    Files are stored with their content type for proper serving.
    """

    def __init__(
        self,
        access_key_id: Optional[str] = None,
        secret_access_key: Optional[str] = None,
        region: Optional[str] = None,
        bucket_name: Optional[str] = None
    ):
        """
        Initialize the S3 file storage service.

        Args:
            access_key_id: AWS access key ID. If not provided, uses settings.AWS_ACCESS_KEY_ID.
            secret_access_key: AWS secret access key. If not provided, uses settings.AWS_SECRET_ACCESS_KEY.
            region: AWS region. If not provided, uses settings.AWS_REGION.
            bucket_name: S3 bucket name. If not provided, uses settings.AWS_S3_BUCKET_NAME.

        Raises:
            ValueError: If required AWS credentials or bucket name are not configured.
        """
        self.access_key_id = access_key_id or settings.AWS_ACCESS_KEY_ID
        self.secret_access_key = secret_access_key or settings.AWS_SECRET_ACCESS_KEY
        self.region = region or settings.AWS_REGION
        self.bucket_name = bucket_name or settings.AWS_S3_BUCKET_NAME

        # Validate required configuration
        if not self.access_key_id or not self.secret_access_key:
            raise ValueError("AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY must be configured for S3 storage")
        if not self.bucket_name:
            raise ValueError("AWS_S3_BUCKET_NAME must be configured for S3 storage")

        # Initialize S3 client
        self.s3_client = boto3.client(
            's3',
            aws_access_key_id=self.access_key_id,
            aws_secret_access_key=self.secret_access_key,
            region_name=self.region
        )

        # Verify bucket exists and is accessible
        try:
            self.s3_client.head_bucket(Bucket=self.bucket_name)
        except ClientError as e:
            error_code = e.response.get('Error', {}).get('Code', '')
            if error_code == '404':
                raise ValueError(f"S3 bucket '{self.bucket_name}' does not exist")
            elif error_code == '403':
                raise ValueError(f"Access denied to S3 bucket '{self.bucket_name}'. Check your AWS credentials.")
            else:
                raise ValueError(f"Error accessing S3 bucket '{self.bucket_name}': {str(e)}")

    def save_file(self, file_content: bytes, file_name: str, content_type: Optional[str] = None) -> str:
        """
        Save a file to S3 with a unique filename.

        Args:
            file_content (bytes): The binary content of the file to be saved.
            file_name (str): The original name of the uploaded file.
            content_type (Optional[str]): MIME content type of the file. If not provided, will be inferred.

        Returns:
            str: The S3 object key (path) of the saved file, which can be used to construct the full S3 URL.

        Raises:
            Exception: If the file cannot be uploaded to S3.
        """
        # Generate a unique filename by prepending a UUID to avoid naming collisions
        unique_file_name = f"{uuid.uuid4()}_{file_name}"
        
        # Determine content type if not provided
        if not content_type:
            # Simple content type detection based on file extension
            content_type = self._get_content_type(file_name)

        try:
            # Upload file to S3
            self.s3_client.put_object(
                Bucket=self.bucket_name,
                Key=unique_file_name,
                Body=file_content,
                ContentType=content_type,
                ServerSideEncryption='AES256'  # Enable server-side encryption
            )

            # Return the S3 object key (can be used to construct URL: s3://bucket/key or https://bucket.s3.region.amazonaws.com/key)
            return f"s3://{self.bucket_name}/{unique_file_name}"
        except (ClientError, BotoCoreError) as e:
            raise Exception(f"Failed to upload file to S3: {str(e)}")

    def _get_content_type(self, file_name: str) -> str:
        """
        Infer content type from file extension.

        Args:
            file_name (str): The file name.

        Returns:
            str: MIME content type.
        """
        extension = file_name.lower().split('.')[-1] if '.' in file_name else ''
        content_types = {
            'pdf': 'application/pdf',
            'txt': 'text/plain',
            'csv': 'text/csv',
            'json': 'application/json',
            'xml': 'application/xml',
            'jpg': 'image/jpeg',
            'jpeg': 'image/jpeg',
            'png': 'image/png',
            'gif': 'image/gif',
            'doc': 'application/msword',
            'docx': 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
            'xls': 'application/vnd.ms-excel',
            'xlsx': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        }
        return content_types.get(extension, 'application/octet-stream')

