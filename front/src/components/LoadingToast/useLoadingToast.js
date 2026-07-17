import { ref } from 'vue'

const loadingToastMessage = ref(null);
const loadingToastVisible = ref(false);

export function useLoadingToast() {

    const showLoadingToast = (visible=false, msg="") => {
        loadingToastMessage.value = msg;
        loadingToastVisible.value = visible;
    }

  return {
    loadingToastMessage,
    loadingToastVisible,
    showLoadingToast,
  }
}
