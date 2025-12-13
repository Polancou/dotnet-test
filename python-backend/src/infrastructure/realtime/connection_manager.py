from typing import List
from fastapi import WebSocket

class ConnectionManager:
    def __init__(self):
        self.active_connections: List[WebSocket] = []

    async def connect(self, websocket: WebSocket):
        await websocket.accept()
        self.active_connections.append(websocket)

    def disconnect(self, websocket: WebSocket):
        self.active_connections.remove(websocket)

    async def broadcast(self, message: dict):
        # Broadcasts a JSON message to all connected clients
        # Iterate over a copy to avoid modification during iteration if disconnects happen
        for connection in self.active_connections[:]:
            try:
                await connection.send_json(message)
            except Exception:
                # Handle broken pipes/disconnections gracefully
                pass

# Global singleton instance
connection_manager = ConnectionManager()
