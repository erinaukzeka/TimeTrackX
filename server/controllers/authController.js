import bcrypt from 'bcryptjs';
import jwt from 'jsonwebtoken';
import { pool } from '../config/db.js';

const JWT_SECRET = process.env.JWT_SECRET || 'supersecret';

export const register = async (req, res) => {
  const { email, password, role } = req.body;

  try {
    const poolConn = await pool;
    const hashedPassword = await bcrypt.hash(password, 10);

    // kontrollo nese ekziston user
    const check = await poolConn.request()
      .input('email', email)
      .query('SELECT * FROM users WHERE email = @email');

    if (check.recordset.length > 0) {
      return res.status(400).json({ success: false, error: 'User already exists' });
    }

    // regjistro user-in
    await poolConn.request()
      .input('email', email)
      .input('password', hashedPassword)
      .input('role', role)
      .query('INSERT INTO users (email, password, role) VALUES (@email, @password, @role)');

    res.status(201).json({ success: true, message: 'User registered successfully' });

  } catch (err) {
    console.error('Register error:', err);
    res.status(500).json({ success: false, error: 'Server error' });
  }
};

export const login = async (req, res) => {
  const { email, password } = req.body;

  try {
    const poolConn = await pool;
    const result = await poolConn.request()
      .input('email', email)
      .query('SELECT * FROM users WHERE email = @email');

    if (result.recordset.length === 0) {
      return res.status(404).json({ success: false, error: 'User not found' });
    }

    const user = result.recordset[0];
    const isMatch = await bcrypt.compare(password, user.password);
    if (!isMatch) {
      return res.status(401).json({ success: false, error: 'Invalid credentials' });
    }

    const token = jwt.sign(
      { id: user.id, role: user.role },
      JWT_SECRET,
      { expiresIn: '1h' }
    );

    res.status(200).json({
      success: true,
      token,
      user: {
        id: user.id,
        email: user.email,
        role: user.role
      }
    });

  } catch (err) {
    console.error('Login error:', err);
    res.status(500).json({ success: false, error: 'Server error' });
  }
};
