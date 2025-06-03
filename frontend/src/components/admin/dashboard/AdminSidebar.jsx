import React from 'react'
import { Link } from 'react-router-dom'

const AdminSidebar = () => {
    return (
        <div className='fixed w-64 h-screen bg-gray-800 text-white'>
            <div className='p-4'>
                <h2 className='text-2xl font-bold'>Admin Panel</h2>
            </div>
            <nav className='mt-8'>
                <Link to="/admin-dashboard" className='block px-4 py-2 hover:bg-gray-700'>
                    Dashboard
                </Link>
                <Link to="/admin-dashboard/departments" className='block px-4 py-2 hover:bg-gray-700'>
                    Departments
                </Link>
                <Link to="/admin-dashboard/statistics/employee-hours" className='block px-4 py-2 hover:bg-gray-700'>
                    Employee Hours Statistics
                </Link>
                <Link to="/admin-dashboard/statistics/checkin-trends" className='block px-4 py-2 hover:bg-gray-700'>
                    Check-in/out Trends
                </Link>
                <Link to="/admin-dashboard/statistics/attendance" className='block px-4 py-2 hover:bg-gray-700'>
                    Attendance & Late Arrivals
                </Link>
                <Link to="/admin-dashboard/statistics/active-employees" className='block px-4 py-2 hover:bg-gray-700'>
                    Most Active Employees
                </Link>
            </nav>
        </div>
    )
}

export default AdminSidebar 