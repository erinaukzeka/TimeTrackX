import express from 'express';
import { pool } from '../db.js';
import sql from 'mssql';

const router = express.Router();

// GET 
router.get('/', async (req, res) => {
  try {
    const conn = await pool;
    const result = await conn.request().query('SELECT * FROM work_hours');
    res.json(result.recordset);
  } catch (err) {
    console.error(err);
    res.status(500).send('Gabim gjatë marrjes së orëve të punës');
  }
});

// POST
router.post('/', async (req, res) => {
  const { employee_id, date, hours_worked } = req.body;
  try {
    const conn = await pool;
    await conn.request()
      .input('employee_id', sql.Int, employee_id)
      .input('date', sql.Date, date)
      .input('hours_worked', sql.Float, hours_worked)
      .query('INSERT INTO work_hours (employee_id, date, hours_worked) VALUES (@employee_id, @date, @hours_worked)');
    res.status(201).json({ message: 'Orari i punës u shtua me sukses' });
  } catch (err) {
    console.error(err);
    res.status(500).send('Gabim gjatë shtimit të orarit të punës');
  }
});

// PUT 
router.put('/:id', async (req, res) => {
  const { id } = req.params;
  const { employee_id, date, hours_worked } = req.body;
  try {
    const conn = await pool;
    await conn.request()
      .input('id', sql.Int, id)
      .input('employee_id', sql.Int, employee_id)
      .input('date', sql.Date, date)
      .input('hours_worked', sql.Float, hours_worked)
      .query('UPDATE work_hours SET employee_id = @employee_id, date = @date, hours_worked = @hours_worked WHERE id = @id');
    res.json({ message: 'Orari i punës u përditësua me sukses' });
  } catch (err) {
    console.error(err);
    res.status(500).send('Gabim gjatë përditësimit të orarit të punës');
  }
});

// DELETE 
router.delete('/:id', async (req, res) => {
  const { id } = req.params;
  try {
    const conn = await pool;
    await conn.request().input('id', sql.Int, id).query('DELETE FROM work_hours WHERE id = @id');
    res.json({ message: 'Orari i punës u fshi me sukses' });
  } catch (err) {
    console.error(err);
    res.status(500).send('Gabim gjatë fshirjes së orarit të punës');
  }
});

export default router;
