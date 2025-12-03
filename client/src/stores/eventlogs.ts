import { defineStore } from 'pinia'
import { ref } from 'vue'
import axios from 'axios'
import { useAuthStore } from './auth'

export const useEventLogsStore = defineStore('eventlogs', () => {
    const logs = ref<any[]>([])
    const loading = ref(false)
    const authStore = useAuthStore()

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

    return { logs, loading, fetchLogs }
})
