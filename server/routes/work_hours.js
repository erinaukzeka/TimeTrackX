const express = require('express');
const router = express.Router();
const db = require('../db');

// GET
router.get('/', async (req, res) => {
  try {
    const [rows] = await db.query('SELECT * FROM work_hours');
    res.json(rows);
  } catch (err) {
    console.error(err);
    res.status(500).send('Gabim gjatë marrjes së orëve të punës');
  }
});

// POST 
router.post('/', async (req, res) => {
  const { employee_id, check_in, check_out } = req.body;

  try {
    const [result] = await db.query(
      'INSERT INTO work_hours (employee_id, check_in, check_out) VALUES (?, ?, ?)',
      [employee_id, check_in, check_out]
    );
    res.status(201).json({ message: 'Orari i punës u shtua me sukses', id: result.insertId });
  } catch (err) {
    console.error(err);
    res.status(500).send('Gabim gjatë shtimit të orarit të punës');
  }
});

// PUT 
router.put('/:id', async (req, res) => {
  const { id } = req.params;
  const { employee_id, check_in, check_out } = req.body;

  try {
    await db.query(
      'UPDATE work_hours SET employee_id = ?, check_in = ?, check_out = ? WHERE id = ?',
      [employee_id, check_in, check_out, id]
    );
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
    await db.query('DELETE FROM work_hours WHERE id = ?', [id]);
    res.json({ message: 'Orari i punës u fshi me sukses' });
  } catch (err) {
    console.error(err);
    res.status(500).send('Gabim gjatë fshirjes së orarit të punës');
  }
});

module.exports = router;