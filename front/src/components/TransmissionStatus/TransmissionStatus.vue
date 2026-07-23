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
              v-for="hour in hours" 
              :key="hour" 
              class="time-header"
              :colspan="interval === 15 ? 4 : 1"
            >
              {{ formatTime(hour) }}
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="[country, loads] in Object.entries(countries)" :key="country">
            <td class="country-name">{{ country }}</td>
            <td
              v-for="(load, index) in loads"
              :key="`${country}-${index}`"
              class="status-cell"
              :class="getStatusClass(load)"
              @mouseenter="showTooltip($event, country, index)"
              @mouseleave="hideTooltip"
            >
            </td>
          </tr>
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
/* eslint-disable */
import { useTransmissionStatus } from './useTransmissionStatus';
  import Flatpickr from 'vue-flatpickr-component';
  import 'flatpickr/dist/flatpickr.css';

const {    
    countries,
    date,
    isDark,
    interval,
    hours,
    tooltip,
    formatTime,
    getStatusClass,
    showTooltip,
    hideTooltip,
    datePickerConfig
    } = useTransmissionStatus();
</script>

<style lang="scss" src="@/styles/DashboardCommon.scss" scoped></style>
<style lang="scss" src="@/styles/TransmissionStatus.scss" scoped></style>