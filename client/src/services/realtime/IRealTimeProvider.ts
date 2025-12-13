export interface IRealTimeProvider {
    connect(url: string, token: string): Promise<void>;
    disconnect(): Promise<void>;
    on(eventName: string, callback: (data: any) => void): void;
    off(eventName: string, callback?: (data: any) => void): void;
    invoke(methodName: string, ...args: any[]): Promise<any>;
}
