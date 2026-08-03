const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:7015/api';

const buildHeaders = (options) => {
  const hasBody = options.body !== undefined && !(options.body instanceof FormData);
  return {
    ...(hasBody && { 'Content-Type': 'application/json' }),
    ...options.headers,
  };
};

/**
 * Sends a JSON request to the backend API.
 * @param {string} url - Relative API URL.
 * @param {RequestInit} [options] - Fetch options.
 * @returns {Promise<any>}
 */
export const request = async (url, options = {}) => {
  const response = await fetch(`${API_BASE_URL}${url}`, {
    ...options,
    credentials: 'include',
    headers: buildHeaders(options),
  });

  if (response.status === 204) {
    return null;
  }

  const data = await response.json().catch(() => null);

  if (!response.ok) {
    throw new Error(data?.message || `HTTP error! status: ${response.status}`);
  }

  return data;
};

/**
 * Sends a multipart/form-data upload request.
 * @param {string} url - Relative API URL.
 * @param {FormData} formData
 * @returns {Promise<any>}
 */
export const upload = async (url, formData) => {
  const response = await fetch(`${API_BASE_URL}${url}`, {
    method: 'POST',
    credentials: 'include',
    body: formData,
  });

  const data = await response.json().catch(() => null);

  if (!response.ok) {
    throw new Error(data?.message || `HTTP error! status: ${response.status}`);
  }

  return data;
};

/**
 * Builds a fully qualified download URL for a backend resource.
 * @param {string} url - Relative API URL.
 * @returns {string}
 */
export const buildUrl = (url) => `${API_BASE_URL}${url}`;

/**
 * Builds a query string from a plain object, skipping empty values.
 * @param {Record<string, unknown>} filter
 * @returns {string}
 */
export const buildQuery = (filter = {}) => {
  const params = new URLSearchParams();
  Object.entries(filter).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      params.append(key, String(value));
    }
  });
  const query = params.toString();
  return query ? `?${query}` : '';
};
