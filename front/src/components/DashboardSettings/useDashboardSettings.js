import { ref, onMounted, onUnmounted } from 'vue'

export function useDashboardSettings() {

    const isOpen = ref(false);
    const wrapperRef = ref(null);

    const toggleMenu = () => {
        isOpen.value = !isOpen.value
    }

    const closeMenu = () => {
        isOpen.value = false
    }

    // Close when clicking outside
    const handleClickOutside = (event) => {
        if (wrapperRef.value && !wrapperRef.value.contains(event.target)) {
            closeMenu()
        }
    }

    onMounted(() => {
        document.addEventListener('click', handleClickOutside)
    })

    onUnmounted(() => {
        document.removeEventListener('click', handleClickOutside)
    })

    return { isOpen, wrapperRef,
             toggleMenu }
}