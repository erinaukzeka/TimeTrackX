const express = require('express');
const router = express.Router();
const db = require('../db');

//GET
router.get('/', async (req, res) => {
    try{
        const [rows] = await db.query('SELECT * FROM employees');
        res.json(rows);
    } catch (err){
        console.error(err);
        res.status(500).send('Gabim gjate marrjes se punetoreve');
    }
});

module.exports = router;