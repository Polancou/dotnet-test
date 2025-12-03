<script setup lang="ts">
import { ref } from 'vue'
import axios from 'axios'
import { useAuthStore } from '../../stores/auth'

const authStore = useAuthStore()
const fileInput = ref<HTMLInputElement | null>(null)
const analysisResult = ref<string | null>(null)
const loading = ref(false)

function triggerUpload() {
    fileInput.value?.click()
}

async function handleFileUpload(event: Event) {
    const target = event.target as HTMLInputElement
    if (target.files && target.files.length > 0) {
        loading.value = true
        analysisResult.value = null

        const formData = new FormData()
        formData.append('file', target.files[0])

        try {
            const apiUrl = import.meta.env.VITE_API_URL || '';
            const response = await axios.post(`${apiUrl}/api/aianalysis/analyze`, formData, {
                headers: {
                    Authorization: `Bearer ${authStore.token}`,
                    'Content-Type': 'multipart/form-data'
                }
            })
            analysisResult.value = response.data.result
        } catch (e) {
            alert('Analysis failed')
        } finally {
            loading.value = false
            if (fileInput.value) fileInput.value.value = ''
        }
    }
}
</script>

<template>
    <div class="p-6 bg-white rounded-lg shadow-md">
        <h3 class="text-xl font-bold mb-4">AI Analysis</h3>
        <p class="mb-4 text-gray-600">Upload a document (PDF, JPG, PNG) to analyze its content type.</p>

        <div class="flex items-center gap-4 mb-6">
            <button @click="triggerUpload" class="bg-purple-500 text-white px-4 py-2 rounded hover:bg-purple-600"
                :disabled="loading">
                {{ loading ? 'Analyzing...' : 'Upload & Analyze' }}
            </button>
            <input type="file" ref="fileInput" accept=".pdf,.jpg,.jpeg,.png" class="hidden"
                @change="handleFileUpload" />
        </div>

        <div v-if="analysisResult" class="mt-6 p-4 bg-gray-50 rounded border border-gray-200">
            <h4 class="text-lg font-semibold mb-2">Analysis Result</h4>
            <p class="text-xl text-purple-700 font-bold">{{ analysisResult }}</p>
        </div>
    </div>
</template>
