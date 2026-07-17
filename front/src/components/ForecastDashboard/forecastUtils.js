import { powerApi, inferenceApi } from '@/api';
import { round, isValidArray } from "@/utils/dashboardUtils"

export const loadObservedData = async (countryCode, resolution, horizon, forecastDate) => {
    const payload = {
      country: countryCode,
      interval: resolution,
      horizon: horizon,
      forecastDate: forecastDate
    }
    const result = await powerApi.forecast(payload);
    return result;
}

export const loadForecastedData = async (countryCode, resolution, horizon, forecastDate) => {
    const payload = {
      country: countryCode,
      interval: resolution,
      horizon: horizon,
      forecast_date: forecastDate
    }
    const result = await inferenceApi.forecast(payload);
    return result;
}

export const alignLoadData = (trueLoad, forecastLoad) => {

  if(!isValidArray(trueLoad) || !isValidArray(forecastLoad)) {
    return {};
  }

  const trueCount = trueLoad.length;
  
  const trueLoadAligned = [
    ...trueLoad, 
    forecastLoad[0]
  ];

  const forecastLoadAligned = [
    ...Array(trueCount).fill(null), 
    ...forecastLoad
  ];

  return { trueLoadAligned, forecastLoadAligned };
};

export const alignRampData = (trueLoad, forecastLoad) => {

  if(!isValidArray(trueLoad) || !isValidArray(forecastLoad)) {
    return {};
  }

  const totalRamp = calculateRamp(trueLoad, forecastLoad);
  
  const trueCount = trueLoad.length;
  const trueRamp = [
    ...totalRamp.slice(0, trueCount),
  ];

  const forecastRampAligned = [
    ...Array(trueCount).fill(null),
    ...totalRamp.slice(trueCount)
  ];

  const firstNullIndex = forecastRampAligned.lastIndexOf(null);
  if (firstNullIndex !== -1) {
    forecastRampAligned[firstNullIndex] = trueRamp.at(-1);
  }

  return { trueRamp, forecastRampAligned };
};

export const calculateRamp = (trueLoad, forecastLoad) => {
  const totalLoad = [...trueLoad, ...forecastLoad]
  const ramp = [];
  for(let i = 1; i < totalLoad.length; i++) {
    ramp.push(totalLoad[i] - totalLoad[i-1]);
  }
  return ramp;
}

export function createTabularData({headers, columnKeys, labels, trueLoad, forecastLoad, ramp, consumption} = {}) {

  const rows = labels.map((label, i) => ({
    label,
    load: round(trueLoad[i]),
    predicted: round(forecastLoad[i]),
    ramp: round(ramp[i]),
    consumption: round(consumption[i])
  }));

  const data = {
    headers, // Used to create table headers <th>
    columnKeys, // Keys used to extract values from rows object
    rows
  }

  return data;
}