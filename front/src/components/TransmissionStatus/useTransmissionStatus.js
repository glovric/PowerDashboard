import { ref, onMounted } from 'vue';
import { countries as rawCountries } from '@/utils/dashboardUtils'; 

export function useTransmissionStatus() {

  const countries = ref(rawCountries.map(c => ({
    value: c.value,
    name: c.label
  })));

  const interval = ref(60);

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

  const generateMeasurements = () => {
    // 2. Use the mapped countries array
    countries.value.forEach(country => {
      // Initialize object for this country using its unique value (e.g., "AT")
      measurements.value[country.value] = {};
      
      hours.value.forEach(hour => {
        const rand = Math.random();
        
        if (rand > 0.15) {
          // Data available
          measurements.value[country.value][hour] = {
            status: 'available',
            load: (Math.random() * 30 + 20).toFixed(2)
          };
        } else {
          // Data unavailable
          measurements.value[country.value][hour] = {
            status: 'unavailable',
            load: null
          };
        }
      });
    });
  };

  const getStatusClass = (countryValue, hour) => {
    const measurement = measurements.value[countryValue.value]?.[hour];
    if (!measurement) return 'unavailable';
    return measurement.status;
  };

  const showTooltip = (event, countryLabel, hour) => {
    // Find the country object by label to get its value for lookup
    const countryObj = countries.value.find(c => c.value === countryLabel.value);
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

  onMounted(() => {
    generateMeasurements();
    console.log(measurements.value);
    console.log(hours.value);
  });

  return {
    countries,
    interval,
    hours,
    tooltip,
    formatTime,
    getStatusClass,
    showTooltip,
    hideTooltip
  };
}