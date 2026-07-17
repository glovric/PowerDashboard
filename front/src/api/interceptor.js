import { refreshAccessTokenInterceptor } from './refreshService';

let refreshPromise = null;

async function getRefreshPromise() {
  if (!refreshPromise) {
    refreshPromise = refreshAccessTokenInterceptor()
      .finally(() => {
        refreshPromise = null;
      });
  }

  return refreshPromise;
}

export const attachAuthInterceptor = (client) => {
  client.interceptors.response.use(
    res => res,
    async (err) => {
      const original = err.config;
      const fullUrl = `${original.baseURL || ''}${original.url || ''}`;

      if (
        !err.response ||
        err.response.status !== 401 || // Exclude refreshing non 401 responses
        original._retry ||
        fullUrl.includes('/auth') // Exclude refreshing auth endpoint calls
      ) {
        return Promise.reject(err);
      }

      original._retry = true;

      try {
        await getRefreshPromise();
        return client(original);
      } catch (e) {
        return Promise.reject(e);
      }
    }
  );
};