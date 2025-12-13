<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useRouter } from 'vue-router'
import { Form, Field, ErrorMessage } from 'vee-validate'
import * as yup from 'yup'

const authStore = useAuthStore()
const router = useRouter()

const errorMsg = ref('')

const schema = yup.object({
  username: yup.string().required('Username is required'),
  password: yup.string().required('Password is required')
})

async function handleLogin(values: any) {
  errorMsg.value = ''
  try {
    await authStore.login(values.username, values.password)
    router.push('/dashboard')
  } catch (error: any) {
    errorMsg.value = error.message || 'Login failed'
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8 bg-white p-8 rounded-lg shadow-md">
      <div>
        <h2 class="mt-6 text-center text-3xl font-extrabold text-gray-900">
          Sign in to your account
        </h2>
      </div>
      <div v-if="errorMsg" class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative" role="alert">
        <span class="block sm:inline">{{ errorMsg }}</span>
      </div>
      <Form @submit="handleLogin" :validation-schema="schema" v-slot="{ errors }" class="mt-8 space-y-6">
        <div class="rounded-md shadow-sm -space-y-px">
          <div class="mb-4">
            <label class="block text-gray-700 text-sm font-bold mb-2" for="username">Username</label>
            <Field name="username" type="text"
              class="appearance-none rounded-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-t-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 focus:z-10 sm:text-sm"
              :class="{ 'border-red-500': errors.username }" placeholder="Username" />
            <ErrorMessage name="username" class="text-red-500 text-xs italic" />
          </div>
          <div class="mb-6">
            <label class="block text-gray-700 text-sm font-bold mb-2" for="password">Password</label>
            <Field name="password" type="password"
              class="appearance-none rounded-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-b-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 focus:z-10 sm:text-sm"
              :class="{ 'border-red-500': errors.password }" placeholder="Password" />
            <ErrorMessage name="password" class="text-red-500 text-xs italic" />
          </div>
        </div>

        <div>
          <button type="submit"
            class="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500">
            Sign in
          </button>
        </div>
        
        <div class="text-center mt-4">
          <p class="text-sm text-gray-600">
            Don't have an account? 
            <router-link to="/register" class="font-medium text-blue-600 hover:text-blue-500">
              Register here
            </router-link>
          </p>
        </div>
      </Form>
    </div>
  </div>
</template>
