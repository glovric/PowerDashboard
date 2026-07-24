import { powerApi } from '@/api';
import { countries } from '@/utils/dashboardUtils'; 

export const datePickerConfig = {
    dateFormat: 'Y-m-d',
    minDate: '2015-01-01',
    maxDate: '2020-09-30'
}

export const loadTransmissionData = async (resolution, date) => {
    const payload = {
      interval: resolution,
      date: date
    }
    const result = await powerApi.transmissionStatus(payload);
    return result;
}

export const transformTransmissionData = (response) => {

    const transformedData = {};

    for (const [countryCode, values] of Object.entries(response)) {
        // Find readable country name
        const countryObj = countries.find(c => c.value === countryCode);
        const countryName = countryObj ? countryObj.label : countryCode;
        // Extract load values
        const loadValues = values.map(x => x.loadValue);
        // Add to result object
        transformedData[countryName] = loadValues;
    }

    // Sort country keys alphabetically
    const sortedCountryKeys = Object.keys(transformedData).sort((countryA, countryB) => countryA.localeCompare(countryB));
    const sortedData = Object.fromEntries(
        sortedCountryKeys.map(key => [key, transformedData[key]])
    );

    return sortedData;
  };

export const getStatusClass = (load) => {
    if (load === null || load === undefined) return 'unavailable';
    return 'available';
};

export const formatTime = (timestampIndex, interval) => {
    if(interval == 15) {
        let hourRemainder = timestampIndex % 4;
        let minutes = 15 * hourRemainder;
        let hour = Math.floor(timestampIndex / 4);
        return `${hour.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
    }
    return `${timestampIndex.toString().padStart(2, '0')}:00`;
};