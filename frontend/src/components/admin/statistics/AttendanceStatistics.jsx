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
    Chip,
    TextField,
    TableSortLabel,
} from '@mui/material';
import {
    BarChart,
    Bar,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    Legend,
    ResponsiveContainer
} from 'recharts';

const AttendanceStatistics = () => {
    const [attendanceData, setAttendanceData] = useState({
        lateArrivals: [],
        absences: [],
        summaryByEmployee: []
    });
    const [timeRange, setTimeRange] = useState('month');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [searchTerm, setSearchTerm] = useState('');
    const [orderBy, setOrderBy] = useState('userName');
    const [order, setOrder] = useState('asc');

    useEffect(() => {
        fetchAttendanceData();
    }, [timeRange]);

    const fetchAttendanceData = async () => {
        try {
            setLoading(true);
            setError('');
            const response = await axios.get(
                `http://localhost:5000/api/statistics/attendance?timeRange=${timeRange}`,
                {
                    headers: {
                        "Authorization": `Bearer ${localStorage.getItem('token')}`
                    }
                }
            );
            if (response.data.success) {
                setAttendanceData(response.data.attendance);
            }
        } catch (error) {
            console.error('Error fetching attendance statistics:', error);
            const errorMessage = error.response?.data?.error || 
                               error.message || 
                               'Failed to fetch attendance statistics';
            setError(errorMessage);
        } finally {
            setLoading(false);
        }
    };

    const handleSort = (property) => {
        const isAsc = orderBy === property && order === 'asc';
        setOrder(isAsc ? 'desc' : 'asc');
        setOrderBy(property);
    };

    const sortData = (data) => {
        return data.sort((a, b) => {
            const aValue = a[orderBy];
            const bValue = b[orderBy];

            if (order === 'desc') {
                return bValue > aValue ? 1 : -1;
            }
            return aValue > bValue ? 1 : -1;
        });
    };

    const filterData = (data) => {
        if (!searchTerm) return data;
        return data.filter(employee => 
            employee.userName.toLowerCase().includes(searchTerm.toLowerCase())
        );
    };

    if (loading) {
        return (
            <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
                <CircularProgress />
            </Box>
        );
    }

    const filteredAndSortedEmployees = filterData(
        sortData([...attendanceData.summaryByEmployee])
    );

    return (
        <div className="max-w-6xl mx-auto mt-10 p-6">
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
                <Typography variant="h5" component="h2">
                    Attendance Statistics
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
                {/* Late Arrivals Chart */}
                <Grid item xs={12} md={6}>
                    <Paper sx={{ p: 3 }}>
                        <Typography variant="h6" gutterBottom>
                            Late Arrivals Trend
                        </Typography>
                        <ResponsiveContainer width="100%" height={300}>
                            <BarChart data={attendanceData.lateArrivals}>
                                <CartesianGrid strokeDasharray="3 3" />
                                <XAxis dataKey="date" />
                                <YAxis />
                                <Tooltip />
                                <Legend />
                                <Bar dataKey="count" fill="#FFA726" name="Late Arrivals" />
                            </BarChart>
                        </ResponsiveContainer>
                    </Paper>
                </Grid>

                {/* Absences Chart */}
                <Grid item xs={12} md={6}>
                    <Paper sx={{ p: 3 }}>
                        <Typography variant="h6" gutterBottom>
                            Absences Trend
                        </Typography>
                        <ResponsiveContainer width="100%" height={300}>
                            <BarChart data={attendanceData.absences}>
                                <CartesianGrid strokeDasharray="3 3" />
                                <XAxis dataKey="date" />
                                <YAxis />
                                <Tooltip />
                                <Legend />
                                <Bar dataKey="count" fill="#EF5350" name="Absences" />
                            </BarChart>
                        </ResponsiveContainer>
                    </Paper>
                </Grid>

                {/* Employee Summary Table */}
                <Grid item xs={12}>
                    <Paper sx={{ p: 3 }}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                            <Typography variant="h6">
                                Employee Attendance Summary
                            </Typography>
                            <TextField
                                size="small"
                                label="Search Employee"
                                variant="outlined"
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                            />
                        </Box>
                        <TableContainer>
                            <Table>
                                <TableHead>
                                    <TableRow>
                                        <TableCell>
                                            <TableSortLabel
                                                active={orderBy === 'userName'}
                                                direction={orderBy === 'userName' ? order : 'asc'}
                                                onClick={() => handleSort('userName')}
                                            >
                                                Employee
                                            </TableSortLabel>
                                        </TableCell>
                                        <TableCell align="center">
                                            <TableSortLabel
                                                active={orderBy === 'lateCount'}
                                                direction={orderBy === 'lateCount' ? order : 'asc'}
                                                onClick={() => handleSort('lateCount')}
                                            >
                                                Late Arrivals
                                            </TableSortLabel>
                                        </TableCell>
                                        <TableCell align="center">
                                            <TableSortLabel
                                                active={orderBy === 'absenceCount'}
                                                direction={orderBy === 'absenceCount' ? order : 'asc'}
                                                onClick={() => handleSort('absenceCount')}
                                            >
                                                Absences
                                            </TableSortLabel>
                                        </TableCell>
                                        <TableCell align="center">
                                            <TableSortLabel
                                                active={orderBy === 'attendanceRate'}
                                                direction={orderBy === 'attendanceRate' ? order : 'asc'}
                                                onClick={() => handleSort('attendanceRate')}
                                            >
                                                Attendance Rate
                                            </TableSortLabel>
                                        </TableCell>
                                        <TableCell align="center">
                                            <TableSortLabel
                                                active={orderBy === 'status'}
                                                direction={orderBy === 'status' ? order : 'asc'}
                                                onClick={() => handleSort('status')}
                                            >
                                                Status
                                            </TableSortLabel>
                                        </TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {filteredAndSortedEmployees.length === 0 ? (
                                        <TableRow>
                                            <TableCell colSpan={5} align="center">
                                                No data available
                                            </TableCell>
                                        </TableRow>
                                    ) : (
                                        filteredAndSortedEmployees.map((employee) => (
                                            <TableRow key={employee.userId}>
                                                <TableCell>{employee.userName}</TableCell>
                                                <TableCell align="center">{employee.lateCount}</TableCell>
                                                <TableCell align="center">{employee.absenceCount}</TableCell>
                                                <TableCell align="center">{employee.attendanceRate}%</TableCell>
                                                <TableCell align="center">
                                                    <Chip
                                                        label={employee.status}
                                                        color={
                                                            employee.status === 'Excellent' ? 'success' :
                                                            employee.status === 'Good' ? 'primary' :
                                                            employee.status === 'Fair' ? 'warning' : 'error'
                                                        }
                                                        size="small"
                                                    />
                                                </TableCell>
                                            </TableRow>
                                        ))
                                    )}
                                </TableBody>
                            </Table>
                        </TableContainer>
                    </Paper>
                </Grid>
            </Grid>
        </div>
    );
};

export default AttendanceStatistics; 