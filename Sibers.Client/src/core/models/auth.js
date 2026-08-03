/**
 * @typedef {Object} LoginRequest
 * @property {string} email
 * @property {string} password
 */

/**
 * @typedef {Object} AuthResult
 * @property {string} token
 * @property {string} email
 * @property {string} role
 * @property {number} employeeId
 * @property {string} fullName
 * @property {string} expiresAt
 */

/**
 * @typedef {Object} UserInfo
 * @property {string} email
 * @property {string} role
 * @property {number} employeeId
 * @property {string} fullName
 * @property {string} expiresAt
 */

export const emptyAuthResult = Object.freeze({
  token: '',
  email: '',
  role: '',
  employeeId: 0,
  fullName: '',
  expiresAt: '',
});
