import { ref, toRaw } from 'vue';
import { exportTableData, downloadData } from './tableUtils';
import { useLoadingToast } from '../LoadingToast/useLoadingToast';

export function useDashboardTable(tabularData) {

    const { showLoadingToast } = useLoadingToast();

    const format = ref('csv');

    const handleExport = async () => {
        showLoadingToast(true, "Exporting to " + format.value.toUpperCase());
        const data = await exportTableData(toRaw(tabularData.value), format.value);
        downloadData(data.data, format.value);
        setTimeout(() => {
            showLoadingToast(false);
        }, 250);
    }

    return { format, handleExport }
}