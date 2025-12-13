<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useUserStore } from '../../stores/users'
import { useAuthStore } from '../../stores/auth'
import PageLayout from '../PageLayout.vue'
import { 
    ArrowPathIcon, 
    PencilSquareIcon, 
    TrashIcon 
} from '@heroicons/vue/24/outline'

const userStore = useUserStore()
const authStore = useAuthStore()
const isLoading = ref(false)
const error = ref('')

const showRoleModal = ref(false)
const selectedUser = ref<any>(null)
const newRole = ref('User')

onMounted(async () => {
    await loadUsers()
})

async function loadUsers() {
    isLoading.value = true
    error.value = ''
    try {
        await userStore.fetchUsers()
    } catch (e) {
        error.value = 'Error loading users.'
    } finally {
        isLoading.value = false
    }
}

function openRoleModal(user: any) {
    selectedUser.value = user
    newRole.value = user.role
    showRoleModal.value = true
}

async function saveRole() {
    if (!selectedUser.value) return
    try {
        await userStore.updateUserRole(selectedUser.value.id, newRole.value)
        showRoleModal.value = false
        selectedUser.value = null
    } catch (e) {
        alert('Failed to update role')
    }
}

async function confirmDelete(user: any) {
    if (confirm(`Are you sure you want to delete user ${user.username}?`)) {
        try {
            await userStore.deleteUser(user.id)
        } catch (e) {
            alert('Failed to delete user')
        }
    }
}
</script>

<template>
    <PageLayout title="User Management" subtitle="Manage system users and their roles.">
        <template #actions>
            <button @click="loadUsers" class="text-blue-600 hover:text-blue-800 text-sm font-medium flex items-center gap-1 transition-colors">
                <ArrowPathIcon class="h-4 w-4" />
                <span>Refresh</span>
            </button>
        </template>

        <div v-if="isLoading" class="flex justify-center py-12">
             <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>
        <div v-else-if="error" class="bg-red-50 text-red-600 p-4 rounded-lg text-center border border-red-100">
            {{ error }}
        </div>

        <div v-else>
            <!-- Desktop Table -->
            <div class="hidden md:block overflow-hidden bg-white shadow-sm rounded-lg border border-gray-200">
                <table class="min-w-full divide-y divide-gray-200">
                    <thead class="bg-gray-50">
                        <tr>
                            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Username</th>
                            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Email</th>
                            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Role</th>
                            <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Status</th>
                            <th class="px-6 py-3 text-right text-xs font-semibold text-gray-500 uppercase tracking-wider">Actions</th>
                        </tr>
                    </thead>
                    <tbody class="bg-white divide-y divide-gray-200">
                        <tr v-for="user in userStore.users" :key="user.id" class="hover:bg-gray-50 transition-colors">
                            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{{ user.username }}</td>
                            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ user.email }}</td>
                            <td class="px-6 py-4 whitespace-nowrap text-sm">
                                <span :class="[
                                    'px-2 inline-flex text-xs leading-5 font-semibold rounded-full',
                                    user.role === 'Admin' ? 'bg-purple-100 text-purple-800' : 'bg-green-100 text-green-800'
                                ]">
                                    {{ user.role }}
                                </span>
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                <span :class="user.isActive ? 'text-green-600' : 'text-gray-400'">
                                    {{ user.isActive ? 'Active' : 'Inactive' }}
                                </span>
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium space-x-3">
                                <button @click="openRoleModal(user)" class="text-indigo-600 hover:text-indigo-900 flex items-center gap-1 inline-flex">
                                    <PencilSquareIcon class="h-4 w-4" /> Edit Role
                                </button>
                                <button @click="confirmDelete(user)" 
                                    class="text-red-600 hover:text-red-900 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-1 inline-flex"
                                    :disabled="user.username === authStore.user?.username">
                                    <TrashIcon class="h-4 w-4" /> Delete
                                </button>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <!-- Mobile Card View -->
            <div class="md:hidden space-y-4">
                <div v-for="user in userStore.users" :key="user.id" class="bg-white p-4 rounded-lg shadow-sm border border-gray-100">
                    <div class="flex justify-between items-start mb-3">
                        <div>
                             <h4 class="text-sm font-bold text-gray-900">{{ user.username }}</h4>
                             <p class="text-xs text-gray-500">{{ user.email }}</p>
                        </div>
                        <span :class="[
                            'px-2 py-1 text-xs font-semibold rounded-full',
                            user.role === 'Admin' ? 'bg-purple-100 text-purple-800' : 'bg-green-100 text-green-800'
                        ]">
                            {{ user.role }}
                        </span>
                    </div>
                    
                    <div class="flex justify-between items-center pt-3 border-t border-gray-50">
                         <span :class="['text-xs font-medium', user.isActive ? 'text-green-600' : 'text-gray-400']">
                             {{ user.isActive ? '● Active' : '○ Inactive' }}
                         </span>
                         <div class="flex space-x-3">
                             <button @click="openRoleModal(user)" class="text-xs font-medium text-indigo-600 flex items-center gap-1">
                                <PencilSquareIcon class="h-4 w-4" /> Edit
                             </button>
                             <button 
                                @click="confirmDelete(user)" 
                                class="text-xs font-medium text-red-600 disabled:opacity-50 flex items-center gap-1"
                                :disabled="user.username === authStore.user?.username">
                                <TrashIcon class="h-4 w-4" /> Delete
                             </button>
                         </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Role Modal -->
        <div v-if="showRoleModal"
            class="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50 flex items-center justify-center p-4">
            <div class="bg-white rounded-lg shadow-xl w-full max-w-sm transform transition-all">
                <div class="px-6 py-4 border-b border-gray-100">
                     <h3 class="text-lg font-bold text-gray-900">Change Role</h3>
                     <p class="text-sm text-gray-500">For {{ selectedUser?.username }}</p>
                </div>
                
                <div class="p-6">
                    <label class="block text-gray-700 text-sm font-bold mb-2">Role</label>
                    <select v-model="newRole"
                        class="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm border p-2">
                        <option value="User">User</option>
                        <option value="Admin">Admin</option>
                    </select>
                </div>
                
                <div class="px-6 py-4 bg-gray-50 rounded-b-lg flex justify-end space-x-3">
                    <button @click="showRoleModal = false"
                        class="px-4 py-2 bg-white text-gray-700 border border-gray-300 rounded-md text-sm font-medium hover:bg-gray-50">
                        Cancel
                    </button>
                    <button @click="saveRole"
                        class="px-4 py-2 bg-blue-600 text-white rounded-md text-sm font-medium hover:bg-blue-700 shadow-sm">
                        Save Changes
                    </button>
                </div>
            </div>
        </div>
    </PageLayout>
</template>
