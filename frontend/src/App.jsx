import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/Login';
import AdminDashboard from './pages/AdminDashboard';
import EmployeeDashboard from './pages/EmployeeDashboard';
import PrivateRoutes from './utils/PrivateRoutes';
import RoleBaseRoutes from './utils/RoleBaseRoutes';
import AdminSummary from './components/admin/dashboard/AdminSummary';
import DepartmentList from './components/admin/department/DepartmentList';
import AddDepartment from './components/admin/department/AddDepartment';
import EmployeeHoursStatistics from './components/admin/statistics/EmployeeHoursStatistics';
import CheckInOutTrends from './components/admin/statistics/CheckInOutTrends';
import AttendanceStatistics from './components/admin/statistics/AttendanceStatistics';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/admin-dashboard" />} />
        <Route path="/login" element={<Login />} />

        <Route path="/admin-dashboard" element={
          <PrivateRoutes>
            <RoleBaseRoutes requiredRole={["admin"]}>
              <AdminDashboard />
            </RoleBaseRoutes>
          </PrivateRoutes>
        }>
          <Route index element={<AdminSummary />}></Route>
          <Route path="/admin-dashboard/departments" element={<DepartmentList />}></Route>
          <Route path="/admin-dashboard/add-department" element={<AddDepartment />}></Route>
          <Route path="/admin-dashboard/statistics/employee-hours" element={<EmployeeHoursStatistics />}></Route>
          <Route path="/admin-dashboard/statistics/checkin-trends" element={<CheckInOutTrends />}></Route>
          <Route path="/admin-dashboard/statistics/attendance" element={<AttendanceStatistics />}></Route>
        </Route>
        <Route path="/employee-dashboard" element={<EmployeeDashboard />}></Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
