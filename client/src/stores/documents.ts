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

        try {
            const apiUrl = import.meta.env.VITE_API_URL || '';
            let url = `${apiUrl}/api/documents/upload`;
            if (type) {
                url += `?type=${encodeURIComponent(type)}`;
            }

            const response = await axios.post(url, formData, {
                headers: {
                    Authorization: `Bearer ${authStore.token}`,
                    'Content-Type': 'multipart/form-data'
                }
            })
            await fetchDocuments() // Refresh list
            return response.data
        } catch (error) {
            console.error('Failed to upload document', error)
            throw error
        }
    }

    async function downloadDocument(id: number, filename: string) {
        try {
            const apiUrl = import.meta.env.VITE_API_URL || '';
            const response = await axios.get(`${apiUrl}/api/documents/${id}/download`, {
                headers: { Authorization: `Bearer ${authStore.token}` },
                responseType: 'blob' // Important for file handling
            })

            // Create a blob link to download
            const url = window.URL.createObjectURL(new Blob([response.data]))
            const link = document.createElement('a')
            link.href = url
            link.setAttribute('download', filename)
            document.body.appendChild(link)
            link.click()
            link.remove()
        } catch (error) {
            console.error('Failed to download document', error)
            throw error
        }
    }

    async function deleteDocument(id: number) {
        try {
            const apiUrl = import.meta.env.VITE_API_URL || '';
            await axios.delete(`${apiUrl}/api/documents/${id}`, {
                headers: { Authorization: `Bearer ${authStore.token}` }
            })
            await fetchDocuments() // Refresh list
        } catch (error) {
            console.error('Failed to delete document', error)
            throw error
        }
    }

    return { documents, loading, fetchDocuments, uploadDocument, downloadDocument, deleteDocument }
})
