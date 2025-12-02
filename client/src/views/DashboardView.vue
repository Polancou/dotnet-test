<script setup lang="ts">
</script>

<template>
  <div class="mt-10">
    <h2 class="text-2xl font-bold mb-4">Dashboard</h2>
    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <div class="p-6 bg-white rounded-lg shadow-md">
        <h3 class="text-xl font-bold mb-2">My Documents</h3>
        <p class="text-gray-600">View and manage your documents here.</p>
        <button class="mt-4 bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600">Upload Document</button>
      </div>
      <div class="p-6 bg-white rounded-lg shadow-md">
        <h3 class="text-xl font-bold mb-2">Event Log</h3>
        <p class="text-gray-600">View system events and history.</p>
        <button class="mt-4 bg-gray-500 text-white px-4 py-2 rounded hover:bg-gray-600">View Logs</button>
      </div>
    </div>
  </div>
</template>
