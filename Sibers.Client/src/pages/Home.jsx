import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const Home = () => {
  const { isAuthenticated, user } = useAuth();

  return (
    <div className="card home-card">
      <h1>Welcome to Sibers Project Management</h1>
      <p className="home-subtitle">
        Manage your projects, employees, and tasks efficiently in one place.
      </p>

      {isAuthenticated ? (
        <div>
          <p>
            Logged in as <strong>{user?.fullName}</strong> ({user?.role})
          </p>
          <div style={{ marginTop: '1.5rem', display: 'flex', gap: '1rem', justifyContent: 'center' }}>
            <Link to="/projects" className="btn btn-primary">
              View Projects
            </Link>
            <Link to="/tasks" className="btn btn-primary">
              View Tasks
            </Link>
          </div>
        </div>
      ) : (
        <div style={{ display: 'flex', gap: '1rem', justifyContent: 'center' }}>
          <Link to="/login" className="btn btn-primary">
            Login
          </Link>
        </div>
      )}
    </div>
  );
};

export default Home;
