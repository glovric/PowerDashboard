<template>
    <div v-if="loading" class="loading-overlay">
        <div class="loading-spinner"></div>
        <p>Loading dashboard data...</p>
    </div>
</template>

<script setup>
  import { useLoadingOverlay } from '@/components/LoadingOverlay/useLoadingOverlay.js';
  const { loading } = useLoadingOverlay();
</script>
<style lang="scss" src="@/styles/LoadingOverlay.scss" scoped></style>