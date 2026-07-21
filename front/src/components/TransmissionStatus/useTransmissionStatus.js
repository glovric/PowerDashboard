import { ref, onMounted, watch } from 'vue';
import { countries, convertDateToISO } from '@/utils/dashboardUtils'; 
import { loadTransmissionData, datePickerConfig } from './transmissionUtils';

export function useTransmissionStatus() {

  const interval = ref(60);
  const date = ref("2020-09-30");
  const dateISO = () => convertDateToISO(date.value);

  const visibleCountries = ref({});

  const hours = ref(Array.from({ length: 24 }, (_, i) => i));
  
  const tooltip = ref({
    visible: false,
    x: 0,
    y: 0,
    country: '',
    time: '',
    status: '',
    load: null
  });

  const formatTime = (hour) => {
    return `${hour.toString().padStart(2, '0')}:00`;
  };

  const generateMeasurements = (response) => {

    console.log(response);

    visibleCountries.value = Object.fromEntries(
        Object.entries(response).map(([country, values]) => [
            countries.find(c => c.value === country)?.label ?? country,
            values.map(x => x.loadValue)
        ])
    );

    console.log(visibleCountries.value);

  };

  const getStatusClass = (load) => {
    if (load === null || load === undefined) return 'unavailable';
    return 'available';
  };

  const showTooltip = (event, countryLabel, hour) => {
    const measurement = visibleCountries.value[countryLabel]?.[hour];
    const status = measurement !== null && measurement !== undefined;

    tooltip.value = {
      visible: true,
      x: event.clientX + 10,
      y: event.clientY + 10,
      country: countryLabel,
      time: formatTime(hour),
      status: status ? 'Data Available' : 'Data Unavailable',
      load: measurement
    };
  };

  const hideTooltip = () => {
    tooltip.value.visible = false;
  };

  onMounted(async () => {
    let result = await loadTransmissionData(interval.value, dateISO());
    generateMeasurements(result.data);
  });

  watch([interval, date], async () => {
    visibleCountries.value = countries.filter(
      country => interval.value !== 15 || country.has15
    );
    let result = await loadTransmissionData(interval.value, dateISO());
    generateMeasurements(result.data);
  });

  return {
    visibleCountries,
    date,
    interval,
    hours,
    tooltip,
    formatTime,
    getStatusClass,
    showTooltip,
    hideTooltip,
    datePickerConfig
  };
}