import { useEffect, useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { tasksApi, projectsApi } from '../api';
import { useAuth } from '../context/AuthContext';
import './TaskDetails.css';

const statusLabels = ['To Do', 'In Progress', 'Done'];
const statusLabel = (status) => statusLabels[status] ?? 'Unknown';

const TaskDetails = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user, isDirector, isProjectManager } = useAuth();
  const [task, setTask] = useState(null);
  const [project, setProject] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [status, setStatus] = useState('');

  useEffect(() => {
    loadTask();
  }, [id]);

  const loadTask = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await tasksApi.getById(Number(id));
      setTask(data);
      setStatus(String(data.status));
      try {
        const projectData = await projectsApi.getById(data.projectId);
        setProject(projectData);
      } catch {
        setProject(null);
      }
    } catch (err) {
      setError(err.message || 'Failed to load task details.');
    } finally {
      setLoading(false);
    }
  };

  const handleStatusChange = async (e) => {
    const newStatus = e.target.value;
    setStatus(newStatus);
    try {
      await tasksApi.updateStatus(task.id, parseInt(newStatus));
      loadTask();
    } catch (err) {
      setError(err.message || 'Failed to update task status.');
      setStatus(String(task.status));
    }
  };

  const handleDelete = async () => {
    if (!window.confirm('Are you sure you want to delete this task?')) {
      return;
    }
    try {
      await tasksApi.delete(task.id);
      navigate('/tasks');
    } catch (err) {
      setError(err.message || 'Failed to delete task.');
    }
  };

  const canManage = isDirector || isProjectManager;
  const isAssignee = task?.executorId === user?.employeeId;

  if (loading) return <div className="loading">Loading task details…</div>;
  if (!task) return <div className="error">Task not found.</div>;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>{task.title}</h1>
          <p className="subtitle">
            {task.projectName} · {statusLabel(task.status)}
          </p>
        </div>
        <div className="task-actions">
          {canManage && (
            <>
              <Link to={`/tasks/${task.id}/edit`} className="btn btn-primary">
                Edit
              </Link>
              {isDirector && (
                <button type="button" className="btn btn-danger" onClick={handleDelete}>
                  Delete
                </button>
              )}
            </>
          )}
        </div>
      </div>

      {error && <div className="error" role="alert" aria-live="polite">{error}</div>}

      <div className="details-grid">
        <section className="card">
          <h3>Task Information</h3>
          <p><strong>Priority:</strong> {task.priority}</p>
          <p><strong>Status:</strong></p>
          {(canManage || isAssignee) ? (
            <select
              id="status"
              name="status"
              className="form-control status-select"
              value={status}
              onChange={handleStatusChange}
              aria-label={`Status for ${task.title}`}
            >
              {statusLabels.map((label, index) => (
                <option key={index} value={index}>
                  {label}
                </option>
              ))}
            </select>
          ) : (
            <p>{statusLabel(task.status)}</p>
          )}
          <p><strong>Project:</strong> <Link to={`/projects/${task.projectId}`}>{task.projectName}</Link></p>
          <p><strong>Author:</strong> {task.authorName}</p>
          <p><strong>Executor:</strong> {task.executorName || 'Unassigned'}</p>
        </section>

        <section className="card">
          <h3>Comment</h3>
          {task.comment ? (
            <p className="task-comment">{task.comment}</p>
          ) : (
            <p className="text-muted">No comment provided.</p>
          )}
        </section>

        {project && (
          <section className="card">
            <h3>Project Team</h3>
            {project.employees.length === 0 ? (
              <p>No team members assigned.</p>
            ) : (
              <ul className="member-list">
                {project.employees.map((emp) => (
                  <li key={emp.id}>
                    {emp.fullName} <span className="member-email">{emp.email}</span>
                  </li>
                ))}
              </ul>
            )}
          </section>
        )}
      </div>
    </div>
  );
};

export default TaskDetails;
