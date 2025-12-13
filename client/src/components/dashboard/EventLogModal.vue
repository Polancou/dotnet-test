<script setup lang="ts">
import { ref, onUnmounted } from 'vue'
import { XMarkIcon } from '@heroicons/vue/24/outline'
import EventLogTab from './EventLogTab.vue'

defineProps({
    isOpen: {
        type: Boolean,
        required: true
    }
})

const emit = defineEmits(['close'])

const modalRef = ref<HTMLElement | null>(null)
const position = ref({ x: 50, y: 50 })
const isDragging = ref(false)
const dragOffset = ref({ x: 0, y: 0 })

const startDrag = (event: MouseEvent) => {
    if (modalRef.value) {
        isDragging.value = true
        dragOffset.value = {
            x: event.clientX - position.value.x,
            y: event.clientY - position.value.y
        }
        document.addEventListener('mousemove', onDrag)
        document.addEventListener('mouseup', stopDrag)
    }
}

const onDrag = (event: MouseEvent) => {
    if (isDragging.value) {
        position.value = {
            x: event.clientX - dragOffset.value.x,
            y: event.clientY - dragOffset.value.y
        }
    }
}

const stopDrag = () => {
    isDragging.value = false
    document.removeEventListener('mousemove', onDrag)
    document.removeEventListener('mouseup', stopDrag)
}

onUnmounted(() => {
    document.removeEventListener('mousemove', onDrag)
    document.removeEventListener('mouseup', stopDrag)
})
</script>

<template>
    <div v-if="isOpen" 
         ref="modalRef"
         class="fixed z-50 bg-white shadow-2xl rounded-lg border border-gray-200 overflow-hidden flex flex-col w-[800px] h-[600px] max-w-[90vw] max-h-[80vh]"
         :style="{ left: position.x + 'px', top: position.y + 'px' }">
        
        <!-- Header (Draggable) -->
        <div @mousedown="startDrag" class="bg-gray-100 p-4 border-b border-gray-200 cursor-move flex justify-between items-center select-none">
            <h3 class="font-bold text-gray-700">Event Logs</h3>
            <button @click="emit('close')" class="text-gray-500 hover:text-gray-700">
                <XMarkIcon class="h-6 w-6" />
            </button>
        </div>

        <!-- Content -->
        <div class="flex-1 overflow-auto bg-gray-50 p-2 relative">
            <!-- Reuse existing content logic. 
                 Since EventLogTab includes PageLayout, we might want to strip PageLayout or just render it. 
                 Ideally, we'd refactor EventLogTab to separate data/view from layout.
                 For now, we'll wrap it and let CSS handle overflow. 
            -->
            <div class="h-full">
                <EventLogTab />
            </div>
        </div>
    </div>
</template>

<style scoped>
/* Ensure the modal is above other elements */
.fixed {
    position: fixed;
}
</style>
