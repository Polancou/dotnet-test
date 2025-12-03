import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import LoginView from '../views/LoginView.vue'
import DashboardView from '../views/DashboardView.vue'

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes: [
        {
            path: '/',
            name: 'home',
            component: HomeView
        },
        {
            path: '/login',
            name: 'login',
            component: LoginView
        },
        {
            path: '/register',
            name: 'register',
            component: () => import('../views/RegisterView.vue')
        },
        {
            path: '/dashboard',
            name: 'dashboard',
            component: DashboardView
        }
    ]
})

router.beforeEach((to, from, next) => {
    const publicPages = ['/login', '/register']
    const authRequired = !publicPages.includes(to.path)

    // We can check localStorage directly for the persisted token
    // The key depends on pinia-plugin-persistedstate config, default is store id ('auth')
    // But we set 'token' in state. Let's assume it's stored in 'auth' key as JSON.
    // Or simpler: check if 'token' exists in localStorage if we were using manual storage, 
    // but now it's inside the 'auth' object in localStorage.
    // Let's just check if the user is authenticated via store if possible, 
    // but accessing store outside component setup might be tricky if pinia not installed yet?
    // Pinia is installed in main.ts before router is used? No, app.use(router) is after pinia.
    // So we can use store.

    const token = localStorage.getItem('token') // Wait, I commented out manual setItem.
    // The plugin saves state to localStorage key 'auth' (store id).
    const authState = localStorage.getItem('auth')
    let loggedIn = false
    if (authState) {
        try {
            const parsed = JSON.parse(authState)
            if (parsed.token) loggedIn = true
        } catch (e) { }
    }

    if (authRequired && !loggedIn) {
        next('/login')
    } else {
        next()
    }
})

export default router
