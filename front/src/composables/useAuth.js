import { ref } from 'vue';
import { authApi } from '@/api';

const user = ref(null);
const isLoggedIn = ref(false);

export function useAuth() {

  const login = async (username, password) => {
    const result = await authApi.login(username, password);
    if(result.success) {
      isLoggedIn.value = true;
    }
    return result;
  }

  const logout = async () => {
    const result = await authApi.logout();
    if(result.success) {
      user.value = null;
      isLoggedIn.value = false;
    }
    return result;
  };

  const register = async (username, email, password) => {
    const result = await authApi.register(username, email, password);
    return result;
  }

  const fetchUser = async () => {
    try {
      const result = await authApi.getUser();
      if(result.success && result.data) {
        user.value = {
          username: result.data.username,
          email: result.data.email,
          roles: result.data.roles?.join(", ") || "None"
        };
        isLoggedIn.value = true;
      }
    } 
    catch (error) {
      user.value = null;
      isLoggedIn.value = false;
    }
  };

  return { login, logout, register, fetchUser, user, isLoggedIn };
}

export { user };