import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import EmployeeSearch from '../components/EmployeeSearch';
import FileUploader from '../components/FileUploader';
import { projectsApi, documentsApi } from '../api';
import './ProjectWizard.css';

const today = () => new Date().toISOString().slice(0, 10);

const initialFormState = {
  name: '',
  startDate: today(),
  endDate: today(),
  priority: 1,
  customerCompany: '',
  executingCompany: '',
  projectManagerId: null,
  employeeIds: [],
};

const ProjectWizard = () => {
  const { id } = useParams();
  const isEditing = !!id;
  const navigate = useNavigate();

  const [step, setStep] = useState(1);
  const [formData, setFormData] = useState(initialFormState);
  const [selectedManager, setSelectedManager] = useState(null);
  const [selectedExecutors, setSelectedExecutors] = useState([]);
  const [files, setFiles] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const totalSteps = 5;

  // Load project data when editing
  useEffect(() => {
    if (!isEditing) {
      setFormData(initialFormState);
      setSelectedManager(null);
      setSelectedExecutors([]);
      setFiles([]);
      return;
    }

    let cancelled = false;

    const loadProject = async () => {
      setLoading(true);
      try {
        const project = await projectsApi.getById(Number(id));
        if (cancelled) return;

        setFormData({
          name: project.name || '',
          startDate: project.startDate ? project.startDate.slice(0, 10) : '',
          endDate: project.endDate ? project.endDate.slice(0, 10) : '',
          priority: project.priority ?? 1,
          customerCompany: project.customerCompany || '',
          executingCompany: project.executingCompany || '',
          projectManagerId: project.projectManagerId ?? null,
          employeeIds: project.employees?.map((e) => e.id) ?? [],
        });

        setSelectedManager(
          project.projectManagerId && project.projectManagerName
            ? {
                id: project.projectManagerId,
                fullName: project.projectManagerName,
                email: project.projectManagerEmail || '',
              }
            : null
        );

        // Project Manager is always part of the team on the backend, so exclude them from the picker.
        setSelectedExecutors(
          (project.employees ?? []).filter((e) => e.id !== project.projectManagerId)
        );
      } catch (err) {
        if (!cancelled) {
          setError(err.message || 'Failed to load project data.');
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    loadProject();

    return () => {
      cancelled = true;
    };
  }, [id, isEditing]);

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: name === 'priority' ? parseInt(value) || 0 : value });
  };

  const handleManagerSelect = (employee) => {
    setSelectedManager(employee);
    setSelectedExecutors((prev) => prev.filter((e) => e.id !== employee.id));
    setFormData((prev) => ({
      ...prev,
      projectManagerId: employee.id,
      employeeIds: prev.employeeIds.filter((id) => id !== employee.id),
    }));
  };

  const handleExecutorsChange = (employees) => {
    setSelectedExecutors(employees);
    setFormData({ ...formData, employeeIds: employees.map((e) => e.id) });
  };

  const handleFilesSelected = (newFiles) => {
    setFiles((prev) => [...prev, ...newFiles]);
  };

  const validateStep = () => {
    switch (step) {
      case 1:
        if (!formData.name || !formData.startDate || !formData.endDate) {
          setError('Please fill in all required fields.');
          return false;
        }
        if (new Date(formData.endDate) < new Date(formData.startDate)) {
          setError('End date cannot be earlier than start date.');
          return false;
        }
        break;
      case 2:
        if (!formData.customerCompany || !formData.executingCompany) {
          setError('Please fill in both company names.');
          return false;
        }
        break;
      case 3:
        if (!formData.projectManagerId) {
          setError('Please select a project manager.');
          return false;
        }
        break;
      case 4:
        if (formData.employeeIds.length === 0) {
          setError('Please select at least one executor.');
          return false;
        }
        break;
      default:
        return true;
    }
    return true;
  };

  const nextStep = () => {
    if (validateStep()) {
      setError('');
      setStep((prev) => Math.min(prev + 1, totalSteps));
    }
  };

  const prevStep = () => {
    setError('');
    setStep((prev) => Math.max(prev - 1, 1));
  };

  const handleSubmit = async () => {
    setLoading(true);
    setError('');

    try {
      const projectData = {
        ...formData,
        startDate: new Date(formData.startDate).toISOString(),
        endDate: new Date(formData.endDate).toISOString(),
      };

      const project = isEditing
        ? await projectsApi.update(Number(id), projectData)
        : await projectsApi.create(projectData);

      // Upload documents if any
      if (files.length > 0 && project?.id) {
        for (const file of files) {
          await documentsApi.upload(project.id, file);
        }
      }

      navigate(`/projects/${project.id}`);
    } catch (err) {
      setError(err.message || 'Failed to save project. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const renderStep = () => {
    switch (step) {
      case 1:
        return (
          <div className="wizard-step">
            <h3>Step 1: Basic Project Information</h3>
            <div className="form-group">
              <label htmlFor="name">Project Name *</label>
              <input
                id="name"
                name="name"
                type="text"
                className="form-control"
                value={formData.name}
                onChange={handleInputChange}
                required
              />
            </div>
            <div className="form-row">
              <div className="form-group">
                <label htmlFor="startDate">Start Date *</label>
                <input
                  id="startDate"
                  name="startDate"
                  type="date"
                  className="form-control"
                  value={formData.startDate}
                  onChange={handleInputChange}
                  required
                />
              </div>
              <div className="form-group">
                <label htmlFor="endDate">End Date *</label>
                <input
                  id="endDate"
                  name="endDate"
                  type="date"
                  className="form-control"
                  value={formData.endDate}
                  onChange={handleInputChange}
                  required
                />
              </div>
            </div>
            <div className="form-group">
              <label htmlFor="priority">Priority *</label>
              <input
                id="priority"
                name="priority"
                type="number"
                min="1"
                className="form-control"
                value={formData.priority}
                onChange={handleInputChange}
                required
              />
            </div>
          </div>
        );
      case 2:
        return (
          <div className="wizard-step">
            <h3>Step 2: Company Information</h3>
            <div className="form-group">
              <label htmlFor="customerCompany">Customer Company *</label>
              <input
                id="customerCompany"
                name="customerCompany"
                type="text"
                className="form-control"
                value={formData.customerCompany}
                onChange={handleInputChange}
                required
              />
            </div>
            <div className="form-group">
              <label htmlFor="executingCompany">Executing Company *</label>
              <input
                id="executingCompany"
                name="executingCompany"
                type="text"
                className="form-control"
                value={formData.executingCompany}
                onChange={handleInputChange}
                required
              />
            </div>
          </div>
        );
      case 3:
        return (
          <div className="wizard-step">
            <h3>Step 3: Project Manager</h3>
            <p>Select a project manager from the list of employees:</p>
            <EmployeeSearch
              key="project-manager-search"
              inputId="project-manager-search"
              selected={selectedManager}
              onSelect={handleManagerSelect}
              placeholder="Search project manager…"
              roleFilter={['ProjectManager']}
            />
            {selectedManager && (
              <div className="selection-info">
                Selected: <strong>{selectedManager.fullName}</strong> ({selectedManager.email})
              </div>
            )}
          </div>
        );
      case 4:
        return (
          <div className="wizard-step">
            <h3>Step 4: Project Executors</h3>
            <p>Select team members working on this project:</p>
            <EmployeeSearch
              key="executors-search"
              inputId="executors-search"
              multiple
              selected={selectedExecutors}
              onMultiSelect={handleExecutorsChange}
              placeholder="Search employees to add…"
              excludeEmployees={selectedManager ? [selectedManager] : []}
            />
          </div>
        );
      case 5:
        return (
          <div className="wizard-step">
            <h3>Step 5: Project Documents</h3>
            <p>Upload any relevant project documents (optional):</p>
            <FileUploader onFilesSelected={handleFilesSelected} />
          </div>
        );
      default:
        return null;
    }
  };

  if (loading && isEditing) {
    return <div className="loading">Loading project…</div>;
  }

  return (
    <div className="card wizard-container">
      <h2>{isEditing ? 'Edit Project' : 'Create New Project'}</h2>
      <div className="progress-bar" role="progressbar" aria-valuenow={step} aria-valuemin={1} aria-valuemax={totalSteps} aria-label="Project wizard progress">
        <div className="progress-fill" style={{ width: `${(step / totalSteps) * 100}%` }} />
      </div>
      <div className="step-indicator">
        Step {step} of {totalSteps}
      </div>

      {error && <div className="error" role="alert" aria-live="polite">{error}</div>}

      {renderStep()}

      <div className="wizard-actions">
        {step > 1 && (
          <button type="button" className="btn" onClick={prevStep} disabled={loading}>
            Previous
          </button>
        )}
        {step < totalSteps ? (
          <button type="button" className="btn btn-primary" onClick={nextStep}>
            Next
          </button>
        ) : (
          <button type="button" className="btn btn-primary" onClick={handleSubmit} disabled={loading}>
            {loading ? 'Saving…' : isEditing ? 'Update Project' : 'Create Project'}
          </button>
        )}
      </div>
    </div>
  );
};

export default ProjectWizard;
