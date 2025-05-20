import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/Login';
import AdminDashboard from './pages/AdminDashboard';
import EmployeeDashboard from './pages/EmployeeDashboard';
import PrivateRouters from './utils/PrivateRoutes';
import RoleBaseRoutes from './utils/RoleBaseRoutes';
import AdminSummary from './components/dashboard/AdminSummary';
import DepartmentList from './components/department/DepartmentList';

function App() {

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/admin-dashboard" />}></Route>
        <Route path="/login" element={<Login />}></Route>
        <Route path="/admin-dashboard" element={
          <PrivateRouters>
            <RoleBaseRoutes requiredRole={["admin"]}>
              <AdminDashboard />
            </RoleBaseRoutes>
          </PrivateRouters>
          }>
          <Route index element={<AdminSummary />}></Route>
          
          <Route path="/admin-dashboard/departments" element={<DepartmentList />}></Route>
          <Route path="/admin-dashboard/add-department" element={<AddDepartment />}></Route>

          </Route>
        <Route path="/employee-dashboard" element={<EmployeeDashboard />}></Route>
    </Routes>
    </BrowserRouter>
  )
}

export default App