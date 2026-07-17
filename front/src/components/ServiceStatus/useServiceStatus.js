import { ref, onMounted } from 'vue';
import { loadServiceStatus } from './serviceStatusUtils';
import { useLoadingOverlay } from "@/components/LoadingOverlay/useLoadingOverlay.js";

export function useServiceStatus() {

    const { showLoadingOverlay } = useLoadingOverlay();

    const serviceStatus = ref({
        power: false,
        auth: false,
        inference: false
    });

    const isLoading = ref(false);
    const lastChecked = ref(null);

    const getStatusLabel = (status) => status ? 'Online' : 'Offline';
    const getStatusClass = (status) => status ? 'status-online' : 'status-offline';

    const fetchData = async () => {
        const res = await loadServiceStatus();

        if(res) {
            serviceStatus.value = res;
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
        setTimeout(() => {
            showLoadingOverlay(false);
        }, 250);
    });

    return { isLoading, lastChecked, refreshData, getStatusLabel, getStatusClass, serviceStatus }

}