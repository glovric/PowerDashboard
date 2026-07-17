<template>
  <div id="app">
    <div class="app-layout">
      <SidebarNav/>
      <main class="main-content">
        <router-view />
      </main>
    </div>
    <ErrorToast/>
    <SuccessToast/>
    <LoadingOverlay/>
    <LoadingToast/>
  </div>
</template>

<script>
import SidebarNav from '@/components/SidebarNav/SidebarNav.vue';
import ErrorToast from '@/components/ErrorToast/ErrorToast.vue';
import LoadingOverlay from '@/components/LoadingOverlay/LoadingOverlay.vue';
import LoadingToast from '@/components/LoadingToast/LoadingToast.vue';
import SuccessToast from '@/components/SuccessToast/SuccessToast.vue';

export default {
  name: 'App',
  components: {
    SidebarNav,
    ErrorToast,
    SuccessToast,
    LoadingOverlay,
    LoadingToast
  }
}
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
}

.app-layout {
  display: grid;
  grid-template-columns: auto 1fr;
  height: 100vh;
  overflow: hidden;
}

.main-content {
  overflow-y: auto;
}
</style>
