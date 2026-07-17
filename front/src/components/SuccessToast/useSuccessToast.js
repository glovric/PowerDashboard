import { ref } from 'vue'

const successMessage = ref(null);
const successVisible = ref(false);
let successTimer = null;

export function useSuccessToast() {

    const showSuccess = (msg, duration = 3000) => {
        successMessage.value = msg;
        successVisible.value = true;

        clearTimeout(successTimer);
        successTimer = setTimeout(() => {
            successVisible.value = false
        }, duration);
    }

  return {
    successMessage,
    successVisible,
    showSuccess,
  }
}
