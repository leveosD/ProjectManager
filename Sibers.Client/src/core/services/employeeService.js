/**
 * Contract for employee-related API operations.
 * @typedef {Object} IEmployeeService
 * @property {(roles?: string[]) => Promise<import('../models/employee.js').Employee[]>} getAll
 * @property {(query: string, roles?: string[]) => Promise<import('../models/employee.js').Employee[]>} search
 * @property {(id: number) => Promise<import('../models/employee.js').Employee>} getById
 * @property {(data: import('../models/employee.js').CreateEmployeeRequest) => Promise<import('../models/employee.js').Employee>} create
 * @property {(id: number, data: import('../models/employee.js').UpdateEmployeeRequest) => Promise<void>} update
 * @property {(id: number) => Promise<void>} delete
 */

/** @type {IEmployeeService} */
export const IEmployeeService = {
  getAll: async () => {
    throw new Error('getAll is not implemented');
  },
  search: async () => {
    throw new Error('search is not implemented');
  },
  getById: async () => {
    throw new Error('getById is not implemented');
  },
  create: async () => {
    throw new Error('create is not implemented');
  },
  update: async () => {
    throw new Error('update is not implemented');
  },
  delete: async () => {
    throw new Error('delete is not implemented');
  },
};
