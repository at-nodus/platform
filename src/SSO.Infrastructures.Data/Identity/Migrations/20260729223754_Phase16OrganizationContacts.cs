using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSO.Infrastructures.Data.Identity.Migrations
{
    /// <inheritdoc />
    public partial class Phase16OrganizationContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                schema: "IdentityDb",
                table: "AspNetUsers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrganizationContacts",
                schema: "IdentityDb",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR(200)", nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR(256)", nullable: true),
                    Phone = table.Column<string>(type: "NVARCHAR(64)", nullable: true),
                    Title = table.Column<string>(type: "NVARCHAR(128)", nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationContacts_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "IdentityDb",
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationContacts_OrganizationId",
                schema: "IdentityDb",
                table: "OrganizationContacts",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationContacts",
                schema: "IdentityDb");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                schema: "IdentityDb",
                table: "AspNetUsers");
        }
    }
}
