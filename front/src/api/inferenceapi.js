import { createClient } from "./apiClient";

const api = createClient('/inference');

export const inferenceApi = {
    latest(payload) {
        return api.post('/nowcast/latest', payload)
    },
    history(payload) {
        return api.post('/nowcast/history', payload);
    },
    forecast(payload) {
        return api.post('/forecast', payload);
    },
    health() {
        return api.get('/health');
    }
};