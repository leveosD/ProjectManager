/**
 * @typedef {Object} Project
 * @property {number} id
 * @property {string} name
 * @property {string} customerCompany
 * @property {string} executingCompany
 * @property {string} startDate
 * @property {string} endDate
 * @property {number} priority
 * @property {number} projectManagerId
 * @property {string} projectManagerName
 * @property {string} projectManagerEmail
 * @property {import('./employee.js').Employee[]} employees
 * @property {import('./document.js').ProjectDocument[]} documents
 * @property {number} tasksCount
 */

/**
 * @typedef {Object} ProjectRequest
 * @property {string} name
 * @property {string} customerCompany
 * @property {string} executingCompany
 * @property {string} startDate
 * @property {string} endDate
 * @property {number} priority
 * @property {number} projectManagerId
 * @property {number[]} employeeIds
 */

/**
 * @typedef {Object} ProjectFilter
 * @property {string} [searchTerm]
 * @property {string} [startDateFrom]
 * @property {string} [startDateTo]
 * @property {string} [priorityMin]
 * @property {string} [priorityMax]
 * @property {string} [sortBy]
 * @property {boolean} [sortDescending]
 */

export const emptyProject = Object.freeze({
  id: 0,
  name: '',
  customerCompany: '',
  executingCompany: '',
  startDate: '',
  endDate: '',
  priority: 0,
  projectManagerId: 0,
  projectManagerName: '',
  projectManagerEmail: '',
  employees: [],
  documents: [],
  tasksCount: 0,
});
