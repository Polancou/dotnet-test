<script setup lang="ts">
import { ref } from 'vue'
import axios from 'axios'
import { useAuthStore } from '../../stores/auth'

const authStore = useAuthStore()
const fileInput = ref<HTMLInputElement | null>(null)
const analysisResult = ref<AnalysisResult | null>(null)
const loading = ref(false)

interface Product {
    name: string
    quantity: number
    unitPrice: number
    total: number
}

interface InvoiceData {
    clientName: string
    clientAddress: string
    providerName: string
    providerAddress: string
    invoiceNumber: string
    date: string
    total: number
    products: Product[]
}

interface InformationData {
    description: string
    summary: string
    sentiment: string
}

interface AnalysisResult {
    documentType: string
    invoiceData?: InvoiceData
    informationData?: InformationData
}

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
            // The backend returns { documentId: ..., analysis: ... }
            analysisResult.value = response.data.analysis
        } catch (e) {
            alert('Analysis failed')
            console.error(e)
        } finally {
            loading.value = false
            if (fileInput.value) fileInput.value.value = ''
        }
    }
}
</script>

<template>
    <div class="p-6 bg-white rounded-lg shadow-md h-full overflow-y-auto">
        <h3 class="text-xl font-bold mb-4">AI Analysis</h3>
        <p class="mb-4 text-gray-600">Upload a document (PDF, JPG, PNG) to analyze its content type.</p>

        <div class="flex items-center gap-4 mb-6">
            <button @click="triggerUpload"
                class="bg-purple-500 text-white px-4 py-2 rounded hover:bg-purple-600 transition" :disabled="loading">
                {{ loading ? 'Analyzing...' : 'Upload & Analyze' }}
            </button>
            <input type="file" ref="fileInput" accept=".pdf,.jpg,.jpeg,.png" class="hidden"
                @change="handleFileUpload" />
        </div>

        <div v-if="analysisResult" class="mt-6 space-y-6">
            <div class="p-4 bg-purple-50 rounded-lg border border-purple-100">
                <span class="text-sm text-purple-600 font-semibold uppercase tracking-wider">Document Type</span>
                <h4 class="text-2xl font-bold text-purple-900">{{ analysisResult.documentType }}</h4>
            </div>

            <!-- Invoice View -->
            <div v-if="analysisResult.documentType === 'Invoice' && analysisResult.invoiceData" class="space-y-6">
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div class="p-4 border rounded-lg bg-gray-50">
                        <h5 class="font-bold text-gray-700 mb-2">Provider</h5>
                        <p class="text-lg font-semibold">{{ analysisResult.invoiceData.providerName }}</p>
                        <p class="text-gray-600">{{ analysisResult.invoiceData.providerAddress }}</p>
                    </div>
                    <div class="p-4 border rounded-lg bg-gray-50">
                        <h5 class="font-bold text-gray-700 mb-2">Client</h5>
                        <p class="text-lg font-semibold">{{ analysisResult.invoiceData.clientName }}</p>
                        <p class="text-gray-600">{{ analysisResult.invoiceData.clientAddress }}</p>
                    </div>
                </div>

                <div class="flex justify-between items-center p-4 bg-gray-100 rounded-lg">
                    <div>
                        <span class="block text-sm text-gray-500">Invoice Number</span>
                        <span class="font-mono font-bold">{{ analysisResult.invoiceData.invoiceNumber }}</span>
                    </div>
                    <div>
                        <span class="block text-sm text-gray-500">Date</span>
                        <span class="font-bold">{{ new Date(analysisResult.invoiceData.date).toLocaleDateString()
                            }}</span>
                    </div>
                    <div class="text-right">
                        <span class="block text-sm text-gray-500">Total</span>
                        <span class="text-2xl font-bold text-green-600">${{ analysisResult.invoiceData.total.toFixed(2)
                            }}</span>
                    </div>
                </div>

                <div class="border rounded-lg overflow-hidden">
                    <table class="min-w-full divide-y divide-gray-200">
                        <thead class="bg-gray-50">
                            <tr>
                                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Product</th>
                                <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Qty</th>
                                <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Unit Price
                                </th>
                                <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Total</th>
                            </tr>
                        </thead>
                        <tbody class="bg-white divide-y divide-gray-200">
                            <tr v-for="(product, index) in analysisResult.invoiceData.products" :key="index">
                                <td class="px-6 py-4 text-sm text-gray-900">{{ product.name }}</td>
                                <td class="px-6 py-4 text-sm text-gray-500 text-right">{{ product.quantity }}</td>
                                <td class="px-6 py-4 text-sm text-gray-500 text-right">${{ product.unitPrice.toFixed(2)
                                    }}</td>
                                <td class="px-6 py-4 text-sm font-medium text-gray-900 text-right">${{
                                    product.total.toFixed(2) }}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>

            <!-- Information View -->
            <div v-if="analysisResult.documentType === 'Information' && analysisResult.informationData"
                class="space-y-4">
                <div class="p-4 border rounded-lg bg-white">
                    <h5 class="font-bold text-gray-700 mb-2">Description</h5>
                    <p class="text-gray-600">{{ analysisResult.informationData.description }}</p>
                </div>

                <div class="p-4 border rounded-lg bg-white">
                    <h5 class="font-bold text-gray-700 mb-2">Summary</h5>
                    <p class="text-gray-800 leading-relaxed">{{ analysisResult.informationData.summary }}</p>
                </div>

                <div class="flex items-center gap-2">
                    <span class="font-bold text-gray-700">Sentiment Analysis:</span>
                    <span class="px-3 py-1 rounded-full text-sm font-bold" :class="{
                        'bg-green-100 text-green-800': analysisResult.informationData.sentiment === 'Positive',
                        'bg-red-100 text-red-800': analysisResult.informationData.sentiment === 'Negative',
                        'bg-gray-100 text-gray-800': analysisResult.informationData.sentiment === 'Neutral'
                    }">
                        {{ analysisResult.informationData.sentiment }}
                    </span>
                </div>
            </div>
        </div>
    </div>
</template>
