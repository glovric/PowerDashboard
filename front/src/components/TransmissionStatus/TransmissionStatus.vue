<template>
  <div class="power-load-status">
    <h2>Power Load Transmission Status</h2>
    
    <!-- Legend -->
    <div class="legend">
      <div class="legend-item">
        <div class="color-box available"></div>
        <span>Data Available</span>
      </div>
      <div class="legend-item">
        <div class="color-box unavailable"></div>
        <span>Data Unavailable</span>
      </div>
    </div>

    <!-- Table -->
    <div class="table-container">
      <table class="status-table">
        <thead>
          <tr>
            <th class="country-header">Country</th>
            <th 
              v-for="hour in hours" 
              :key="hour" 
              class="time-header"
            >
              {{ formatTime(hour) }}
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="country in countries" :key="country.name">
            <td class="country-name">{{ country.name }}</td>
            <td 
              v-for="hour in hours" 
              :key="`${country.name}-${hour}`"
              class="status-cell"
              :class="getStatusClass(country, hour)"
              @mouseenter="showTooltip($event, country, hour)"
              @mouseleave="hideTooltip"
            >
              <!-- Cell is now fully colored, no inner rectangle needed -->
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Tooltip -->
    <div 
      v-if="tooltip.visible" 
      class="tooltip"
      :style="{ top: tooltip.y + 'px', left: tooltip.x + 'px' }"
    >
      <div><strong>{{ tooltip.country }}</strong></div>
      <div>Time: {{ tooltip.time }}</div>
      <div>Status: {{ tooltip.status }}</div>
      <div v-if="tooltip.load">Load: {{ tooltip.load }} GW</div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'PowerLoadStatus',
  data() {
    return {
      countries: [
        { name: 'Germany' },
        { name: 'France' },
        { name: 'UK' },
        { name: 'Spain' },
        { name: 'Italy' }
      ],
      hours: Array.from({ length: 24 }, (_, i) => i),
      measurements: {},
      tooltip: {
        visible: false,
        x: 0,
        y: 0,
        country: '',
        time: '',
        status: '',
        load: null
      }
    }
  },
  created() {
    this.generateMeasurements()
  },
  methods: {
    formatTime(hour) {
      return `${hour.toString().padStart(2, '0')}:00`
    },
    
    generateMeasurements() {
      // Simulate measurement data
      this.countries.forEach(country => {
        this.measurements[country.name] = {}
        this.hours.forEach(hour => {
          const rand = Math.random()
          if (rand > 0.15) {
            // Data available
            this.measurements[country.name][hour] = {
              status: 'available',
              load: (Math.random() * 30 + 20).toFixed(2) // 20-50 GW
            }
          } else {
            // Data unavailable
            this.measurements[country.name][hour] = {
              status: 'unavailable',
              load: null
            }
          }
        })
      })
    },
    
    getStatusClass(country, hour) {
      const measurement = this.measurements[country.name]?.[hour]
      if (!measurement) return 'unavailable'
      return measurement.status
    },
    
    showTooltip(event, country, hour) {
      const measurement = this.measurements[country.name]?.[hour]
      if (!measurement) return
      
      this.tooltip = {
        visible: true,
        x: event.clientX + 10,
        y: event.clientY + 10,
        country: country.name,
        time: this.formatTime(hour),
        status: this.getStatusText(measurement.status),
        load: measurement.load
      }
    },
    
    hideTooltip() {
      this.tooltip.visible = false
    },
    
    getStatusText(status) {
      const statusMap = {
        available: 'Data Available',
        unavailable: 'Data Unavailable',
      }
      return statusMap[status] || status
    }
  }
}
</script>

<style lang="scss" src="@/styles/TransmissionStatus.scss" scoped></style>