<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { useEventLogsStore } from '../../stores/eventlogs'
import PageLayout from '../PageLayout.vue'
import * as XLSX from 'xlsx'
import { 
    ArrowDownTrayIcon, 
    ClockIcon 
} from '@heroicons/vue/24/outline'

const eventLogsStore = useEventLogsStore()

const filterType = ref('')
const filterDescription = ref('')
const startDate = ref('')
const endDate = ref('')

const filteredLogs = computed(() => {
    return eventLogsStore.logs.filter(log => {
        const matchesType = !filterType.value || log.eventType === filterType.value
        const matchesDescription = !filterDescription.value || log.description.toLowerCase().includes(filterDescription.value.toLowerCase())

        let matchesDate = true
        if (startDate.value) {
            matchesDate = matchesDate && new Date(log.timestamp || log.creationDate) >= new Date(startDate.value)
        }
        if (endDate.value) {
            // Set end date to end of day
            const end = new Date(endDate.value)
            end.setHours(23, 59, 59, 999)
            matchesDate = matchesDate && new Date(log.timestamp || log.creationDate) <= end
        }

        return matchesType && matchesDescription && matchesDate
    })
})

const exportToExcel = () => {
    const data = filteredLogs.value.map(log => ({
        ID: log.id,
        Type: log.eventType,
        Description: log.description,
        Date: new Date(log.timestamp || log.creationDate).toLocaleString()
    }))

    const ws = XLSX.utils.json_to_sheet(data)
    const wb = XLSX.utils.book_new()
    XLSX.utils.book_append_sheet(wb, ws, "Event Logs")
    XLSX.writeFile(wb, "EventLogs.xlsx")
}

onMounted(() => {
    eventLogsStore.fetchLogs()
    eventLogsStore.initializeConnection()
})
</script>

<template>
    <PageLayout title="Event Logs" subtitle="Real-time system events and history.">
        <template #actions>
            <button @click="exportToExcel"
                class="bg-green-600 text-white px-3 py-2 rounded-md hover:bg-green-700 transition flex items-center gap-2 text-sm shadow-sm">
                <ArrowDownTrayIcon class="h-4 w-4" />
                <span>Export Excel</span>
            </button>
        </template>

        <!-- Filters -->
        <div class="bg-white p-4 rounded-lg shadow-sm mb-6 border border-gray-100">
             <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                <div>
                    <label class="block text-xs font-semibold text-gray-500 uppercase tracking-wider mb-1">Type</label>
                    <select v-model="filterType"
                        class="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm border p-2 bg-gray-50">
                        <option value="">All Types</option>
                        <option value="Document Upload">Document Upload</option>
                        <option value="AI Analysis">AI Analysis</option>
                        <option value="User Interaction">User Interaction</option>
                    </select>
                </div>
                <div>
                    <label class="block text-xs font-semibold text-gray-500 uppercase tracking-wider mb-1">Description</label>
                    <input v-model="filterDescription" type="text" placeholder="Search description..."
                        class="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm border p-2 bg-gray-50">
                </div>
                <div>
                    <label class="block text-xs font-semibold text-gray-500 uppercase tracking-wider mb-1">Start Date</label>
                    <input v-model="startDate" type="date"
                        class="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm border p-2 bg-gray-50">
                </div>
                <div>
                    <label class="block text-xs font-semibold text-gray-500 uppercase tracking-wider mb-1">End Date</label>
                    <input v-model="endDate" type="date"
                        class="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm border p-2 bg-gray-50">
                </div>
            </div>
        </div>

        <div v-if="eventLogsStore.loading && eventLogsStore.logs.length === 0" class="flex justify-center py-12">
             <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>

        <div v-else>
            <!-- Desktop Table View -->
            <div class="hidden md:block overflow-hidden border border-gray-200 rounded-lg bg-white shadow-sm">
                <table class="min-w-full divide-y divide-gray-200">
                    <thead class="bg-gray-50">
                        <tr>
                            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">ID</th>
                            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Type</th>
                            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Description</th>
                            <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Date & Time</th>
                        </tr>
                    </thead>
                    <tbody class="bg-white divide-y divide-gray-200">
                        <tr v-for="log in filteredLogs" :key="log.id" class="hover:bg-gray-50 transition-colors">
                            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-400">#{{ log.id }}</td>
                            <td class="px-6 py-4 whitespace-nowrap">
                                <span class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-blue-100 text-blue-800">
                                    {{ log.eventType }}
                                </span>
                            </td>
                            <td class="px-6 py-4 text-sm text-gray-600">{{ log.description }}</td>
                            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                {{ new Date(log.timestamp || log.creationDate).toLocaleString() }}
                            </td>
                        </tr>
                        <tr v-if="filteredLogs.length === 0">
                            <td colspan="4" class="px-6 py-12 text-center text-sm text-gray-500">
                                No logs found matching criteria.
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <!-- Mobile Card View -->
            <div class="md:hidden space-y-4">
                 <div v-for="log in filteredLogs" :key="log.id" class="bg-white p-4 rounded-lg shadow-sm border border-gray-100">
                     <div class="flex justify-between items-start mb-2">
                         <span class="px-2 py-1 text-xs font-semibold rounded-full bg-blue-100 text-blue-800">
                            {{ log.eventType }}
                         </span>
                         <span class="text-xs text-gray-400">#{{ log.id }}</span>
                     </div>
                     <p class="text-gray-800 font-medium text-sm mb-2">{{ log.description }}</p>
                     <p class="text-xs text-gray-500 flex items-center">
                         <ClockIcon class="h-3 w-3 mr-1" />
                         {{ new Date(log.timestamp || log.creationDate).toLocaleString() }}
                     </p>
                 </div>
                 <div v-if="filteredLogs.length === 0" class="text-center py-8 text-gray-500">
                     No logs found.
                 </div>
            </div>
        </div>
    </PageLayout>
</template>
