export interface User {
    id: number;
    username: string;
    email: string;
    firstName: string;
    lastName: string;
    role: string;
    createdAt: string;
    isActive: boolean;
    password?: string;
    assignedProjects?: Project[];
    timeEntries?: TimeEntry[];
}

export interface TimeEntry {
    id: number;
    startTime: string;
    projectName?: string;
    taskName?: string;
    endTime?: string;
    description: string;
    userId: number;
    user?: User;
    projectId: number;
    project?: Project;
    taskId?: number;
    task?: ProjectTask;
}

export interface Project {
    id: number;
    name: string;
    description: string;
    status: string;
    startDate: string;
    endDate?: string;
    createdAt: string;
    assignedUsers?: User[];
    tasks?: ProjectTask[];
}

export interface ProjectTask {
    id: number;
    name: string;
    title?: string;
    projectName?: string;
    assignedUserName?: string;
    description: string;
    status: string;
    priority: number;
    dueDate?: string;
    createdAt: string;
    projectId: number;
    project?: Project;
    assignedUserId?: number;
    assignedUser?: User;
    timeEntries?: TimeEntry[];
} 