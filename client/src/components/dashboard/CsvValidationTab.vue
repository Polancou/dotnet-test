<script setup lang="ts">
import { ref } from 'vue'
import { useDocumentsStore } from '../../stores/documents'

const documentsStore = useDocumentsStore()
const fileInput = ref<HTMLInputElement | null>(null)
const validationErrors = ref<string[]>([])
const uploadSuccess = ref(false)

function triggerUpload() {
    fileInput.value?.click()
}

async function handleFileUpload(event: Event) {
    const target = event.target as HTMLInputElement
    if (target.files && target.files.length > 0) {
        try {
            validationErrors.value = []
            uploadSuccess.value = false
            const response = await documentsStore.uploadDocument(target.files[0], 'UserBulk')

            if (response.validationErrors && response.validationErrors.length > 0) {
                validationErrors.value = response.validationErrors
            } else {
                uploadSuccess.value = true
            }
        } catch (e) {
            alert('Upload failed')
        } finally {
            // Reset input
            if (fileInput.value) fileInput.value.value = ''
        }
    }
}
</script>

<template>
    <div class="p-6 bg-white rounded-lg shadow-md">
        <h3 class="text-xl font-bold mb-4">CSV Validation</h3>
        <p class="mb-4 text-gray-600">Upload a CSV file to validate and bulk import users.</p>

        <div class="flex items-center gap-4 mb-6">
            <button @click="triggerUpload" class="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600">
                Upload CSV
            </button>
            <input type="file" ref="fileInput" accept=".csv" class="hidden" @change="handleFileUpload" />
        </div>

        <div v-if="uploadSuccess" class="p-4 mb-4 text-green-700 bg-green-100 rounded">
            File uploaded and processed successfully!
        </div>

        <div v-if="validationErrors.length > 0" class="mt-6">
            <h4 class="text-lg font-semibold mb-2 text-red-600">Validation Errors</h4>
            <div class="overflow-x-auto">
                <table class="min-w-full bg-white border border-gray-200">
                    <thead>
                        <tr>
                            <th class="py-2 px-4 border-b text-left">Error Details</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="(error, index) in validationErrors" :key="index" class="hover:bg-gray-50">
                            <td class="py-2 px-4 border-b text-red-500">{{ error }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</template>
