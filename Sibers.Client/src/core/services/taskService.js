/**
 * Contract for project-task-related API operations.
 * @typedef {Object} ITaskService
 * @property {(filter?: import('../models/task.js').ProjectTaskFilter) => Promise<import('../models/task.js').ProjectTask[]>} getAll
 * @property {(id: number) => Promise<import('../models/task.js').ProjectTask>} getById
 * @property {(data: import('../models/task.js').CreateProjectTaskRequest) => Promise<import('../models/task.js').ProjectTask>} create
 * @property {(id: number, data: import('../models/task.js').UpdateProjectTaskRequest) => Promise<void>} update
 * @property {(id: number, status: number) => Promise<void>} updateStatus
 * @property {(id: number, executorId: number) => Promise<void>} assignExecutor
 * @property {(id: number) => Promise<void>} delete
 */

/** @type {ITaskService} */
export const ITaskService = {
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
  updateStatus: async () => {
    throw new Error('updateStatus is not implemented');
  },
  assignExecutor: async () => {
    throw new Error('assignExecutor is not implemented');
  },
  delete: async () => {
    throw new Error('delete is not implemented');
  },
};
