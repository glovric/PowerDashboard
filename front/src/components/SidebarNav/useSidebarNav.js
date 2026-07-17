import { ref } from 'vue'
import { useRouter } from "vue-router";
import { useAuth } from '@/composables/useAuth';
import { useSuccessToast } from '@/components/SuccessToast/useSuccessToast';

export function useSidebarNav() {

    const sidebarNavCollapsed = ref(true);
    const router = useRouter();
    const { logout } = useAuth();
    const { showSuccess } = useSuccessToast();

    const toggleSidebarNav = () => {
        sidebarNavCollapsed.value = !sidebarNavCollapsed.value;
    }

    const handleLogout = async () => {
      const res = await logout();
      if(res.success) {
        showSuccess("Successfully logged out");
        router.push("/login");
        return
      }
    }

    return { sidebarNavCollapsed, toggleSidebarNav, handleLogout };

}