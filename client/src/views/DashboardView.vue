<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useDocumentsStore } from '../stores/documents'
import { useEventLogsStore } from '../stores/eventlogs'
import { useAuthStore } from '../stores/auth'

const documentsStore = useDocumentsStore()
const eventLogsStore = useEventLogsStore()
const authStore = useAuthStore()
const router = useRouter()

const fileInput = ref<HTMLInputElement | null>(null)
const isBulk = ref(false)

onMounted(() => {
  documentsStore.fetchDocuments()
  eventLogsStore.fetchLogs()
})

function triggerUpload() {
  fileInput.value?.click()
}

async function handleFileUpload(event: Event) {
  const target = event.target as HTMLInputElement
  if (target.files && target.files.length > 0) {
    try {
      const type = isBulk.value ? 'UserBulk' : undefined
      await documentsStore.uploadDocument(target.files[0], type)
      alert('File uploaded successfully')
      isBulk.value = false // Reset
    } catch (e) {
      alert('Upload failed')
    }
  }
}

function handleLogout() {
  authStore.logout()
  router.push('/login')
}
</script>

<template>
  <div class="mt-10">
    <div class="flex justify-between items-center mb-6">
      <h2 class="text-2xl font-bold">Dashboard</h2>
      <div class="text-gray-600">
        Welcome, <span class="font-semibold">{{ authStore.user?.username }}</span>
        <span v-if="authStore.user?.role" class="text-sm bg-blue-100 text-blue-800 py-1 px-2 rounded ml-2">{{
          authStore.user.role }}</span>
        <button @click="handleLogout" class="ml-4 text-sm text-red-600 hover:text-red-800 underline">Logout</button>
      </div>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
      <!-- Documents Section -->
      <div class="p-6 bg-white rounded-lg shadow-md">
        <div class="flex justify-between items-center mb-4">
          <h3 class="text-xl font-bold">My Documents</h3>
          <div class="flex items-center gap-2">
            <label v-if="authStore.user?.role === 'Admin'" class="flex items-center text-sm text-gray-600">
              <input type="checkbox" v-model="isBulk" class="mr-1"> Bulk Users
            </label>
            <button @click="triggerUpload" class="bg-green-500 text-white px-3 py-1 rounded hover:bg-green-600 text-sm">
              Upload
            </button>
          </div>
          <input type="file" ref="fileInput" class="hidden" @change="handleFileUpload" />
        </div>

        <div v-if="documentsStore.loading" class="text-gray-500">Loading documents...</div>
        <ul v-else-if="documentsStore.documents.length > 0" class="divide-y divide-gray-200">
          <li v-for="doc in documentsStore.documents" :key="doc.id" class="py-2">
            <div class="flex justify-between">
              <span class="font-medium">{{ doc.fileName }}</span>
              <span class="text-sm text-gray-500">{{ new Date(doc.uploadDate).toLocaleDateString() }}</span>
            </div>
          </li>
        </ul>
        <p v-else class="text-gray-500">No documents found.</p>
      </div>

      <!-- Event Logs Section -->
      <div class="p-6 bg-white rounded-lg shadow-md">
        <h3 class="text-xl font-bold mb-4">Event Log</h3>

        <div v-if="eventLogsStore.loading" class="text-gray-500">Loading logs...</div>
        <div v-else-if="eventLogsStore.logs.length > 0" class="overflow-y-auto max-h-64">
          <ul class="divide-y divide-gray-200">
            <li v-for="log in eventLogsStore.logs" :key="log.id" class="py-2 text-sm">
              <div class="flex justify-between">
                <span class="font-medium">{{ log.eventType }}</span>
                <span class="text-gray-500">{{ new Date(log.timestamp).toLocaleString() }}</span>
              </div>
              <p class="text-gray-600 truncate">{{ log.details }}</p>
            </li>
          </ul>
        </div>
        <p v-else class="text-gray-500">No logs found.</p>
      </div>
    </div>
  </div>
</template>
