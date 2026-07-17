import { ref, onMounted, onBeforeUnmount, watch } from "vue";
import { useRoute, useRouter } from 'vue-router'
import { approveCountryCode, convertDateToISO,
         datePickerConfig, countries, checkHas15, calculateConsumption } from "@/utils/dashboardUtils";
import { loadObservedData, loadForecastedData, alignLoadData, 
         alignRampData, calculateRamp,
         createTabularData } from "./forecastUtils";
import { useTheme } from "@/composables/useTheme.js";
import { useDashboardViewMode } from "@/composables/useDashboardViewMode";
import { useDashboardLifecycle } from "@/composables/useDashboardLifecycle";

export function useForecastDashboard() {

  const route = useRoute();
  const router = useRouter();
  const urlSelectedCountry = route.query?.selectedCountry;
  const fallbackCountry = "DE";

  const country = ref(approveCountryCode(urlSelectedCountry) ? urlSelectedCountry.toUpperCase() : fallbackCountry);
  const interval = ref(60);
  const horizon = ref(6);
  const forecastDate = ref("2020-09-30 20:00");
  const forecastDateISO = () => convertDateToISO(forecastDate.value);

  const tableHeaders = [
    "Timestamp", 
    "Load", 
    "Forecast Load",
    "Ramp",
    "Consumption",
  ];

  const columnKeys = [
    'label',
    'load',
    'predicted',
    'ramp',
    'consumption',
  ];

  const forecastDashboardConfig = {
    intervalRef: interval,
    hidePredicted: false,
    
    fetchTrue: () => loadObservedData(country.value, interval.value, horizon.value, forecastDateISO()),
    fetchPredicted: () => loadForecastedData(country.value, interval.value, horizon.value, forecastDateISO()),

    processData: (trueData, predictedData) => { 

      const { loadValues: trueLoad, histValues, histLabels } = trueData;
      const { labels, predicted: forecastLoad, hist: forecastHist } = predictedData;

      const { trueLoadAligned, forecastLoadAligned } = alignLoadData(trueLoad, forecastLoad);
      const { trueRamp, forecastRampAligned } = alignRampData(trueLoad, forecastLoad);

      const totalLoad = [...trueLoad, ...forecastLoad]
      const totalRampTabular = calculateRamp(trueLoad, forecastLoad);
      const totalConsumptionTabular = calculateConsumption(totalLoad, interval.value);

      const tabular = createTabularData({
        headers: tableHeaders,
        columnKeys,
        labels: labels,
        trueLoad: trueLoad,
        forecastLoad: forecastLoadAligned,
        ramp: [null, ...totalRampTabular], // Null is added to adjust for first timestamp gap
        consumption: totalConsumptionTabular
      });

      const chartArgs = {
        "Line": [trueLoadAligned, forecastLoadAligned, labels],
        "Area": [trueLoadAligned, forecastLoadAligned, labels],
        "Histogram": [histValues, forecastHist, histLabels],
        "Ramp": [trueRamp, forecastRampAligned, labels.slice(1)]
      };

      return { tabularData: tabular, chartArgs };    
    }
    
  };

  const { init, cleanup, reloadData, charts, tabularData, editLayout } = useDashboardLifecycle(forecastDashboardConfig);
  const { chartViewMode, tabularView } = useDashboardViewMode();
  const { isDark } = useTheme();

  onMounted(init);

  onBeforeUnmount(cleanup);
  
  watch(country, (newCountry) => {
    if (route.query.selectedCountry !== newCountry) {
        router.replace({
          query: {
            ...route.query,
            selectedCountry: newCountry
          }
        })
    }

    interval.value = 60;

    reloadData();
  });

  watch([interval, horizon, forecastDate], () => {
    reloadData();
  });

  return { country, interval, horizon, forecastDate, 
           isDark, chartViewMode, charts, editLayout, datePickerConfig, countries, checkHas15, tabularView, tabularData };
}
