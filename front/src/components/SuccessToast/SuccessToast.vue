<template>
  <transition name="toast">
    <div v-if="successVisible" class="success-message">
      <p>{{ successMessage }}</p>
    </div>
  </transition>
</template>

<script setup>
  import { useSuccessToast } from "@/components/SuccessToast/useSuccessToast.js";
  const { successVisible, successMessage } = useSuccessToast();
</script>
<style lang="scss" src="@/styles/SuccessToast.scss" scoped></style>