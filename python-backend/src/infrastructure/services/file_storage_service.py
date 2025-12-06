import os
import uuid
from src.application.interfaces.interfaces import IFileStorageService

class FileStorageService(IFileStorageService):
    """
    Service responsible for handling file storage operations on the local filesystem.

    This implementation stores uploaded files in a dedicated 'uploads' directory within the current working directory.
    It ensures that every saved file has a unique name by prefixing the original filename with a random UUID.
    """

    def __init__(self):
        """
        Initialize the file storage service.

        Sets up the upload directory where files will be saved. Creates the directory if it doesn't already exist.
        """
        # Define the path to the uploads directory relative to the current working directory
        self.upload_directory = os.path.join(os.getcwd(), "uploads")
        # Create the uploads directory if it does not exist
        if not os.path.exists(self.upload_directory):
            os.makedirs(self.upload_directory)

    def save_file(self, file_content: bytes, file_name: str) -> str:
        """
        Save a file to the uploads directory with a unique filename.

        Args:
            file_content (bytes): The binary content of the file to be saved.
            file_name (str): The original name of the uploaded file.

        Returns:
            str: The full path to the saved file.
        """
        # Generate a unique filename by prepending a UUID to avoid naming collisions
        unique_file_name = f"{uuid.uuid4()}_{file_name}"
        # Build the full path to where the file will be saved
        file_path = os.path.join(self.upload_directory, unique_file_name)
        
        # Write the file's binary content to disk
        with open(file_path, "wb") as f:
            f.write(file_content)
            
        # Return the absolute file path of the saved file
        return file_path
