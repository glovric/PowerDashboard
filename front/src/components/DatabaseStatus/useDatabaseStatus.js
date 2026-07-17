import { ref, computed, onMounted, onBeforeUnmount } from 'vue';
import { createPieChart, loadDatabaseStatus, formatBytes } from "./databaseStatusUtils"
import { useErrorToast } from "@/components/ErrorToast/useErrorToast.js";
import { useLoadingOverlay } from "@/components/LoadingOverlay/useLoadingOverlay.js";

export function useDatabaseStatus() {

    const { showError } = useErrorToast();
    const { showLoadingOverlay } = useLoadingOverlay();

    const isOnline = ref(false);

    const lastUpdateTimeHour = ref(null);
    const lastUpdateTimeQuarter = ref(null);

    const totalRecordsHour = ref(0);
    const totalRecordsQuarter = ref(0);

    const sizeHour = ref(0);
    const sizeQuarter = ref(0);
    const sizeDatabase = ref(0); 

    const isLoading = ref(false);
    const lastChecked = ref(null);

    const pieChartRef = ref(null);
    let pieChartInstance = null;

    const statusLabel = computed(() => {
        return isOnline.value ? 'Online' : 'Offline';
    });

    const statusColorClass = computed(() => {
        return isOnline.value ? 'status-online' : 'status-offline';
    });

    const sizeDatabaseFormatted = computed(() => {
        return formatBytes(sizeDatabase.value);
    })

    const fetchData = async () => {
        const res = await loadDatabaseStatus();
        if(res && res.success) {

            const data = res.data;

            isOnline.value = data.isOnline || false;

            lastUpdateTimeHour.value = data.lastDataTimeHour || null;
            lastUpdateTimeQuarter.value = data.lastDataTimeQuarter || null;

            totalRecordsHour.value = data.totalRecordsHour || 0;
            totalRecordsQuarter.value = data.totalRecordsQuarter || 0;

            sizeHour.value = data.sizeHour || null;
            sizeQuarter.value = data.sizeQuarter || null;
            sizeDatabase.value = data.sizeDatabase || null;
        }
        else {
            isOnline.value = false;
            lastUpdateTimeHour.value = null;
            lastUpdateTimeQuarter.value = null;
            totalRecordsHour.value = 0;
            totalRecordsQuarter.value = 0;
            sizeHour.value = null;
            sizeQuarter.value = null;
            sizeDatabase.value = null;

            let errorMessage = res.error.message;
            let errorDetail = res.error.status + ", " + res.error.statusText;
            showError(errorMessage, errorDetail);
        }

        lastChecked.value = new Date().toLocaleTimeString('en-GB', {
            hour: '2-digit',
            minute: '2-digit',
            hour12: false
        });
    }

    const refreshData = async () => {
        isLoading.value = true;
        await fetchData();
        isLoading.value = false;
    };

    onMounted(async () => {
        showLoadingOverlay(true);
        await fetchData();
        const sizeSystem = sizeDatabase.value - sizeHour.value - sizeQuarter.value
        pieChartInstance = createPieChart(pieChartRef, sizeHour.value, sizeQuarter.value, sizeSystem);
        showLoadingOverlay(false)
    });

    onBeforeUnmount(() => {
        pieChartInstance?.destroy();
    });

    return { statusLabel, statusColorClass,
             totalRecordsHour, totalRecordsQuarter,
             lastUpdateTimeHour, lastUpdateTimeQuarter,
             sizeHour, sizeQuarter, sizeDatabaseFormatted,
             isLoading, lastChecked, pieChartRef, refreshData }

}