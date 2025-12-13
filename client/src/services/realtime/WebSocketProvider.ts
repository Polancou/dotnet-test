import { IRealTimeProvider } from './IRealTimeProvider';

export class WebSocketProvider implements IRealTimeProvider {
    private socket: WebSocket | null = null;
    private handlers: Map<string, Array<(data: any) => void>> = new Map();
    private url: string = '';
    private token: string = '';
    private reconnectInterval: number = 3000;
    private isExplicitDisconnect: boolean = false;

    async connect(url: string, token: string): Promise<void> {
        this.url = url;
        this.token = token;
        this.isExplicitDisconnect = false;

        return new Promise((resolve, reject) => {
            // Adjust URL for WebSocket protocol (http -> ws, https -> wss)
            // And append token as query param since standard WS doesn't support headers easily in browser
            const wsUrl = new URL(url);
            wsUrl.protocol = wsUrl.protocol === 'https:' ? 'wss:' : 'ws:';
            // Assuming python backend expects token in query for WS
            wsUrl.searchParams.set('token', token);

            this.socket = new WebSocket(wsUrl.toString());

            this.socket.onopen = () => {
                console.log('WebSocket Connected');
                resolve();
            };

            this.socket.onerror = (error) => {
                console.error('WebSocket Error:', error);
                // Only reject if it's the initial connection
                if (this.socket?.readyState === WebSocket.CONNECTING) {
                    reject(error);
                }
            };

            this.socket.onclose = () => {
                console.log('WebSocket Disconnected');
                if (!this.isExplicitDisconnect) {
                    this.reconnect();
                }
            };

            this.socket.onmessage = (event) => {
                try {
                    const message = JSON.parse(event.data);
                    // Expecting message format: { type: "EventName", payload: { ... } }
                    if (message.type && this.handlers.has(message.type)) {
                        this.handlers.get(message.type)?.forEach(cb => cb(message.payload));
                    }
                } catch (e) {
                    console.error('Failed to parse WebSocket message', e);
                }
            };
        });
    }

    private reconnect() {
        console.log(`Reconnecting WebSocket in ${this.reconnectInterval}ms...`);
        setTimeout(() => {
            if (!this.isExplicitDisconnect) {
                this.connect(this.url, this.token).catch(() => {
                    // Swallow error on reconnect attempt, logic handles retry loop via onclose
                });
            }
        }, this.reconnectInterval);
    }

    async disconnect(): Promise<void> {
        this.isExplicitDisconnect = true;
        if (this.socket) {
            this.socket.close();
            this.socket = null;
        }
    }

    on(eventName: string, callback: (data: any) => void): void {
        if (!this.handlers.has(eventName)) {
            this.handlers.set(eventName, []);
        }
        this.handlers.get(eventName)?.push(callback);
    }

    off(eventName: string, callback?: (data: any) => void): void {
        const callbacks = this.handlers.get(eventName);
        if (callbacks) {
            if (callback) {
                const index = callbacks.indexOf(callback);
                if (index !== -1) {
                    callbacks.splice(index, 1);
                }
            } else {
                this.handlers.delete(eventName);
            }
        }
    }

    async invoke(methodName: string, ...args: any[]): Promise<any> {
        // Simple send for now, assumes 'invoke' means sending a message
        if (this.socket && this.socket.readyState === WebSocket.OPEN) {
            this.socket.send(JSON.stringify({ type: methodName, payload: args }));
        } else {
            console.warn('WebSocket not connected, cannot invoke', methodName);
        }
    }
}
