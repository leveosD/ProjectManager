import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import EmployeeSearch from '../components/EmployeeSearch';
import { tasksApi, projectsApi, employeesApi } from '../api';
import { useAuth } from '../context/AuthContext';
import './TaskEdit.css';

const statusOptions = [
  { value: 0, label: 'To Do' },
  { value: 1, label: 'In Progress' },
  { value: 2, label: 'Done' },
];

const TaskEdit = () => {
  const { id } = useParams();
  const { projectId } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();

  const isEditing = !!id;
  const presetProjectId = projectId ? Number(projectId) : null;

  const [title, setTitle] = useState('');
  const [comment, setComment] = useState('');
  const [priority, setPriority] = useState(1);
  const [status, setStatus] = useState(0);
  const [projectIdValue, setProjectIdValue] = useState(presetProjectId ?? '');
  const [executorId, setExecutorId] = useState(null);
  const [executor, setExecutor] = useState(null);
  const [projects, setProjects] = useState([]);
  const [projectTeam, setProjectTeam] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    loadProjects();
  }, []);

  useEffect(() => {
    if (projectIdValue) {
      loadProjectTeam(Number(projectIdValue));
    } else {
      setProjectTeam([]);
    }
  }, [projectIdValue]);

  useEffect(() => {
    if (!isEditing) return;

    let cancelled = false;

    const loadTask = async () => {
      setLoading(true);
      try {
        const task = await tasksApi.getById(Number(id));
        if (cancelled) return;

        setTitle(task.title || '');
        setComment(task.comment || '');
        setPriority(task.priority ?? 1);
        setStatus(task.status ?? 0);
        setProjectIdValue(task.projectId ?? '');
        setExecutorId(task.executorId ?? null);

        if (task.executorId) {
          try {
            const emp = await employeesApi.getById(task.executorId);
            if (!cancelled) {
              setExecutor(emp);
            }
          } catch {
            setExecutor(null);
          }
        }
      } catch (err) {
        if (!cancelled) {
          setError(err.message || 'Failed to load task.');
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    loadTask();

    return () => {
      cancelled = true;
    };
  }, [id, isEditing]);

  const loadProjects = async () => {
    try {
      const data = await projectsApi.getAll({});
      setProjects(data);
    } catch {
      setProjects([]);
    }
  };

  const loadProjectTeam = async (pid) => {
    try {
      const project = await projectsApi.getById(pid);
      const team = project?.employees ?? [];
      // Include project manager as a possible executor
      if (project?.projectManagerId && project?.projectManagerName) {
        const manager = {
          id: project.projectManagerId,
          fullName: project.projectManagerName,
          email: project.projectManagerEmail || '',
        };
        if (!team.some((e) => e.id === manager.id)) {
          team.unshift(manager);
        }
      }
      setProjectTeam(team);

      // Ensure current executor still belongs to the selected project team
      if (executorId && !team.some((e) => e.id === executorId)) {
        setExecutor(null);
        setExecutorId(null);
      }
    } catch {
      setProjectTeam([]);
    }
  };

  const handleExecutorSelect = (employee) => {
    setExecutor(employee);
    setExecutorId(employee?.id ?? null);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const taskData = {
        title: title.trim(),
        comment: comment.trim() || null,
        priority: Number(priority),
        status: Number(status),
        projectId: Number(projectIdValue),
        executorId: executorId,
      };

      if (isEditing) {
        await tasksApi.update(Number(id), taskData);
      } else {
        await tasksApi.create({
          ...taskData,
          authorId: user?.employeeId ?? 0,
        });
      }

      navigate('/tasks');
    } catch (err) {
      setError(err.message || 'Failed to save task. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditing) {
    return <div className="loading">Loading task…</div>;
  }

  return (
    <div className="card task-edit-container">
      <h2>{isEditing ? 'Edit Task' : 'Create Task'}</h2>
      {error && <div className="error" role="alert" aria-live="polite">{error}</div>}

      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="projectId">Project *</label>
          <select
            id="projectId"
            name="projectId"
            className="form-control"
            value={projectIdValue}
            onChange={(e) => setProjectIdValue(e.target.value)}
            required
            disabled={!!presetProjectId}
          >
            <option value="">Select project…</option>
            {projects.map((project) => (
              <option key={project.id} value={project.id}>
                {project.name}
              </option>
            ))}
          </select>
        </div>

        <div className="form-group">
          <label htmlFor="title">Title *</label>
          <input
            id="title"
            name="title"
            type="text"
            className="form-control"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="comment">Comment</label>
          <textarea
            id="comment"
            name="comment"
            className="form-control"
            rows="3"
            value={comment}
            onChange={(e) => setComment(e.target.value)}
          />
        </div>

        <div className={`form-row ${!isEditing ? 'form-row-single' : ''}`}>
          <div className="form-group">
            <label htmlFor="priority">Priority *</label>
            <input
              id="priority"
              name="priority"
              type="number"
              min="1"
              className="form-control"
              value={priority}
              onChange={(e) => setPriority(e.target.value)}
              required
            />
          </div>

          {isEditing && (
            <div className="form-group">
              <label htmlFor="status">Status *</label>
              <select
                id="status"
                name="status"
                className="form-control"
                value={status}
                onChange={(e) => setStatus(e.target.value)}
                required
              >
                {statusOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>
          )}
        </div>

        <div className="form-group">
          <label htmlFor="executor-search">Executor</label>
          {projectIdValue ? (
            <>
              <EmployeeSearch
                inputId="executor-search"
                selected={executor}
                onSelect={handleExecutorSelect}
                placeholder="Search executor from project team…"
                employees={projectTeam}
              />
              {executor && (
                <div className="selection-info">
                  Selected: <strong>{executor.fullName}</strong> ({executor.email})
                </div>
              )}
            </>
          ) : (
            <p className="hint">Select a project first to choose an executor.</p>
          )}
        </div>

        <div className="form-actions">
          <button type="submit" className="btn btn-primary" disabled={loading}>
            {loading ? 'Saving…' : isEditing ? 'Update Task' : 'Create Task'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default TaskEdit;
