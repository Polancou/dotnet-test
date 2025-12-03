<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '../stores/auth'
import CsvValidationTab from './dashboard/CsvValidationTab.vue'
import AiAnalysisTab from './dashboard/AiAnalysisTab.vue'
import EventLogTab from './dashboard/EventLogTab.vue'
import UserManagementTab from './dashboard/UserManagementTab.vue'

const authStore = useAuthStore()
const activeTab = ref('')

const allTabs = [
    { id: 'csv', label: 'Validación de documentación CSV', component: CsvValidationTab, roles: ['Admin'] },
    { id: 'ai', label: 'Análisis con IA', component: AiAnalysisTab, roles: ['Admin', 'User'] },
    { id: 'logs', label: 'Event Log', component: EventLogTab, roles: ['Admin', 'User'] },
    { id: 'users', label: 'Gestión de Usuarios', component: UserManagementTab, roles: ['Admin'] }
]

const visibleTabs = computed(() => {
    const userRole = authStore.user?.role || 'User'
    return allTabs.filter(tab => tab.roles.includes(userRole))
})

onMounted(() => {
    if (visibleTabs.value.length > 0) {
        activeTab.value = visibleTabs.value[0].id
    }
})
</script>

<template>
    <div class="flex flex-col h-full">
        <div class="border-b border-gray-200 mb-6">
            <nav class="-mb-px flex space-x-8">
                <button v-for="tab in visibleTabs" :key="tab.id" @click="activeTab = tab.id" :class="[
                    activeTab === tab.id
                        ? 'border-blue-500 text-blue-600'
                        : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300',
                    'whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm'
                ]">
                    {{ tab.label }}
                </button>
            </nav>
        </div>

        <div class="flex-1">
            <component :is="visibleTabs.find(t => t.id === activeTab)?.component" />
        </div>
    </div>
</template>
