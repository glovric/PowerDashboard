import { createClient } from "./apiClient";

const api = createClient('/auth');

export const authApi = {
  login: (username, password) => api.post('/login', { userName: username, password: password }),
  logout: () => api.get('/logout'),
  register: (username, email, password) => api.post('/register', { userName: username, email: email, password: password }),
  getUser: () => api.get('/getuser'),
  health: () => api.get('/health')
};