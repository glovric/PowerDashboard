import { ref, onMounted, watch, computed } from 'vue';
import { convertDateToISO } from '@/utils/dashboardUtils'; 
import { loadTransmissionData, datePickerConfig, getStatusClass, formatTime, transformTransmissionData } from './transmissionUtils';
import { useTheme } from "@/composables/useTheme.js";
import { useErrorToast } from '../ErrorToast/useErrorToast';

export function useTransmissionStatus() {

  const { isDark } = useTheme();
  const { showError } = useErrorToast();

  const interval = ref(60);
  const date = ref("2020-09-30");
  const dateISO = () => convertDateToISO(date.value);

  const loading = ref(false);
  const transmissionData = ref({});
  const hourColumns = ref(Array.from({ length: 24 }, (_, i) => i));
  const tooltip = ref({
    visible: false,
    x: 0,
    y: 0,
    country: '',
    time: '',
    load: null
  });

  const totalColumns = computed(() => {
    const hourColumnsCount = 24;
    const colspanPerHour = interval.value === 15 ? 4 : 1;
    return 1 + (hourColumnsCount * colspanPerHour);
  });

  const showLoading = (newValue) => {
    loading.value = newValue;
  }

  const showTooltip = (event, countryLabel, timestampIndex) => {
    const loadValue = transmissionData.value[countryLabel]?.[timestampIndex];

    tooltip.value = {
      visible: true,
      x: event.clientX - 150,
      y: event.clientY - 50,
      country: countryLabel,
      time: formatTime(timestampIndex, interval.value),
      load: loadValue
    };
  };

  const moveTooltip = (event) => {
    if (!tooltip.value.visible) return;
    tooltip.value.x = event.clientX - 150;  
    tooltip.value.y = event.clientY - 50;
  };

  const hideTooltip = () => {
    tooltip.value.visible = false;
  };

  const fetchData = async () => {
    const res = await loadTransmissionData(interval.value, dateISO());
    if(res && res.success) {
      const resData = res.data.data;
      transmissionData.value = transformTransmissionData(resData);
    }
    else {
      let errorMessage = res.error.message;
      let errorDetail = res.error.status + ", " + res.error.statusText;
      showError(errorMessage, errorDetail);
    }
  }

  onMounted(async () => {
    showLoading(true);
    await fetchData();
    setTimeout(() => {
        showLoading(false);
    }, 250);
  });

  watch([interval, date], async () => {
    showLoading(true);
    await fetchData();
    setTimeout(() => {
        showLoading(false);
    }, 250);
  });

  return { 
    transmissionData, isDark, date, interval, hourColumns, tooltip,
    formatTime, getStatusClass, showTooltip, hideTooltip, moveTooltip,
    datePickerConfig, totalColumns, loading
  };
}