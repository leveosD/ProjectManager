import { request } from '../httpClient.js';

/** @type {import('../../core/services/authService.js').IAuthService} */
export const authService = {
  login: (email, password) =>
    request('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    }),

  logout: () =>
    request('/auth/logout', {
      method: 'POST',
    }),
};
