const express = require('express');
const router = express.Router();
const db = require('../db');

// GET 
router.get('/', async (req, res) => {
  try {
    const [rows] = await db.query('SELECT * FROM departments');
    res.json(rows);
  } catch (err) {
    console.error(err);
    res.status(500).send('Gabim gjatë marrjes së departamenteve');
  }
});

// POST 
router.post('/', async (req, res) => {
  const { name } = req.body;
  try {
    const [result] = await db.query(
      'INSERT INTO departments (name) VALUES (?)',
      [name]
    );
    res.status(201).json({ message: 'Departamenti u shtua me sukses', id: result.insertId });
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
    await db.query(
      'UPDATE departments SET name = ? WHERE id = ?',
      [name, id]
    );
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
    await db.query('DELETE FROM departments WHERE id = ?', [id]);
    res.json({ message: 'Departamenti u fshi me sukses' });
  } catch (err) {
    console.error(err);
    res.status(500).send('Gabim gjatë fshirjes së departamentit');
  }
});

module.exports = router;
