import axios from 'axios';
import { request } from './utils';
import { attachAuthInterceptor } from './interceptor';

export const createClient = (url) => {
  const client = axios.create({
    baseURL: '/api' + url,
    withCredentials: true
  });

  attachAuthInterceptor(client);

  return {
    get: (url, config) => request(() => client.get(url, config)),
    post: (url, data, config) => request(() => client.post(url, data, config)),
  };
};