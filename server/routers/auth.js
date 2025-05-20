import express from 'express';
import { pool } from '../db.js';
import sql from 'mssql';

const router = express.Router();

router.post('/login', async (req, res) => {
  const { email, password } = req.body;

  try {
    const conn = await pool;
    const result = await conn
      .request()
      .input('email', sql.VarChar, email)
      .input('password', sql.VarChar, password)
      .query('SELECT * FROM users WHERE email = @email AND password = @password');

    if (result.recordset.length > 0) {
      return res.json({ success: true, user: result.recordset[0] });
    } else {
      return res.status(401).json({ success: false, message: "Invalid credentials" });
    }
  } catch (err) {
    console.error(err);
    return res.status(500).json({ success: false, message: "Database error" });
  }
});

export default router;
