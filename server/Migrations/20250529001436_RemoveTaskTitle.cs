using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTrackX.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTaskTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            // First, check if users exist
            migrationBuilder.Sql(@"
                INSERT INTO Users (Id, Username, Email, PasswordHash, FirstName, LastName, Role, CreatedAt, IsActive)
                SELECT 1, 'admin', 'admin@example.com', 'PrP+ZrMeO00Q+nC1ytSccRIpSvauTkdqHEBRVdRaoSE=', 'Erina', 'Ukzeka', 'Admin', '2025-05-28 23:38:24.9870395', 1
                WHERE NOT EXISTS (SELECT 1 FROM Users WHERE Id = 1);

                INSERT INTO Users (Id, Username, Email, PasswordHash, FirstName, LastName, Role, CreatedAt, IsActive)
                SELECT 2, 'employee', 'employee@gmail.com', 'PrP+ZrMeO00Q+nC1ytSccRIpSvauTkdqHEBRVdRaoSE=', 'Filan', 'Fisteku', 'Employee', '2025-05-29 00:29:36.3955768', 1
                WHERE NOT EXISTS (SELECT 1 FROM Users WHERE Id = 2);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Projects");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "Projects",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            // Remove the seeded users in the Down method
            migrationBuilder.Sql(@"
                DELETE FROM Users WHERE Id IN (1, 2);
            ");
        }
    }
}
