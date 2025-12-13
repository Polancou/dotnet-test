import { IRealTimeProvider } from './IRealTimeProvider';
import { SignalRProvider } from './SignalRProvider';
import { WebSocketProvider } from './WebSocketProvider';

export class RealTimeService {
    private static instance: IRealTimeProvider;

    public static getInstance(): IRealTimeProvider {
        if (!this.instance) {
            const backendType = import.meta.env.VITE_BACKEND_TYPE || 'DOTNET';

            if (backendType === 'PYTHON') {
                console.log('Using WebSocket Provider (Python)');
                this.instance = new WebSocketProvider();
            } else {
                console.log('Using SignalR Provider (.NET)');
                this.instance = new SignalRProvider();
            }
        }
        return this.instance;
    }
}
