import { defineStore } from 'pinia'
import { ref } from 'vue'
import axios from 'axios'
import { useAuthStore } from './auth'
import { HubConnectionBuilder, HubConnection } from '@microsoft/signalr'

export const useEventLogsStore = defineStore('eventlogs', () => {
    const logs = ref<any[]>([])
    const loading = ref(false)
    const authStore = useAuthStore()
    const connection = ref<HubConnection | null>(null)

    async function fetchLogs() {
        loading.value = true
        try {
            const apiUrl = import.meta.env.VITE_API_URL || '';
            const response = await axios.get(`${apiUrl}/api/eventlogs`, {
                headers: { Authorization: `Bearer ${authStore.token}` }
            })
            logs.value = response.data
        } catch (error) {
            console.error('Failed to fetch event logs', error)
        } finally {
            loading.value = false
        }
    }

    async function initializeConnection() {
        if (connection.value) return;

        const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000'; // Fallback or env
        // Note: SignalR usually needs the base URL, not full API URL if it's relative, but here we need absolute if separate.
        // Assuming VITE_API_URL is like http://localhost:5000

        connection.value = new HubConnectionBuilder()
            .withUrl(`${apiUrl}/hubs/eventLogs`)
            .withAutomaticReconnect()
            .build();

        connection.value.on("ReceiveLog", (log: any) => {
            logs.value.unshift(log); // Add to top
        });

        try {
            await connection.value.start();
            console.log("SignalR Connected");
        } catch (err) {
            console.error("SignalR Connection Error: ", err);
        }
    }

    return { logs, loading, fetchLogs, initializeConnection }
})
