function createErrorMessage(status) {
    switch (status) {
      case 401:
        return "Your session expired. Please log in.";
      case 403:
        return "You don't have permission to access this content.";
      case 429:
        return "Too many requests, try again later."
      case 500:
        return "Server error.";
      default:
        return "Unknown error.";
  }
}

export function formatApiError(error) {
  const msgCondition = typeof error.response?.data === 'string' 
                       &&  error.response?.data != ""
                       && error.response?.data.length < 100;
  const msg = msgCondition ? error.response?.data : createErrorMessage(error.response?.status);
  return {
    message: msg,
    status: error.response?.status || null,
    statusText: error.response?.statusText || null,
    timestamp: new Date().toISOString(),
  };
}

export const request = async (apiCall) => {
    try {
      const response = await apiCall();
      return { data: response.data, error: null, success: true };
    } 
    catch (error) {
      return { data: null, error: formatApiError(error), success: false };
    }
};
