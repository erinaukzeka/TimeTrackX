import React from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import {
    Drawer,
    List,
    ListItem,
    ListItemIcon,
    ListItemText,
    Toolbar,
    useTheme,
    useMediaQuery,
} from '@mui/material';
import {
    Dashboard as DashboardIcon,
    Timer as TimerIcon,
    Folder as ProjectsIcon,
    Assignment as TasksIcon,
    People as UsersIcon,
} from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';

interface SidebarProps {
    open?: boolean;
    onClose?: () => void;
}

const drawerWidth = 240;

const Sidebar = ({ open, onClose }: SidebarProps) => {
    const navigate = useNavigate();
    const location = useLocation();
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
    const { user } = useAuth();

    const menuItems = [
        { path: '/dashboard', label: 'Dashboard', icon: <DashboardIcon /> },
        { path: '/time-tracking', label: 'Time Tracking', icon: <TimerIcon /> },
        { path: '/projects', label: 'Projects', icon: <ProjectsIcon /> },
        { path: '/tasks', label: 'Tasks', icon: <TasksIcon /> },
        ...(user?.role === 'Admin' ? [
            { path: '/users', label: 'Users', icon: <UsersIcon /> },
        ] : []),
    ];

    const drawer = (
        <>
            <Toolbar />
            <List>
                {menuItems.map((item) => (
                    <ListItem
                        button
                        key={item.path}
                        selected={location.pathname === item.path}
                        onClick={() => {
                            navigate(item.path);
                            if (onClose) onClose();
                        }}
                    >
                        <ListItemIcon>{item.icon}</ListItemIcon>
                        <ListItemText primary={item.label} />
                    </ListItem>
                ))}
            </List>
        </>
    );

    return (
        <Drawer
            variant={isMobile ? 'temporary' : 'permanent'}
            open={isMobile ? open : true}
            onClose={onClose}
            sx={{
                width: drawerWidth,
                flexShrink: 0,
                '& .MuiDrawer-paper': {
                    width: drawerWidth,
                    boxSizing: 'border-box',
                },
            }}
        >
            {drawer}
        </Drawer>
    );
};

export default Sidebar; 