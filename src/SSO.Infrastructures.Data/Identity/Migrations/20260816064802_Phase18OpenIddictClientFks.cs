using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSO.Infrastructures.Data.Identity.Migrations
{
    /// <inheritdoc />
    public partial class Phase18OpenIddictClientFks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM [IdentityDb].[OpenIddictApplications] WHERE [ClientId] IS NULL OR LTRIM(RTRIM([ClientId])) = N'')
    THROW 50001, 'OpenIddictApplications.ClientId null/empty — clean data before Phase18 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[AuthClientMetadata] m LEFT JOIN [IdentityDb].[OpenIddictApplications] a ON m.[ClientId] = a.[ClientId] WHERE a.[Id] IS NULL)
    THROW 50001, 'Orphan AuthClientMetadata.ClientId — clean data before Phase18 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[ClientProductBindings] c LEFT JOIN [IdentityDb].[OpenIddictApplications] a ON c.[ClientId] = a.[ClientId] WHERE a.[Id] IS NULL)
    THROW 50001, 'Orphan ClientProductBindings.ClientId — clean data before Phase18 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[ClientWebhookEndpoints] w LEFT JOIN [IdentityDb].[OpenIddictApplications] a ON w.[ClientId] = a.[ClientId] WHERE a.[Id] IS NULL)
    THROW 50001, 'Orphan ClientWebhookEndpoints.ClientId — clean data before Phase18 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[UserSessions] s LEFT JOIN [IdentityDb].[OpenIddictApplications] a ON s.[ClientId] = a.[ClientId] WHERE a.[Id] IS NULL)
    THROW 50001, 'Orphan UserSessions.ClientId — clean data before Phase18 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[AuthClientMetadata] WHERE LEN([ClientId]) > 100)
    THROW 50001, 'AuthClientMetadata.ClientId longer than 100 — clean data before Phase18 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[ClientProductBindings] WHERE LEN([ClientId]) > 100)
    THROW 50001, 'ClientProductBindings.ClientId longer than 100 — clean data before Phase18 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[ClientWebhookEndpoints] WHERE LEN([ClientId]) > 100)
    THROW 50001, 'ClientWebhookEndpoints.ClientId longer than 100 — clean data before Phase18 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[UserSessions] WHERE LEN([ClientId]) > 100)
    THROW 50001, 'UserSessions.ClientId longer than 100 — clean data before Phase18 FKs.', 1;
");

            migrationBuilder.DropIndex(
                name: "IX_OpenIddictApplications_ClientId",
                schema: "IdentityDb",
                table: "OpenIddictApplications");

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                schema: "IdentityDb",
                table: "UserSessions",
                type: "nvarchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)");

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                schema: "IdentityDb",
                table: "OpenIddictApplications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                schema: "IdentityDb",
                table: "ClientWebhookEndpoints",
                type: "nvarchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)");

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                schema: "IdentityDb",
                table: "ClientProductBindings",
                type: "nvarchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(128)");

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                schema: "IdentityDb",
                table: "AuthClientMetadata",
                type: "nvarchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_OpenIddictApplications_ClientId",
                schema: "IdentityDb",
                table: "OpenIddictApplications",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_ClientId",
                schema: "IdentityDb",
                table: "UserSessions",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictApplications_ClientId",
                schema: "IdentityDb",
                table: "OpenIddictApplications",
                column: "ClientId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuthClientMetadata_OpenIddictApplications_ClientId",
                schema: "IdentityDb",
                table: "AuthClientMetadata",
                column: "ClientId",
                principalSchema: "IdentityDb",
                principalTable: "OpenIddictApplications",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientProductBindings_OpenIddictApplications_ClientId",
                schema: "IdentityDb",
                table: "ClientProductBindings",
                column: "ClientId",
                principalSchema: "IdentityDb",
                principalTable: "OpenIddictApplications",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientWebhookEndpoints_OpenIddictApplications_ClientId",
                schema: "IdentityDb",
                table: "ClientWebhookEndpoints",
                column: "ClientId",
                principalSchema: "IdentityDb",
                principalTable: "OpenIddictApplications",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSessions_OpenIddictApplications_ClientId",
                schema: "IdentityDb",
                table: "UserSessions",
                column: "ClientId",
                principalSchema: "IdentityDb",
                principalTable: "OpenIddictApplications",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthClientMetadata_OpenIddictApplications_ClientId",
                schema: "IdentityDb",
                table: "AuthClientMetadata");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientProductBindings_OpenIddictApplications_ClientId",
                schema: "IdentityDb",
                table: "ClientProductBindings");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientWebhookEndpoints_OpenIddictApplications_ClientId",
                schema: "IdentityDb",
                table: "ClientWebhookEndpoints");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSessions_OpenIddictApplications_ClientId",
                schema: "IdentityDb",
                table: "UserSessions");

            migrationBuilder.DropIndex(
                name: "IX_UserSessions_ClientId",
                schema: "IdentityDb",
                table: "UserSessions");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_OpenIddictApplications_ClientId",
                schema: "IdentityDb",
                table: "OpenIddictApplications");

            migrationBuilder.DropIndex(
                name: "IX_OpenIddictApplications_ClientId",
                schema: "IdentityDb",
                table: "OpenIddictApplications");

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                schema: "IdentityDb",
                table: "UserSessions",
                type: "nvarchar(128)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)");

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                schema: "IdentityDb",
                table: "OpenIddictApplications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                schema: "IdentityDb",
                table: "ClientWebhookEndpoints",
                type: "nvarchar(128)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)");

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                schema: "IdentityDb",
                table: "ClientProductBindings",
                type: "NVARCHAR(128)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)");

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                schema: "IdentityDb",
                table: "AuthClientMetadata",
                type: "nvarchar(128)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictApplications_ClientId",
                schema: "IdentityDb",
                table: "OpenIddictApplications",
                column: "ClientId",
                unique: true,
                filter: "[ClientId] IS NOT NULL");
        }
    }
}
