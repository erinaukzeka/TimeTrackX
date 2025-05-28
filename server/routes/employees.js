import express from 'express';
import { pool } from '../db.js';
import sql from 'mssql';

const router = express.Router();

// GET
router.get('/', async (req, res) => {
  try {
    const conn = await pool;
    const result = await conn.request().query('SELECT * FROM employee');
    res.json(result.recordset);
  } catch (err) {
    console.error(err);
    res.status(500).send('Gabim gjatë marrjes së punëtorëve');
  }
});

// POST 
router.post('/', async (req, res) => {
  const { name, position, department_id } = req.body;
  try {
    const conn = await pool;
    await conn.request()
      .input('name', sql.VarChar, name)
      .input('position', sql.VarChar, position)
      .input('department_id', sql.Int, department_id)
      .query('INSERT INTO employee (name, position, department_id) VALUES (@name, @position, @department_id)');
    res.status(201).json({ message: 'Punëtori u shtua me sukses' });
  } catch (err) {
    console.error(err);
    res.status(500).send('Gabim gjatë shtimit të punëtorit');
  }
});

// PUT 
router.put('/:id', async (req, res) => {
  const { id } = req.params;
  const { name, position, department_id } = req.body;
  try {
    const conn = await pool;
    await conn.request()
      .input('id', sql.Int, id)
      .input('name', sql.VarChar, name)
      .input('position', sql.VarChar, position)
      .input('department_id', sql.Int, department_id)
      .query('UPDATE employee SET name = @name, position = @position, department_id = @department_id WHERE id = @id');
    res.json({ message: 'Punëtori u përditësua me sukses' });
  } catch (err) {
    console.error(err);
    res.status(500).send('Gabim gjatë përditësimit të punëtorit');
  }
});

// DELETE 
router.delete('/:id', async (req, res) => {
  const { id } = req.params;
  try {
    const conn = await pool;
    await conn.request().input('id', sql.Int, id).query('DELETE FROM employee WHERE id = @id');
    res.json({ message: 'Punëtori u fshi me sukses' });
  } catch (err) {
    console.error(err);
    res.status(500).send('Gabim gjatë fshirjes së punëtorit');
  }
});

export default router;
