import { defineStore } from 'pinia'
import { ref } from 'vue'
import axios from 'axios'

export const useAuthStore = defineStore('auth', () => {
    const user = ref<any>(null)
    const token = ref('')
    const refreshToken = ref('')

    async function register(username: string, email: string, password: string) {
        try {
            const apiUrl = import.meta.env.VITE_API_URL || '';
            await axios.post(`${apiUrl}/api/auth/register`, { username, email, password });
        } catch (error: any) {
            console.error('Registration failed:', error);
            throw new Error(error.response?.data || 'Registration failed');
        }
    }

    async function login(username: string, password: string) {
        try {
            const apiUrl = import.meta.env.VITE_API_URL || '';
            const response = await axios.post(`${apiUrl}/api/auth/login`, { username, password });

            const { accessToken, refreshToken: newRefreshToken, role } = response.data;
            token.value = accessToken;
            refreshToken.value = newRefreshToken;
            user.value = { username, role };
        } catch (error: any) {
            console.error('Login failed:', error);
            throw new Error(error.response?.data || 'Login failed');
        }
    }

    async function refresh() {
        try {
            const apiUrl = import.meta.env.VITE_API_URL || '';
            const response = await axios.post(`${apiUrl}/api/auth/refresh`, { refreshToken: refreshToken.value });
            const { accessToken, refreshToken: newRefreshToken } = response.data;
            token.value = accessToken;
            refreshToken.value = newRefreshToken;
            return accessToken;
        } catch (error) {
            logout();
            throw error;
        }
    }

    function logout() {
        token.value = ''
        refreshToken.value = ''
        user.value = null
    }

    // Axios interceptor setup
    axios.interceptors.response.use(
        (response) => response,
        async (error) => {
            const originalRequest = error.config;
            if (error.response?.status === 401 && !originalRequest._retry) {
                // Prevent infinite loop: don't retry if the failed request was already a refresh attempt
                if (originalRequest.url?.includes('/auth/refresh')) {
                    return Promise.reject(error);
                }

                originalRequest._retry = true;
                try {
                    const newToken = await refresh();
                    axios.defaults.headers.common['Authorization'] = `Bearer ${newToken}`;
                    originalRequest.headers['Authorization'] = `Bearer ${newToken}`;
                    return axios(originalRequest);
                } catch (refreshError) {
                    return Promise.reject(refreshError);
                }
            }
            return Promise.reject(error);
        }
    );

    return { user, token, refreshToken, login, logout, register, refresh }
}, {
    persist: true
})
