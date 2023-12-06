using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedSecureSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateclientapplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "OpenIddictApplications",
                type: "bit",
                nullable: false,
                defaultValue: true);

            // SQL command to update existing rows
            migrationBuilder.Sql("UPDATE OpenIddictApplications SET IsActive = 1"); // Use 'TRUE' for PostgreSQL
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "OpenIddictApplications");
        }
    }
}
