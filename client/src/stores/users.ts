import { defineStore } from 'pinia'
import { ref } from 'vue'
import axios from 'axios'
import { useAuthStore } from './auth'

export interface User {
    id: number
    username: string
    email: string
    role: string
    isActive: boolean
    creationDate: string
}

export const useUserStore = defineStore('users', () => {
    const users = ref<User[]>([])
    const authStore = useAuthStore()

    async function fetchUsers() {
        try {
            const apiUrl = import.meta.env.VITE_API_URL || '';
            const response = await axios.get(`${apiUrl}/api/users`, {
                headers: { Authorization: `Bearer ${authStore.token}` }
            });
            users.value = response.data;
        } catch (error) {
            console.error('Failed to fetch users:', error);
            throw error;
        }
    }

    async function updateUserRole(userId: number, newRole: string) {
        try {
            const apiUrl = import.meta.env.VITE_API_URL || '';
            await axios.put(`${apiUrl}/api/users/${userId}/role`, { newRole }, {
                headers: { Authorization: `Bearer ${authStore.token}` }
            });
            await fetchUsers(); // Refresh list
        } catch (error) {
            console.error('Failed to update user role:', error);
            throw error;
        }
    }

    async function deleteUser(userId: number) {
        try {
            const apiUrl = import.meta.env.VITE_API_URL || '';
            await axios.delete(`${apiUrl}/api/users/${userId}`, {
                headers: { Authorization: `Bearer ${authStore.token}` }
            });
            await fetchUsers(); // Refresh list
        } catch (error) {
            console.error('Failed to delete user:', error);
            throw error;
        }
    }

    return { users, fetchUsers, updateUserRole, deleteUser }
})
