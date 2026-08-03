/**
 * Contract for project-related API operations.
 * @typedef {Object} IProjectService
 * @property {(filter?: import('../models/project.js').ProjectFilter) => Promise<import('../models/project.js').Project[]>} getAll
 * @property {(id: number|string) => Promise<import('../models/project.js').Project>} getById
 * @property {(data: import('../models/project.js').ProjectRequest) => Promise<import('../models/project.js').Project>} create
 * @property {(id: number, data: import('../models/project.js').ProjectRequest) => Promise<import('../models/project.js').Project>} update
 * @property {(id: number|string) => Promise<void>} delete
 * @property {(id: number|string, employeeIds: number[]) => Promise<void>} addEmployees
 * @property {(id: number|string, employeeId: number|string) => Promise<void>} removeEmployee
 */

/** @type {IProjectService} */
export const IProjectService = {
  getAll: async () => {
    throw new Error('getAll is not implemented');
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
  addEmployees: async () => {
    throw new Error('addEmployees is not implemented');
  },
  removeEmployee: async () => {
    throw new Error('removeEmployee is not implemented');
  },
};
