import { ref, watch } from 'vue';
import { useChartResize } from '@/utils/dashboardUtils';
import { useErrorToast } from "@/components/ErrorToast/useErrorToast.js";
import { useLoadingToast } from "@/components/LoadingToast/useLoadingToast.js";
import { useLoadingOverlay } from '@/components/LoadingOverlay/useLoadingOverlay.js';
import { updateChartDatasets, updateAreaChartIntervalCache, 
         updateChartColors, chartColors, chartConfigs, createEmptyPredictionData } from '@/utils/dashboardUtils';
import { useTheme } from './useTheme';

export function useDashboardLifecycle(config) {

  const { showLoadingOverlay } = useLoadingOverlay();
  const { showError } = useErrorToast();
  const { showLoadingToast } = useLoadingToast();
  const { isDark } = useTheme();

  const editLayout = ref(false);
  const tabularData = ref({});
  const charts = ref(
      chartConfigs.map(config => ({
        id: config.id,
        canvas: null,
        createFn: config.create
      }))
  );

  let chartInstances = {};
  let chartInstancesArray;
  let resizeCleanup;

  const init = async () => {

    // 1. Fetch data
    showLoadingOverlay(true);
    const [resTrue, resPredicted] = await Promise.all([
        config.fetchTrue(),
        config.fetchPredicted()
    ]);
    showLoadingOverlay(false);

    if (!resPredicted || !resPredicted.success) {
        resPredicted.data = createEmptyPredictionData();
        let errorMessage = resPredicted.error.message;
        let errorDetail = resPredicted.error.status + ", " + resPredicted.error.statusText;
        showError(errorMessage, errorDetail);
    }

    if (!resTrue || !resTrue.success) {
        let errorMessage = resTrue.error.message;
        let errorDetail = resTrue.error.status + ", " + resTrue.error.statusText;
        showError(errorMessage, errorDetail);
    }
    else {

      // 2. Transform fetched data (creates tabular data, chart arguments)
      const transformationResult = config.processData(resTrue.data, resPredicted.data);
      
      tabularData.value = transformationResult.tabularData;

      const colors = chartColors(isDark.value);

      // 3. Instantiate charts
      charts.value.forEach(chart => {
        const chartArgs = transformationResult.chartArgs[chart.id];
        if (chartArgs && chart.createFn) {
          chartInstances[chart.id] = chart.createFn(chart.canvas, ...chartArgs, colors, config.hidePredicted);
        }
      });

      const areaChartInstance = chartInstances['Area'];
      updateAreaChartIntervalCache(areaChartInstance, config.intervalRef.value);

      // 4. Setup Resize Observer
      chartInstancesArray = Object.values(chartInstances);
      const { observeChartResize, stopObservingChartResize } = useChartResize(chartInstancesArray);
      resizeCleanup = stopObservingChartResize;
      observeChartResize();
    }

  };

  const cleanup = () => {

    if (resizeCleanup) {
      resizeCleanup();
    }

    if (chartInstancesArray) {
      chartInstancesArray.forEach(instance => {
        if (instance) {
          instance.destroy();
        }
      });
      chartInstances = {}; 
    }
  };

  const reloadData = async () => {
      showLoadingToast(true, "Loading new data");
      const [resTrue, resPredicted] = await Promise.all([
        config.fetchTrue(),
        config.fetchPredicted()
      ]);
      setTimeout(() => {
        showLoadingToast(false);
      }, 250);

      if (!resPredicted || !resPredicted.success) {
        resPredicted.data = createEmptyPredictionData();
        let errorMessage = resPredicted.error.message;
        let errorDetail = resPredicted.error.status + ", " + resPredicted.error.statusText;
        showError(errorMessage, errorDetail);
      }

      if(resTrue && resTrue.success) {
        const reloadResult = config.processData(resTrue.data, resPredicted.data);
        if (reloadResult.tabularData) {
          tabularData.value = reloadResult.tabularData;
        }
        if(reloadResult.chartArgs) {
          updateChartsData(reloadResult.chartArgs);
        }
      }
      else {
        let errorMessage = resTrue.error.message;
        let errorDetail = resTrue.error.status + ", " + resTrue.error.statusText;
        showError(errorMessage, errorDetail);
      }
  }

  const updateChartsData = (chartArgs) => {

    Object.keys(chartArgs).forEach(key => {
      const chartInstanceArgs = chartArgs[key];
      let instance = chartInstances[key];
      if (chartInstanceArgs) {
        updateChartDatasets(instance, ...chartInstanceArgs);
      }
    });

    const areaChartInstance = chartInstances['Area'];
    updateAreaChartIntervalCache(areaChartInstance, config.intervalRef.value);
  }

  const updateChartsTheme = () => {
    const colors = chartColors(isDark.value);
    if(!chartInstancesArray.some(chart => chart === null)) {
      chartInstancesArray.forEach((chart) => {
        updateChartColors(chart, colors);
      });
    }
  }

  watch(isDark, () => {
    updateChartsTheme();
  });

  return { init, cleanup, reloadData, charts, tabularData, editLayout };
}