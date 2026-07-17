import { createRouter, createWebHistory } from "vue-router"
import { useAuth } from '@/composables/useAuth'

import PowerDashboard from "@/components/PowerDashboard/PowerDashboard.vue"
import EuroMap from "@/components/EuroMap/EuroMap.vue"
import HistoricalDashboard from "@/components/HistoricalDashboard/HistoricalDashboard.vue"
import LoginPage from "@/components/LoginPage/LoginPage.vue";
import RegisterPage from "@/components/RegisterPage/RegisterPage.vue"
import ProfilePage from "@/components/ProfilePage.vue"
import ForecastDashboard from "@/components/ForecastDashboard/ForecastDashboard.vue"
import DatabaseStatus from "@/components/DatabaseStatus/DatabaseStatus.vue"
import ServiceStatus from "@/components/ServiceStatus/ServiceStatus.vue"

const routes = [
  {
    path: "/",
    redirect: "/map"
  },
  {
    path: "/current",
    name: "Current",
    component: PowerDashboard,
    meta: { requiresAuth: true }
  },
  {
    path: "/map",
    name: "Map",
    component: EuroMap,
    meta: { requiresAuth: true }
  },
  {
    path: "/history",
    name: "History",
    component: HistoricalDashboard,
    meta: { requiresAuth: true }
  },
  {
    path: "/forecast",
    name: "Forecast",
    component: ForecastDashboard,
    meta: { requiresAuth: true }
  },
  {
    path: "/status",
    name: "Database Status",
    component: DatabaseStatus,
    meta: { requiresAuth: true }
  },
  {
    path: "/service_status",
    name: "Services Status",
    component: ServiceStatus,
    meta: { requiresAuth: true }
  },
  {
    path: "/login",
    name: "Login",
    component: LoginPage
  },
  {
    path: "/register",
    name: "Register",
    component: RegisterPage
  },
  {
    path: "/profile",
    name: "Profile",
    component: ProfilePage,
    meta: { requiresAuth: true }
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes
});

const { isLoggedIn } = useAuth();

router.beforeEach((to, from, next) => {
  if (to.meta.requiresAuth && !isLoggedIn.value) {
    next({
      path: '/login',
      query: { redirect: to.fullPath }
    });
  } else {
    next();
  }
});

export default router;
