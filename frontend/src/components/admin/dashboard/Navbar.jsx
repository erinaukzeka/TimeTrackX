import React from 'react'
import { useAuth } from '../../../context/AuthContext'; 


const Navbar = () => {
   const { user, logout } = useAuth();

    return (
    <div>
      Welcome {user?.name}
      <button onClick={logout}>Logout</button>
    </div>
    );

}

export default Navbar