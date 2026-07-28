using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSO.Infrastructures.Data.Identity.Migrations
{
    /// <inheritdoc />
    public partial class Phase15ProductEnablement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductEnablements",
                schema: "IdentityDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    ProductId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductEnablements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductEnablements_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "IdentityDb",
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductEnablements_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "IdentityDb",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductEnablements_OrganizationId_ProductId",
                schema: "IdentityDb",
                table: "ProductEnablements",
                columns: new[] { "OrganizationId", "ProductId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductEnablements_ProductId",
                schema: "IdentityDb",
                table: "ProductEnablements",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductEnablements",
                schema: "IdentityDb");
        }
    }
}
