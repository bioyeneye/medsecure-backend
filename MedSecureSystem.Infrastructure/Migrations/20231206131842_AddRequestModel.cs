using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedSecureSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessEntities_BusinessCredentials_BusinessCredentialId",
                table: "BusinessEntities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BusinessEntities",
                table: "BusinessEntities");

            migrationBuilder.RenameTable(
                name: "BusinessEntities",
                newName: "Businesses");

            migrationBuilder.RenameIndex(
                name: "IX_BusinessEntities_BusinessCredentialId",
                table: "Businesses",
                newName: "IX_Businesses_BusinessCredentialId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Businesses",
                table: "Businesses",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "DeliveryRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    BusinessId = table.Column<long>(type: "bigint", nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DriverId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AgentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CodeToGiveToDriver = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodeToConfirmDelivery = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodeToConfirmReception = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgentAcceptedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AgentCompletedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DriverAcceptedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DriverStartedDeliveryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DriverCompletedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PatientReceivedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryRequests_AspNetUsers_AgentId",
                        column: x => x.AgentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliveryRequests_AspNetUsers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliveryRequests_AspNetUsers_PatientId",
                        column: x => x.PatientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryRequests_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryRequestItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryRequestId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryRequestItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryRequestItems_DeliveryRequests_DeliveryRequestId",
                        column: x => x.DeliveryRequestId,
                        principalTable: "DeliveryRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRequestItems_DeliveryRequestId",
                table: "DeliveryRequestItems",
                column: "DeliveryRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRequests_AgentId",
                table: "DeliveryRequests",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRequests_BusinessId",
                table: "DeliveryRequests",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRequests_DriverId",
                table: "DeliveryRequests",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRequests_PatientId",
                table: "DeliveryRequests",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_BusinessCredentials_BusinessCredentialId",
                table: "Businesses",
                column: "BusinessCredentialId",
                principalTable: "BusinessCredentials",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Businesses_BusinessCredentials_BusinessCredentialId",
                table: "Businesses");

            migrationBuilder.DropTable(
                name: "DeliveryRequestItems");

            migrationBuilder.DropTable(
                name: "DeliveryRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Businesses",
                table: "Businesses");

            migrationBuilder.RenameTable(
                name: "Businesses",
                newName: "BusinessEntities");

            migrationBuilder.RenameIndex(
                name: "IX_Businesses_BusinessCredentialId",
                table: "BusinessEntities",
                newName: "IX_BusinessEntities_BusinessCredentialId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BusinessEntities",
                table: "BusinessEntities",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessEntities_BusinessCredentials_BusinessCredentialId",
                table: "BusinessEntities",
                column: "BusinessCredentialId",
                principalTable: "BusinessCredentials",
                principalColumn: "Id");
        }
    }
}
