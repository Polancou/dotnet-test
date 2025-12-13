from typing import List, Optional
from fastapi import APIRouter, Depends, UploadFile, File, Form, HTTPException, Query
from src.application.interfaces.interfaces import IDocumentService
from src.application.dtos.document_dtos import DocumentResponse
from src.api.dependencies import get_document_service, get_current_user
from src.domain.entities.user import User

# -----------------------------------------------------------------------------
# Documents Controller
# -----------------------------------------------------------------------------
# Exposes endpoints for uploading documents and retrieving a user's documents.
# Utilizes dependency-injected DocumentService for business logic.
# -----------------------------------------------------------------------------

router = APIRouter(prefix="/documents", tags=["Documents"])

@router.post("/upload", response_model=DocumentResponse)
async def upload_document(
    file: UploadFile = File(...), 
    type: Optional[str] = Query(None),
    document_service: IDocumentService = Depends(get_document_service),
    current_user: User = Depends(get_current_user)
):
    """
    Upload a new document for the authenticated user.

    This endpoint accepts a file upload and (optionally) a process type.
    The document is stored and associated with the current user. Returns the
    saved document's metadata.

    Args:
        file (UploadFile): File object submitted by the client.
        type (Optional[str]): Optional document process type.
        document_service (IDocumentService): Service for handling document storage.
        current_user (User): The authenticated user.

    Returns:
        DocumentResponse: Details of the uploaded document.

    Raises:
        HTTPException: 400 if upload fails.
    """
    try:
        # Read the file contents into memory
        content = await file.read()
        # Call the application service to handle persistence and metadata
        return await document_service.upload_document(
            file_name=file.filename,
            content=content,
            content_type=file.content_type,
            user_id=current_user.id,
            role=current_user.role,
            process_type=type
        )
    except Exception as e:
        # Catch and report any error during file processing or business logic
        raise HTTPException(status_code=400, detail=str(e))

@router.get("/", response_model=List[DocumentResponse])
async def get_my_documents(
    document_service: IDocumentService = Depends(get_document_service),
    current_user: User = Depends(get_current_user)
):
    """
    Retrieve all documents belonging to the authenticated user.

    Returns a list of document metadata associated with the current user.

    Args:
        document_service (IDocumentService): Document service dependency.
        current_user (User): The authenticated user.

    Returns:
        List[DocumentResponse]: List of user's documents.
    """
    # Fetch all documents belonging to the current user via the service
    return await document_service.get_user_documents(current_user.id)

@router.get("/{document_id}/download")
async def download_document(
    document_id: int,
    document_service: IDocumentService = Depends(get_document_service),
    current_user: User = Depends(get_current_user)
):
    """
    Download a specific document.
    """
    try:
        # Retrieve file stream (or body) from service
        file_body = await document_service.download_document(document_id, current_user.id)
        
        # Return as a StreamingResponse
        # Note: In a real app, you might want to look up the filename/content-type to set headers
        from fastapi.responses import StreamingResponse
        return StreamingResponse(file_body, media_type="application/octet-stream")
    except Exception as e:
        if "not found" in str(e).lower():
            raise HTTPException(status_code=404, detail=str(e))
        if "authorized" in str(e).lower():
            raise HTTPException(status_code=403, detail=str(e))
        raise HTTPException(status_code=400, detail=str(e))

@router.delete("/{document_id}")
async def delete_document(
    document_id: int,
    document_service: IDocumentService = Depends(get_document_service),
    current_user: User = Depends(get_current_user)
):
    """
    Delete a specific document.
    """
    try:
        await document_service.delete_document(document_id, current_user.id)
        # Return 204 No Content
        from fastapi.responses import Response
        return Response(status_code=204)
    except Exception as e:
        if "not found" in str(e).lower():
            raise HTTPException(status_code=404, detail=str(e))
        if "authorized" in str(e).lower():
            raise HTTPException(status_code=403, detail=str(e))
        raise HTTPException(status_code=400, detail=str(e))
