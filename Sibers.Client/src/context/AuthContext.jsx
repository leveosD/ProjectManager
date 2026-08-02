import { createContext, useContext, useState, useEffect } from 'react';
import { authApi } from '../api';

const AuthContext = createContext(null);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const storedUser = localStorage.getItem('user');
    if (storedUser) {
      try {
        setUser(JSON.parse(storedUser));
      } catch {
        localStorage.removeItem('user');
      }
    }
    setLoading(false);
  }, []);

  const login = (authResponse) => {
    const userData = {
      email: authResponse.email ?? authResponse.Email,
      role: authResponse.role ?? authResponse.Role,
      employeeId: authResponse.employeeId ?? authResponse.EmployeeId,
      fullName: authResponse.fullName ?? authResponse.FullName,
      expiresAt: authResponse.expiresAt ?? authResponse.ExpiresAt,
    };
    localStorage.setItem('user', JSON.stringify(userData));
    setUser(userData);
  };

  const logout = async () => {
    try {
      await authApi.logout();
    } catch {
      // Ignore network errors on logout
    } finally {
      localStorage.removeItem('user');
      setUser(null);
    }
  };

  const isDirector = user?.role === 'Director';
  const isProjectManager = user?.role === 'ProjectManager';
  const isEmployee = user?.role === 'Employee';

  const value = {
    user,
    isAuthenticated: !!user,
    isDirector,
    isProjectManager,
    isEmployee,
    login,
    logout,
    loading,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
