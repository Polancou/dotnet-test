<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import DashboardLayout from '../components/DashboardLayout.vue'

const authStore = useAuthStore()
const router = useRouter()

function handleLogout() {
  authStore.logout()
  router.push('/login')
}
</script>

<template>
  <div class="mt-10 h-screen flex flex-col">
    <div class="flex justify-between items-center mb-6">
      <h2 class="text-2xl font-bold">Dashboard</h2>
      <div class="text-gray-600">
        Welcome, <span class="font-semibold">{{ authStore.user?.username }}</span>
        <span v-if="authStore.user?.role" class="text-sm bg-blue-100 text-blue-800 py-1 px-2 rounded ml-2">{{
          authStore.user.role }}</span>
        <button @click="handleLogout" class="ml-4 text-sm text-red-600 hover:text-red-800 underline">Logout</button>
      </div>
    </div>

    <div class="flex-1">
      <DashboardLayout />
    </div>
  </div>
</template>
