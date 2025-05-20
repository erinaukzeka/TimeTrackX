import sql from 'mssql';

const config = {
  user: 'agnesa@email',              
  password: 'agnesa12',   
  server: 'DESKTOP-NQRUPKS\MSSQLSERVER',     
  database: 'timetrackx',
  options: {
    encrypt: false,
    trustServerCertificate: true,
  }
};

export const pool = new sql.ConnectionPool(config)
  .connect()
  .then(pool => {
    console.log("✅ Connected to SQL Server");
    return pool;
  })
  .catch(err => console.log("❌ Database connection failed", err));