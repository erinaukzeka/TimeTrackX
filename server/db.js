import sql from 'mssql';

const config = {
  user: 'sa',              
  password: 'admin123',   
  server: 'localhost',     
  database: 'timetrackx',
  port: 1433,
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