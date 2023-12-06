using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedSecureSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedBusinessEntitys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "BusinessEntities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "BusinessEntities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "BusinessEntities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VerificationToken",
                table: "BusinessEntities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "BusinessCredentials",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "BusinessEntities");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "BusinessEntities");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "BusinessEntities");

            migrationBuilder.DropColumn(
                name: "VerificationToken",
                table: "BusinessEntities");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "BusinessCredentials");
        }
    }
}
