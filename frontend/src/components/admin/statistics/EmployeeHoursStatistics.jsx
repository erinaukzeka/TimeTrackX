import React, { useState, useEffect } from 'react';
import axios from 'axios';
import {
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Typography,
    Box,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
} from '@mui/material';

const EmployeeHoursStatistics = () => {
    const [employeeStats, setEmployeeStats] = useState([]);
    const [timeRange, setTimeRange] = useState('week'); // week, month, year, all

    useEffect(() => {
        fetchEmployeeStats();
    }, [timeRange]);

    const fetchEmployeeStats = async () => {
        try {
            const response = await axios.get(`http://localhost:5000/api/statistics/employee-hours?timeRange=${timeRange}`, {
                headers: {
                    "Authorization": `Bearer ${localStorage.getItem('token')}`
                }
            });
            if (response.data.success) {
                setEmployeeStats(response.data.stats);
            }
        } catch (error) {
            console.error('Error fetching employee statistics:', error);
            alert('Failed to fetch employee statistics');
        }
    };

    const handleTimeRangeChange = (event) => {
        setTimeRange(event.target.value);
    };

    return (
        <div className="max-w-6xl mx-auto mt-10 p-6">
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
                <Typography variant="h5" component="h2">
                    Employee Hours Statistics
                </Typography>
                <FormControl sx={{ minWidth: 200 }}>
                    <InputLabel>Time Range</InputLabel>
                    <Select
                        value={timeRange}
                        label="Time Range"
                        onChange={handleTimeRangeChange}
                    >
                        <MenuItem value="week">This Week</MenuItem>
                        <MenuItem value="month">This Month</MenuItem>
                        <MenuItem value="year">This Year</MenuItem>
                        <MenuItem value="all">All Time</MenuItem>
                    </Select>
                </FormControl>
            </Box>

            <TableContainer component={Paper}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell>Employee Name</TableCell>
                            <TableCell align="right">Total Hours</TableCell>
                            <TableCell align="right">Number of Projects</TableCell>
                            <TableCell align="right">Average Hours/Day</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {employeeStats.map((stat) => (
                            <TableRow key={stat.userId}>
                                <TableCell>{stat.userName}</TableCell>
                                <TableCell align="right">{stat.totalHours.toFixed(2)}</TableCell>
                                <TableCell align="right">{stat.projectCount}</TableCell>
                                <TableCell align="right">{stat.averageHoursPerDay.toFixed(2)}</TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
        </div>
    );
};

export default EmployeeHoursStatistics; 