import axios from 'axios';
import { User, TimeEntry, Project, ProjectTask } from '../types';

const API_URL = 'http://localhost:5054/api';

// Create axios instance with default config
const api = axios.create({
    baseURL: API_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Add token to requests if available
api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

// Authentication types
export interface LoginRequest {
    username: string;
    password: string;
}

export interface RegisterRequest {
    username: string;
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    role: string;
}

export interface AuthResponse {
    token: string;
    username: string;
    role: string;
    userId: number;
}

// Authentication API
export const login = async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/users/login', data);
    localStorage.setItem('token', response.data.token);
    localStorage.setItem('user', JSON.stringify(response.data));
    return response.data;
};

export const register = async (data: RegisterRequest): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/users/register', data);
    localStorage.setItem('token', response.data.token);
    localStorage.setItem('user', JSON.stringify(response.data));
    return response.data;
};

export const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
};

// Users API
export const getUsers = async (): Promise<User[]> => {
    const response = await api.get<User[]>('/users');
    return response.data;
};

export const getUser = async (id: number): Promise<User> => {
    const response = await api.get<User>(`/users/${id}`);
    return response.data;
};

export const createUser = async (user: Partial<User>): Promise<User> => {
    const response = await api.post<User>('/users', user);
    return response.data;
};

export const updateUser = async (id: number, user: Partial<User>): Promise<void> => {
    await api.put<void>(`/users/${id}`, user);
};

export const deleteUser = async (id: number): Promise<void> => {
    await api.delete<void>(`/users/${id}`);
};

// Time Entries API
export const getTimeEntries = async (): Promise<TimeEntry[]> => {
    const response = await api.get<TimeEntry[]>('/timeentries');
    return response.data;
};

export const getUserTimeEntries = async (userId: number): Promise<TimeEntry[]> => {
    const response = await api.get<TimeEntry[]>(`/timeentries/user/${userId}`);
    return response.data;
};

export const createTimeEntry = async (entry: Partial<TimeEntry>): Promise<TimeEntry> => {
    const response = await api.post<TimeEntry>('/timeentries', entry);
    return response.data;
};

export const stopTimeEntry = async (id: number): Promise<void> => {
    await api.put<void>(`/timeentries/${id}/stop`);
};

export const updateTimeEntry = async (id: number, entry: Partial<TimeEntry>): Promise<void> => {
    await api.put<void>(`/timeentries/${id}`, entry);
};

export const deleteTimeEntry = async (id: number): Promise<void> => {
    await api.delete<void>(`/timeentries/${id}`);
};

// Projects API
export const getProjects = async (): Promise<Project[]> => {
    const response = await api.get<Project[]>('/projects');
    return response.data;
};

export const getUserProjects = async (userId: number): Promise<Project[]> => {
    const response = await api.get<Project[]>(`/projects/user/${userId}`);
    return response.data;
};

export const getProject = async (id: number): Promise<Project> => {
    const response = await api.get<Project>(`/projects/${id}`);
    return response.data;
};

export const createProject = async (project: Partial<Project>): Promise<Project> => {
    const response = await api.post<Project>('/projects', project);
    return response.data;
};

export const updateProject = async (id: number, project: Partial<Project>): Promise<void> => {
    await api.put<void>(`/projects/${id}`, project);
};

export const assignUsersToProject = async (projectId: number, userIds: number[]): Promise<void> => {
    await api.post<void>(`/projects/${projectId}/users`, userIds);
};

export const deleteProject = async (id: number): Promise<void> => {
    await api.delete<void>(`/projects/${id}`);
};

// Tasks API
export const getTasks = async (): Promise<ProjectTask[]> => {
    const response = await api.get<ProjectTask[]>('/tasks');
    return response.data;
};

export const getProjectTasks = async (projectId: number): Promise<ProjectTask[]> => {
    const response = await api.get<ProjectTask[]>(`/tasks/project/${projectId}`);
    return response.data;
};

export const getUserTasks = async (userId: number): Promise<ProjectTask[]> => {
    const response = await api.get<ProjectTask[]>(`/tasks/user/${userId}`);
    return response.data;
};

export const createTask = async (task: Partial<ProjectTask>): Promise<ProjectTask> => {
    const response = await api.post<ProjectTask>('/tasks', task);
    return response.data;
};

export const updateTask = async (id: number, task: Partial<ProjectTask>): Promise<void> => {
    await api.put<void>(`/tasks/${id}`, task);
};

export const updateTaskStatus = async (id: number, status: string): Promise<void> => {
    await api.put<void>(`/tasks/${id}/status`, JSON.stringify(status));
};

export const assignTask = async (id: number, userId: number | null): Promise<void> => {
    await api.put<void>(`/tasks/${id}/assign`, userId);
};

export const deleteTask = async (id: number): Promise<void> => {
    await api.delete<void>(`/tasks/${id}`);
};

// Statistics types
export interface StatisticsResponse {
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

// Statistics API
export const getStatistics = async (): Promise<StatisticsResponse> => {
    const response = await api.get<StatisticsResponse>('/statistics');
    return response.data;
}; 