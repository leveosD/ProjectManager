import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { projectService } from '@infrastructure';
import { useAuth } from '../context/AuthContext';
import './Projects.css';

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

const Projects = () => {
  const navigate = useNavigate();
  const [projects, setProjects] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [filter, setFilter] = useState({
    searchTerm: '',
    startDateFrom: '',
    startDateTo: '',
    priorityMin: '',
    priorityMax: '',
    sortBy: 'priority',
    sortDescending: true,
  });
  const { isDirector, isProjectManager } = useAuth();

  useEffect(() => {
    loadProjects();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    const timer = setTimeout(() => {
      loadProjects(false);
    }, 300);

    return () => clearTimeout(timer);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter]);

  const loadProjects = async (showLoader = true) => {
    if (showLoader) {
      setLoading(true);
    }
    try {
      const data = await projectService.getAll(filter);
      setProjects(data);
    } catch (err) {
      setError(err.message || 'Failed to load projects.');
    } finally {
      if (showLoader) {
        setLoading(false);
      }
    }
  };

  const handleFilterChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFilter({
      ...filter,
      [name]: type === 'checkbox' ? checked : value,
    });
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this project?')) {
      return;
    }

    try {
      await projectService.delete(id);
      loadProjects();
    } catch (err) {
      setError(err.message || 'Failed to delete project.');
    }
  };

  if (loading) return <div className="loading">Loading projects…</div>;

  return (
    <div>
      <div className="page-header">
        <h1>Projects</h1>
        {isDirector && (
          <Link to="/projects/new" className="btn btn-primary">
            New Project
          </Link>
        )}
      </div>

      {error && <div className="error" role="alert" aria-live="polite">{error}</div>}

      <div className="card filters">
        <h3>Filters & Sorting</h3>
        <div className="filter-grid">
          <div className="form-group">
            <label htmlFor="searchTerm">Search</label>
            <input
              id="searchTerm"
              type="text"
              name="searchTerm"
              className="form-control"
              value={filter.searchTerm}
              onChange={handleFilterChange}
              placeholder="Project name, company…"
            />
          </div>
          <div className="form-group">
            <label htmlFor="startDateFrom">Start Date From</label>
            <input
              id="startDateFrom"
              type="date"
              name="startDateFrom"
              className="form-control"
              value={filter.startDateFrom}
              onChange={handleFilterChange}
            />
          </div>
          <div className="form-group">
            <label htmlFor="startDateTo">Start Date To</label>
            <input
              id="startDateTo"
              type="date"
              name="startDateTo"
              className="form-control"
              value={filter.startDateTo}
              onChange={handleFilterChange}
            />
          </div>
          <div className="form-group">
            <label htmlFor="priorityMin">Min Priority</label>
            <input
              id="priorityMin"
              type="number"
              name="priorityMin"
              className="form-control"
              value={filter.priorityMin}
              onChange={handleFilterChange}
            />
          </div>
          <div className="form-group">
            <label htmlFor="priorityMax">Max Priority</label>
            <input
              id="priorityMax"
              type="number"
              name="priorityMax"
              className="form-control"
              value={filter.priorityMax}
              onChange={handleFilterChange}
            />
          </div>
          <div className="form-group">
            <label htmlFor="sortBy">Sort By</label>
            <select
              id="sortBy"
              name="sortBy"
              className="form-control"
              value={filter.sortBy}
              onChange={handleFilterChange}
            >
              <option value="priority">Priority</option>
              <option value="name">Name</option>
              <option value="startdate">Start Date</option>
              <option value="enddate">End Date</option>
            </select>
          </div>
        </div>
        <div className="form-group checkbox-group">
          <label htmlFor="sortDescending">
            <input
              id="sortDescending"
              type="checkbox"
              name="sortDescending"
              checked={filter.sortDescending}
              onChange={handleFilterChange}
            />{' '}
            Descending Order
          </label>
        </div>
      </div>

      <div className="projects-grid">
        {projects.length === 0 ? (
          <p className="empty-state">No projects found.</p>
        ) : (
          projects.map((project) => (
            <article
              key={project.id}
              className="project-card project-card-clickable"
              onClick={() => navigate(`/projects/${project.id}`)}
              role="button"
              tabIndex={0}
              onKeyDown={(e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                  e.preventDefault();
                  navigate(`/projects/${project.id}`);
                }
              }}
              aria-label={`Open project ${project.name}`}
            >
              <h3>{project.name}</h3>
              <div className="project-meta">
                <span>Priority: {project.priority}</span>
                <span>
                  {formatDate(project.startDate)} – {formatDate(project.endDate)}
                </span>
              </div>
              <p>
                <strong>Manager:</strong> {project.projectManagerName}
              </p>
              <p>
                <strong>Team:</strong> {project.employees.length} members
              </p>
              <p>
                <strong>Tasks:</strong> {project.tasksCount}
              </p>
            </article>
          ))
        )}
      </div>
    </div>
  );
};

export default Projects;
