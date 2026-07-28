import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import Layout from './components/Layout';
import PrivateRoute from './components/PrivateRoute';
import Home from './pages/Home';
import Login from './pages/Login';
import Projects from './pages/Projects';
import ProjectDetails from './pages/ProjectDetails';
import ProjectWizard from './pages/ProjectWizard';
import Tasks from './pages/Tasks';
import TaskDetails from './pages/TaskDetails';
import TaskEdit from './pages/TaskEdit';
import Employees from './pages/Employees';
import EmployeeCreate from './pages/EmployeeCreate';
import EmployeeEdit from './pages/EmployeeEdit';

const FallbackHome = () => <Navigate to="/" replace />;

const App = () => {
  return (
    <AuthProvider>
      <Router>
        <Layout>
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/login" element={<Login />} />

            <Route
              path="/projects"
              element={
                <PrivateRoute>
                  <Projects />
                </PrivateRoute>
              }
            />
            <Route
              path="/projects/new"
              element={
                <PrivateRoute allowedRoles={['Director']}>
                  <ProjectWizard />
                </PrivateRoute>
              }
            />
            <Route
              path="/projects/:id"
              element={
                <PrivateRoute>
                  <ProjectDetails />
                </PrivateRoute>
              }
            />
            <Route
              path="/projects/:id/edit"
              element={
                <PrivateRoute allowedRoles={['Director', 'ProjectManager']}>
                  <ProjectWizard />
                </PrivateRoute>
              }
            />

            <Route
              path="/tasks"
              element={
                <PrivateRoute>
                  <Tasks />
                </PrivateRoute>
              }
            />
            <Route
              path="/tasks/new"
              element={
                <PrivateRoute allowedRoles={['Director', 'ProjectManager']}>
                  <TaskEdit />
                </PrivateRoute>
              }
            />
            <Route
              path="/projects/:projectId/tasks/new"
              element={
                <PrivateRoute allowedRoles={['Director', 'ProjectManager']}>
                  <TaskEdit />
                </PrivateRoute>
              }
            />
            <Route
              path="/tasks/:id"
              element={
                <PrivateRoute>
                  <TaskDetails />
                </PrivateRoute>
              }
            />
            <Route
              path="/tasks/:id/edit"
              element={
                <PrivateRoute allowedRoles={['Director', 'ProjectManager']}>
                  <TaskEdit />
                </PrivateRoute>
              }
            />

            <Route
              path="/employees"
              element={
                <PrivateRoute allowedRoles={['Director']}>
                  <Employees />
                </PrivateRoute>
              }
            />
            <Route
              path="/employees/new"
              element={
                <PrivateRoute allowedRoles={['Director']}>
                  <EmployeeCreate />
                </PrivateRoute>
              }
            />
            <Route
              path="/employees/:id/edit"
              element={
                <PrivateRoute allowedRoles={['Director']}>
                  <EmployeeEdit />
                </PrivateRoute>
              }
            />

            <Route path="*" element={<FallbackHome />} />
          </Routes>
        </Layout>
      </Router>
    </AuthProvider>
  );
};

export default App;
