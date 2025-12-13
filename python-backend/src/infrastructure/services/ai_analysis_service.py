import asyncio
import io
import base64
from datetime import datetime
from typing import List, Any
from src.application.interfaces.interfaces import IAiAnalysisService, IEventLogService
from src.application.dtos.ai_analysis_dtos import AnalysisResultDto, InvoiceData, InformationData, ProductDto
from src.config import settings

class AiAnalysisService(IAiAnalysisService):
    """
    Service responsible for analyzing documents (images or text) using a generative AI model (Gemini).
    It evaluates if a document is an Invoice or Information, extracts key fields, and gracefully
    falls back to a local mock if the API is unavailable.
    All analysis attempts are logged via IEventLogService.
    """
    def __init__(self, event_log_service: IEventLogService):
        """
        Initialize the AI analysis service.

        Args:
            event_log_service (IEventLogService): Service used for logging analytics and errors.
        """
        self.event_log_service = event_log_service

    async def analyze_document(self, file_content: bytes, file_name: str) -> Any:
        """
        Analyze a document (image or text-based) for semantic classification (Invoice/Information) and extract data.
        Relies on Gemini API if available, otherwise uses a mock method.

        Args:
            file_content (bytes): The raw content of the document file.
            file_name (str): The name of the uploaded file.

        Returns:
            AnalysisResultDto: Structured result of analysis (Invoice fields or Information summary).
        """
        api_key = settings.GEMINI_API_KEY
        
        # Use fallback if API key is missing
        if not api_key:
            await self.event_log_service.log_event("AI Analysis Warning", "Gemini API Key not found. Using mock.")
            return await self._mock_analyze(file_name)

        try:
            import google.generativeai as genai
            import json
            
            genai.configure(api_key=api_key)
            model = genai.GenerativeModel('gemini-2.5-flash')
            
            # Determine file type (image vs. text)
            extension = file_name.lower().split('.')[-1] if '.' in file_name else ""
            is_image = self._is_image(extension)
            
            parts = []
            
            # ----- SYSTEM PROMPT DESIGN -----
            # Provides clear JSON contract to the AI model
            system_prompt = """
You are an expert document analyzer. Analyze the provided document (text or image) and determine if it is an 'Invoice' or 'Information'.
Return ONLY a valid JSON object matching this structure:
{
    "documentType": "Invoice" | "Information",
    "invoiceData": {
        "clientName": "string",
        "clientAddress": "string",
        "providerName": "string",
        "providerAddress": "string",
        "invoiceNumber": "string",
        "date": "YYYY-MM-DD",
        "total": 0.00,
        "products": [
            { "name": "string", "quantity": 0, "unitPrice": 0.00, "total": 0.00 }
        ]
    },
    "informationData": {
        "description": "string",
        "summary": "string",
        "sentiment": "Positive" | "Negative" | "Neutral"
    }
}
If it is an Invoice, populate 'invoiceData' and leave 'informationData' null.
If it is Information, populate 'informationData' and leave 'invoiceData' null.
Ensure dates are valid ISO 8601 strings. Do not use markdown code blocks in response, just raw JSON.
"""
            parts.append(system_prompt)

            if is_image:
                # Build image payload for Gemini API (set appropriate image MIME type)
                mime_type = "image/png" if extension == "png" else "image/jpeg"
                parts.append({
                    "mime_type": mime_type,
                    "data": file_content
                })
                parts.append("Analyze this image document.")
            else:
                # Extract text content from file; raise if not possible
                extracted_text = self._extract_content(file_content, file_name)
                if not extracted_text or not extracted_text.strip():
                    raise Exception("Could not extract text.")
                
                # Limit text length to avoid API overload
                if len(extracted_text) > 30000:
                    extracted_text = extracted_text[:30000]
                
                parts.append(f"Analyze this document content:\n\n{extracted_text}")
            
            # ----- GEMINI API CALL -----
            response = model.generate_content(parts)
            response_text = response.text
            
            # ----- Postprocess AI Output for JSON Only -----
            # Remove any accidental markdown or code fences around the JSON that Gemini may return
            if "```json" in response_text:
                response_text = response_text.split("```json")[1].split("```")[0]
            elif "```" in response_text:
                response_text = response_text.split("```")[1].split("```")[0]
                
            # Parse JSON result
            data = json.loads(response_text.strip())
            
            # Convert dict to DTO (will validate fields)
            result = AnalysisResultDto(**data)
            
            # ----- LOGGING -----
            log_summary = ""
            if result.document_type == "Invoice" and result.invoice_data:
                log_summary = f"Invoice {result.invoice_data.invoice_number} for {result.invoice_data.total}"
            elif result.information_data:
                summary_text = result.information_data.summary or ""
                log_summary = f"Info: {summary_text[:50]}..."
                
            await self.event_log_service.log_event("AI Analysis", f"Analyzed {file_name}: {log_summary}")
            
            return result
            
        except Exception as e:
            # Handle API/model errors gracefully and log failure event
            await self.event_log_service.log_event("AI Analysis Error", f"Failed to analyze {file_name}: {str(e)}")
            print(f"Gemini Error: {e}")
            # Return fallback "analysis failed" info type result for visibility
            return AnalysisResultDto(
                document_type="Information",
                information_data=InformationData(
                    description="Analysis Failed",
                    summary=f"Error: {str(e)}",
                    sentiment="Neutral"
                )
            )

    def _is_image(self, extension: str) -> bool:
        """
        Helper to check if the file extension is a supported image type.

        Args:
            extension (str): The file extension.

        Returns:
            bool: True if the extension is an image, else False.
        """
        return extension in ["jpg", "jpeg", "png"]

    def _extract_content(self, file_content: bytes, file_name: str) -> str:
        """
        Extracts text content from a file based on its format.

        - For PDFs, uses pypdf to extract all pages.
        - For plain text formats (txt/csv/json/md), decodes as UTF-8.
        - Returns filename string for unrecognized types.

        Args:
            file_content (bytes): Raw file bytes
            file_name (str): Name of uploaded file

        Returns:
            str: Extracted text content or error placeholder.
        """
        extension = file_name.lower().split('.')[-1] if '.' in file_name else ""
        
        if extension == "pdf":
            try:
                import pypdf
                pdf_file = io.BytesIO(file_content)
                reader = pypdf.PdfReader(pdf_file)
                text = ""
                for page in reader.pages:
                    text += page.extract_text() + " "
                return text
            except Exception as e:
                print(f"PDF Extraction Error: {e}")
                return "PDF content could not be extracted."
        
        elif extension in ["txt", "csv", "json", "md"]:
            try:
                return file_content.decode('utf-8', errors='ignore')
            except:
                return ""
        
        # If file could not be parsed, return its name for display/logging
        return f"[File: {file_name}]"

    async def _mock_analyze(self, file_name: str) -> AnalysisResultDto:
        """
        Fake/mock analysis for local development and when the AI API key is missing.
        Synthesizes an Invoice or Information result based on file_name.

        Args:
            file_name (str): The uploaded file name.

        Returns:
            AnalysisResultDto: Mock analysis result matching the DTO contract.
        """
        await asyncio.sleep(1.5)  # Simulate async delay
        if "invoice" in file_name.lower():
            return AnalysisResultDto(
                document_type="Invoice",
                invoice_data=InvoiceData(
                    client_name="Tech Solutions Inc.",
                    client_address="123 Innovation Dr",
                    provider_name="Cloud Services LLC",
                    provider_address="456 Server Ave",
                    invoice_number="INV-MOCK-001",
                    date=datetime.now(),
                    total=1500.00,
                    products=[
                        ProductDto(name="Mock Service", quantity=1, unit_price=1500.0, total=1500.0)
                    ]
                )
            )
        else:
            return AnalysisResultDto(
                document_type="Information",
                information_data=InformationData(
                    description="Mock Info",
                    summary="This is a mock response.",
                    sentiment="Neutral"
                )
            )
