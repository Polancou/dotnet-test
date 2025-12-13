import { defineStore } from 'pinia'
import { ref } from 'vue'
import axios from 'axios'
import { useAuthStore } from './auth'
import { RealTimeService } from '../services/realtime/RealTimeService'

export const useEventLogsStore = defineStore('eventlogs', () => {
    const logs = ref<any[]>([])
    const loading = ref(false)
    const authStore = useAuthStore()


    const isConnected = ref(false)

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
        if (isConnected.value) return;

        const provider = RealTimeService.getInstance();
        const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000';
        const backendType = import.meta.env.VITE_BACKEND_TYPE || 'DOTNET';
        let connectionUrl = '';

        if (backendType === 'PYTHON') {
            connectionUrl = `${apiUrl}/ws/eventlogs`;
        } else {
            connectionUrl = `${apiUrl}/hubs/eventLogs`;
        }

        // Set up listener before connecting
        // Note: provider.on handles multiple listeners safely (pushes to array)
        // But we only want ONE listener for the store to update state.
        // We should clear previous listeners if any to be safe or ensure idempotency.
        provider.off("ReceiveLog");

        provider.on("ReceiveLog", (log: any) => {
            console.log("Received new log:", log);
            logs.value.unshift(log);
        });

        try {
            await provider.connect(connectionUrl, authStore.token || '');
            isConnected.value = true;
        } catch (err) {
            console.error("RealTime Connection Error: ", err);
            isConnected.value = false;
        }
    }

    return { logs, loading, fetchLogs, initializeConnection, isConnected }
})
