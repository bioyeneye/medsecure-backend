using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedSecureSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedDateToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.Sql("UPDATE AspNetUsers SET DateCreated = GETDATE()"); // Use 'TRUE' for PostgreSQL
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "AspNetUsers");
        }
    }
}
