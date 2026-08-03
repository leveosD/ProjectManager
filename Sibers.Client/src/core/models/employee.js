/**
 * @typedef {Object} Employee
 * @property {number} id
 * @property {string} firstName
 * @property {string} lastName
 * @property {string} [middleName]
 * @property {string} email
 * @property {string} fullName
 * @property {string} [userId]
 * @property {string} [role]
 */

/**
 * @typedef {Object} CreateEmployeeRequest
 * @property {string} firstName
 * @property {string} lastName
 * @property {string} [middleName]
 * @property {string} email
 * @property {string} password
 * @property {string} role
 */

/**
 * @typedef {Object} UpdateEmployeeRequest
 * @property {string} firstName
 * @property {string} lastName
 * @property {string} [middleName]
 * @property {string} email
 * @property {string} [role]
 */

export const emptyEmployee = Object.freeze({
  id: 0,
  firstName: '',
  lastName: '',
  middleName: null,
  email: '',
  fullName: '',
  userId: null,
  role: null,
});
