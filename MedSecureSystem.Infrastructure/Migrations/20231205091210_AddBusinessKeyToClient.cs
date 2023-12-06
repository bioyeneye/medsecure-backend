using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedSecureSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessKeyToClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusinessKey",
                table: "OpenIddictApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessKey",
                table: "OpenIddictApplications");
        }
    }
}
