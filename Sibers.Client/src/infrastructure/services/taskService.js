import { request, buildQuery } from '../httpClient.js';

/** @type {import('../../core/services/taskService.js').ITaskService} */
export const taskService = {
  getAll: (filter = {}) => request(`/tasks${buildQuery(filter)}`),

  getById: (id) => request(`/tasks/${id}`),

  create: (data) =>
    request('/tasks', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  update: (id, data) =>
    request(`/tasks/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  updateStatus: (id, status) =>
    request(`/tasks/${id}/status`, {
      method: 'PATCH',
      body: JSON.stringify(status),
    }),

  assignExecutor: (id, executorId) =>
    request(`/tasks/${id}/executor`, {
      method: 'PATCH',
      body: JSON.stringify(executorId),
    }),

  delete: (id) =>
    request(`/tasks/${id}`, {
      method: 'DELETE',
    }),
};
