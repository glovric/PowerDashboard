<template>
  <div class="login-container">
    <div class="login-card">
      <div class="login-header">
        <h2>User profile</h2>
      </div>

      <div v-if="user">
        <p>Username: {{ user.username }}</p>
        <p>Email: {{ user.email }}</p>
        <p>Roles: {{ user.roles }}</p>
      </div>

    </div>
  </div>
</template>

<script setup>
import { useAuth } from '@/composables/useAuth';
const { user } = useAuth()
</script>

<style lang="scss" src="@/styles/LoginPage.scss" scoped></style>
