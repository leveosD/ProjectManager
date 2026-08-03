/**
 * @typedef {Object} ProjectTask
 * @property {number} id
 * @property {string} title
 * @property {string} [comment]
 * @property {number} priority
 * @property {number} status
 * @property {number} projectId
 * @property {string} projectName
 * @property {number} authorId
 * @property {string} authorName
 * @property {number} [executorId]
 * @property {string} [executorName]
 */

/**
 * @typedef {Object} CreateProjectTaskRequest
 * @property {string} title
 * @property {string} [comment]
 * @property {number} priority
 * @property {number} status
 * @property {number} projectId
 * @property {number} authorId
 * @property {number} [executorId]
 */

/**
 * @typedef {Object} UpdateProjectTaskRequest
 * @property {string} title
 * @property {string} [comment]
 * @property {number} priority
 * @property {number} status
 * @property {number} [executorId]
 */

/**
 * @typedef {Object} ProjectTaskFilter
 * @property {string|number} [projectId]
 * @property {string|number} [status]
 * @property {string|number} [authorId]
 * @property {string|number} [executorId]
 * @property {string} [searchTerm]
 * @property {string} [sortBy]
 * @property {boolean} [sortDescending]
 */

export const emptyProjectTask = Object.freeze({
  id: 0,
  title: '',
  comment: null,
  priority: 0,
  status: 0,
  projectId: 0,
  projectName: '',
  authorId: 0,
  authorName: '',
  executorId: null,
  executorName: null,
});
