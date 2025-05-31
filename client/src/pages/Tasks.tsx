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
import { getTasks, getProjects, getUsers, createTask, updateTask, deleteTask } from '../services/api';
import { ProjectTask, Project, User } from '../types';
import { useAuth } from '../contexts/AuthContext';
import { useLoading } from '../contexts/LoadingContext';

const Tasks = () => {
  const { user } = useAuth();
  const { setIsLoading, setLoadingText } = useLoading();
  const [tasks, setTasks] = useState<ProjectTask[]>([]);
  const [projects, setProjects] = useState<Project[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingTask, setEditingTask] = useState<ProjectTask | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    projectId: '',
    assignedUserId: '',
    dueDate: '',
    status: 'Todo',
    priority: '1',
  });

  const fetchData = async () => {
    try {
      setIsLoading(true);
      setLoadingText('Loading tasks...');
      const [tasksData, projectsData, usersData] = await Promise.all([
        getTasks(),
        getProjects(),
        getUsers(),
      ]);
      setTasks(tasksData);
      setProjects(projectsData);
      setUsers(usersData);
    } catch (error) {
      console.error('Error fetching data:', error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleOpenDialog = (task?: ProjectTask) => {
    if (task) {
      setEditingTask(task);
      setFormData({
        name: task.name,
        description: task.description || '',
        projectId: task.projectId.toString(),
        assignedUserId: task.assignedUserId?.toString() || '',
        dueDate: task.dueDate || '',
        status: task.status,
        priority: task.priority.toString(),
      });
    } else {
      setEditingTask(null);
      setFormData({
        name: '',
        description: '',
        
        projectId: '',
        assignedUserId: '',
        dueDate: '',
        status: 'Todo',
        priority: '1',
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingTask(null);
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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setIsLoading(true);
      setLoadingText(editingTask ? 'Updating task...' : 'Creating task...');
      
      const taskData = {
        name: formData.name,
        description: formData.description,
        projectId: parseInt(formData.projectId),
        assignedUserId: formData.assignedUserId ? parseInt(formData.assignedUserId) : undefined,
        dueDate: formData.dueDate || undefined,
        status: formData.status,
        priority: parseInt(formData.priority),
      };

      if (editingTask) {
        await updateTask(editingTask.id, taskData);
      } else {
        await createTask({
          ...taskData,
          createdAt: new Date().toISOString(),
        });
      }
      handleCloseDialog();
      await fetchData();
    } catch (error) {
      console.error('Error saving task:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleDelete = async (taskId: number) => {
    if (window.confirm('Are you sure you want to delete this task?')) {
      try {
        setIsLoading(true);
        setLoadingText('Deleting task...');
        await deleteTask(taskId);
        await fetchData();
      } catch (error) {
        console.error('Error deleting task:', error);
      } finally {
        setIsLoading(false);
      }
    }
  };

  return (
    <Container>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
        <Typography variant="h4" component="h1">
          Tasks
        </Typography>
        <Button variant="contained" color="primary" onClick={() => handleOpenDialog()}>
          Create Task
        </Button>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Project</TableCell>
              <TableCell>Assigned To</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Priority</TableCell>
              <TableCell>Due Date</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {tasks.map((task) => (
              <TableRow key={task.id}>
                <TableCell>{task.name}</TableCell>
                <TableCell>{task.projectName}</TableCell>
                <TableCell>{task.assignedUserName}</TableCell>
                <TableCell>{task.status}</TableCell>
                <TableCell>{task.priority}</TableCell>
                <TableCell>
                  {task.dueDate && new Date(task.dueDate).toLocaleDateString()}
                </TableCell>
                <TableCell>
                  <IconButton onClick={() => handleOpenDialog(task)} size="small">
                    <EditIcon />
                  </IconButton>
                  <IconButton onClick={() => handleDelete(task.id)} size="small">
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
          <DialogTitle>{editingTask ? 'Edit Task' : 'Create Task'}</DialogTitle>
          <DialogContent>
            <TextField
              fullWidth
              label="Name"
              name="name"
              value={formData.name}
              onChange={handleInputChange}
              margin="normal"
              required
            />

            <TextField
              fullWidth
              label="Description"
              name="description"
              value={formData.description}
              onChange={handleInputChange}
              margin="normal"
              multiline
              rows={3}
            />

            <FormControl fullWidth margin="normal" required>
              <InputLabel>Project</InputLabel>
              <Select<string>
                name="projectId"
                value={formData.projectId}
                onChange={handleSelectChange}
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
              <InputLabel>Assigned To</InputLabel>
              <Select<string>
                name="assignedUserId"
                value={formData.assignedUserId}
                onChange={handleSelectChange}
                label="Assigned To"
              >
                <MenuItem value="">
                  <em>None</em>
                </MenuItem>
                {users.map((user) => (
                  <MenuItem key={user.id} value={user.id.toString()}>
                    {user.username}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            <FormControl fullWidth margin="normal" required>
              <InputLabel>Status</InputLabel>
              <Select<string>
                name="status"
                value={formData.status}
                onChange={handleSelectChange}
                label="Status"
              >
                <MenuItem value="Todo">Todo</MenuItem>
                <MenuItem value="InProgress">In Progress</MenuItem>
                <MenuItem value="Done">Done</MenuItem>
              </Select>
            </FormControl>

            <FormControl fullWidth margin="normal" required>
              <InputLabel>Priority</InputLabel>
              <Select<string>
                name="priority"
                value={formData.priority}
                onChange={handleSelectChange}
                label="Priority"
              >
                <MenuItem value="1">Low</MenuItem>
                <MenuItem value="2">Medium</MenuItem>
                <MenuItem value="3">High</MenuItem>
              </Select>
            </FormControl>

            <TextField
              fullWidth
              label="Due Date"
              name="dueDate"
              type="date"
              value={formData.dueDate}
              onChange={handleInputChange}
              margin="normal"
              InputLabelProps={{
                shrink: true,
              }}
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={handleCloseDialog}>Cancel</Button>
            <Button type="submit" variant="contained" color="primary">
              {editingTask ? 'Update' : 'Create'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </Container>
  );
};

export default Tasks; 