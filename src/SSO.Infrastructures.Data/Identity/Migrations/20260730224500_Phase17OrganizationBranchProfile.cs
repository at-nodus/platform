using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSO.Infrastructures.Data.Identity.Migrations
{
    /// <inheritdoc />
    public partial class Phase17OrganizationBranchProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "City", schema: "IdentityDb", table: "Organizations", type: "NVARCHAR(128)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Complement", schema: "IdentityDb", table: "Organizations", type: "NVARCHAR(128)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Description", schema: "IdentityDb", table: "Organizations", type: "NVARCHAR(1000)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "LegalName", schema: "IdentityDb", table: "Organizations", type: "NVARCHAR(256)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Number", schema: "IdentityDb", table: "Organizations", type: "NVARCHAR(32)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "PostalCode", schema: "IdentityDb", table: "Organizations", type: "NVARCHAR(16)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Segment", schema: "IdentityDb", table: "Organizations", type: "NVARCHAR(64)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "State", schema: "IdentityDb", table: "Organizations", type: "NVARCHAR(2)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Street", schema: "IdentityDb", table: "Organizations", type: "NVARCHAR(256)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "TaxId", schema: "IdentityDb", table: "Organizations", type: "NVARCHAR(18)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "TradeName", schema: "IdentityDb", table: "Organizations", type: "NVARCHAR(128)", nullable: true);

            migrationBuilder.AddColumn<string>(name: "City", schema: "IdentityDb", table: "Branches", type: "NVARCHAR(128)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Complement", schema: "IdentityDb", table: "Branches", type: "NVARCHAR(128)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Description", schema: "IdentityDb", table: "Branches", type: "NVARCHAR(1000)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "LegalName", schema: "IdentityDb", table: "Branches", type: "NVARCHAR(256)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Number", schema: "IdentityDb", table: "Branches", type: "NVARCHAR(32)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "PostalCode", schema: "IdentityDb", table: "Branches", type: "NVARCHAR(16)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Segment", schema: "IdentityDb", table: "Branches", type: "NVARCHAR(64)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "State", schema: "IdentityDb", table: "Branches", type: "NVARCHAR(2)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Street", schema: "IdentityDb", table: "Branches", type: "NVARCHAR(256)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "TaxId", schema: "IdentityDb", table: "Branches", type: "NVARCHAR(18)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "TradeName", schema: "IdentityDb", table: "Branches", type: "NVARCHAR(128)", nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "City", schema: "IdentityDb", table: "Organizations");
            migrationBuilder.DropColumn(name: "Complement", schema: "IdentityDb", table: "Organizations");
            migrationBuilder.DropColumn(name: "Description", schema: "IdentityDb", table: "Organizations");
            migrationBuilder.DropColumn(name: "LegalName", schema: "IdentityDb", table: "Organizations");
            migrationBuilder.DropColumn(name: "Number", schema: "IdentityDb", table: "Organizations");
            migrationBuilder.DropColumn(name: "PostalCode", schema: "IdentityDb", table: "Organizations");
            migrationBuilder.DropColumn(name: "Segment", schema: "IdentityDb", table: "Organizations");
            migrationBuilder.DropColumn(name: "State", schema: "IdentityDb", table: "Organizations");
            migrationBuilder.DropColumn(name: "Street", schema: "IdentityDb", table: "Organizations");
            migrationBuilder.DropColumn(name: "TaxId", schema: "IdentityDb", table: "Organizations");
            migrationBuilder.DropColumn(name: "TradeName", schema: "IdentityDb", table: "Organizations");

            migrationBuilder.DropColumn(name: "City", schema: "IdentityDb", table: "Branches");
            migrationBuilder.DropColumn(name: "Complement", schema: "IdentityDb", table: "Branches");
            migrationBuilder.DropColumn(name: "Description", schema: "IdentityDb", table: "Branches");
            migrationBuilder.DropColumn(name: "LegalName", schema: "IdentityDb", table: "Branches");
            migrationBuilder.DropColumn(name: "Number", schema: "IdentityDb", table: "Branches");
            migrationBuilder.DropColumn(name: "PostalCode", schema: "IdentityDb", table: "Branches");
            migrationBuilder.DropColumn(name: "Segment", schema: "IdentityDb", table: "Branches");
            migrationBuilder.DropColumn(name: "State", schema: "IdentityDb", table: "Branches");
            migrationBuilder.DropColumn(name: "Street", schema: "IdentityDb", table: "Branches");
            migrationBuilder.DropColumn(name: "TaxId", schema: "IdentityDb", table: "Branches");
            migrationBuilder.DropColumn(name: "TradeName", schema: "IdentityDb", table: "Branches");
        }
    }
}
