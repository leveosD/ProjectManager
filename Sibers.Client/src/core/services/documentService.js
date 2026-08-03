/**
 * Contract for document-related API operations.
 * @typedef {Object} IDocumentService
 * @property {(projectId: number|string) => Promise<import('../models/document.js').ProjectDocument[]>} getProjectDocuments
 * @property {(projectId: number|string, file: File) => Promise<import('../models/document.js').ProjectDocument>} upload
 * @property {(id: number|string) => string} downloadUrl
 * @property {(id: number|string) => Promise<void>} delete
 */

/** @type {IDocumentService} */
export const IDocumentService = {
  getProjectDocuments: async () => {
    throw new Error('getProjectDocuments is not implemented');
  },
  upload: async () => {
    throw new Error('upload is not implemented');
  },
  downloadUrl: () => {
    throw new Error('downloadUrl is not implemented');
  },
  delete: async () => {
    throw new Error('delete is not implemented');
  },
};
