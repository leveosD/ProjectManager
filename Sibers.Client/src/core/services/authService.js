/**
 * Contract for authentication-related API operations.
 * @typedef {Object} IAuthService
 * @property {(email: string, password: string) => Promise<import('../models/auth.js').AuthResult>} login
 * @property {() => Promise<void>} logout
 */

/** @type {IAuthService} */
export const IAuthService = {
  login: async () => {
    throw new Error('login is not implemented');
  },
  logout: async () => {
    throw new Error('logout is not implemented');
  },
};
