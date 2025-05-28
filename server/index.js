import express from 'express';
import cors from 'cors';
import authRouter from './routes/auth.js';
import { pool } from './db.js';  
import dotenv from 'dotenv';
import departmentRouter from './routes/departments.js'

dotenv.config(); 

const app = express();

app.use(cors());
app.use(express.json());
app.use('/api/auth', authRouter);
app.use('/api/departments', departmentRouter);


const PORT = process.env.PORT || 5000;

app.listen(PORT, () => {
  console.log(`🚀 Server is running on port ${PORT}`);
});
