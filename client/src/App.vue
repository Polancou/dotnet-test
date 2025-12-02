<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <header class="bg-blue-600 text-white p-4">
    <div class="container mx-auto flex justify-between items-center">
      <h1 class="text-xl font-bold">Clean Architecture App</h1>
      <nav>
        <RouterLink to="/" class="mr-4 hover:underline">Home</RouterLink>
        <RouterLink to="/dashboard" class="mr-4 hover:underline">Dashboard</RouterLink>
        <RouterLink to="/login" class="hover:underline">Login</RouterLink>
      </nav>
    </div>
  </header>

  <main class="container mx-auto p-4">
    <RouterView />
  </main>
</template>
