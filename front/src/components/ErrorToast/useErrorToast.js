import { ref } from 'vue'

const errorMessage = ref(null);
const errorDetail = ref(null);
const errorVisible = ref(false);
let errorTimer = null;

export function useErrorToast() {

    const showError = (msg, detail, duration = 3000) => {
        errorMessage.value = msg;
        errorDetail.value = detail;
        errorVisible.value = true;

        clearTimeout(errorTimer);
        errorTimer = setTimeout(() => {
            errorVisible.value = false
        }, duration);
    }

  return {
    errorMessage,
    errorDetail,
    errorVisible,
    showError,
  }
}
