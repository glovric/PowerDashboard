<template>
  <div class="power-load-status">
    <h2>Power Load Transmission Status</h2>

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
                    <label>Forecast Date & Time</label>
                    <Flatpickr
                    v-model="forecastDate"
                    :config="datePickerConfig"
                    class="datepicker"
                    placeholder="Select forecast date"
                    />
                </div>
            </div>
        </div>

        <div class="controls-right">
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

    <div class="table-container">
      <table class="status-table">
        <thead>
          <tr>
            <th class="country-header">Country</th>
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
          <tr v-for="country in countries" :key="country.value">
            <td class="country-name">{{ country.label }}</td>
            <td 
              v-for="timestamp in timestamps" 
              :key="`${country.value}-${timestamp}`"
              class="status-cell"
              :class="getStatusClass(country, timestamp)"
              @mouseenter="showTooltip($event, country, timestamp)"
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
      <div>Status: {{ tooltip.status }}</div>
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
    interval,
    timestamps,
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