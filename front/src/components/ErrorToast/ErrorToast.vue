<template>
  <transition name="toast">
    <div v-if="errorVisible" class="error-message">
      <p>{{ errorMessage }}</p>
      <p>Details: {{ errorDetail }}</p>
    </div>
  </transition>
</template>

<script setup>
  import { useErrorToast } from "@/components/ErrorToast/useErrorToast.js";
  const { errorVisible, errorMessage, errorDetail } = useErrorToast();
</script>
<style lang="scss" src="@/styles/ErrorToast.scss" scoped></style>