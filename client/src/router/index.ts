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

router.beforeEach((to, _from, next) => {
    const publicPages = ['/login', '/register']
    const authRequired = !publicPages.includes(to.path)

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
