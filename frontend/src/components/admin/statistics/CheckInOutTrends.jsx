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
    Alert,
    CircularProgress,
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

const CheckInOutTrends = () => {
    const [trendsData, setTrendsData] = useState({
        checkInTrends: [],
        checkOutTrends: []
    });
    const [timeRange, setTimeRange] = useState('week');
    const [viewType, setViewType] = useState('daily'); // daily, hourly
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    useEffect(() => {
        fetchTrendsData();
    }, [timeRange, viewType]);

    const fetchTrendsData = async () => {
        try {
            setLoading(true);
            setError('');
            const response = await axios.get(
                `http://localhost:5000/api/statistics/checkin-trends?timeRange=${timeRange}&viewType=${viewType}`,
                {
                    headers: {
                        "Authorization": `Bearer ${localStorage.getItem('token')}`
                    }
                }
            );
            if (response.data.success) {
                setTrendsData(response.data.trends);
            }
        } catch (error) {
            console.error('Error fetching check-in/out trends:', error);
            setError(error.response?.data?.error || 'Failed to fetch check-in/out trends');
        } finally {
            setLoading(false);
        }
    };

    const formatXAxis = (value) => {
        if (viewType === 'hourly') {
            return `${value}:00`;
        }
        return value; // For daily view, return the day name as is
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
                    Check-in/Check-out Trends
                </Typography>
                <Box sx={{ display: 'flex', gap: 2 }}>
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
                    <FormControl sx={{ minWidth: 150 }}>
                        <InputLabel>View Type</InputLabel>
                        <Select
                            value={viewType}
                            label="View Type"
                            onChange={(e) => setViewType(e.target.value)}
                        >
                            <MenuItem value="daily">Daily</MenuItem>
                            <MenuItem value="hourly">Hourly</MenuItem>
                        </Select>
                    </FormControl>
                </Box>
            </Box>

            {error && (
                <Alert severity="error" sx={{ mb: 3 }}>
                    {error}
                </Alert>
            )}

            <Grid container spacing={3}>
                <Grid item xs={12} md={6}>
                    <Paper sx={{ p: 3 }}>
                        <Typography variant="h6" gutterBottom>
                            Check-in Trends
                        </Typography>
                        <ResponsiveContainer width="100%" height={300}>
                            <BarChart data={trendsData.checkInTrends}>
                                <CartesianGrid strokeDasharray="3 3" />
                                <XAxis dataKey="time" tickFormatter={formatXAxis} />
                                <YAxis />
                                <Tooltip />
                                <Legend />
                                <Bar dataKey="count" fill="#4CAF50" name="Check-ins" />
                            </BarChart>
                        </ResponsiveContainer>
                    </Paper>
                </Grid>
                <Grid item xs={12} md={6}>
                    <Paper sx={{ p: 3 }}>
                        <Typography variant="h6" gutterBottom>
                            Check-out Trends
                        </Typography>
                        <ResponsiveContainer width="100%" height={300}>
                            <BarChart data={trendsData.checkOutTrends}>
                                <CartesianGrid strokeDasharray="3 3" />
                                <XAxis dataKey="time" tickFormatter={formatXAxis} />
                                <YAxis />
                                <Tooltip />
                                <Legend />
                                <Bar dataKey="count" fill="#F44336" name="Check-outs" />
                            </BarChart>
                        </ResponsiveContainer>
                    </Paper>
                </Grid>
            </Grid>
        </div>
    );
};

export default CheckInOutTrends; 