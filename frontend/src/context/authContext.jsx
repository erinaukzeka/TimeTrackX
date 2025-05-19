import React, { useEffect } from "react";
import { useState } from "react";

const authContext = () => {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

useEffect(() => {
  const verifyUser = async () => {
    try{
      const token = localStorage.getItem("token");
      if(token){
        const response = await axios.get(
          "http://localhost:5000/api/auth/verify",
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );
        if (response.data.success) {
          setUser(response.data.user);
        }
      }else{
        setUser(null);
        setLoading(false);
      }
    } catch (error) {
      if (error.response && !error.response.data.error){
        setUser(null)
      }
    }
  }
})

    const login = a
  return (
    <div>authContext</div>
  )
}

export default authContext