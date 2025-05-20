const express = require('express');
const router = express.Router();
const db = require('../db');

// GET all
router.get('/', async (req, res) => {
  try {
    const [rows] = await db.query('SELECT * FROM employees');
    res.json(rows);
  } catch (err) {
    console.error(err);
    res.status(500).send('Gabim gjate marrjes se punetoreve');
  }
});

// POST
router.post('/', async (req, res) => {
  const { first_name, last_name, email, department_id } = req.body;
  try {
    const [result] = await db.query(
      'INSERT INTO employees (first_name, last_name, email, department_id) VALUES (?, ?, ?, ?)',
      [first_name, last_name, email, department_id]
    );
    res.status(201).json({ message: 'Punëtori u shtua me sukses', id: result.insertId });
  } catch (err) {
    console.error(err);
    res.status(500).send('Gabim gjatë shtimit të punëtorit');
  }
});

// PUT
router.put('/:id', async (req, res) => {
  const { id } = req.params;
  const { first_name, last_name, email, department_id } = req.body;
  try {
    await db.query(
      'UPDATE employees SET first_name = ?, last_name = ?, email = ?, department_id = ? WHERE id = ?',
      [first_name, last_name, email, department_id, id]
    );
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
    await db.query('DELETE FROM employees WHERE id = ?', [id]);
    res.json({ message: 'Punëtori u fshi me sukses' });
  } catch (err) {
    console.error(err);
    res.status(500).send('Gabim gjatë fshirjes së punëtorit');
  }
});

module.exports = router;