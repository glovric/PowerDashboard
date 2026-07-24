<template>
  <div class="dashboard" :class="{ dark: isDark }">
    <div class="header">
      <h2>📶 Transmission Status</h2>
    </div>

    <div class="controls">

        <div class="controls-left">

            <div class="controls-left-first">
                <div class="control-group">
                    <label>Interval</label>
                    <select v-model="interval" class="dropdown">
                        <option :value="15">15 Minutes</option>
                        <option :value="60">1 Hour</option>
                    </select>
                </div>
            </div>

            <div class="controls-left-second">
                <div class="control-group">
                    <label>Date</label>
                    <Flatpickr
                    v-model="date"
                    :config="datePickerConfig"
                    class="datepicker"
                    placeholder="Select date"
                    />
                </div>
            </div>
        </div>

        <div class="controls-right">
            <div class="control-group">
              <label>Legend</label>
              <div class="legend-items">
                <div class="legend-item">
                    <div class="color-box available"></div>
                    <span>Data Available</span>
                </div>
                <div class="legend-item">
                    <div class="color-box unavailable"></div>
                    <span>Data Unavailable</span>
                </div>
              </div>
            </div>
        </div>

    </div>

    <div class="table-container">
      <table class="status-table">
        <thead>
          <tr>
            <th class="country-header"></th>
            <th 
              v-for="hour in hourColumns"
              :key="hour" 
              class="time-header"
              :colspan="interval === 15 ? 4 : 1"
            >
              {{ formatTime(hour, 60) }}
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading">
            <td :colspan="totalColumns">
              <div class="loading-content">
                <div class="loading-spinner"></div>
                <p>Loading data...</p>
              </div>
            </td>
          </tr>
          <template v-else>
            <tr v-for="[country, loads] in Object.entries(transmissionData)" :key="country">
              <td class="country-name">{{ country }}</td>
              <td
                v-for="(load, index) in loads"
                :key="`${country}-${index}`"
                class="status-cell"
                :class="getStatusClass(load)"
                @mouseenter="showTooltip($event, country, index)"
                @mousemove="moveTooltip"
                @mouseleave="hideTooltip"
              >
              </td>
            </tr>
          </template>
        </tbody>
      </table>
    </div>

    <div 
      v-if="tooltip.visible" 
      class="tooltip"
      :style="{ top: tooltip.y + 'px', left: tooltip.x + 'px' }"
    >
      <div><strong>{{ tooltip.country }}</strong></div>
      <div>Time: {{ tooltip.time }}</div>
      <div v-if="tooltip.load">Load: {{ tooltip.load }} MW</div>
    </div>
  </div>
</template>

<script setup>
  import Flatpickr from 'vue-flatpickr-component';
  import 'flatpickr/dist/flatpickr.css';
  import { useTransmissionStatus } from './useTransmissionStatus';

  const { 
    interval, date, transmissionData, hourColumns, totalColumns,
    isDark, loading, tooltip, datePickerConfig,
    showTooltip, hideTooltip, moveTooltip, 
    formatTime, getStatusClass 
  } = useTransmissionStatus();
</script>

<style lang="scss" src="@/styles/DashboardCommon.scss" scoped></style>
<style lang="scss" src="@/styles/TransmissionStatus.scss" scoped></style>
<style lang="scss" src="@/styles/LoadingOverlay.scss" scoped></style>