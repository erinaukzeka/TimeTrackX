import axios from "axios"; 
import React, { useState } from "react";

const Login = () => {
  const [email, setEmail]= useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState(null);

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const response= await axios.post("http://localhost:5000/api/auth/login", { email , password });
      if(response.data.success){
        alert("Successfully login");
      }
    } catch(error) {
      if(error.response && !error.response.data.success){
        setError(error.response.data.success);
      } else {
        setError("Server Error!");
      }
    }
  };

  return (
    <div className="flex flex-col items-center h-screen justify-center bg-gradient-to-b from-teal-600 to-gray-100 space-y-6">
      <h2 className="text-3xl sm:text-4xl font-bold text-white text-center drop-shadow-md">
        Employee Management System
      </h2>
      <div className="border shadow p-6 w-full max-w-sm bg-white dark:bg-gray-800 dark:border-gray-700 rounded-lg">
        <h2 className="text-2xl font-bold mb-4 text-teal-700 dark:text-white">Login</h2>
        {error && <p className="text-red-500">{error}</p>}
        <form onSubmit={handleSubmit}>
          <div className="mb-4">
            <label htmlFor='email' className="block text-gray-700 dark:text-gray-300 font-medium">Email</label>
            <input
              type="email"
              className="w-full px-3 py-2 border rounded focus:outline-none focus:ring-2 focus:ring-teal-500 dark:bg-gray-700 dark:text-white dark:border-gray-600"
              placeholder="Enter Email"
              onChange={(e) => setEmail(e.target.value)}
            />
          </div>
          <div className="mb-4">
            <label htmlFor='password' className="block text-gray-700 dark:text-gray-300 font-medium">Password</label>
            <input
              type="password"
              className="w-full px-3 py-2 border rounded focus:outline-none focus:ring-2 focus:ring-teal-500 dark:bg-gray-700 dark:text-white dark:border-gray-600"
              placeholder="******"
              onChange={(e) => setPassword(e.target.value)}
            />
          </div>
          <div className="mb-4 flex items-center justify-between">
            <label className="inline-flex items-center">
              <input type="checkbox" className="form-checkbox" />
              <span className="ml-2 text-gray-700 dark:text-gray-300">Remember me</span>
            </label>
            <a href="#" className="text-teal-600 text-sm hover:underline dark:text-teal-400">Forgot password?</a>
          </div>
          <div className="mb-4">
            <button type="submit" className="w-full bg-teal-600 hover:bg-teal-700 text-white font-semibold py-2 rounded">
              Login
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default Login;
