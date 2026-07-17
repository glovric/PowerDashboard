import { powerApi, inferenceApi } from '@/api';

export const loadHistoricData = async (countryCode, resolution, startDate, endDate) => {
    const payload = {
      country: countryCode,
      interval: resolution,
      startDate: startDate,
      endDate: endDate
    }
    const result = await powerApi.history(payload);
    return result;
};

export const loadNowcastData = async (countryCode, resolution, startDate, endDate) => {
    const payload = {
      "country": countryCode,
      "interval": resolution,
      "start_date": startDate,
      "end_date": endDate
    }
    const result = await inferenceApi.history(payload);
    return result;
};

export const isValidDateRange = (startDate, endDate) => {
  let startDateObj = new Date(startDate);
  let endDateObj = new Date(endDate);
  return startDateObj < endDateObj;
}