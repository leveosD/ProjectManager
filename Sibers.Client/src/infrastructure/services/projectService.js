import { request, buildQuery } from '../httpClient.js';

/** @type {import('../../core/services/projectService.js').IProjectService} */
export const projectService = {
  getAll: (filter = {}) => request(`/projects${buildQuery(filter)}`),

  getById: (id) => request(`/projects/${id}`),

  create: (data) =>
    request('/projects', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  update: (id, data) =>
    request(`/projects/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  delete: (id) =>
    request(`/projects/${id}`, {
      method: 'DELETE',
    }),

  addEmployees: (id, employeeIds) =>
    request(`/projects/${id}/employees`, {
      method: 'POST',
      body: JSON.stringify(employeeIds),
    }),

  removeEmployee: (id, employeeId) =>
    request(`/projects/${id}/employees/${employeeId}`, {
      method: 'DELETE',
    }),
};
