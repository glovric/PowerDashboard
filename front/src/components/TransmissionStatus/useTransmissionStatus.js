import { ref, onMounted, watch } from 'vue';
import { countries } from '@/utils/dashboardUtils'; 
import { loadTransmissionData, datePickerConfig } from './transmissionUtils';

export function useTransmissionStatus() {

  const interval = ref(15);

  const visibleCountries = countries.filter(
    country => interval.value !== 15 || country.has15
  );

  const timestamps = ref([]);

  const hours = ref(Array.from({ length: 24 }, (_, i) => i));
  
  const measurements = ref({});
  
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

    for (const country of visibleCountries) {
      console.log("Prolazim kroz ", country.value);

      measurements.value[country.value] = [];

      response.forEach(element => {
        let currentLoads = element["loads"];
        if(country.value in currentLoads) {
          measurements.value[country.value].push({
            status: currentLoads[country.value] != null ? 'available' : 'unavailable',
            load: currentLoads[country.value]
          });
        }
      });
    }

    console.log("Measurements nakon mojeg prvog loopa: ", measurements.value);

  };

  const getStatusClass = (countryValue, hour) => {
    const measurement = measurements.value[countryValue.value]?.[hour];
    if (!measurement) return 'unavailable';
    return measurement.status;
  };

  const showTooltip = (event, countryLabel, hour) => {
    // Find the country object by label to get its value for lookup
    const countryObj = countries.find(c => c.value === countryLabel.value);
    if (!countryObj) return;

    const measurement = measurements.value[countryObj.value]?.[hour];
    if (!measurement) return;

    tooltip.value = {
      visible: true,
      x: event.clientX + 10,
      y: event.clientY + 10,
      country: countryLabel.name,
      time: formatTime(hour),
      status: measurement.status === 'available' ? 'Data Available' : 'Data Unavailable',
      load: measurement.load
    };
  };

  const hideTooltip = () => {
    tooltip.value.visible = false;
  };

  onMounted(async () => {
    let timestampCount = interval.value == 60 ? 24 : 96;
    timestamps.value = Array.from({ length: timestampCount }, (_, i) => i)
    let result = await loadTransmissionData(interval.value, "2017-01-17T00:00:00.000Z");
    generateMeasurements(result.data);
  });

  watch([interval], async () => {
    let result = await loadTransmissionData(interval.value, "2017-01-17T00:00:00.000Z");
    generateMeasurements(result.data);
  });

  return {
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
  };
}