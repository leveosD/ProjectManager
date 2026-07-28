import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { tasksApi, projectsApi } from '../api';
import { useAuth } from '../context/AuthContext';
import EmployeeSearch from '../components/EmployeeSearch';
import './Tasks.css';

const statusLabels = ['To Do', 'In Progress', 'Done'];
const statusLabel = (status) => statusLabels[status] ?? 'Unknown';

const Tasks = () => {
  const [tasks, setTasks] = useState([]);
  const [projects, setProjects] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [filter, setFilter] = useState({
    projectId: '',
    status: '',
    authorId: '',
    executorId: '',
    searchTerm: '',
    sortBy: 'priority',
    sortDescending: true,
  });
  const [selectedAuthor, setSelectedAuthor] = useState(null);
  const [selectedExecutor, setSelectedExecutor] = useState(null);
  const { isDirector, isProjectManager } = useAuth();

  const sortableColumns = [
    { key: 'title', label: 'Title' },
    { key: 'projectName', label: 'Project' },
    { key: 'authorName', label: 'Author' },
    { key: 'executorName', label: 'Executor' },
    { key: 'priority', label: 'Priority' },
    { key: 'status', label: 'Status' },
  ];

  useEffect(() => {
    loadProjects();
    loadTasks();
  }, [filter]);

  const loadProjects = async () => {
    try {
      const data = await projectsApi.getAll({});
      setProjects(data);
    } catch {
      setProjects([]);
    }
  };

  const loadTasks = async () => {
    setLoading(true);
    try {
      const data = await tasksApi.getAll(filter);
      setTasks(data);
    } catch (err) {
      setError(err.message || 'Failed to load tasks.');
    } finally {
      setLoading(false);
    }
  };

  const handleFilterChange = (e) => {
    const { name, value } = e.target;
    setFilter({ ...filter, [name]: value });
  };

  const handleSort = (columnKey) => {
    setFilter((prev) => {
      if (prev.sortBy === columnKey) {
        return { ...prev, sortDescending: !prev.sortDescending };
      }
      return { ...prev, sortBy: columnKey, sortDescending: false };
    });
  };

  const renderSortArrow = (columnKey) => {
    if (filter.sortBy !== columnKey) {
      return <span className="sort-arrow sort-arrow-inactive" aria-hidden="true">↕</span>;
    }
    return (
      <span className="sort-arrow" aria-hidden="true">
        {filter.sortDescending ? '↓' : '↑'}
      </span>
    );
  };

  const handleAuthorSelect = (employee) => {
    setSelectedAuthor(employee);
    setFilter((prev) => ({ ...prev, authorId: employee?.id ?? '' }));
  };

  const handleExecutorSelect = (employee) => {
    setSelectedExecutor(employee);
    setFilter((prev) => ({ ...prev, executorId: employee?.id ?? '' }));
  };

  const handleStatusChange = async (taskId, newStatus) => {
    try {
      await tasksApi.updateStatus(taskId, parseInt(newStatus));
      loadTasks();
    } catch (err) {
      setError(err.message || 'Failed to update task status.');
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this task?')) {
      return;
    }

    try {
      await tasksApi.delete(id);
      loadTasks();
    } catch (err) {
      setError(err.message || 'Failed to delete task.');
    }
  };

  if (loading) return <div className="loading">Loading tasks…</div>;

  return (
    <div>
      <div className="page-header">
        <h1>Tasks</h1>
        {(isDirector || isProjectManager) && (
          <Link to="/tasks/new" className="btn btn-primary">
            New Task
          </Link>
        )}
      </div>

      {error && <div className="error" role="alert" aria-live="polite">{error}</div>}

      <div className="card filters">
        <h3>Filters</h3>
        <div className="filter-grid">
          <div className="form-group">
            <label htmlFor="projectId">Project</label>
            <select
              id="projectId"
              name="projectId"
              className="form-control"
              value={filter.projectId}
              onChange={handleFilterChange}
            >
              <option value="">All Projects</option>
              {projects.map((project) => (
                <option key={project.id} value={project.id}>
                  {project.name}
                </option>
              ))}
            </select>
          </div>
          <div className="form-group">
            <label htmlFor="status">Status</label>
            <select
              id="status"
              name="status"
              className="form-control"
              value={filter.status}
              onChange={handleFilterChange}
            >
              <option value="">All Statuses</option>
              <option value="0">To Do</option>
              <option value="1">In Progress</option>
              <option value="2">Done</option>
            </select>
          </div>
          <div className="form-group">
            <label htmlFor="author-search">Author</label>
            <div className="filter-employee-search">
              <EmployeeSearch
                inputId="author-search"
                selected={selectedAuthor}
                onSelect={handleAuthorSelect}
                placeholder="Search author…"
                roleFilter={['Director', 'ProjectManager']}
              />
              {selectedAuthor && (
                <button
                  type="button"
                  className="btn btn-sm"
                  onClick={() => handleAuthorSelect(null)}
                  aria-label="Clear author filter"
                >
                  Clear
                </button>
              )}
            </div>
          </div>
          <div className="form-group">
            <label htmlFor="executor-search">Executor</label>
            <div className="filter-employee-search">
              <EmployeeSearch
                inputId="executor-search"
                selected={selectedExecutor}
                onSelect={handleExecutorSelect}
                placeholder="Search executor…"
              />
              {selectedExecutor && (
                <button
                  type="button"
                  className="btn btn-sm"
                  onClick={() => handleExecutorSelect(null)}
                  aria-label="Clear executor filter"
                >
                  Clear
                </button>
              )}
            </div>
          </div>
          <div className="form-group">
            <label htmlFor="searchTerm">Search</label>
            <input
              id="searchTerm"
              type="text"
              name="searchTerm"
              className="form-control"
              value={filter.searchTerm}
              onChange={handleFilterChange}
              placeholder="Task title…"
            />
          </div>
        </div>
      </div>

      <div className="tasks-table-container card">
        <table className="tasks-table">
          <caption className="visually-hidden">Tasks list</caption>
          <thead>
            <tr>
              {sortableColumns.map((column) => (
                <th key={column.key} scope="col">
                  <button
                    type="button"
                    className="sort-header"
                    onClick={() => handleSort(column.key)}
                    aria-label={`Sort by ${column.label}${filter.sortBy === column.key ? (filter.sortDescending ? ', descending' : ', ascending') : ''}`}
                  >
                    {column.label}
                    {renderSortArrow(column.key)}
                  </button>
                </th>
              ))}
              <th scope="col">Actions</th>
            </tr>
          </thead>
          <tbody>
            {tasks.length === 0 ? (
              <tr>
                <td colSpan="7" className="empty-row">
                  No tasks found.
                </td>
              </tr>
            ) : (
              tasks.map((task) => (
                <tr key={task.id}>
                  <td>{task.title}</td>
                  <td>{task.projectName}</td>
                  <td>{task.authorName}</td>
                  <td>{task.executorName || 'Unassigned'}</td>
                  <td>{task.priority}</td>
                  <td>
                    <select
                      className="form-control status-select"
                      value={task.status}
                      onChange={(e) => handleStatusChange(task.id, e.target.value)}
                      aria-label={`Status for ${task.title}`}
                    >
                      {statusLabels.map((label, index) => (
                        <option key={index} value={index}>
                          {label}
                        </option>
                      ))}
                    </select>
                  </td>
                  <td>
                    <Link to={`/tasks/${task.id}`} className="btn">
                      Open
                    </Link>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default Tasks;
