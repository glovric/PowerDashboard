<template>
  <div class="dashboard" :class="{ dark: isDark }">
    <div class="header">
      <h2>🕒 Power Data - Historical</h2>
    </div>

    <div class="controls">
      <div class="controls-left">
        <div class="control-group">
        <label>Country</label>
        <select v-model="country" class="dropdown">
          <option 
            v-for="c in countries" 
            :key="c.value" 
            :value="c.value"
          >
            {{ c.label }}
          </option>
        </select>
        </div>

        <div class="control-group">
          <label>Interval</label>
          <select v-model="interval" class="dropdown">
            <option :disabled="!checkHas15(country)" :value="15">15 Minutes</option>
            <option :value="60">1 Hour</option>
          </select>
        </div>
      </div>

      <div class="controls-middle">
        <div class="control-group">
          <label>Start Date & Time</label>
          <Flatpickr
            v-model="startDate"
            :config="datePickerConfig"
            class="datepicker"
            placeholder="Select start date"
          />
        </div>
        <div class="control-group">
          <label>End Date & Time</label>
          <Flatpickr
            v-model="endDate"
            :config="datePickerConfig"
            class="datepicker"
            placeholder="Select end date"
          />
        </div>
      </div>

      <div class="controls-right">
        <DashboardSettings v-model="editLayout"/>
      </div>
    </div>

    <div>

      <DashboardTable 
        v-show="tabularView" 
        v-model="tabularData"
      />

      <draggable
        v-show="!tabularView"
        v-model="charts" 
        item-key="id" 
        :class="chartViewMode === 'tile' ? 'charts-grid' : 'charts-list'"
        ghost-class="ghost"
        drag-class="dragging"
        :disabled="!editLayout"
      >
        <template #item="{ element }">
          <div class="chart-wrapper">
            
            <!-- Bind the ref dynamically from the array object -->
            <canvas :ref="el => element.canvas = el"></canvas>
          </div>
        </template>
      </draggable>

    </div>
  </div>
</template>

<script setup>
  import draggable from 'vuedraggable';
  import Flatpickr from 'vue-flatpickr-component';
  import 'flatpickr/dist/flatpickr.css';
  import DashboardSettings from '@/components/DashboardSettings/DashboardSettings.vue';
  import DashboardTable from "@/components/DashboardTable/DashboardTable.vue";
  import { useHistoricDashboard } from "./useHistoricalDashboard.js";
  const { country, interval, startDate, endDate, 
          chartViewMode, editLayout, isDark,
          charts, datePickerConfig, countries, checkHas15, tabularView, tabularData } = useHistoricDashboard();
</script>
<style lang="scss" src="@/styles/DashboardCommon.scss" scoped></style>
<style lang="scss" src="@/styles/HistoricalDashboard.scss" scoped></style>