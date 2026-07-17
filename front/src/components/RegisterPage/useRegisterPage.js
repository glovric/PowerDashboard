import { ref } from 'vue'
import { useAuth } from '@/composables/useAuth';
import { useSuccessToast } from '@/components/SuccessToast/useSuccessToast';

export function useRegisterPage() {

  const username = ref('');
  const email = ref('');
  const password = ref('');
  const showPassword = ref(false);

  const error = ref('');
  const message = ref('');

  const { register } = useAuth();
  const { showSuccess } = useSuccessToast();

  const togglePasswordVisibility = () => showPassword.value = !showPassword.value;

  const validate = () => {
    if (!username.value || !email.value || !password.value) {
      error.value = 'All fields are required'
      return false
    }
    if (!email.value.includes('@')) {
      error.value = 'Invalid email'
      return false
    }
    if (password.value.length < 4) {
      error.value = 'Password must be at least 4 characters'
      return false
    }
    error.value = '';
    return true
  }

  const handleRegister = async () => {
    if (!validate()) return

    error.value = '';
    message.value = '';

    const res = await register(username.value, email.value, password.value);

    username.value = ''
    email.value = ''
    password.value = ''

    if(res.success) {
      showSuccess("Successfully registered.", 5000);
      message.value = "Admin needs to approve your account before you can log in."
      return;
    }

    error.value = res.error.message;

  }

  return { username, email, password, showPassword, message, error, togglePasswordVisibility, handleRegister }
}
