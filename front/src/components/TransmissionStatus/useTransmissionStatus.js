/* eslint-disable */

import { ref, onMounted, watch } from 'vue';
import { convertDateToISO } from '@/utils/dashboardUtils'; 
import { loadTransmissionData, datePickerConfig, getStatusClass, formatTime, generateMeasurements } from './transmissionUtils';
import { useTheme } from "@/composables/useTheme.js";

export function useTransmissionStatus() {

  const interval = ref(60);
  const date = ref("2020-09-30");
  const dateISO = () => convertDateToISO(date.value);
  const { isDark } = useTheme();

  const countries = ref({});
  const hours = ref(Array.from({ length: 24 }, (_, i) => i));
  const tooltip = ref({
    visible: false,
    x: 0,
    y: 0,
    country: '',
    time: '',
    load: null
  });

  const showTooltip = (event, countryLabel, hour) => {
    const measurement = countries.value[countryLabel]?.[hour];

    tooltip.value = {
      visible: true,
      x: event.clientX + 50,
      y: event.clientY - 50,
      country: countryLabel,
      time: formatTime(hour, interval.value),
      load: measurement
    };
  };

  const hideTooltip = () => {
    tooltip.value.visible = false;
  };

  onMounted(async () => {
    let result = await loadTransmissionData(interval.value, dateISO());
    countries.value = generateMeasurements(result.data.data);
    console.log(countries.value);
  });

  watch([interval, date], async () => {
    let result = await loadTransmissionData(interval.value, dateISO());
    countries.value = generateMeasurements(result.data.data);
    console.log(countries.value);
  });

  return { 
    countries, isDark, date, interval, hours, tooltip,
    formatTime, getStatusClass, showTooltip, hideTooltip, 
    datePickerConfig
  };
}