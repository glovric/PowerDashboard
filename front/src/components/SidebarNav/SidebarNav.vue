<template>
  <aside
    class="sidebar"
    :class="{ collapsed: sidebarNavCollapsed }"
  >
    <button class="toggle-btn" @click="toggleSidebarNav">
      ☰
    </button>

    <nav class="nav">
      <template v-if="isLoggedIn">
        <router-link to="/map" class="nav-link">
          🗺️ <span class="label">Map</span>
        </router-link>

        <router-link to="/current" class="nav-link">
          ⚡ <span class="label">Latest</span>
        </router-link>

        <router-link to="/history" class="nav-link">
          🕒 <span class="label">Historical</span>
        </router-link>

        <router-link to="/forecast" class="nav-link">
          🔮 <span class="label">Forecast</span>
        </router-link>

        <router-link to="/status" class="nav-link">
          🗄️ <span class="label">Database Status</span>
        </router-link>

        <router-link to="/service_status" class="nav-link">
          ⚙️ <span class="label">Services Status</span>
        </router-link>
      </template>

      <template v-if="!isLoggedIn">
        <router-link to="/login" class="nav-link">
          🔐 <span class="label">Login</span>
        </router-link>

        <router-link to="/register" class="nav-link">
          📝 <span class="label">Register</span>
        </router-link>
      </template>

    </nav>

    <div class="sidebar-footer">
      
      <template v-if="isLoggedIn">
        <router-link to="/profile" class="nav-link" aria-label="User profile">
          👤 <span class="label">Profile</span>
        </router-link>

        <button class="nav-link" @click="handleLogout" aria-label="Log out">
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" 
              fill="none" stroke="currentColor" stroke-width="2" 
              stroke-linecap="round" stroke-linejoin="round">
            <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
            <polyline points="16 17 21 12 16 7"/>
            <line x1="21" y1="12" x2="9" y2="12"/>
          </svg>
          <span class="label">Log out</span>
        </button>

      </template>

      <label class="toggle-slider" :class="{ collapsed: sidebarNavCollapsed }">
        <input type="checkbox" v-model="isDark" />
        <span class="slider"></span>
        <span class="label-text">{{ isDark ? "Dark" : "Light" }}</span>
      </label>

    </div>
  </aside>
</template>

<script setup>
  import { useSidebarNav } from './useSidebarNav.js';
  import { useTheme } from '@/composables/useTheme.js';
  import { useAuth } from '@/composables/useAuth.js';
  const { sidebarNavCollapsed, toggleSidebarNav, handleLogout } = useSidebarNav();
  const { isDark } = useTheme();
  const { isLoggedIn } = useAuth();
</script>
<style lang="scss" src="@/styles/SidebarNav.scss" scoped></style>
