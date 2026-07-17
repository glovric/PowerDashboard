import { Chart, DoughnutController, ArcElement } from "chart.js";
import { powerApi } from "@/api";

Chart.register(
  DoughnutController,
  ArcElement
);

export const chartColors = (isDark) => ({
  text: isDark ? "#E5E7EB" : "#374151",
  grid: isDark ? "#374151" : "#E5E7EB",
  lineTrue: "rgba(66, 165, 245, 1)",
  fillTrue: isDark ? "rgba(66,165,245,0.15)" : "rgba(66, 165, 245, 0.25)",
  linePredicted: '#ff6384',
  fillPredicted: 'rgba(255, 99, 132, 0.3)',
});

export function updateChartDatasets(chartInstance, newTrueData, newPredictedData, newLabels) {
  chartInstance.data.labels = newLabels;
  chartInstance.data.datasets[0].data = newTrueData;
  if (chartInstance.data.datasets[1]?.data && Array.isArray(newPredictedData) && newPredictedData.length > 0) {
    chartInstance.data.datasets[1].data = newPredictedData;
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

export function createPieChart(pieChartRef, sizeHour, sizeQuarter, sizeSystem) {
    let pieChartInstance = new Chart(pieChartRef.value, {
        type: 'doughnut',
        data: {
            labels: ['Hourly Storage', 'Quarterly Storage', 'System Storage'],
            datasets: [{
                label: 'Storage Distribution (MB)',
                data: [sizeHour, sizeQuarter, sizeSystem],
                backgroundColor: [
                    'rgba(54, 162, 235, 0.8)', // Blue
                    'rgba(255, 99, 132, 0.8)',  // Red
                    'rgba(238, 237, 124, 0.7)'  // Red
                ],
                borderColor: [
                    'rgba(54, 162, 235, 1)',
                    'rgba(255, 99, 132, 1)',
                    'rgba(238, 237, 124, 1)'  // Red
                ],
                borderWidth: 2,
                hoverOffset: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        usePointStyle: true,
                        padding: 15
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const value = context.raw;
                            const data = context.dataset.data;
                            const total = data.reduce((sum, val) => sum + val, 0);
                            const percentage = total ? (value / total) * 100 : 0;
                            return `${context.label}: ${formatBytes(value)} (${percentage.toFixed(1)}%)`;
                        }
                    }
                }
            },
            animation: {
                animateScale: true,
                animateRotate: true
            }
        }
    });
    return pieChartInstance;
}

export function formatBytes(bytes) {
  if (bytes === 0) return "0 B";

  const units = ["B", "KB", "MB", "GB", "TB"];
  const i = Math.floor(Math.log(bytes) / Math.log(1024));
  const value = bytes / Math.pow(1024, i);

  return `${value.toFixed(1)} ${units[i]}`;
}

export const loadDatabaseStatus = async () => {
    const result = await powerApi.dbStatus();
    return result;
}