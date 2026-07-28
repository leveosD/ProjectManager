import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { tasksApi } from '../api';
import { useAuth } from '../context/AuthContext';
import './Tasks.css';

const statusLabels = ['To Do', 'In Progress', 'Done'];
const statusLabel = (status) => statusLabels[status] ?? 'Unknown';

const MyTasks = () => {
  const { user } = useAuth();
  const [tasks, setTasks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [filter, setFilter] = useState({
    status: '',
    sortBy: 'priority',
    sortDescending: true,
  });

  useEffect(() => {
    loadTasks();
  }, [filter]);

  const loadTasks = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await tasksApi.getAll({
        executorId: user?.employeeId,
        status: filter.status,
        sortBy: filter.sortBy,
        sortDescending: filter.sortDescending,
      });
      setTasks(data);
    } catch (err) {
      setError(err.message || 'Failed to load your tasks.');
    } finally {
      setLoading(false);
    }
  };

  const handleFilterChange = (e) => {
    const { name, value } = e.target;
    setFilter((prev) => ({ ...prev, [name]: value }));
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

  const handleStatusChange = async (taskId, newStatus) => {
    try {
      await tasksApi.updateStatus(taskId, parseInt(newStatus));
      loadTasks();
    } catch (err) {
      setError(err.message || 'Failed to update task status.');
    }
  };

  const sortableColumns = [
    { key: 'title', label: 'Title' },
    { key: 'projectName', label: 'Project' },
    { key: 'authorName', label: 'Author' },
    { key: 'priority', label: 'Priority' },
    { key: 'status', label: 'Status' },
  ];

  if (loading) return <div className="loading">Loading your tasks…</div>;

  return (
    <div>
      <div className="page-header">
        <h1>My Tasks</h1>
      </div>

      {error && <div className="error" role="alert" aria-live="polite">{error}</div>}

      <div className="card filters">
        <h3>Filters</h3>
        <div className="filter-grid">
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
        </div>
      </div>

      <div className="tasks-table-container card">
        <table className="tasks-table">
          <caption className="visually-hidden">My tasks list</caption>
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
                <td colSpan="6" className="empty-row">
                  No tasks assigned to you.
                </td>
              </tr>
            ) : (
              tasks.map((task) => (
                <tr key={task.id}>
                  <td>{task.title}</td>
                  <td>{task.projectName}</td>
                  <td>{task.authorName}</td>
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

export default MyTasks;
