<template>
    <div class="animate-pulse bg-gray-200 rounded" :class="className"></div>
</template>

<script setup lang="ts">
defineProps({
    className: {
        type: String,
        default: 'h-4 w-full'
    }
})
</script>
