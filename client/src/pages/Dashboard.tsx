import React from 'react';
import {
  Paper,
  Typography,
  Box,
  Card,
  CardContent,
  List,
  ListItem,
  ListItemText,
} from '@mui/material';
import Grid2 from '@mui/material/Unstable_Grid2';
import { TimeEntry, Project, ProjectTask } from '../types';
import { getUserTimeEntries, getUserProjects, getUserTasks } from '../services/api';

const Dashboard: React.FC = () => {
  const [timeEntries, setTimeEntries] = React.useState<TimeEntry[]>([]);
  const [projects, setProjects] = React.useState<Project[]>([]);
  const [tasks, setTasks] = React.useState<ProjectTask[]>([]);
  const userId = 1; // TODO: Get from auth context

  React.useEffect(() => {
    const fetchData = async () => {
      try {
        const [timeEntriesRes, projectsRes, tasksRes] = await Promise.all([
          getUserTimeEntries(userId),
          getUserProjects(userId),
          getUserTasks(userId),
        ]);

        setTimeEntries(timeEntriesRes);
        setProjects(projectsRes);
        setTasks(tasksRes);
      } catch (error) {
        console.error('Error fetching dashboard data:', error);
      }
    };

    fetchData();
  }, [userId]);

  const totalHoursToday = timeEntries
    .filter((entry) => {
      const today = new Date().toISOString().split('T')[0];
      return entry.startTime.startsWith(today);
    })
    .reduce((total, entry) => {
      if (!entry.endTime) return total;
      const duration = new Date(entry.endTime).getTime() - new Date(entry.startTime).getTime();
      return total + duration / (1000 * 60 * 60);
    }, 0);

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Dashboard
      </Typography>
      <Grid2 container spacing={3}>
        {/* Summary Cards */}
        <Grid2 xs={12} sm={12} md={4}>
          <Paper elevation={2} sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>
              Today's Work
            </Typography>
            <Typography variant="h3">{totalHoursToday.toFixed(1)}h</Typography>
          </Paper>
        </Grid2>
        <Grid2 xs={12} sm={12} md={4}>
          <Paper elevation={2} sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>
              Active Projects
            </Typography>
            <Typography variant="h3">{projects.length}</Typography>
          </Paper>
        </Grid2>
        <Grid2 xs={12} sm={12} md={4}>
          <Paper elevation={2} sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>
              Pending Tasks
            </Typography>
            <Typography variant="h3">
              {tasks.filter((task) => task.status !== 'Completed').length}
            </Typography>
          </Paper>
        </Grid2>

        {/* Recent Time Entries */}
        <Grid2 xs={12} sm={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Recent Time Entries
              </Typography>
              <List>
                {timeEntries.slice(0, 5).map((entry) => (
                  <ListItem key={entry.id}>
                    <ListItemText
                      primary={entry.project?.name}
                      secondary={`${new Date(entry.startTime).toLocaleTimeString()} - ${
                        entry.endTime
                          ? new Date(entry.endTime).toLocaleTimeString()
                          : 'Ongoing'
                      }`}
                    />
                  </ListItem>
                ))}
              </List>
            </CardContent>
          </Card>
        </Grid2>

        {/* Active Tasks */}
        <Grid2 xs={12} sm={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Active Tasks
              </Typography>
              <List>
                {tasks
                  .filter((task) => task.status !== 'Completed')
                  .slice(0, 5)
                  .map((task) => (
                    <ListItem key={task.id}>
                      <ListItemText
                        primary={task.name}
                        secondary={`${task.project?.name} - ${task.status}`}
                      />
                    </ListItem>
                  ))}
              </List>
            </CardContent>
          </Card>
        </Grid2>
      </Grid2>
    </Box>
  );
};

export default Dashboard; 