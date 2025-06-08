import React from 'react';
import {
  Box,
  Typography,
  Button,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  IconButton,
  MenuItem,
  Chip,
  Container,
} from '@mui/material';
import { Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon } from '@mui/icons-material';
import { User } from '../types';
import { getUsers, createUser, updateUser, deleteUser } from '../services/api';
import { useLoading } from '../contexts/LoadingContext';
import axios from '../axios';

const roleOptions = ['Admin', 'Employee'];

interface FormData {
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  password: string;
}

const Users: React.FC = () => {
  const { setIsLoading, setLoadingText } = useLoading();
  const [users, setUsers] = React.useState<User[]>([]);
  const [dialogOpen, setDialogOpen] = React.useState(false);
  const [editingUser, setEditingUser] = React.useState<User | null>(null);
  const [formData, setFormData] = React.useState<FormData>({
    username: '',
    email: '',
    firstName: '',
    lastName: '',
    role: 'Employee',
    password: '',
  });

  const fetchUsers = async () => {
    try {
      setIsLoading(true);
      setLoadingText('Loading users...');
      const response = await axios.get<User[]>('/api/users');
      setUsers(response.data);
    } catch (error) {
      console.error('Error fetching users:', error);
    } finally {
      setIsLoading(false);
    }
  };

  React.useEffect(() => {
    fetchUsers();
  }, []);

  const handleDialogOpen = (user?: User) => {
    if (user) {
      setEditingUser(user);
      setFormData({
        username: user.username,
        email: user.email,
        firstName: user.firstName,
        lastName: user.lastName,
        role: user.role,
        password: '',
      });
    } else {
      setFormData({
        username: '',
        email: '',
        firstName: '',
        lastName: '',
        role: 'Employee',
        password: '',
      });
    }
    setDialogOpen(true);
  };

  const handleDialogClose = () => {
    setDialogOpen(false);
    setFormData({
      username: '',
      email: '',
      firstName: '',
      lastName: '',
      role: 'Employee',
      password: '',
    });
    setEditingUser(null);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>, field: keyof FormData) => {
    const { value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [field]: value
    }));
  };

  const handleSubmit = async () => {
    try {
      setIsLoading(true);
      setLoadingText(editingUser ? 'Updating user...' : 'Creating user...');
      
      if (editingUser) {
        await axios.put(`/api/users/${editingUser.id}`, formData);
      } else {
        await axios.post('/api/users', formData);
      }
      handleDialogClose();
      await fetchUsers();
    } catch (error) {
      console.error('Error saving user:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleDelete = async (userId: number) => {
    if (window.confirm('Are you sure you want to delete this user?')) {
      try {
        setIsLoading(true);
        setLoadingText('Deleting user...');
        await deleteUser(userId);
        await fetchUsers();
      } catch (error) {
        console.error('Error deleting user:', error);
      } finally {
        setIsLoading(false);
      }
    }
  };

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
        <Typography variant="h4">Users</Typography>
        <Button
          variant="contained"
          color="primary"
          startIcon={<AddIcon />}
          onClick={() => handleDialogOpen()}
        >
          New User
        </Button>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Username</TableCell>
              <TableCell>Email</TableCell>
              <TableCell>Name</TableCell>
              <TableCell>Role</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {users.map((user) => (
              <TableRow key={user.id}>
                <TableCell>{user.username}</TableCell>
                <TableCell>{user.email}</TableCell>
                <TableCell>
                  {user.firstName} {user.lastName}
                </TableCell>
                <TableCell>
                  <Chip
                    label={user.role}
                    color={user.role === 'Admin' ? 'primary' : 'default'}
                    size="small"
                  />
                </TableCell>
                <TableCell>
                  <Chip
                    label={user.isActive ? 'Active' : 'Inactive'}
                    color={user.isActive ? 'success' : 'error'}
                    size="small"
                  />
                </TableCell>
                <TableCell>
                  <IconButton
                    size="small"
                    onClick={() => handleDialogOpen(user)}
                    color="primary"
                  >
                    <EditIcon />
                  </IconButton>
                  <IconButton
                    size="small"
                    onClick={() => handleDelete(user.id)}
                    color="error"
                  >
                    <DeleteIcon />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={dialogOpen} onClose={handleDialogClose}>
        <DialogTitle>{editingUser ? 'Edit User' : 'Add New User'}</DialogTitle>
        <DialogContent>
          <TextField
            fullWidth
            label="Username"
            value={formData.username}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleInputChange(e, 'username')}
            margin="normal"
            required
          />
          <TextField
            fullWidth
            label="Email"
            type="email"
            value={formData.email}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleInputChange(e, 'email')}
            margin="normal"
            required
          />
          <TextField
            fullWidth
            label="First Name"
            value={formData.firstName}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleInputChange(e, 'firstName')}
            margin="normal"
          />
          <TextField
            fullWidth
            label="Last Name"
            value={formData.lastName}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleInputChange(e, 'lastName')}
            margin="normal"
          />
          <TextField
            select
            fullWidth
            label="Role"
            value={formData.role}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleInputChange(e, 'role')}
            margin="normal"
            required
          >
            <MenuItem value="Admin">Admin</MenuItem>
            <MenuItem value="Employee">Employee</MenuItem>
          </TextField>
          <TextField
            fullWidth
            label="Password"
            type="password"
            value={formData.password}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleInputChange(e, 'password')}
            margin="normal"
            required={!editingUser}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleDialogClose}>Cancel</Button>
          <Button
            onClick={handleSubmit}
            variant="contained"
            color="primary"
            disabled={!formData.username || !formData.email || (!editingUser && !formData.password)}
          >
            {editingUser ? 'Save' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default Users; 