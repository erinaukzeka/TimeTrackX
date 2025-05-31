using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTrackX.API.Migrations
{
    public partial class SeedInitialUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO Users (Id, Username, Email, PasswordHash, FirstName, LastName, Role, CreatedAt, IsActive)
                SELECT 1, 'admin', 'admin@example.com', 'PrP+ZrMeO00Q+nC1ytSccRIpSvauTkdqHEBRVdRaoSE=', 'Erina', 'Ukzeka', 'Admin', '2025-05-28 23:38:24.9870395', 1
                WHERE NOT EXISTS (SELECT 1 FROM Users WHERE Id = 1);

                INSERT INTO Users (Id, Username, Email, PasswordHash, FirstName, LastName, Role, CreatedAt, IsActive)
                SELECT 2, 'employee', 'employee@gmail.com', 'PrP+ZrMeO00Q+nC1ytSccRIpSvauTkdqHEBRVdRaoSE=', 'Filan', 'Fisteku', 'Employee', '2025-05-29 00:29:36.3955768', 1
                WHERE NOT EXISTS (SELECT 1 FROM Users WHERE Id = 2);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM Users WHERE Id IN (1, 2);
            ");
        }
    }
} 