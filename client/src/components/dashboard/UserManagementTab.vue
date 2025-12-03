<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useUserStore } from '../../stores/users'
import { useAuthStore } from '../../stores/auth'

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
    <div class="p-6">
        <div class="flex justify-between items-center mb-6">
            <h2 class="text-xl font-semibold text-gray-800">User Management</h2>
            <button @click="loadUsers" class="text-blue-600 hover:text-blue-800">Refresh</button>
        </div>

        <div v-if="isLoading" class="text-center py-4">Loading users...</div>
        <div v-else-if="error" class="text-red-600 text-center py-4">{{ error }}</div>

        <div v-else class="overflow-x-auto bg-white shadow rounded-lg">
            <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                    <tr>
                        <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                            Username</th>
                        <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Email
                        </th>
                        <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Role
                        </th>
                        <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                            Status</th>
                        <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                            Actions</th>
                    </tr>
                </thead>
                <tbody class="bg-white divide-y divide-gray-200">
                    <tr v-for="user in userStore.users" :key="user.id">
                        <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{{ user.username }}
                        </td>
                        <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ user.email }}</td>
                        <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            <span
                                :class="user.role === 'Admin' ? 'bg-purple-100 text-purple-800' : 'bg-green-100 text-green-800'"
                                class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full">
                                {{ user.role }}
                            </span>
                        </td>
                        <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                            {{ user.isActive ? 'Active' : 'Inactive' }}
                        </td>
                        <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                            <button @click="openRoleModal(user)" class="text-indigo-600 hover:text-indigo-900 mr-4">Edit
                                Role</button>
                            <button @click="confirmDelete(user)" class="text-red-600 hover:text-red-900"
                                :disabled="user.username === authStore.user?.username">Delete</button>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

        <!-- Role Modal -->
        <div v-if="showRoleModal"
            class="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full flex items-center justify-center">
            <div class="bg-white p-5 rounded-lg shadow-xl w-96">
                <h3 class="text-lg font-bold mb-4">Change Role for {{ selectedUser?.username }}</h3>
                <div class="mb-4">
                    <label class="block text-gray-700 text-sm font-bold mb-2">Role</label>
                    <select v-model="newRole"
                        class="shadow border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline">
                        <option value="User">User</option>
                        <option value="Admin">Admin</option>
                    </select>
                </div>
                <div class="flex justify-end space-x-2">
                    <button @click="showRoleModal = false"
                        class="bg-gray-300 hover:bg-gray-400 text-gray-800 font-bold py-2 px-4 rounded">Cancel</button>
                    <button @click="saveRole"
                        class="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded">Save</button>
                </div>
            </div>
        </div>
    </div>
</template>
