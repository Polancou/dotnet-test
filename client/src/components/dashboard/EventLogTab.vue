<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { useEventLogsStore } from '../../stores/eventlogs'
import * as XLSX from 'xlsx'

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
    <div class="p-6 bg-white rounded-lg shadow-md h-full flex flex-col">
        <div class="flex justify-between items-center mb-4">
            <div>
                <h3 class="text-xl font-bold">Event Log</h3>
                <p class="text-gray-600">Real-time system events and history.</p>
            </div>
            <button @click="exportToExcel"
                class="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700 transition">
                Export to Excel
            </button>
        </div>

        <!-- Filters -->
        <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-4">
            <div>
                <label class="block text-sm font-medium text-gray-700">Type</label>
                <select v-model="filterType"
                    class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm border p-2">
                    <option value="">All Types</option>
                    <option value="Document Upload">Document Upload</option>
                    <option value="AI Analysis">AI Analysis</option>
                    <option value="User Interaction">User Interaction</option>
                </select>
            </div>
            <div>
                <label class="block text-sm font-medium text-gray-700">Description</label>
                <input v-model="filterDescription" type="text" placeholder="Search description..."
                    class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm border p-2">
            </div>
            <div>
                <label class="block text-sm font-medium text-gray-700">Start Date</label>
                <input v-model="startDate" type="date"
                    class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm border p-2">
            </div>
            <div>
                <label class="block text-sm font-medium text-gray-700">End Date</label>
                <input v-model="endDate" type="date"
                    class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm border p-2">
            </div>
        </div>

        <div v-if="eventLogsStore.loading && eventLogsStore.logs.length === 0" class="text-gray-500">Loading logs...
        </div>

        <div v-else class="flex-1 overflow-auto border border-gray-200 rounded bg-gray-50">
            <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50 sticky top-0">
                    <tr>
                        <th scope="col"
                            class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">ID
                        </th>
                        <th scope="col"
                            class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Type
                        </th>
                        <th scope="col"
                            class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                            Description</th>
                        <th scope="col"
                            class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Date
                            & Time</th>
                    </tr>
                </thead>
                <tbody class="bg-white divide-y divide-gray-200">
                    <tr v-for="log in filteredLogs" :key="log.id">
                        <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ log.id }}</td>
                        <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{{ log.eventType }}
                        </td>
                        <td class="px-6 py-4 text-sm text-gray-500">{{ log.description }}</td>
                        <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ new Date(log.timestamp ||
                            log.creationDate).toLocaleString() }}</td>
                    </tr>
                    <tr v-if="filteredLogs.length === 0">
                        <td colspan="4" class="px-6 py-4 text-center text-sm text-gray-500">No logs found matching
                            criteria.</td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
</template>
