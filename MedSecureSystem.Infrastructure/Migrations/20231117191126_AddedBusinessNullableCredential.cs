using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedSecureSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedBusinessNullableCredential : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessEntities_BusinessCredentials_BusinessCredentialId",
                table: "BusinessEntities");

            migrationBuilder.DropIndex(
                name: "IX_BusinessEntities_BusinessCredentialId",
                table: "BusinessEntities");

            migrationBuilder.AlterColumn<long>(
                name: "BusinessCredentialId",
                table: "BusinessEntities",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessEntities_BusinessCredentialId",
                table: "BusinessEntities",
                column: "BusinessCredentialId",
                unique: true,
                filter: "[BusinessCredentialId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessEntities_BusinessCredentials_BusinessCredentialId",
                table: "BusinessEntities",
                column: "BusinessCredentialId",
                principalTable: "BusinessCredentials",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessEntities_BusinessCredentials_BusinessCredentialId",
                table: "BusinessEntities");

            migrationBuilder.DropIndex(
                name: "IX_BusinessEntities_BusinessCredentialId",
                table: "BusinessEntities");

            migrationBuilder.AlterColumn<long>(
                name: "BusinessCredentialId",
                table: "BusinessEntities",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessEntities_BusinessCredentialId",
                table: "BusinessEntities",
                column: "BusinessCredentialId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessEntities_BusinessCredentials_BusinessCredentialId",
                table: "BusinessEntities",
                column: "BusinessCredentialId",
                principalTable: "BusinessCredentials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
