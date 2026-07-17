<template>
  <transition name="toast">
    <div v-if="loadingToastVisible" class="loading-toast-message">
      <p>{{ loadingToastMessage }}</p>
      <div class="loading-spinner"></div>
    </div>
  </transition>
</template>

<script setup>
  import { useLoadingToast } from "@/components/LoadingToast/useLoadingToast.js";
  const { loadingToastMessage, loadingToastVisible } = useLoadingToast();
</script>
<style lang="scss" src="@/styles/LoadingToast.scss" scoped></style>