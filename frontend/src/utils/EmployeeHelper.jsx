export const fetchDepartments = async () => {
    try{
        const response = await axois.get(
            "http://localhost:5000/api/department",
            {
                headers: {
                    Authorization: `Bearer ${localStorage.getItem("token")}`,
                },
            }
        );
        if (response.data.success){
        }
    }catch (error) {
        if(error.response && !error.response.data.success){
            alert(error.response.data.error)
        }
    }
};