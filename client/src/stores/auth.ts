import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useAuthStore = defineStore('auth', () => {
    const user = ref(null)
    const token = ref(localStorage.getItem('token') || '')

    async function login(username: string, password: string) {
        // In real app, call API
        // const response = await axios.post('/api/auth/login', { username, password })
        // token.value = response.data.accessToken
        // localStorage.setItem('token', token.value)

        // Mock login
        if (username === 'admin' && password === 'password') {
            token.value = 'mock-token'
            localStorage.setItem('token', token.value)
        } else {
            throw new Error('Invalid credentials')
        }
    }

    function logout() {
        token.value = ''
        user.value = null
        localStorage.removeItem('token')
    }

    return { user, token, login, logout }
})
