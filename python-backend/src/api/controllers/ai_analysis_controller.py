import json
from fastapi import APIRouter, Depends, UploadFile, File, HTTPException
from src.application.interfaces.interfaces import IAiAnalysisService, IDocumentService
from src.api.dependencies import get_ai_analysis_service, get_document_service, get_current_user
from src.domain.entities.user import User

from src.application.dtos.ai_analysis_dtos import AnalyzeDocumentResponse

# -----------------------------------------------------------------------------
# AI Analysis Controller
# -----------------------------------------------------------------------------
# This module defines the API endpoints related to AI-powered document analysis.
# It allows users to upload a document, analyzes it using an AI service,
# stores both the document and its analysis, and returns the analysis result.
# -----------------------------------------------------------------------------

router = APIRouter(prefix="/aianalysis", tags=["AI Analysis"])

@router.post("/analyze", response_model=AnalyzeDocumentResponse)
async def analyze_document(
    file: UploadFile = File(...),
    ai_analysis_service: IAiAnalysisService = Depends(get_ai_analysis_service),
    document_service: IDocumentService = Depends(get_document_service),
    current_user: User = Depends(get_current_user)
):
    """
    Analyze a document using AI:

    1. Uploads the provided document and associates it with the current user.
    2. Invokes the AI analysis service to process the document.
    3. Updates the uploaded document with the AI-generated analysis results.
    4. Returns a structured response containing the document ID and analysis.

    Args:
        file (UploadFile): The file object uploaded by the client.
        ai_analysis_service (IAiAnalysisService): Service for analyzing documents.
        document_service (IDocumentService): Service for handling document persistence.
        current_user (User): The authenticated user making the request.

    Returns:
        AnalyzeDocumentResponse: Contains the document ID and analysis result.
    """
    try:
        # Read the entire file content into memory (suitable for reasonable file sizes)
        content = await file.read()
        
        # ---------------------------------------------------------------------
        # 1. Upload the document, storing its metadata and content.
        #    The document is linked to the current user and marked as used for AI analysis.
        # ---------------------------------------------------------------------
        document_response = await document_service.upload_document(
            file_name=file.filename,
            content=content,
            content_type=file.content_type,
            user_id=current_user.id,
            role=current_user.role,
            process_type="AiAnalysis"
        )

        # ---------------------------------------------------------------------
        # 2. Perform AI analysis on the file content.
        #    This is an asynchronous operation that uses the AI analysis backend/service.
        # ---------------------------------------------------------------------
        analysis_result = await ai_analysis_service.analyze_document(content, file.filename)

        # ---------------------------------------------------------------------
        # 3. Update the document record with the serialized AI analysis result.
        #    The result is converted to JSON, using field aliases for consistent storage.
        #    Handles compatibility between Pydantic v1 & v2 serialization.
        # ---------------------------------------------------------------------
        try:
            # Pydantic v2: use model_dump_json for serialization with aliases
            json_result = analysis_result.model_dump_json(by_alias=True)
        except AttributeError:
            # Pydantic v1 fallback: use .json(by_alias=True)
            json_result = analysis_result.json(by_alias=True)
        
        document_service.update_analysis_result(document_response.id, json_result)

        # ---------------------------------------------------------------------
        # 4. Construct and return the API response.
        #    Includes the document's ID and the AI analysis result model.
        # ---------------------------------------------------------------------
        return AnalyzeDocumentResponse(
            document_id=document_response.id,
            analysis=analysis_result
        )
    except Exception as e:
        # Wrap any raised exceptions in a FastAPI HTTPException for client feedback
        raise HTTPException(status_code=400, detail=str(e))

