import * as signalR from '@microsoft/signalr';
import { IRealTimeProvider } from './IRealTimeProvider';

export class SignalRProvider implements IRealTimeProvider {
    private connection: signalR.HubConnection | null = null;
    private handlers: Map<string, Array<(data: any) => void>> = new Map();

    async connect(url: string, token: string): Promise<void> {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(url, {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

        // Re-attach handlers if connection rebuilds
        this.handlers.forEach((callbacks, event) => {
            callbacks.forEach(cb => this.connection?.on(event, cb));
        });

        try {
            await this.connection.start();
            console.log('SignalR Connected');
        } catch (err) {
            console.error('SignalR Connection Error: ', err);
            throw err;
        }
    }

    async disconnect(): Promise<void> {
        if (this.connection) {
            await this.connection.stop();
            this.connection = null;
        }
    }

    on(eventName: string, callback: (data: any) => void): void {
        if (!this.handlers.has(eventName)) {
            this.handlers.set(eventName, []);
        }
        this.handlers.get(eventName)?.push(callback);

        if (this.connection) {
            this.connection.on(eventName, callback);
        }
    }

    off(eventName: string, callback?: (data: any) => void): void {
        const callbacks = this.handlers.get(eventName);
        if (callbacks) {
            if (callback) {
                const index = callbacks.indexOf(callback);
                if (index !== -1) {
                    callbacks.splice(index, 1);
                    this.connection?.off(eventName, callback);
                }
            } else {
                this.handlers.delete(eventName);
                this.connection?.off(eventName);
            }
        }
    }

    async invoke(methodName: string, ...args: any[]): Promise<any> {
        if (!this.connection) {
            throw new Error('SignalR not connected');
        }
        return await this.connection.invoke(methodName, ...args);
    }
}
