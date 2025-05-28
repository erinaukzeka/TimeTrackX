import express from 'express';
import cors from 'cors';
import authRoutes from './routes/auth.js';
import { pool } from './db.js';  
import dotenv from 'dotenv';
import employeeRoutes from './routes/employee.js';
import workHoursRoutes from './routes/work_hours.js';
import departmentRoutes from './routes/departments.js';

dotenv.config(); 

const app = express();

app.use(cors());
app.use(express.json());
app.use('/api/auth', authRoutes);
app.use('/api/employees', employeeRoutes);
app.use('/api/work-hours', workHoursRoutes);
app.use('/api/departments', departmentRoutes);


const PORT = process.env.PORT || 5000;

app.listen(PORT, () => {
  console.log(`🚀 Server is running on port ${PORT}`);
});
