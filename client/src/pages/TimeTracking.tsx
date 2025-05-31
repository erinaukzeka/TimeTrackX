import React, { useState, useEffect } from 'react';
import {
  Container,
  Typography,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  MenuItem,
  IconButton,
  Box,
  FormControl,
  InputLabel,
  Select,
  SelectChangeEvent,
} from '@mui/material';
import { Edit as EditIcon, Delete as DeleteIcon } from '@mui/icons-material';
import {
  getTimeEntries,
  createTimeEntry,
  updateTimeEntry,
  deleteTimeEntry,
  stopTimeEntry,
  getProjects,
  getProjectTasks,
} from '../services/api';
import { TimeEntry, Project, ProjectTask } from '../types';
import { useAuth } from '../contexts/AuthContext';
import { useLoading } from '../contexts/LoadingContext';

const TimeTracking = () => {
  const { user } = useAuth();
  const { setIsLoading, setLoadingText } = useLoading();
  const [timeEntries, setTimeEntries] = useState<TimeEntry[]>([]);
  const [projects, setProjects] = useState<Project[]>([]);
  const [tasks, setTasks] = useState<ProjectTask[]>([]);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingEntry, setEditingEntry] = useState<TimeEntry | null>(null);
  const [formData, setFormData] = useState({
    projectId: '',
    taskId: '',
    description: '',
  });

  const fetchData = async () => {
    try {
      setIsLoading(true);
      setLoadingText('Loading time entries...');
      const [entriesData, projectsData] = await Promise.all([
        getTimeEntries(),
        getProjects(),
      ]);
      setTimeEntries(entriesData);
      setProjects(projectsData);
    } catch (error) {
      console.error('Error fetching data:', error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleProjectChange = async (e: SelectChangeEvent<string>) => {
    const projectId = e.target.value;
    setFormData(prev => ({
      ...prev,
      projectId,
      taskId: '',
    }));

    if (projectId) {
      try {
        setIsLoading(true);
        setLoadingText('Loading tasks...');
        const tasksData = await getProjectTasks(parseInt(projectId));
        setTasks(tasksData);
      } catch (error) {
        console.error('Error fetching tasks:', error);
      } finally {
        setIsLoading(false);
      }
    } else {
      setTasks([]);
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSelectChange = (e: SelectChangeEvent<string>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleOpenDialog = (entry?: TimeEntry) => {
    if (entry) {
      setEditingEntry(entry);
      setFormData({
        projectId: entry.projectId.toString(),
        taskId: entry.taskId?.toString() || '',
        description: entry.description,
      });
    } else {
      setEditingEntry(null);
      setFormData({
        projectId: '',
        taskId: '',
        description: '',
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingEntry(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setIsLoading(true);
      setLoadingText(editingEntry ? 'Updating time entry...' : 'Creating time entry...');
      
      const entryData = {
        projectId: parseInt(formData.projectId),
        taskId: formData.taskId ? parseInt(formData.taskId) : undefined,
        description: formData.description,
      };

      if (editingEntry) {
        await updateTimeEntry(editingEntry.id, entryData);
      } else {
        await createTimeEntry({
          ...entryData,
          startTime: new Date().toISOString(),
        });
      }
      handleCloseDialog();
      await fetchData();
    } catch (error) {
      console.error('Error saving time entry:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleStop = async (entryId: number) => {
    try {
      setIsLoading(true);
      setLoadingText('Stopping time entry...');
      await stopTimeEntry(entryId);
      await fetchData();
    } catch (error) {
      console.error('Error stopping time entry:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleDelete = async (entryId: number) => {
    if (window.confirm('Are you sure you want to delete this time entry?')) {
      try {
        setIsLoading(true);
        setLoadingText('Deleting time entry...');
        await deleteTimeEntry(entryId);
        await fetchData();
      } catch (error) {
        console.error('Error deleting time entry:', error);
      } finally {
        setIsLoading(false);
      }
    }
  };

  return (
    <Container>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
        <Typography variant="h4" component="h1">
          Time Tracking
        </Typography>
        <Button variant="contained" color="primary" onClick={() => handleOpenDialog()}>
          Start New Entry
        </Button>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Project</TableCell>
              <TableCell>Task</TableCell>
              <TableCell>Description</TableCell>
              <TableCell>Start Time</TableCell>
              <TableCell>End Time</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {timeEntries.map((entry) => (
              <TableRow key={entry.id}>
                <TableCell>{entry.projectName}</TableCell>
                <TableCell>{entry.taskName}</TableCell>
                <TableCell>{entry.description}</TableCell>
                <TableCell>
                  {new Date(entry.startTime).toLocaleString()}
                </TableCell>
                <TableCell>
                  {entry.endTime && new Date(entry.endTime).toLocaleString()}
                </TableCell>
                <TableCell>
                  {!entry.endTime && (
                    <Button
                      variant="outlined"
                      color="primary"
                      size="small"
                      onClick={() => handleStop(entry.id)}
                      sx={{ mr: 1 }}
                    >
                      Stop
                    </Button>
                  )}
                  <IconButton onClick={() => handleOpenDialog(entry)} size="small">
                    <EditIcon />
                  </IconButton>
                  <IconButton onClick={() => handleDelete(entry.id)} size="small">
                    <DeleteIcon />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <form onSubmit={handleSubmit}>
          <DialogTitle>{editingEntry ? 'Edit Time Entry' : 'Start New Entry'}</DialogTitle>
          <DialogContent>
            <FormControl fullWidth margin="normal" required>
              <InputLabel>Project</InputLabel>
              <Select<string>
                name="projectId"
                value={formData.projectId}
                onChange={handleProjectChange}
                label="Project"
              >
                {projects.map((project) => (
                  <MenuItem key={project.id} value={project.id.toString()}>
                    {project.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            <FormControl fullWidth margin="normal">
              <InputLabel>Task</InputLabel>
              <Select<string>
                name="taskId"
                value={formData.taskId}
                onChange={handleSelectChange}
                label="Task"
                disabled={!formData.projectId}
              >
                <MenuItem value="">
                  <em>None</em>
                </MenuItem>
                {tasks.map((task) => (
                  <MenuItem key={task.id} value={task.id.toString()}>
                    {task.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            <TextField
              fullWidth
              label="Description"
              name="description"
              value={formData.description}
              onChange={handleInputChange}
              margin="normal"
              multiline
              rows={3}
              required
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={handleCloseDialog}>Cancel</Button>
            <Button type="submit" variant="contained" color="primary">
              {editingEntry ? 'Save' : 'Start'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </Container>
  );
};

export default TimeTracking; 