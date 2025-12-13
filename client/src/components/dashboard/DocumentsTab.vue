<script setup lang="ts">
import { onMounted } from 'vue'
import { useDocumentsStore } from '../../stores/documents'
import PageLayout from '../PageLayout.vue'
import { 
    ArrowPathIcon, 
    DocumentIcon, 
    ArrowDownTrayIcon, 
    TrashIcon 
} from '@heroicons/vue/24/outline'

const documentsStore = useDocumentsStore()


onMounted(async () => {
    await documentsStore.fetchDocuments()
})



const handleDownload = async (doc: any) => {
    try {
        await documentsStore.downloadDocument(doc.id, doc.fileName)
    } catch (error) {
        alert('Failed to download document')
    }
}

const handleDelete = async (doc: any) => {
    if (!confirm(`Are you sure you want to delete "${doc.fileName}"?`)) return

    try {
        await documentsStore.deleteDocument(doc.id)
    } catch (error) {
        alert('Failed to delete document')
    }
}

const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString()
}
</script>

<template>
    <PageLayout title="My Documents" subtitle="Manage and view your uploaded documents.">
        <template #actions>
            <button @click="documentsStore.fetchDocuments()" 
                class="px-3 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200 text-sm font-medium flex items-center gap-1 transition-colors">
                <ArrowPathIcon class="h-4 w-4" />
                <span>Refresh</span>
            </button>
        </template>



        <div v-if="documentsStore.loading" class="flex justify-center py-12">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>

        <div v-else>
            <!-- Desktop Table -->
            <div class="hidden md:block overflow-hidden bg-white shadow-sm rounded-lg border border-gray-200">
                <table class="min-w-full divide-y divide-gray-200">
                    <thead class="bg-gray-50">
                        <tr>
                            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">File Name</th>
                            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Type</th>
                            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Size</th>
                            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Date</th>
                            <th class="px-6 py-3 text-right text-xs font-semibold text-gray-500 uppercase tracking-wider">Actions</th>
                        </tr>
                    </thead>
                    <tbody class="bg-white divide-y divide-gray-200">
                        <tr v-for="doc in documentsStore.documents" :key="doc.id" class="hover:bg-gray-50 transition-colors">
                            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 flex items-center gap-2">
                                <DocumentIcon class="h-5 w-5 text-gray-400" />
                                {{ doc.fileName }}
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ doc.contentType }}</td>
                            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ (doc.fileSize / 1024).toFixed(2) }} KB</td>
                            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ formatDate(doc.creationDate) }}</td>
                            <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium space-x-3">
                                <button @click="handleDownload(doc)" class="text-blue-600 hover:text-blue-900 font-medium flex items-center gap-1 inline-flex">
                                    <ArrowDownTrayIcon class="h-4 w-4" /> Download
                                </button>
                                <button @click="handleDelete(doc)" class="text-red-600 hover:text-red-900 font-medium flex items-center gap-1 inline-flex">
                                    <TrashIcon class="h-4 w-4" /> Delete
                                </button>
                            </td>
                        </tr>
                        <tr v-if="documentsStore.documents.length === 0">
                            <td colspan="5" class="px-6 py-12 text-center text-gray-500">
                                No documents found. Upload one above!
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <!-- Mobile Card View -->
            <div class="md:hidden space-y-4">
                 <div v-for="doc in documentsStore.documents" :key="doc.id" class="bg-white p-4 rounded-lg shadow-sm border border-gray-100">
                     <div class="flex justify-between items-start mb-2">
                         <div class="flex items-start gap-2 max-w-[80%]">
                             <DocumentIcon class="h-6 w-6 text-gray-400 flex-shrink-0" />
                             <div>
                                 <h4 class="text-sm font-bold text-gray-900 break-all">{{ doc.fileName }}</h4>
                                 <p class="text-xs text-gray-500">{{ formatDate(doc.creationDate) }}</p>
                             </div>
                         </div>
                         <span class="text-xs font-medium bg-gray-100 text-gray-600 px-2 py-1 rounded">
                             {{ (doc.fileSize / 1024).toFixed(0) }} KB
                         </span>
                     </div>
                     
                     <div class="flex justify-end items-center pt-3 mt-2 border-t border-gray-50 space-x-4">
                         <button @click="handleDownload(doc)" class="text-sm font-semibold text-blue-600 hover:text-blue-800 flex items-center gap-1">
                             <ArrowDownTrayIcon class="h-4 w-4" /> Download
                         </button>
                         <button @click="handleDelete(doc)" class="text-sm font-semibold text-red-600 hover:text-red-800 flex items-center gap-1">
                             <TrashIcon class="h-4 w-4" /> Delete
                         </button>
                     </div>
                 </div>
                 <div v-if="documentsStore.documents.length === 0" class="text-center py-8 text-gray-500">
                     No documents found.
                 </div>
            </div>
        </div>
    </PageLayout>
</template>
