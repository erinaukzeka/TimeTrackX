import React, { useEffect, useState } from 'react';
import axios from 'axios';
import {
  Box,
  Grid,
  Paper,
  Typography,
  CircularProgress,
  Card,
  CardContent,
  useTheme,
} from '@mui/material';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  PieChart,
  Pie,
  Cell,
  ResponsiveContainer,
} from 'recharts';

interface StatisticsResponse {
  totalUsers: number;
  activeUsers: number;
  totalProjects: number;
  activeProjects: number;
  totalTasks: number;
  tasksByStatus: { [key: string]: number };
  averageTimePerProject: { [key: string]: number };
  topUsersByHours: Array<{
    username: string;
    totalHours: number;
    completedTasks: number;
  }>;
  shiftDistribution: { [key: string]: number };
}

const AdminStatistics: React.FC = () => {
  const [stats, setStats] = useState<StatisticsResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const theme = useTheme();

  const COLORS = [
    theme.palette.primary.main,
    theme.palette.secondary.main,
    theme.palette.error.main,
    theme.palette.warning.main,
    theme.palette.success.main,
  ];

  useEffect(() => {
    const fetchStats = async () => {
      try {
        const response = await axios.get<StatisticsResponse>('/api/statistics');
        setStats(response.data);
        setError(null);
      } catch (err) {
        setError('Failed to fetch statistics');
        console.error('Error fetching statistics:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchStats();
  }, []);

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    );
  }

  if (error || !stats) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <Typography color="error">{error || 'No data available'}</Typography>
      </Box>
    );
  }

  const taskStatusData = Object.entries(stats.tasksByStatus).map(([status, count]) => ({
    status,
    count,
  }));

  const projectTimeData = Object.entries(stats.averageTimePerProject).map(([project, hours]) => ({
    project,
    hours: Number(hours.toFixed(2)),
  }));

  const shiftData = Object.entries(stats.shiftDistribution).map(([type, count]) => ({
    type,
    count,
  }));

  return (
    <Box p={3}>
      <Typography variant="h4" gutterBottom>
        Admin Dashboard Statistics
      </Typography>

      <Grid container spacing={3}>
        {/* Summary Cards */}
        <Grid item xs={12} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Total Users
              </Typography>
              <Typography variant="h5">
                {stats.totalUsers} ({stats.activeUsers} active)
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Total Projects
              </Typography>
              <Typography variant="h5">
                {stats.totalProjects} ({stats.activeProjects} active)
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Total Tasks
              </Typography>
              <Typography variant="h5">{stats.totalTasks}</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Task Completion Rate
              </Typography>
              <Typography variant="h5">
                {((stats.tasksByStatus['Completed'] || 0) / stats.totalTasks * 100).toFixed(1)}%
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        {/* Charts */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 2, height: '400px' }}>
            <Typography variant="h6" gutterBottom>
              Tasks by Status
            </Typography>
            <ResponsiveContainer>
              <BarChart data={taskStatusData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="status" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Bar dataKey="count" fill={theme.palette.primary.main} />
              </BarChart>
            </ResponsiveContainer>
          </Paper>
        </Grid>

        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 2, height: '400px' }}>
            <Typography variant="h6" gutterBottom>
              Average Time per Project (Hours)
            </Typography>
            <ResponsiveContainer>
              <BarChart data={projectTimeData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="project" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Bar dataKey="hours" fill={theme.palette.secondary.main} />
              </BarChart>
            </ResponsiveContainer>
          </Paper>
        </Grid>

        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 2, height: '400px' }}>
            <Typography variant="h6" gutterBottom>
              Top 5 Users by Hours Worked
            </Typography>
            <ResponsiveContainer>
              <BarChart data={stats.topUsersByHours}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="username" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Bar dataKey="totalHours" name="Total Hours" fill={theme.palette.success.main} />
                <Bar dataKey="completedTasks" name="Completed Tasks" fill={theme.palette.info.main} />
              </BarChart>
            </ResponsiveContainer>
          </Paper>
        </Grid>

        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 2, height: '400px' }}>
            <Typography variant="h6" gutterBottom>
              Shift Distribution
            </Typography>
            <ResponsiveContainer>
              <PieChart>
                <Pie
                  data={shiftData}
                  dataKey="count"
                  nameKey="type"
                  cx="50%"
                  cy="50%"
                  outerRadius={130}
                  label
                >
                  {shiftData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip />
                <Legend />
              </PieChart>
            </ResponsiveContainer>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default AdminStatistics; 