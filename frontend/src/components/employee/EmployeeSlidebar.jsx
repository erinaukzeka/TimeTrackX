import React from 'react';
import { Outlet } from 'react-router-dom';
import EmployeeSidebar from '../components/employee/dashboard/EmployeeSidebar';
import EmployeeNavbar from '../components/employee/dashboard/EmployeeNavbar';

const EmployeeDashboard = () => {
  return (
    <div className="flex">
      <EmployeeSidebar />
      <div className="flex-1 ml-64 bg-gray-100 h-screen">
        <EmployeeNavbar />
        <Outlet />
      </div>
    </div>
  );
};

export default EmployeeDashboard;