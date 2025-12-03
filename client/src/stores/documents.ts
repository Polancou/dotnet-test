import { defineStore } from 'pinia'
import { ref } from 'vue'
import axios from 'axios'
import { useAuthStore } from './auth'

export const useDocumentsStore = defineStore('documents', () => {
    const documents = ref<any[]>([])
    const loading = ref(false)
    const authStore = useAuthStore()

    async function fetchDocuments() {
        loading.value = true
        try {
            const apiUrl = import.meta.env.VITE_API_URL || '';
            const response = await axios.get(`${apiUrl}/api/documents`, {
                headers: { Authorization: `Bearer ${authStore.token}` }
            })
            documents.value = response.data
        } catch (error) {
            console.error('Failed to fetch documents', error)
        } finally {
            loading.value = false
        }
    }

    async function uploadDocument(file: File, type?: string) {
        const formData = new FormData()
        formData.append('file', file)
        if (type) formData.append('type', type)

        try {
            const apiUrl = import.meta.env.VITE_API_URL || '';
            await axios.post(`${apiUrl}/api/documents/upload`, formData, {
                headers: {
                    Authorization: `Bearer ${authStore.token}`,
                    'Content-Type': 'multipart/form-data'
                }
            })
            await fetchDocuments() // Refresh list
        } catch (error) {
            console.error('Failed to upload document', error)
            throw error
        }
    }

    return { documents, loading, fetchDocuments, uploadDocument }
})
