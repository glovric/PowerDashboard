import { createApp } from 'vue'
import App from './App.vue'
import router from "./router"
import { useAuth } from './composables/useAuth'
import { refreshAccessTokenStartup } from './api/refreshService.js'
import '@/styles/main.scss';

const { fetchUser } = useAuth();

(async () => {

  await refreshAccessTokenStartup();
  await fetchUser();

  createApp(App)
    .use(router)
    .mount('#app');

})();