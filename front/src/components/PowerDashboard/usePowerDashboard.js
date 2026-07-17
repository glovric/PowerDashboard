import { ref, onMounted, onBeforeUnmount, watch } from "vue";
import { useRoute, useRouter } from 'vue-router'
import { approveCountryCode, countries, checkHas15, 
         calculateConsumption, createTabularData } from "@/utils/dashboardUtils";
import { loadLatestData, loadNowcastData } from "./latestUtils";
import { useTheme } from "@/composables/useTheme.js";
import { useDashboardViewMode } from "@/composables/useDashboardViewMode";
import { useDashboardLifecycle } from "@/composables/useDashboardLifecycle";

export function usePowerDashboard() {

  const route = useRoute();
  const router = useRouter();
  const urlSelectedCountry = route.query?.selectedCountry;
  const fallbackCountry = "DE";

  const country = ref(approveCountryCode(urlSelectedCountry) ? urlSelectedCountry.toUpperCase() : fallbackCountry);
  const interval = ref(60);
  const window = ref(6);

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

  const powerDashboardConfig = {
    intervalRef: interval,
    hidePredicted: true,
    
    fetchTrue: () => loadLatestData(country.value, window.value, interval.value),
    fetchPredicted: () => loadNowcastData(country.value, window.value, interval.value),

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

  const { init, cleanup, reloadData, charts, tabularData, editLayout } = useDashboardLifecycle(powerDashboardConfig);
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

  watch([interval, window], () => {
    reloadData();
  });

  return { country, interval, window, 
           editLayout, isDark, chartViewMode, 
           charts, countries, checkHas15,
           tabularView, tabularData };
}