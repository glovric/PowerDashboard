import { powerApi } from '@/api';
import { countries } from '@/utils/dashboardUtils'; 


export const loadTransmissionData = async (resolution, date) => {
    const payload = {
      interval: resolution,
      date: date
    }
    const result = await powerApi.transmissionStatus(payload);
    return result;
}

export const datePickerConfig = {
    dateFormat: 'Y-m-d',
    minDate: '2015-01-01',
    maxDate: '2020-09-30'
}

export const generateMeasurements = (response) => {
    return Object.fromEntries(
        Object.entries(response)
            .map(([country, values]) => [
                countries.find(c => c.value === country)?.label ?? country,
                values.map(x => x.loadValue)
            ])
            .sort(([countryA], [countryB]) => countryA.localeCompare(countryB))
    );
  };

export const getStatusClass = (load) => {
    if (load === null || load === undefined) return 'unavailable';
    return 'available';
};

export const formatTime = (hour, interval) => {
    if(interval == 15) {
        let remaining = hour % 4;
        let minutes = 15 * remaining;
        let newHour = Math.floor(hour / 4);
        return `${newHour.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
    }
    return `${hour.toString().padStart(2, '0')}:00`;
  };