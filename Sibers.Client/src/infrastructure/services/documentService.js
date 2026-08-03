import { request, upload, buildUrl } from '../httpClient.js';

/** @type {import('../../core/services/documentService.js').IDocumentService} */
export const documentService = {
  getProjectDocuments: (projectId) => request(`/documents/project/${projectId}`),

  upload: (projectId, file) => {
    const formData = new FormData();
    formData.append('file', file);
    return upload(`/documents/upload/${projectId}`, formData);
  },

  downloadUrl: (id) => buildUrl(`/documents/download/${id}`),

  delete: (id) =>
    request(`/documents/${id}`, {
      method: 'DELETE',
    }),
};
