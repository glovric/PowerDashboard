import { ref } from 'vue'

const chartViewMode = ref('tile'); // charts orientation, tile (4x4 grid) or list (charts stacked in one column)
const tabularView = ref(false); // whether dashboard shows charts or table

export function useDashboardViewMode() {

    const setChartViewMode = (mode) => {
        if (mode === 'tile' || mode === 'list') chartViewMode.value = mode
    }

    const toggleTabularView = () => {
        tabularView.value = !tabularView.value;
    }

    return { chartViewMode, tabularView, setChartViewMode, toggleTabularView }
}