import { ref } from 'vue'

const loading = ref(false);

export function useLoadingOverlay() {

    const showLoadingOverlay = (newValue) => {

        if(newValue == true || newValue == false) {
            loading.value = newValue;
        }

    }

    return { loading, showLoadingOverlay };

}