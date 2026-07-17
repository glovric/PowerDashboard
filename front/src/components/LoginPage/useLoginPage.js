import { ref } from "vue";
import { useRouter } from "vue-router";
import { useAuth } from "@/composables/useAuth.js";
import { useSuccessToast } from '@/components/SuccessToast/useSuccessToast';

export function useLoginPage() {

    const userName = ref("");
    const password = ref("");
    const showPassword = ref(false);
    const error = ref(null);
    const router = useRouter();
    const { login, fetchUser } = useAuth();
    const { showSuccess } = useSuccessToast();
    
    const togglePasswordVisibility = () => showPassword.value = !showPassword.value;

    const handleLogin = async () => {
      error.value = null;
      const res = await login(userName.value, password.value);
      if(res.success) {
        await fetchUser();
        showSuccess("Successfully logged in.", 3000);
        router.push("/map");
        return;
      }
      error.value = res.error.message;
    };

    return { userName, password, showPassword, error, handleLogin, togglePasswordVisibility };

}