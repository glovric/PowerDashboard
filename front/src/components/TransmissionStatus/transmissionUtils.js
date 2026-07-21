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
    enableTime: false,
    dateFormat: 'Y-m-d',
    minDate: '2015-01-01',
    time_24hr: true,
    minuteIncrement: 15
}