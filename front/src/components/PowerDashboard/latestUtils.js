import { powerApi, inferenceApi } from '@/api';

export const loadLatestData = async (countryCode, lastHours, resolution) => {
    const payload = {
      country: countryCode,
      interval: resolution,
      count: lastHours
    }
    const result = await powerApi.latest(payload);
    return result;
};

export const loadNowcastData = async (countryCode, lastHours, resolution) => {
    const payload = {
      "country": countryCode,
      "count": lastHours,
      "interval": resolution
    }
    const result = await inferenceApi.latest(payload);
    return result
};