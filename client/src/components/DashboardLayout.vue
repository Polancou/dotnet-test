<script setup lang="ts">
import { ref } from 'vue'
import CsvValidationTab from './dashboard/CsvValidationTab.vue'
import AiAnalysisTab from './dashboard/AiAnalysisTab.vue'
import EventLogTab from './dashboard/EventLogTab.vue'

const activeTab = ref('csv')

const tabs = [
    { id: 'csv', label: 'Validación de documentación CSV', component: CsvValidationTab },
    { id: 'ai', label: 'Análisis con IA', component: AiAnalysisTab },
    { id: 'logs', label: 'Event Log', component: EventLogTab }
]
</script>

<template>
    <div class="flex flex-col h-full">
        <div class="border-b border-gray-200 mb-6">
            <nav class="-mb-px flex space-x-8">
                <button v-for="tab in tabs" :key="tab.id" @click="activeTab = tab.id" :class="[
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
            <component :is="tabs.find(t => t.id === activeTab)?.component" />
        </div>
    </div>
</template>
