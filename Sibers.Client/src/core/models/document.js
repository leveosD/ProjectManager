/**
 * @typedef {Object} ProjectDocument
 * @property {number} id
 * @property {string} fileName
 * @property {string} storedFileName
 * @property {string} contentType
 * @property {number} fileSize
 * @property {string} uploadedAt
 * @property {number} projectId
 */

export const emptyProjectDocument = Object.freeze({
  id: 0,
  fileName: '',
  storedFileName: '',
  contentType: '',
  fileSize: 0,
  uploadedAt: '',
  projectId: 0,
});
