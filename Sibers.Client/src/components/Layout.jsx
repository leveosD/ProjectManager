import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './Layout.css';

const Layout = ({ children }) => {
  const { user, isAuthenticated, logout, isDirector, isProjectManager } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="layout">
      <a href="#main-content" className="skip-link">
        Skip to main content
      </a>
      <header className="header">
        <nav className="nav" aria-label="Primary navigation">
          <Link to="/" className="logo">
            PM
          </Link>
          <div className="nav-links">
            {isAuthenticated && (
              <>
                <Link to="/projects">Projects</Link>
                <Link to="/tasks">Tasks</Link>
                {isDirector && <Link to="/employees">Employees</Link>}
              </>
            )}
          </div>
          <div className="auth-section">
            {isAuthenticated ? (
              <>
                <span className="user-name">{user?.fullName}</span>
                <button type="button" onClick={handleLogout} className="btn-logout">
                  Logout
                </button>
              </>
            ) : (
              <Link to="/login">Login</Link>
            )}
          </div>
        </nav>
      </header>
      <main id="main-content" className="main-content">{children}</main>
    </div>
  );
};

export default Layout;
