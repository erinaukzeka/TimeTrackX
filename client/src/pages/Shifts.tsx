import React, { useState, useEffect } from 'react';
import {
  Box,
  Button,
  Container,
  Typography,
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
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  SelectChangeEvent,
  IconButton,
  Chip,
  List,
  ListItem,
  ListItemText,
  Checkbox,
} from '@mui/material';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { LocalizationProvider, TimePicker } from '@mui/x-date-pickers';
import { format } from 'date-fns';
import { Edit as EditIcon, Group as GroupIcon } from '@mui/icons-material';
import axios from '../axios';

type ShiftType = 'Morning' | 'Evening' | 'Night';

interface User {
  id: number;
  username: string;
  email: string;
  role: string;
}

interface Shift {
  id: number;
  type: number;
  startTime: string;
  endTime: string;
  description: string;
  isActive: boolean;
  assignedEmployees: User[];
}

interface NewShift {
  type: ShiftType;
  startTime: Date;
  endTime: Date;
  description: string;
}

const getShiftTypeString = (type: number): string => {
  switch (type) {
    case 0:
      return 'Morning';
    case 1:
      return 'Evening';
    case 2:
      return 'Night';
    default:
      return 'Unknown';
  }
};

const Shifts: React.FC = () => {
  const [shifts, setShifts] = useState<Shift[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [open, setOpen] = useState(false);
  const [assignDialogOpen, setAssignDialogOpen] = useState(false);
  const [selectedShift, setSelectedShift] = useState<Shift | null>(null);
  const [selectedUsers, setSelectedUsers] = useState<number[]>([]);
  const [newShift, setNewShift] = useState<NewShift>({
    type: 'Morning',
    startTime: new Date(),
    endTime: new Date(),
    description: '',
  });

  useEffect(() => {
    fetchShifts();
    fetchUsers();
  }, []);

  const fetchShifts = async () => {
    try {
      const response = await axios.get<Shift[]>('/api/shift');
      setShifts(response.data);
    } catch (error) {
      console.error('Error fetching shifts:', error);
    }
  };

  const fetchUsers = async () => {
    try {
      const response = await axios.get<User[]>('/api/users');
      setUsers(response.data);
    } catch (error) {
      console.error('Error fetching users:', error);
    }
  };

  const handleOpen = () => setOpen(true);
  const handleClose = () => setOpen(false);

  const handleAssignDialogOpen = (shift: Shift) => {
    setSelectedShift(shift);
    setSelectedUsers(shift.assignedEmployees.map(u => u.id));
    setAssignDialogOpen(true);
  };

  const handleAssignDialogClose = () => {
    setAssignDialogOpen(false);
    setSelectedShift(null);
    setSelectedUsers([]);
  };

  const handleUserToggle = (userId: number) => {
    setSelectedUsers(prev => {
      if (prev.includes(userId)) {
        return prev.filter(id => id !== userId);
      } else {
        return [...prev, userId];
      }
    });
  };

  const handleAssignUsers = async () => {
    if (!selectedShift) return;

    try {
      await axios.put(`/api/shift/${selectedShift.id}`, {
        type: selectedShift.type,
        startTime: selectedShift.startTime,
        endTime: selectedShift.endTime,
        description: selectedShift.description,
        assignedEmployeeIds: selectedUsers
      });
      handleAssignDialogClose();
      fetchShifts();
    } catch (error) {
      console.error('Error assigning users to shift:', error);
    }
  };

  const handleSubmit = async () => {
    try {
      const createShiftDto = {
        type: 0,
        startTime: format(newShift.startTime, 'HH:mm:ss'),
        endTime: format(newShift.endTime, 'HH:mm:ss'),
        description: newShift.description,
        assignedEmployeeIds: []
      };

      switch (newShift.type) {
        case 'Morning':
          createShiftDto.type = 0;
          break;
        case 'Evening':
          createShiftDto.type = 1;
          break;
        case 'Night':
          createShiftDto.type = 2;
          break;
      }

      await axios.post('/api/shift', createShiftDto);
      handleClose();
      fetchShifts();
    } catch (error) {
      console.error('Error creating shift:', error);
    }
  };

  const handleTypeChange = (event: SelectChangeEvent<ShiftType>) => {
    setNewShift({ ...newShift, type: event.target.value as ShiftType });
  };

  const handleDescriptionChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setNewShift({ ...newShift, description: event.target.value });
  };

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" component="h1">
          Shift Schedule
        </Typography>
        <Button variant="contained" color="primary" onClick={handleOpen}>
          Add New Shift
        </Button>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Type</TableCell>
              <TableCell>Start Time</TableCell>
              <TableCell>End Time</TableCell>
              <TableCell>Description</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Assigned Employees</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {shifts.map((shift) => (
              <TableRow key={shift.id}>
                <TableCell>{getShiftTypeString(shift.type)}</TableCell>
                <TableCell>{shift.startTime}</TableCell>
                <TableCell>{shift.endTime}</TableCell>
                <TableCell>{shift.description}</TableCell>
                <TableCell>{shift.isActive ? 'Active' : 'Inactive'}</TableCell>
                <TableCell>
                  {shift.assignedEmployees.map(employee => (
                    <Chip
                      key={employee.id}
                      label={employee.username}
                      size="small"
                      sx={{ m: 0.5 }}
                    />
                  ))}
                </TableCell>
                <TableCell>
                  <IconButton
                    size="small"
                    onClick={() => handleAssignDialogOpen(shift)}
                    title="Assign Users"
                  >
                    <GroupIcon />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Create Shift Dialog */}
      <Dialog open={open} onClose={handleClose}>
        <DialogTitle>Add New Shift</DialogTitle>
        <DialogContent>
          <Box sx={{ mt: 2 }}>
            <FormControl fullWidth sx={{ mb: 2 }}>
              <InputLabel>Shift Type</InputLabel>
              <Select
                value={newShift.type}
                label="Shift Type"
                onChange={handleTypeChange}
              >
                <MenuItem value="Morning">Morning</MenuItem>
                <MenuItem value="Evening">Evening</MenuItem>
                <MenuItem value="Night">Night</MenuItem>
              </Select>
            </FormControl>

            <LocalizationProvider dateAdapter={AdapterDateFns}>
              <TimePicker
                label="Start Time"
                value={newShift.startTime}
                onChange={(newValue) => setNewShift({ ...newShift, startTime: newValue || new Date() })}
                sx={{ mb: 2, width: '100%' }}
              />
              <TimePicker
                label="End Time"
                value={newShift.endTime}
                onChange={(newValue) => setNewShift({ ...newShift, endTime: newValue || new Date() })}
                sx={{ mb: 2, width: '100%' }}
              />
            </LocalizationProvider>

            <TextField
              fullWidth
              label="Description"
              multiline
              rows={3}
              value={newShift.description}
              onChange={handleDescriptionChange}
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose}>Cancel</Button>
          <Button onClick={handleSubmit} variant="contained" color="primary">
            Create Shift
          </Button>
        </DialogActions>
      </Dialog>

      {/* Assign Users Dialog */}
      <Dialog 
        open={assignDialogOpen} 
        onClose={handleAssignDialogClose}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Assign Users to Shift</DialogTitle>
        <DialogContent>
          <List>
            {users.map((user) => (
              <ListItem key={user.id} dense button onClick={() => handleUserToggle(user.id)}>
                <Checkbox
                  edge="start"
                  checked={selectedUsers.includes(user.id)}
                  tabIndex={-1}
                  disableRipple
                />
                <ListItemText 
                  primary={user.username}
                  secondary={user.email}
                />
              </ListItem>
            ))}
          </List>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleAssignDialogClose}>Cancel</Button>
          <Button onClick={handleAssignUsers} variant="contained" color="primary">
            Save Assignments
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default Shifts; 