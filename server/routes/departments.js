import express from 'express';
import { pool } from '../db.js';
import sql from 'mssql';

const router = express.Router();

// GET 
router.get('/', async (req, res) => {
  try {
    const conn = await pool;
    const result = await conn.request().query('SELECT * FROM departments');
    res.json(result.recordset);
  } catch (err) {
    console.error(err);
    res.status(500).send('Gabim gjatë marrjes së departamenteve');
  }
});

// POST
router.post('/', async (req, res) => {
  const { name } = req.body;
  try {
    const conn = await pool;
    const result = await conn.request()
      .input('name', sql.VarChar, name)
      .query('INSERT INTO departments (name) VALUES (@name)');

    res.status(201).json({
      message: 'Departamenti u shtua me sukses',
      rowsAffected: result.rowsAffected[0]
    });
  } catch (err) {
    console.error(err);
    res.status(500).send('Gabim gjatë shtimit të departamentit');
  }
});

// PUT 
router.put('/:id', async (req, res) => {
  const { id } = req.params;
  const { name } = req.body;

  try {
    const conn = await pool;
    await conn.request()
      .input('id', sql.Int, id)
      .input('name', sql.VarChar, name)
      .query('UPDATE departments SET name = @name WHERE id = @id');

    res.json({ message: 'Departamenti u përditësua me sukses' });
  } catch (err) {
    console.error(err);
    res.status(500).send('Gabim gjatë përditësimit të departamentit');
  }
});

// DELETE 
router.delete('/:id', async (req, res) => {
  const { id } = req.params;

  try {
    const conn = await pool;
    await conn.request()
      .input('id', sql.Int, id)
      .query('DELETE FROM departments WHERE id = @id');

    res.json({ message: 'Departamenti u fshi me sukses' });
  } catch (err) {
    console.error(err);
    res.status(500).send('Gabim gjatë fshirjes së departamentit');
  }
});

export default router;
