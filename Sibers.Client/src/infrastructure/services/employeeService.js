import { request } from '../httpClient.js';

/** @type {import('../../core/services/employeeService.js').IEmployeeService} */
export const employeeService = {
  getAll: (roles = []) => {
    const params = new URLSearchParams();
    roles.forEach((role) => params.append('role', role));
    const query = params.toString();
    return request(`/employees${query ? `?${query}` : ''}`);
  },

  search: (query, roles = []) => {
    const params = new URLSearchParams();
    params.append('q', query || '');
    roles.forEach((role) => params.append('role', role));
    return request(`/employees/search?${params.toString()}`);
  },

  getById: (id) => request(`/employees/${id}`),

  create: (data) =>
    request('/employees', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  update: (id, data) =>
    request(`/employees/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  delete: (id) =>
    request(`/employees/${id}`, {
      method: 'DELETE',
    }),
};
