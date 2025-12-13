<script setup lang="ts">
defineProps({
    title: {
        type: String,
        required: true
    },
    subtitle: {
        type: String,
        default: ''
    }
})
</script>

<template>
    <div class="flex flex-col h-full bg-white rounded-lg shadow-sm overflow-hidden">
        <!-- Header -->
        <div class="px-6 py-4 border-b border-gray-100 flex justify-between items-center bg-white sticky top-0 z-10">
            <div>
                <h2 class="text-xl font-bold text-gray-800">{{ title }}</h2>
                <p v-if="subtitle" class="text-sm text-gray-500 mt-0.5">{{ subtitle }}</p>
            </div>
            <div class="flex items-center gap-3">
                <slot name="actions"></slot>
            </div>
        </div>

        <!-- Content -->
        <div class="flex-1 overflow-y-auto p-6 bg-gray-50/50">
            <slot></slot>
        </div>
    </div>
</template>
