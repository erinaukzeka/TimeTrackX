import React, { useState, useEffect } from 'react';
import axios from 'axios';
import {
    Paper,
    Typography,
    Box,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    Grid,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Alert,
    CircularProgress,
    Card,
    CardContent,
    Avatar,
    LinearProgress,
} from '@mui/material';
import {
    PieChart,
    Pie,
    Cell,
    ResponsiveContainer,
    Tooltip,
    Legend
} from 'recharts';

const COLORS = ['#4CAF50', '#2196F3', '#FFC107', '#FF5722', '#9C27B0', '#607D8B'];

const ActiveEmployeesStats = () => {
    const [activeData, setActiveData] = useState({
        topEmployees: [],
        projectDistribution: [],
        taskCompletion: []
    });
    const [timeRange, setTimeRange] = useState('month');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    useEffect(() => {
        fetchActiveData();
    }, [timeRange]);

    const fetchActiveData = async () => {
        try {
            setLoading(true);
            setError('');
            const response = await axios.get(
                `http://localhost:5000/api/statistics/active-employees?timeRange=${timeRange}`,
                {
                    headers: {
                        "Authorization": `Bearer ${localStorage.getItem('token')}`
                    }
                }
            );
            if (response.data.success) {
                setActiveData(response.data.data);
            }
        } catch (error) {
            console.error('Error fetching active employees statistics:', error);
            setError(error.response?.data?.error || 'Failed to fetch active employees statistics');
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return (
            <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
                <CircularProgress />
            </Box>
        );
    }

    return (
        <div className="max-w-6xl mx-auto mt-10 p-6">
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
                <Typography variant="h5" component="h2">
                    Most Active Employees
                </Typography>
                <FormControl sx={{ minWidth: 150 }}>
                    <InputLabel>Time Range</InputLabel>
                    <Select
                        value={timeRange}
                        label="Time Range"
                        onChange={(e) => setTimeRange(e.target.value)}
                    >
                        <MenuItem value="week">This Week</MenuItem>
                        <MenuItem value="month">This Month</MenuItem>
                        <MenuItem value="year">This Year</MenuItem>
                    </Select>
                </FormControl>
            </Box>

            {error && (
                <Alert severity="error" sx={{ mb: 3 }}>
                    {error}
                </Alert>
            )}

            <Grid container spacing={3}>
                {/* Top Employees Cards */}
                <Grid item xs={12}>
                    <Typography variant="h6" gutterBottom>
                        Top Performing Employees
                    </Typography>
                    <Grid container spacing={2}>
                        {activeData.topEmployees.length > 0 ? (
                            activeData.topEmployees.map((employee, index) => (
                                <Grid item xs={12} sm={6} md={4} key={employee.userId}>
                                    <Card>
                                        <CardContent>
                                            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                                                <Avatar sx={{ bgcolor: COLORS[index % COLORS.length], mr: 2 }}>
                                                    {employee.userName.charAt(0)}
                                                </Avatar>
                                                <Typography variant="h6">
                                                    {employee.userName}
                                                </Typography>
                                            </Box>
                                            <Box sx={{ mb: 1.5 }}>
                                                <Typography variant="body2" color="text.secondary">
                                                    Hours Worked
                                                </Typography>
                                                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                                                    <Box sx={{ flexGrow: 1, mr: 1 }}>
                                                        <LinearProgress 
                                                            variant="determinate" 
                                                            value={(employee.hoursWorked / activeData.topEmployees[0].hoursWorked) * 100}
                                                            sx={{ height: 8, borderRadius: 5 }}
                                                        />
                                                    </Box>
                                                    <Typography variant="body2">
                                                        {employee.hoursWorked.toFixed(1)}h
                                                    </Typography>
                                                </Box>
                                            </Box>
                                            <Typography variant="body2">
                                                Projects: {employee.projectCount}
                                            </Typography>
                                            <Typography variant="body2">
                                                Tasks Completed: {employee.completedTasks}
                                            </Typography>
                                        </CardContent>
                                    </Card>
                                </Grid>
                            ))
                        ) : (
                            <Grid item xs={12}>
                                <Paper sx={{ p: 3, textAlign: 'center' }}>
                                    <Typography color="text.secondary">
                                        No employee data available
                                    </Typography>
                                </Paper>
                            </Grid>
                        )}
                    </Grid>
                </Grid>

                {/* Project Distribution Chart */}
                <Grid item xs={12} md={6}>
                    <Paper sx={{ p: 3 }}>
                        <Typography variant="h6" gutterBottom>
                            Project Distribution
                        </Typography>
                        <Box sx={{ height: 300 }}>
                            {activeData.projectDistribution.length > 0 ? (
                                <ResponsiveContainer>
                                    <PieChart>
                                        <Pie
                                            data={activeData.projectDistribution}
                                            dataKey="value"
                                            nameKey="name"
                                            cx="50%"
                                            cy="50%"
                                            outerRadius={80}
                                            label
                                        >
                                            {activeData.projectDistribution.map((entry, index) => (
                                                <Cell key={entry.name} fill={COLORS[index % COLORS.length]} />
                                            ))}
                                        </Pie>
                                        <Tooltip />
                                        <Legend />
                                    </PieChart>
                                </ResponsiveContainer>
                            ) : (
                                <Box display="flex" justifyContent="center" alignItems="center" height="100%">
                                    <Typography color="text.secondary">
                                        No project data available
                                    </Typography>
                                </Box>
                            )}
                        </Box>
                    </Paper>
                </Grid>

                {/* Task Completion Stats */}
                <Grid item xs={12} md={6}>
                    <Paper sx={{ p: 3 }}>
                        <Typography variant="h6" gutterBottom>
                            Task Completion Rate
                        </Typography>
                        {activeData.taskCompletion.length > 0 ? (
                            <TableContainer>
                                <Table>
                                    <TableHead>
                                        <TableRow>
                                            <TableCell>Employee</TableCell>
                                            <TableCell align="center">Completed Tasks</TableCell>
                                            <TableCell align="center">Completion Rate</TableCell>
                                        </TableRow>
                                    </TableHead>
                                    <TableBody>
                                        {activeData.taskCompletion.map((employee) => (
                                            <TableRow key={employee.userId}>
                                                <TableCell>{employee.userName}</TableCell>
                                                <TableCell align="center">{employee.completedTasks}</TableCell>
                                                <TableCell align="center">
                                                    <Box sx={{ display: 'flex', alignItems: 'center' }}>
                                                        <Box sx={{ flexGrow: 1, mr: 1 }}>
                                                            <LinearProgress
                                                                variant="determinate"
                                                                value={employee.completionRate}
                                                                sx={{ height: 8, borderRadius: 5 }}
                                                            />
                                                        </Box>
                                                        <Typography variant="body2">
                                                            {employee.completionRate}%
                                                        </Typography>
                                                    </Box>
                                                </TableCell>
                                            </TableRow>
                                        ))}
                                    </TableBody>
                                </Table>
                            </TableContainer>
                        ) : (
                            <Box display="flex" justifyContent="center" alignItems="center" minHeight={200}>
                                <Typography color="text.secondary">
                                    No task completion data available
                                </Typography>
                            </Box>
                        )}
                    </Paper>
                </Grid>
            </Grid>
        </div>
    );
};

export default ActiveEmployeesStats; 