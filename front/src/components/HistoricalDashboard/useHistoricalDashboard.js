import { ref, onMounted, onBeforeUnmount, watch } from "vue";
import { useRoute, useRouter } from 'vue-router'
import { approveCountryCode,
         convertDateToISO, datePickerConfig, countries, checkHas15, 
         createTabularData, calculateConsumption } from "@/utils/dashboardUtils";
import { loadHistoricData, loadNowcastData, isValidDateRange } from "./historicalUtils";
import { useTheme } from "@/composables/useTheme.js";
import { useDashboardViewMode } from "@/composables/useDashboardViewMode";
import { useDashboardLifecycle } from "@/composables/useDashboardLifecycle";

export function useHistoricDashboard() {

  const route = useRoute();
  const router = useRouter();
  const urlSelectedCountry = route.query?.selectedCountry;
  const fallbackCountry = "DE";

  const country = ref(approveCountryCode(urlSelectedCountry) ? urlSelectedCountry.toUpperCase() : fallbackCountry);
  const interval = ref(60);
  const startDate = ref("2017-01-15 00:00");
  const endDate = ref("2017-01-17 00:00");
  const startDateISO = () => convertDateToISO(startDate.value);
  const endDateISO = () => convertDateToISO(endDate.value);

  const tableHeaders = [
    "Timestamp", 
    "Load", 
    "Predicted Load",
    "Load Percentage Error",
    "Ramp",
    "Predicted Ramp",
    "Ramp Percentage Error",
    "Consumption",
    "Predicted Consumption",
    "Consumption Percentage Error"
  ];

  const columnKeys = [
    'label',
    'load',
    'predicted',
    'error',
    'ramp',
    'rampPredicted',
    'errorRamp',
    'consumption',
    'consumptionPred',
    'errorConsumption'
  ];

  const historicDashboardConfig = {
    intervalRef: interval,
    hidePredicted: true,
    
    fetchTrue: () => loadHistoricData(country.value, interval.value, startDateISO(), endDateISO()),
    fetchPredicted: () => loadNowcastData(country.value, interval.value, startDateISO(), endDateISO()),

    processData: (trueData, predictedData) => { 

      const { labels, loadValues, rampValues, histValues, histLabels } = trueData;
      const {predicted: predictedLoadValues, hist: predictedHistValues, ramp: predictedRampValues} = predictedData;

      const trueConsumption = calculateConsumption(loadValues, interval.value);
      const predictedConsumption = calculateConsumption(predictedLoadValues, interval.value);

      const tabular = createTabularData({
        headers: tableHeaders,
        columnKeys,
        labels,
        trueLoad: loadValues,
        predLoad: predictedLoadValues,
        trueRamp: [null, ...rampValues],
        predRamp: [null, ...predictedRampValues],
        consumption: trueConsumption,
        predConsumption: predictedConsumption
      });

      const chartArgs = {
        "Line": [loadValues, predictedLoadValues, labels],
        "Area": [loadValues, predictedLoadValues, labels],
        "Histogram": [histValues, predictedHistValues, histLabels],
        "Ramp": [rampValues, predictedRampValues, labels.slice(1)]
      };

      return { tabularData: tabular, chartArgs };    
    }
  
  };
  
  const { init, cleanup, reloadData, charts, tabularData, editLayout } = useDashboardLifecycle(historicDashboardConfig);
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

    if(isValidDateRange(startDate.value, endDate.value)) {
      reloadData();
    }

  });

  watch([interval, startDate, endDate], () => {
    if(isValidDateRange(startDate.value, endDate.value)) {
      reloadData();
    }
  });

  return { country, interval, startDate, endDate, 
           chartViewMode, charts, editLayout, isDark, 
           datePickerConfig, countries, checkHas15, tabularView, 
           tabularData };
}
