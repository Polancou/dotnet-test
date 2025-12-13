<script setup lang="ts">
import { ref } from 'vue'

const props = defineProps({
    accept: {
        type: String,
        default: '*'
    },
    multiple: {
        type: Boolean,
        default: false
    },
    loading: {
        type: Boolean,
        default: false
    },
    label: {
        type: String,
        default: 'Drag and drop files here, or click to select'
    }
})

const emit = defineEmits(['files-selected'])

const fileInput = ref<HTMLInputElement | null>(null)
const isDragging = ref(false)

const triggerInput = () => {
    if (!props.loading) {
        fileInput.value?.click()
    }
}

const handleDragOver = (e: DragEvent) => {
    e.preventDefault()
    if (!props.loading) {
        isDragging.value = true
    }
}

const handleDragLeave = (e: DragEvent) => {
    e.preventDefault()
    isDragging.value = false
}

const handleDrop = (e: DragEvent) => {
    e.preventDefault()
    isDragging.value = false
    
    if (props.loading) return

    if (e.dataTransfer?.files && e.dataTransfer.files.length > 0) {
        emit('files-selected', e.dataTransfer.files)
    }
}

const handleInputChange = (e: Event) => {
    const target = e.target as HTMLInputElement
    if (target.files && target.files.length > 0) {
        emit('files-selected', target.files)
    }
    // Reset input so the same file can be selected again if needed
    if (fileInput.value) fileInput.value.value = ''
}
</script>

<template>
    <div 
        @click="triggerInput"
        @dragover="handleDragOver"
        @dragleave="handleDragLeave"
        @drop="handleDrop"
        :class="[
            'border-2 border-dashed rounded-lg p-8 text-center cursor-pointer transition-colors duration-200',
            isDragging ? 'border-blue-500 bg-blue-50' : 'border-gray-300 hover:border-gray-400 hover:bg-gray-50',
            loading ? 'opacity-50 cursor-not-allowed' : ''
        ]"
    >
        <input 
            type="file" 
            ref="fileInput" 
            :accept="accept" 
            :multiple="multiple" 
            class="hidden" 
            @change="handleInputChange"
        />
        
        <div class="space-y-2 pointer-events-none">
            <svg xmlns="http://www.w3.org/2000/svg" class="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
            </svg>
            <p class="text-gray-600 font-medium">{{ loading ? 'Processing...' : label }}</p>
            <p class="text-xs text-gray-500">
                Supported files: {{ accept === '*' ? 'All files' : accept.replace(/\./g, ' ').toUpperCase() }}
            </p>
        </div>
    </div>
</template>
