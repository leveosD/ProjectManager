import { useEffect, useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { projectsApi, documentsApi, tasksApi } from '../api';
import { useAuth } from '../context/AuthContext';
import FileUploader from '../components/FileUploader';
import './ProjectDetails.css';

const dateFormatter = new Intl.DateTimeFormat(undefined, {
  year: 'numeric',
  month: 'short',
  day: 'numeric',
});

const formatDate = (value) => {
  if (!value) return '—';
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? '—' : dateFormatter.format(date);
};

const formatFileSize = (bytes) => {
  if (bytes === 0) return '0 Bytes';
  return new Intl.NumberFormat(undefined, {
    style: 'unit',
    unit: 'kilobyte',
    maximumFractionDigits: 1,
  }).format(bytes / 1024);
};

const ProjectDetails = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user, isDirector, isProjectManager } = useAuth();
  const [project, setProject] = useState(null);
  const [tasks, setTasks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [uploading, setUploading] = useState(false);

  useEffect(() => {
    loadProject();
    loadTasks();
  }, [id]);

  const loadProject = async () => {
    try {
      const data = await projectsApi.getById(id);
      setProject(data);
    } catch (err) {
      setError(err.message || 'Failed to load project.');
    } finally {
      setLoading(false);
    }
  };

  const loadTasks = async () => {
    try {
      const data = await tasksApi.getAll({ projectId: id });
      setTasks(data);
    } catch {
      setTasks([]);
    }
  };

  const handleUpload = async (files) => {
    setUploading(true);
    try {
      for (const file of files) {
        await documentsApi.upload(project.id, file);
      }
      loadProject();
    } catch (err) {
      setError(err.message || 'Failed to upload documents.');
    } finally {
      setUploading(false);
    }
  };

  const handleDelete = async () => {
    if (!window.confirm('Are you sure you want to delete this project?')) {
      return;
    }

    try {
      await projectsApi.delete(id);
      navigate('/projects');
    } catch (err) {
      setError(err.message || 'Failed to delete project.');
    }
  };

  const canManage = isDirector || isProjectManager;

  if (loading) return <div className="loading">Loading project…</div>;
  if (!project) return <div className="error">Project not found.</div>;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>{project.name}</h1>
          <p className="subtitle">
            {project.customerCompany} → {project.executingCompany}
          </p>
        </div>
        <div className="project-actions">
          {canManage && (
            <>
              <Link to={`/projects/${id}/edit`} className="btn btn-primary">
                Edit Project
              </Link>
              {isDirector && (
                <button type="button" className="btn btn-danger" onClick={handleDelete}>
                  Delete Project
                </button>
              )}
            </>
          )}
        </div>
      </div>

      {error && <div className="error" role="alert" aria-live="polite">{error}</div>}

      <div className="details-grid">
        <section className="card">
          <h3>Project Information</h3>
          <p><strong>Priority:</strong> {project.priority}</p>
          <p><strong>Start Date:</strong> {formatDate(project.startDate)}</p>
          <p><strong>End Date:</strong> {formatDate(project.endDate)}</p>
          <p><strong>Project Manager:</strong> {project.projectManagerName}</p>
          <p><strong>Manager Email:</strong> {project.projectManagerEmail}</p>
        </section>

        <section className="card">
          <h3>Team Members ({project.employees.length})</h3>
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

        <section className="card">
          <div className="section-header">
            <h3>Tasks ({tasks.length})</h3>
            {canManage && (
              <Link to={`/projects/${id}/tasks/new`} className="btn btn-primary">
                Add Task
              </Link>
            )}
          </div>
          {tasks.length === 0 ? (
            <p>No tasks yet.</p>
          ) : (
            <div className="task-list">
              {tasks.map((task) => (
                <div key={task.id} className={`task-item status-${task.status}`}>
                  <div>
                    <strong>{task.title}</strong>
                    <span className="task-status">{task.status}</span>
                  </div>
                  <span className="task-executor">
                    {task.executorName || 'Unassigned'}
                  </span>
                </div>
              ))}
            </div>
          )}
        </section>

        <section className="card">
          <h3>Documents ({project.documents.length})</h3>
          {project.documents.length === 0 && <p>No documents uploaded.</p>}
          <ul className="document-list">
            {project.documents.map((doc) => (
              <li key={doc.id}>
                <a
                  href={documentsApi.downloadUrl(doc.id)}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  {doc.fileName}
                </a>
                <span className="doc-size">({formatFileSize(doc.fileSize)})</span>
              </li>
            ))}
          </ul>
          {canManage && (
            <div className="uploader-wrapper">
              {uploading ? (
                <p>Uploading…</p>
              ) : (
                <FileUploader onFilesSelected={handleUpload} />
              )}
            </div>
          )}
        </section>
      </div>
    </div>
  );
};

export default ProjectDetails;
