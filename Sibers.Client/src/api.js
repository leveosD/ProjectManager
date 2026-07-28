const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:7015/api';

const request = async (url, options = {}) => {
  const hasBody = options.body !== undefined;
  const headers = {
    ...(hasBody && { 'Content-Type': 'application/json' }),
    ...options.headers,
  };

  const response = await fetch(`${API_BASE_URL}${url}`, {
    ...options,
    credentials: 'include',
    headers,
  });

  if (response.status === 204) {
    return null;
  }

  const data = await response.json().catch(() => null);

  if (!response.ok) {
    throw new Error(data?.message || `HTTP error! status: ${response.status}`);
  }

  return data;
};

export const authApi = {
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

export const employeesApi = {
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

export const projectsApi = {
  getAll: (filter = {}) => {
    const params = new URLSearchParams();
    Object.entries(filter).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params.append(key, value);
      }
    });
    const query = params.toString();
    return request(`/projects${query ? `?${query}` : ''}`);
  },
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

export const tasksApi = {
  getAll: (filter = {}) => {
    const params = new URLSearchParams();
    Object.entries(filter).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params.append(key, value);
      }
    });
    const query = params.toString();
    return request(`/tasks${query ? `?${query}` : ''}`);
  },
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

export const documentsApi = {
  getProjectDocuments: (projectId) => request(`/documents/project/${projectId}`),
  upload: (projectId, file) => {
    const formData = new FormData();
    formData.append('file', file);

    return fetch(`${API_BASE_URL}/documents/upload/${projectId}`, {
      method: 'POST',
      credentials: 'include',
      body: formData,
    }).then(async (response) => {
      const data = await response.json().catch(() => null);
      if (!response.ok) {
        throw new Error(data?.message || `HTTP error! status: ${response.status}`);
      }
      return data;
    });
  },
  downloadUrl: (id) => `${API_BASE_URL}/documents/download/${id}`,
  delete: (id) =>
    request(`/documents/${id}`, {
      method: 'DELETE',
    }),
};
