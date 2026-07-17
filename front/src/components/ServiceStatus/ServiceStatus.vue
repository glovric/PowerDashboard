<template>
  <div class="svc-status-container">
    <div class="svc-status">

      <div class="metrics-grid">
        
        <div class="metric-card">
          <div class="card-icon">💾</div>
          <div class="card-content">
            <h3>Data Service</h3>
            <div class="live-indicator" :class="getStatusClass(serviceStatus.power)">
              <span class="pulse-dot"></span>
              {{ getStatusLabel(serviceStatus.power) }}
            </div>
          </div>
        </div>

        <div class="metric-card">
          <div class="card-icon">🤖</div>
          <div class="card-content">
            <h3>Inference Service</h3>
            <div class="live-indicator" :class="getStatusClass(serviceStatus.inference)">
              <span class="pulse-dot"></span>
              {{ getStatusLabel(serviceStatus.inference) }}
            </div>
          </div>
        </div>

        <div class="metric-card">
          <div class="card-icon">🔐</div>
          <div class="card-content">
            <h3>Auth Service</h3>
            <div class="live-indicator" :class="getStatusClass(serviceStatus.auth)">
              <span class="pulse-dot"></span>
              {{ getStatusLabel(serviceStatus.auth) }}
            </div>
          </div>
        </div>

      </div>

      <div class="action-area">
        <button @click="refreshData" class="btn-refresh" :disabled="isLoading">
          {{ isLoading ? 'Checking...' : '↻ Refresh' }}
        </button>
        <span v-if="lastChecked" class="last-check-time">
          Updated: {{ lastChecked }}
        </span>
      </div>

    </div>
  </div>
</template>

<script setup>
import { useServiceStatus } from "./useServiceStatus"

const { isLoading, lastChecked, refreshData, getStatusClass, getStatusLabel, serviceStatus } = useServiceStatus();
</script>

<style lang="scss" src="@/styles/ServiceStatus.scss" scoped></style>