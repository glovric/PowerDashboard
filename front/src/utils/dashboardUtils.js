import { Chart, LineController, LineElement, PointElement,
        LinearScale, CategoryScale, Tooltip, Legend, Filler, BarController, BarElement } from "chart.js";
import Zoom from "chartjs-plugin-zoom";

Chart.register(
  LineController,
  LineElement,
  PointElement,
  LinearScale,
  CategoryScale,
  Tooltip,
  Legend,
  Filler,
  BarController, 
  BarElement,
  Zoom
);

export const countries = [
  { value: "AT", label: "Austria", iso3: "AUT", has15: true },
  { value: "BE", label: "Belgium", iso3: "BEL", has15: true },
  { value: "BG", label: "Bulgaria", iso3: "BGR" },
  { value: "CH", label: "Switzerland", iso3: "CHE" },
  { value: "CY", label: "Cyprus", iso3: "CYP" },
  { value: "CZ", label: "Czech Republic", iso3: "CZE" },
  { value: "DE", label: "Germany", iso3: "DEU", has15: true },
  { value: "DK", label: "Denmark", iso3: "DNK" },
  { value: "EE", label: "Estonia", iso3: "EST" },
  { value: "ES", label: "Spain", iso3: "ESP" },
  { value: "FI", label: "Finland", iso3: "FIN" },
  { value: "FR", label: "France", iso3: "FRA" },
  { value: "GB", label: "United Kingdom", iso3: "GBR" },
  { value: "GR", label: "Greece", iso3: "GRC" },
  { value: "HR", label: "Croatia", iso3: "HRV" },
  { value: "HU", label: "Hungary", iso3: "HUN", has15: true },
  { value: "IE", label: "Ireland", iso3: "IRL" },
  { value: "IT", label: "Italy", iso3: "ITA" },
  { value: "LT", label: "Lithuania", iso3: "LTU" },
  { value: "LU", label: "Luxembourg", iso3: "LUX", has15: true },
  { value: "LV", label: "Latvia", iso3: "LVA" },
  { value: "ME", label: "Montenegro", iso3: "MNE" },
  { value: "NL", label: "Netherlands", iso3: "NLD", has15: true },
  { value: "NO", label: "Norway", iso3: "NOR" },
  { value: "PL", label: "Poland", iso3: "POL" },
  { value: "PT", label: "Portugal", iso3: "PRT" },
  { value: "RO", label: "Romania", iso3: "ROU" },
  { value: "RS", label: "Serbia", iso3: "SRB" },
  { value: "SE", label: "Sweden", iso3: "SWE" },
  { value: "SI", label: "Slovenia", iso3: "SVN" },
  { value: "SK", label: "Slovakia", iso3: "SVK" },
  { value: "UA", label: "Ukraine", iso3: "UKR" },
];

export function checkHas15(country) {
    const selectedCountry = countries.find(c => c.value === country);
    return selectedCountry?.has15;
}

export function isValidArray(arr) {
  return Array.isArray(arr) && arr.length > 0
}

export const datePickerConfig = {
    enableTime: true,
    dateFormat: 'Y-m-d H:i',
    minDate: '2015-01-01',
    time_24hr: true,
    minuteIncrement: 15
}

function computeAreaUnderCurve(data, unit) {
  let area = 0;

  let dxObj = {
    60: 1,
    15: 0.25
  }

  const dx = dxObj[unit];
  
  for (let i = 1; i < data.length; i++) {
    const y1 = data[i-1];
    const y2 = data[i];

    if (y1 == null || y2 == null) {
      continue;
    }

    const segmentArea = (y1 + y2) * dx / 2;
    area += segmentArea;

  }
  
  return area;
}

export const areaTooltipPlugin = {
    id: 'areaTooltip',
    beforeInit: function() {
      // Create the tooltip element if it doesn't exist
      let areaTooltipEl = document.getElementById('area-tooltip');
      if (!areaTooltipEl) {
        areaTooltipEl = document.createElement('div');
        areaTooltipEl.id = 'area-tooltip';
        areaTooltipEl.style.position = 'absolute';
        areaTooltipEl.style.padding = '8px 12px';
        areaTooltipEl.style.backgroundColor = 'rgba(0, 0, 0, 0.8)';
        areaTooltipEl.style.color = 'white';
        areaTooltipEl.style.borderRadius = '6px';
        areaTooltipEl.style.pointerEvents = 'none';
        areaTooltipEl.style.zIndex = '1000';
        areaTooltipEl.style.opacity = '0';
        areaTooltipEl.style.transition = 'opacity 0.2s ease';
        areaTooltipEl.style.fontSize = '14px';
        areaTooltipEl.style.fontFamily = 'Arial, sans-serif';
        areaTooltipEl.style.textAlign = 'center';
        areaTooltipEl.style.boxShadow = '0 2px 6px rgba(0,0,0,0.3)';
        areaTooltipEl.style.whiteSpace = 'nowrap';
        document.body.appendChild(areaTooltipEl);
      }
    },
    afterEvent: function(chart, args) {
      const { event } = args;
      const areaTooltipEl = document.getElementById('area-tooltip');
      
      const rect = chart.canvas.getBoundingClientRect();
      
      let clientX, clientY;
      
      if (event && event.native && event.native.clientX !== undefined) {
        clientX = event.native.clientX;
        clientY = event.native.clientY;
      } else {
        areaTooltipEl.style.opacity = '0';
        return;
      }
      
      const x = clientX - rect.left;
      const y = clientY - rect.top;
      const scrollX = window.scrollX || window.pageXOffset;
      const scrollY = window.scrollY || window.pageYOffset;
      
      const chartArea = chart.chartArea;
      const isOverChart = x >= chartArea.left && x <= chartArea.right && 
                        y >= chartArea.top && y <= chartArea.bottom;
      
      if (isOverChart) {
        const currentUnit = chart.$interval;
        let areaCache = chart.$areaCache;

        const datasets = chart.data.datasets;
        let tooltipContent = '';

        datasets.forEach((dataset, index) => {
          if (chart.isDatasetVisible(index)) {

            if(!areaCache[index]) {
              areaCache[index] = computeAreaUnderCurve(dataset.data, currentUnit);
            }

            tooltipContent += `<strong>${dataset.label}:</strong><br>${areaCache[index].toFixed(2)} MWh<br>`;

          }
        });

        if(tooltipContent != '') {
          areaTooltipEl.style.left = clientX + scrollX + 10 + 'px';
          areaTooltipEl.style.top = clientY + scrollY - 10 + 'px';
          areaTooltipEl.innerHTML = tooltipContent;
          areaTooltipEl.style.opacity = '1';
        }
      } else {
        areaTooltipEl.style.opacity = '0';
      }
    }
};

export const chartColors = (isDark) => ({
  text: isDark ? "#E5E7EB" : "#374151",
  grid: isDark ? "#374151" : "#E5E7EB",
  lineTrue: "rgba(66, 165, 245, 1)",
  fillTrue: isDark ? "rgba(66,165,245,0.15)" : "rgba(66, 165, 245, 0.25)",
  linePredicted: '#ff6384',
  fillPredicted: 'rgba(255, 99, 132, 0.3)',
});

const zoomOptions = () => ({
  pan: { enabled: true, mode: 'x', modifierKey: null, scaleMode: 'x' },
  zoom: {
    wheel: { enabled: true },
    drag: { enabled: false, },
    pinch: { enabled: true, },
    click: { enabled: true, },
    mode: 'x',
    animation: {
      duration: 5000,
      easing: 'easeOutCubic'
    }
  },
  limits: {
    x: { min: 'original', max: 'original' },
    y: { min: 'original', max: 'original' }
  }
})

const scaleOptions = (colors, xText, yText) => ({
  x: { 
    ticks: { color: colors.text }, 
    grid: { color: colors.grid }, 
    title: { display: true, text: xText, color: colors.text }
  },
  y: {
    ticks: { color: colors.text },
    grid: { color: colors.grid },
    title: { display: true, text: yText, font: {weight: "bold"}, color: colors.text },
  }
})

const lineChartOptions = (colors) => ({
    responsive: true,
    maintainAspectRatio: true,
    plugins: {
      zoom: zoomOptions(),
      legend: { labels: { color: colors.text } }
    },
    scales: scaleOptions(colors, "Time", "MW"),
    transitions: {
      zoom: {
        animation: {
          duration: 1000,
          easing: 'easeOutCubic'
        }
      }
    }
});

const areaChartOptions = (colors) => ({
    responsive: true,
    maintainAspectRatio: true,
    interaction: { mode: 'nearest', intersect: false },
    plugins: {
      tooltip: { enabled: false },
      legend: { labels: { color: colors.text } }
    },
    scales: scaleOptions(colors, "Time", "MW")
});

const histogramChartOptions = (colors) => ({
    responsive: true,
    plugins: {
        title: {
            display: true,
            text: 'Sample Histogram'
        },
        legend: { labels: { color: colors.text } }
    },
    scales: scaleOptions(colors, "MW", "Frequency")
});

function createLineDatasets(trueData, predictedData, colors, hidePredicted) {

  const datasets = [];

  if(isValidArray(trueData)) {
    datasets.push({
      label: "Load",
      data: trueData,
      borderColor: colors.lineTrue,
      backgroundColor: "transparent",
      tension: 0.2,
      pointRadius: 0,
    });
  }

  if(isValidArray(predictedData)) {
    datasets.push({
      label: "Predicted Load",
      data: predictedData,
      borderColor: colors.linePredicted,
      backgroundColor: "transparent",
      tension: 0.2,
      pointRadius: 0,
      hidden: hidePredicted
    });
  }

  return datasets;
}

function createAreaDatasets(trueData, predictedData, colors, hidePredicted) {

  const datasets = [];

  if(isValidArray(trueData)) {
    datasets.push({
      label: "Consumption",
      data: trueData,
      borderColor: colors.lineTrue,
      backgroundColor: colors.fillTrue,
      fill: 'origin',
      tension: 0.2,
      pointRadius: 0,
    });
  }

  if(isValidArray(predictedData)) {
    datasets.push({
      label: "Predicted Consumption",
      data: predictedData,
      borderColor: colors.linePredicted,
      backgroundColor: colors.fillPredicted,
      fill: 'origin',
      tension: 0.2,
      pointRadius: 0,
      hidden: hidePredicted
    })
  }

  return datasets;
}

function createHistogramDatasets(trueData, predictedData, colors, hidePredicted) {

  const datasets = [];

  if(isValidArray(trueData)) {
    datasets.push({
      label: 'Frequency',
      data: trueData,
      borderColor: colors.lineTrue,
      backgroundColor: colors.fillTrue,
      borderWidth: 2,
      fill: 'origin',
    });
  }

  if(isValidArray(predictedData)) {
    datasets.push({
      label: 'Predicted Frequency',
      data: predictedData,
      borderColor: colors.linePredicted,
      backgroundColor: colors.fillPredicted,
      borderWidth: 2,
      fill: 'origin',
      hidden: hidePredicted
    })
  }

  return datasets;
}

function createRampDatasets(trueData, predictedData, colors, hidePredicted) {

  const datasets = [];

  if(isValidArray(trueData)) {
    datasets.push({
      label: "Ramp Load",
      data: trueData,
      borderColor: colors.lineTrue,
      backgroundColor: "transparent",
      tension: 0.2,
      pointRadius: 3,
    });
  }

  if(isValidArray(predictedData)) {
    datasets.push(          {
      label: "Predicted Ramp Load",
      data: predictedData,
      borderColor: colors.linePredicted,
      backgroundColor: "transparent",
      tension: 0.2,
      pointRadius: 3,
      hidden: hidePredicted
    })
  }

  return datasets;
}

export function createLineChart(canvas, trueData, predictedData=null, labels, colors, hidePredicted) {
  const datasets = createLineDatasets(trueData, predictedData, colors, hidePredicted);
  let lineChartInstance = new Chart(canvas, {
        type: "line",
        data: {
          labels: labels,
          datasets: datasets
        },
        options: lineChartOptions(colors)
      });
  return lineChartInstance;
}

export function createAreaChart(canvas, trueData, predictedData, labels, colors, hidePredicted) {

  const datasets = createAreaDatasets(trueData, predictedData, colors, hidePredicted);
  let areaChartInstance = new Chart(canvas, {
      type: "line",
      data: {
        labels: labels,
        datasets: datasets
      },
      options: areaChartOptions(colors),
      plugins: [areaTooltipPlugin]
    });

  return areaChartInstance;

}

export function createHistogramChart(canvas, trueData, predictedData, labels, colors, hidePredicted) {

  const datasets = createHistogramDatasets(trueData, predictedData, colors, hidePredicted)

  let histogramChartInstance = new Chart(canvas, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: datasets
            },
            options: histogramChartOptions(colors)
    });

  return histogramChartInstance;

}

export function createRampChart(canvas, trueData, predictedData, labels, colors, hidePredicted) {

  const datasets = createRampDatasets(trueData, predictedData, colors, hidePredicted)

  let rampChartInstance = new Chart(canvas, {
      type: "line",
      data: {
        labels: labels,
        datasets: datasets
      },
      options: lineChartOptions(colors)
    });

  return rampChartInstance;

}

export function updateChartDatasets(chartInstance, newTrueData, newPredictedData, newLabels) {
  chartInstance.data.labels = newLabels;

  if (chartInstance.data.datasets[0]?.data && isValidArray(newTrueData)) {
    chartInstance.data.datasets[0].data = newTrueData;
  }

  // If predicted dataset exist in chartInstance, replace it with new data
  if (chartInstance.data.datasets[1]?.data && isValidArray(newPredictedData)) {
    chartInstance.data.datasets[1].data = newPredictedData;
  }

  // If predicted dataset doesnt exist in chartInstance, create and push new dataset object
  else if (!chartInstance.data.datasets[1]?.data && isValidArray(newPredictedData)) {

    // If the label is Load, it is a Line chart
    if(chartInstance.data.datasets[0].label == "Load") {
      chartInstance.data.datasets.push({
        label: "Predicted Load",
        data: newPredictedData,
        borderColor: '#ff6384',
        backgroundColor: 'rgba(255, 99, 132, 0.3)',
        tension: 0.2,
        pointRadius: 0,
        hidden: true
      })
    }

    // If the label is Consumption, it is an Area chart
    else if(chartInstance.data.datasets[0].label == "Consumption") {
      chartInstance.data.datasets.push({
        label: "Predicted Load",
        data: newPredictedData,
        borderColor: '#ff6384',
        backgroundColor: 'rgba(255, 99, 132, 0.3)',
        tension: 0.2,
        fill: 'origin',
        pointRadius: 0,
        hidden: true
      })
    }

    else if(chartInstance.data.datasets[0].label == "Frequency") {
      chartInstance.data.datasets.push({
        label: "Predicted Frequency",
        data: newPredictedData,
        borderColor: '#ff6384',
        backgroundColor: 'rgba(255, 99, 132, 0.3)',
        borderWidth: 2,
        fill: 'origin',
      })
    }

  }
  chartInstance.resetZoom();
  chartInstance.update();
}

export function updateChartColors(chartInstance, colors) {
  chartInstance.options.plugins.legend.labels.color = colors.text;
  chartInstance.options.scales.x.ticks.color = colors.text;
  chartInstance.options.scales.x.grid.color = colors.grid;
  chartInstance.options.scales.x.title.color = colors.text;
  chartInstance.options.scales.y.ticks.color = colors.text;
  chartInstance.options.scales.y.grid.color = colors.grid;
  chartInstance.options.scales.y.title.color = colors.text;
  chartInstance.data.datasets[0].backgroundColor = chartInstance.data.datasets[0].fill ? colors.fillTrue : "transparent";
  chartInstance.resetZoom();
  chartInstance.update();
}

export function updateAreaChartIntervalCache(areaChartInstance, newInterval) {
  areaChartInstance.$areaCache = {};
  areaChartInstance.$interval = newInterval;
}

export function approveCountryCode(countryCode) {
  if(countryCode == null || countryCode == undefined) {
    return false;
  }
  const supportedCountries = countries.map(c => c.value);
  return supportedCountries.includes(countryCode.toUpperCase());
}

export function convertDateToISO(date) {
  if (!date) return '';
  const utcString = typeof date === 'string' 
    ? date.replace(' ', 'T') + 'Z' 
    : date.toISOString();
  return new Date(utcString).toISOString();
}

export const chartConfigs = [
  { id: "Line", create: createLineChart },
  { id: "Area", create: createAreaChart },
  { id: "Histogram", create: createHistogramChart },
  { id: "Ramp", create: createRampChart },
];

export function absolutePercentageError(actual, forecast) {
  if(actual == 0 || isNaN(actual) || actual == null || isNaN(forecast) || forecast == null) return "-";
  let num = Math.abs((actual - forecast) / actual) * 100 
  return Number(num.toFixed(4));

}

export function round(num, digits=3) {
  if(isNaN(num) || num == null) return "-";
  return Number(num.toFixed(digits));
}

export function calculateConsumption(load, unit) {

  let dxObj = {
    60: 1,
    15: 0.25
  }

  const dx = dxObj[unit];

  let result = [null]
  
  for (let i = 1; i < load.length; i++) {
    const y1 = load[i-1];
    const y2 = load[i];

    if (y1 == null || y2 == null) {
      result.push(null);
      continue;
    }

    const segmentArea = (y1 + y2) * dx / 2;
    result.push(segmentArea);

  }
  
  return result;
}

export function createTabularData({headers, columnKeys, labels, trueLoad, predLoad, trueRamp, predRamp, consumption, predConsumption} = {}) {

  const rows = labels.map((label, i) => ({
    label,
    load: round(trueLoad[i]),
    predicted: round(predLoad[i]),
    ramp: round(trueRamp[i]),
    rampPredicted: round(predRamp[i]),
    error: absolutePercentageError(trueLoad[i], predLoad[i]),
    errorRamp: absolutePercentageError(trueRamp[i], predRamp[i]),
    consumption: round(consumption[i]),
    consumptionPred: round(predConsumption[i]),
    errorConsumption: absolutePercentageError(consumption[i], predConsumption[i]),
  }));

  const data = {
    headers, // Used to create table headers <th>
    columnKeys, // Keys used to extract values from rows object
    rows
  }

  return data;
  
}

/**
 * Creates an empty prediction data object. 
 * This is used when prediction response data is null.
 *
 * @returns {{
 *   predicted: [],
 *   hist: [],
 *   ramp: [],
 *   labels: []
 * }}
 */
export function createEmptyPredictionData() {
  return { predicted: [], hist: [], ramp: [], labels: [] };
}

export function useChartResize(chartInstances) {
  let resizeObserver = null
  let resizeRAF = null

  // Call this once after charts are created
  const observeChartResize = () => {
    if (!chartInstances || chartInstances.length === 0) return

    resizeObserver = new ResizeObserver(() => {
      if (resizeRAF) return
      resizeRAF = requestAnimationFrame(() => {
        chartInstances.forEach(chart => {
          if (chart && chart.canvas && chart.canvas.ownerDocument) {
            chart.resize();
          }
        })
        resizeRAF = null
      })
    })

    chartInstances.forEach(chart => {
      const container = chart.canvas?.parentElement
      if (container) resizeObserver.observe(container)
    })
  }

  const stopObservingChartResize = () => {
    if (resizeObserver) {
      resizeObserver.disconnect();
      resizeObserver = null;
    }
    if (resizeRAF) {
      cancelAnimationFrame(resizeRAF);
      resizeRAF = null;
    }
  }


  return { observeChartResize, stopObservingChartResize }
}