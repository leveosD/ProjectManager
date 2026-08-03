import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { employeeService } from '@infrastructure';
import { useAuth } from '../context/AuthContext';
import './TaskEdit.css';

const roleOptions = [
  { value: 'Director', label: 'Director' },
  { value: 'ProjectManager', label: 'Project Manager' },
  { value: 'Employee', label: 'Employee' },
];

const EmployeeEdit = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { isDirector } = useAuth();

  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [middleName, setMiddleName] = useState('');
  const [email, setEmail] = useState('');
  const [role, setRole] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    let cancelled = false;

    const loadEmployee = async () => {
      setLoading(true);
      try {
        const employee = await employeeService.getById(Number(id));
        if (cancelled) return;

        setFirstName(employee.firstName || '');
        setLastName(employee.lastName || '');
        setMiddleName(employee.middleName || '');
        setEmail(employee.email || '');
        setRole(employee.role || '');
      } catch (err) {
        if (!cancelled) {
          setError(err.message || 'Failed to load employee.');
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    loadEmployee();

    return () => {
      cancelled = true;
    };
  }, [id]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const employeeData = {
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        middleName: middleName.trim() || null,
        email: email.trim(),
        role: role || null,
      };

      await employeeService.update(Number(id), employeeData);
      navigate('/employees');
    } catch (err) {
      setError(err.message || 'Failed to update employee. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return <div className="loading">Loading employee…</div>;
  }

  return (
    <div className="card task-edit-container">
      <h2>Edit Employee</h2>
      {error && <div className="error" role="alert" aria-live="polite">{error}</div>}

      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="firstName">First Name *</label>
          <input
            id="firstName"
            name="firstName"
            type="text"
            className="form-control"
            value={firstName}
            onChange={(e) => setFirstName(e.target.value)}
            autoComplete="given-name"
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="lastName">Last Name *</label>
          <input
            id="lastName"
            name="lastName"
            type="text"
            className="form-control"
            value={lastName}
            onChange={(e) => setLastName(e.target.value)}
            autoComplete="family-name"
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="middleName">Middle Name</label>
          <input
            id="middleName"
            name="middleName"
            type="text"
            className="form-control"
            value={middleName}
            onChange={(e) => setMiddleName(e.target.value)}
            autoComplete="additional-name"
          />
        </div>

        <div className="form-group">
          <label htmlFor="email">Email *</label>
          <input
            id="email"
            name="email"
            type="email"
            className="form-control"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            autoComplete="email"
            spellCheck={false}
            required
          />
        </div>

        {isDirector && (
          <div className="form-group">
            <label htmlFor="role">Role</label>
            <select
              id="role"
              name="role"
              className="form-control"
              value={role}
              onChange={(e) => setRole(e.target.value)}
            >
              <option value="">Select role…</option>
              {roleOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>
        )}

        <div className="form-actions">
          <button type="submit" className="btn btn-primary" disabled={loading}>
            {loading ? 'Saving…' : 'Update Employee'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default EmployeeEdit;
