<script setup lang="ts">
import { ref } from 'vue'
import { useDocumentsStore } from '../../stores/documents'
import SkeletonLoader from '../SkeletonLoader.vue'
import FileDropzone from '../FileDropzone.vue'

const documentsStore = useDocumentsStore()
const validationErrors = ref<string[]>([])
const uploadSuccess = ref(false)
const loading = ref(false)

async function handleFilesSelected(files: FileList) {
    if (files.length > 0) {
        loading.value = true
        try {
            validationErrors.value = []
            uploadSuccess.value = false
            const response = await documentsStore.uploadDocument(files[0], 'UserBulk')

            if (response.validationErrors && response.validationErrors.length > 0) {
                validationErrors.value = response.validationErrors
            } else {
                uploadSuccess.value = true
            }
        } catch (e) {
            alert('Upload failed')
        } finally {
            loading.value = false
        }
    }
}
</script>

<template>
    <div class="p-6 bg-white rounded-lg shadow-md">
        <h3 class="text-xl font-bold mb-4">CSV Validation</h3>
        <p class="mb-4 text-gray-600">Upload a CSV file to validate and bulk import users.</p>

        <div class="mb-6">
            <FileDropzone 
                accept=".csv" 
                label="Drag and drop CSV file here or click to upload" 
                :loading="loading"
                @files-selected="handleFilesSelected"
            />
        </div>

        <div v-if="loading" class="space-y-4">
            <SkeletonLoader className="h-12 w-full rounded-lg" />
            <SkeletonLoader className="h-64 w-full rounded-lg" />
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
