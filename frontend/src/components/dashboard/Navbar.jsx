import React from 'react'
import { useAuth } from '../../../context/authContext'; 


const Navbar = () => {
  const { user, logout } = useAuth();

  return (
    <div className="bg-white shadow px-6 py-3 flex justify-between items-center">
      <h1 className="text-lg font-bold text-gray-800">Admin Panel</h1>
      <div className="flex items-center gap-4">
        <span className="text-gray-600">Welcome, {user?.name}</span>
        <button
          onClick={logout}
          className="bg-red-500 hover:bg-red-600 text-white px-3 py-1 rounded"
        >
          Logout
        </button>
      </div>
    </div>
  );
};


export default Navbar