<template>
  <div class="db-status-container">
    <div class="db-status">

      <!-- ROW 1: Status Info + Pie Chart -->
      <div class="top-row">
        
        <!-- Left: Status Header -->
        <div class="status-info-col">
          <header class="status-header">
            <h2>🗄️ Database Status</h2>
            <div class="live-indicator" :class="statusColorClass">
              <span class="pulse-dot"></span>
              {{ statusLabel }}
            </div>
          </header>
          
        </div>

        <!-- Right: Pie Chart -->
        <div class="chart-col">
          <div class="chart-legend-text">
            Storage Distribution
          </div>
          <div class="chart-legend-subtext">
            Total size: {{ sizeDatabaseFormatted }}
          </div>
          <div class="chart-wrapper">
            <canvas ref="pieChartRef"></canvas>
          </div>
        </div>

      </div>

      <!-- ROW 2: Measurement Cards -->
      <div class="metrics-grid">
        
        <div class="metric-card">
          <div class="card-icon">⏳</div>
          <div class="card-content">
            <h3>Hourly Measurements</h3>
            <div class="big-value text-normal">
              {{ totalRecordsHour }}
            </div>
            <p class="sub-text">
              <span v-if="lastUpdateTimeHour" class="tiny-date">
                Last measurement: {{ lastUpdateTimeHour }}
              </span>
            </p>
          </div>
        </div>

        <div class="metric-card">
          <div class="card-icon">📊</div>
          <div class="card-content">
            <h3>Quarterly Measurements</h3>
            <div class="big-value text-normal">
              {{ totalRecordsQuarter }}
            </div>
            <p class="sub-text">
              <span v-if="lastUpdateTimeQuarter" class="tiny-date">
                Last measurement: {{ lastUpdateTimeQuarter }}
              </span>
            </p>
          </div>
        </div>

      </div>

      <!-- Footer Actions -->
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
import { useDatabaseStatus } from "./useDatabaseStatus"

const { statusLabel, statusColorClass,
        totalRecordsHour, totalRecordsQuarter,
        lastUpdateTimeHour, lastUpdateTimeQuarter,
        sizeDatabaseFormatted,
        isLoading, lastChecked, pieChartRef, refreshData } = useDatabaseStatus();
</script>

<style lang="scss" src="@/styles/DatabaseStatus.scss" scoped></style>