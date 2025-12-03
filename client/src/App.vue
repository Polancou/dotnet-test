<script setup lang="ts">
import { RouterLink, RouterView, useRouter } from 'vue-router'
import { useAuthStore } from './stores/auth'

const authStore = useAuthStore()
const router = useRouter()

function handleLogout() {
  authStore.logout()
  router.push('/login')
}
</script>

<template>
  <header class="bg-blue-600 text-white p-4">
    <div class="container mx-auto flex justify-between items-center">
      <h1 class="text-xl font-bold">Clean Architecture App</h1>
      <nav>
        <template v-if="authStore.token">
          <RouterLink to="/" class="mr-4 hover:underline">Home</RouterLink>
          <RouterLink to="/dashboard" class="mr-4 hover:underline">Dashboard</RouterLink>
          <button @click="handleLogout" class="hover:underline">Logout</button>
        </template>
        <template v-else>
          <RouterLink to="/login" class="mr-4 hover:underline">Login</RouterLink>
          <RouterLink to="/register" class="hover:underline">Register</RouterLink>
        </template>
      </nav>
    </div>
  </header>

  <main class="container mx-auto p-4">
    <RouterView />
  </main>
</template>
