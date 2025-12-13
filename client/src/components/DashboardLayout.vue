<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useEventLogsStore } from '../stores/eventlogs'
import CsvValidationTab from './dashboard/CsvValidationTab.vue'
import UserManagementTab from './dashboard/UserManagementTab.vue'
import DocumentsTab from './dashboard/DocumentsTab.vue'
import AiAnalysisTab from './dashboard/AiAnalysisTab.vue'
import EventLogModal from './dashboard/EventLogModal.vue'
import { 
    FolderIcon, 
    ChartBarIcon, 
    TableCellsIcon, 
    UsersIcon, 
    Bars3Icon,
    XMarkIcon,
    ArrowRightStartOnRectangleIcon,
    ClipboardDocumentListIcon
} from '@heroicons/vue/24/outline'

const authStore = useAuthStore()
const eventLogsStore = useEventLogsStore()
const router = useRouter()

// Initialize real-time connection
onMounted(() => {
    eventLogsStore.initializeConnection()
})
const activeTabId = ref('docs')
const isMobileMenuOpen = ref(false)
const showEventLogModal = ref(false)

const allTabs = [
    { id: 'docs', label: 'My Documents', icon: FolderIcon, component: DocumentsTab, roles: ['Admin', 'User'] },
    { id: 'analysis', label: 'AI Analysis', icon: ChartBarIcon, component: AiAnalysisTab, roles: ['Admin', 'User'] },
    { id: 'csv', label: 'CSV Validation', icon: TableCellsIcon, component: CsvValidationTab, roles: ['Admin'] },
    { id: 'users', label: 'User Management', icon: UsersIcon, component: UserManagementTab, roles: ['Admin'] }
]

const availableTabs = computed(() => {
    const role = authStore.user?.role || ''
    return allTabs.filter(tab => tab.roles.includes(role))
})

const currentTabComponent = computed(() => {
    return allTabs.find(t => t.id === activeTabId.value)?.component || DocumentsTab
})

function setActiveTab(id: string) {
    activeTabId.value = id
    isMobileMenuOpen.value = false
}

function toggleMobileMenu() {
    isMobileMenuOpen.value = !isMobileMenuOpen.value
}

function handleLogout() {
    authStore.logout()
    router.push('/login')
}
</script>

<template>
    <div class="flex h-screen bg-gray-100 relative">
        <!-- Desktop Sidebar -->
        <aside class="hidden md:flex flex-col w-64 bg-white border-r border-gray-200">
            <div class="p-4 border-b border-gray-100">
                 <h2 class="text-xs font-semibold text-gray-400 uppercase tracking-wider">Menu</h2>
            </div>
            <nav class="flex-1 overflow-y-auto py-4">
                <button 
                    v-for="tab in availableTabs" 
                    :key="tab.id"
                    @click="setActiveTab(tab.id)"
                    :class="[
                        'w-full flex items-center px-6 py-3 text-sm font-medium transition-colors border-l-4 group',
                        activeTabId === tab.id 
                            ? 'border-blue-500 text-blue-600 bg-blue-50' 
                            : 'border-transparent text-gray-600 hover:bg-gray-50 hover:text-gray-900'
                    ]"
                >
                    <component :is="tab.icon" 
                        :class="[
                            'mr-3 h-5 w-5',
                            activeTabId === tab.id ? 'text-blue-500' : 'text-gray-400 group-hover:text-gray-500'
                        ]" 
                    />
                    {{ tab.label }}
                </button>
            </nav>
            <div class="p-4 border-t border-gray-100">
                <div class="flex items-center justify-between">
                    <div class="flex items-center overflow-hidden">
                        <div class="h-8 w-8 rounded-full bg-blue-100 flex-shrink-0 flex items-center justify-center text-blue-600 font-bold">
                            {{ authStore.user?.username.charAt(0).toUpperCase() }}
                        </div>
                        <div class="ml-3 min-w-0">
                            <p class="text-sm font-medium text-gray-700 truncate">{{ authStore.user?.username }}</p>
                            <p class="text-xs text-gray-500 truncate">{{ authStore.user?.role }}</p>
                        </div>
                    </div>
                    <button @click="handleLogout" class="text-gray-400 hover:text-red-600 transition-colors p-1" title="Logout">
                        <ArrowRightStartOnRectangleIcon class="h-5 w-5" />
                    </button>
                </div>
            </div>
        </aside>

        <!-- Mobile Header & Menu -->
        <div class="md:hidden w-full absolute z-20" v-if="isMobileMenuOpen">
            <div class="fixed inset-0 bg-gray-600 bg-opacity-75" @click="toggleMobileMenu"></div>
            <div class="relative bg-white shadow-xl max-w-xs w-full h-full flex flex-col">
                <div class="px-4 py-6 border-b flex justify-between items-center">
                    <h2 class="text-lg font-bold">Menu</h2>
                    <button @click="toggleMobileMenu" class="text-gray-500 hover:text-gray-700">
                        <XMarkIcon class="h-6 w-6" />
                    </button>
                </div>
                <nav class="flex-1 overflow-y-auto py-4">
                     <button 
                        v-for="tab in availableTabs" 
                        :key="tab.id"
                        @click="setActiveTab(tab.id)"
                        :class="[
                            'w-full flex items-center px-6 py-4 text-base font-medium border-l-4',
                            activeTabId === tab.id 
                                ? 'border-blue-500 text-blue-600 bg-blue-50' 
                                : 'border-transparent text-gray-600 hover:bg-gray-50'
                        ]"
                    >
                        <component :is="tab.icon" 
                            :class="[
                                'mr-3 h-6 w-6',
                                activeTabId === tab.id ? 'text-blue-500' : 'text-gray-400'
                            ]" 
                        />
                        <span class="mr-3">{{ tab.label }}</span>
                    </button>
                     <div class="border-t border-gray-100 mt-2 pt-2">
                        <button @click="handleLogout" class="w-full flex items-center px-6 py-4 text-base font-medium border-l-4 border-transparent text-red-600 hover:bg-red-50">
                            <ArrowRightStartOnRectangleIcon class="mr-3 h-6 w-6" />
                            Logout
                        </button>
                    </div>
                </nav>
            </div>
        </div>

        <!-- Mobile Toggle Button --> 
        <div class="md:hidden absolute top-16 left-0 right-0 z-10 bg-white border-b px-4 py-3 flex items-center justify-between shadow-sm">
             <div class="flex items-center">
                 <component :is="availableTabs.find(t => t.id === activeTabId)?.icon" class="h-5 w-5 text-blue-500 mr-2" />
                 <span class="font-semibold text-gray-700">{{ availableTabs.find(t => t.id === activeTabId)?.label }}</span>
             </div>
             <button @click="toggleMobileMenu" class="text-gray-600 p-1 border rounded hover:bg-gray-50">
                 <Bars3Icon class="h-6 w-6" />
             </button>
        </div>

        <!-- Content Area -->
        <main class="flex-1 flex flex-col min-w-0 overflow-hidden md:static relative pt-14 md:pt-0">
            <div class="flex-1 overflow-y-auto p-4 md:p-8">
                <component :is="currentTabComponent" />
            </div>
        </main>

        <!-- Floating Action Button for Event Logs -->
        <button 
            v-if="authStore.user?.role === 'Admin'"
            @click="showEventLogModal = !showEventLogModal"
            class="fixed bottom-8 right-8 bg-blue-600 hover:bg-blue-700 text-white rounded-full p-4 shadow-lg transition-transform hover:scale-110 z-30 flex items-center justify-center"
            title="Toggle Event Logs"
        >
            <ClipboardDocumentListIcon class="h-6 w-6" />
        </button>

        <!-- Draggable Event Log Modal -->
        <EventLogModal 
            :is-open="showEventLogModal" 
            @close="showEventLogModal = false"
        />
    </div>
</template>
