import axios from 'axios';
import { formatApiError } from './utils';

// Mainly used for refresh tokens
const refreshClient = axios.create({
  baseURL: "/api/auth",
  withCredentials: true
});

export async function refreshAccessTokenInterceptor() {
  // No error handling so that axios interceptor catches errors
  await refreshClient.get("/refresh");
}

export async function refreshAccessTokenStartup() {
  // Error handling so that app doesnt crash on startup
  try {
    await refreshClient.get("/refresh");
    return { error: null, success: true };
  } catch (err) {
    return { error: formatApiError(err), success: false };
  }
}
