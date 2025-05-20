import express from 'express';
import cors from 'cors';
import authRoutes from './routes/auth.js';
import connectToDatabase from './db/db.js';
import dotenv from 'dotenv';

dotenv.config(); 

const app = express();

connectToDatabase(); 

app.use(cors());
app.use(express.json());
app.use('/api/auth', authRoutes);
``
const PORT = process.env.PORT || 5000;

app.listen(PORT, () => {
  console.log(`🚀 Server is running on port ${PORT}`);
});
