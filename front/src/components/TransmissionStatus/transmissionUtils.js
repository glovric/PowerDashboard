import { powerApi } from '@/api';

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