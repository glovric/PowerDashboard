import { createClient } from "./apiClient";

const api = createClient('/power');

export const powerApi = {
  latest: (payload) => api.post('/powerdata/front/latest', payload),
  history: (payload) => api.post('/powerdata/front/history', payload),
  forecast: (payload) => api.post('/powerdata/front/forecast', payload),
  dbStatus: () => api.get('/powerdata/front/db_status'),
  transmissionStatus: (payload) => api.post('/powerdata/front/transmission_status', payload),
  tableExport: (payload) => api.post('/powerdata/front/export', payload, { responseType: 'blob' }),
  health: () => api.get('/powerdata/front/health')
};