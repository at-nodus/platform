using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSO.Infrastructures.Data.Identity.Migrations
{
    /// <inheritdoc />
    public partial class Phase18bCompositeBranchOrgFks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM [IdentityDb].[UserRoleAssignments] a
    INNER JOIN [IdentityDb].[Branches] b ON a.[BranchId] = b.[Id]
    WHERE a.[BranchId] IS NOT NULL AND a.[OrganizationId] <> b.[OrganizationId])
    THROW 50001, 'UserRoleAssignments BranchId/OrganizationId mismatch — clean data before Phase18b FKs.', 1;
IF EXISTS (
    SELECT 1 FROM [IdentityDb].[UserClaimAssignments] a
    INNER JOIN [IdentityDb].[Branches] b ON a.[BranchId] = b.[Id]
    WHERE a.[BranchId] IS NOT NULL AND a.[OrganizationId] <> b.[OrganizationId])
    THROW 50001, 'UserClaimAssignments BranchId/OrganizationId mismatch — clean data before Phase18b FKs.', 1;
IF EXISTS (
    SELECT 1 FROM [IdentityDb].[UserSessions] s
    INNER JOIN [IdentityDb].[Branches] b ON s.[BranchId] = b.[Id]
    WHERE s.[BranchId] IS NOT NULL AND s.[OrganizationId] <> b.[OrganizationId])
    THROW 50001, 'UserSessions BranchId/OrganizationId mismatch — clean data before Phase18b FKs.', 1;
IF EXISTS (
    SELECT 1 FROM [IdentityDb].[LdapGroupRoleMaps] m
    INNER JOIN [IdentityDb].[Branches] b ON m.[BranchId] = b.[Id]
    WHERE m.[BranchId] IS NOT NULL AND m.[OrganizationId] <> b.[OrganizationId])
    THROW 50001, 'LdapGroupRoleMaps BranchId/OrganizationId mismatch — clean data before Phase18b FKs.', 1;
IF EXISTS (
    SELECT 1 FROM [IdentityDb].[Branches] c
    INNER JOIN [IdentityDb].[Branches] p ON c.[ParentBranchId] = p.[Id]
    WHERE c.[ParentBranchId] IS NOT NULL AND c.[OrganizationId] <> p.[OrganizationId])
    THROW 50001, 'Branches ParentBranchId belongs to another organization — clean data before Phase18b FKs.', 1;
IF EXISTS (
    SELECT 1 FROM [IdentityDb].[UserRoleAssignments]
    WHERE [BranchId] IS NOT NULL AND [OrganizationId] IS NULL)
    THROW 50001, 'UserRoleAssignments with BranchId and null OrganizationId — clean data before Phase18b check constraints.', 1;
IF EXISTS (
    SELECT 1 FROM [IdentityDb].[UserClaimAssignments]
    WHERE [BranchId] IS NOT NULL AND [OrganizationId] IS NULL)
    THROW 50001, 'UserClaimAssignments with BranchId and null OrganizationId — clean data before Phase18b check constraints.', 1;
IF EXISTS (
    SELECT 1 FROM [IdentityDb].[UserSessions]
    WHERE [BranchId] IS NOT NULL AND [OrganizationId] IS NULL)
    THROW 50001, 'UserSessions with BranchId and null OrganizationId — clean data before Phase18b check constraints.', 1;
IF EXISTS (
    SELECT 1 FROM [IdentityDb].[LdapGroupRoleMaps]
    WHERE [BranchId] IS NOT NULL AND [OrganizationId] IS NULL)
    THROW 50001, 'LdapGroupRoleMaps with BranchId and null OrganizationId — clean data before Phase18b check constraints.', 1;
IF EXISTS (
    SELECT 1 FROM [IdentityDb].[UserRoleAssignments]
    WHERE [IsDeleted] = 0 AND [OrganizationId] IS NULL AND [BranchId] IS NULL
    GROUP BY [UserId], [RoleId], [ProductId]
    HAVING COUNT(*) > 1)
    THROW 50001, 'Duplicate platform-scoped UserRoleAssignments — clean data before Phase18b unique indexes.', 1;
IF EXISTS (
    SELECT 1 FROM [IdentityDb].[UserClaimAssignments]
    WHERE [IsDeleted] = 0 AND [OrganizationId] IS NULL AND [BranchId] IS NULL
    GROUP BY [UserId], [ClaimDefinitionId], [ProductId]
    HAVING COUNT(*) > 1)
    THROW 50001, 'Duplicate platform-scoped UserClaimAssignments — clean data before Phase18b unique indexes.', 1;
IF EXISTS (
    SELECT 1 FROM [IdentityDb].[OrganizationContacts]
    WHERE [IsDeleted] = 0 AND [IsPrimary] = 1
    GROUP BY [OrganizationId]
    HAVING COUNT(*) > 1)
    THROW 50001, 'Multiple primary OrganizationContacts per organization — clean data before Phase18b unique index.', 1;
");

            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Branches_ParentBranchId",
                schema: "IdentityDb",
                table: "Branches");

            migrationBuilder.DropForeignKey(
                name: "FK_LdapGroupRoleMaps_Branches_BranchId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps");

            migrationBuilder.DropForeignKey(
                name: "FK_UserClaimAssignments_Branches_BranchId",
                schema: "IdentityDb",
                table: "UserClaimAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoleAssignments_Branches_BranchId",
                schema: "IdentityDb",
                table: "UserRoleAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSessions_Branches_BranchId",
                schema: "IdentityDb",
                table: "UserSessions");

            migrationBuilder.DropIndex(
                name: "IX_UserSessions_BranchId",
                schema: "IdentityDb",
                table: "UserSessions");

            migrationBuilder.DropIndex(
                name: "IX_UserRoleAssignments_BranchId",
                schema: "IdentityDb",
                table: "UserRoleAssignments");

            migrationBuilder.DropIndex(
                name: "IX_UserRoleAssignments_UserId_RoleId_OrganizationId_BranchId_ProductId",
                schema: "IdentityDb",
                table: "UserRoleAssignments");

            migrationBuilder.DropIndex(
                name: "IX_UserClaimAssignments_BranchId",
                schema: "IdentityDb",
                table: "UserClaimAssignments");

            migrationBuilder.DropIndex(
                name: "IX_UserClaimAssignments_UserId_ClaimDefinitionId_OrganizationId_BranchId_ProductId",
                schema: "IdentityDb",
                table: "UserClaimAssignments");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationContacts_OrganizationId",
                schema: "IdentityDb",
                table: "OrganizationContacts");

            migrationBuilder.DropIndex(
                name: "IX_LdapGroupRoleMaps_BranchId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps");

            migrationBuilder.DropIndex(
                name: "IX_Branches_ParentBranchId",
                schema: "IdentityDb",
                table: "Branches");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Branches_Id_OrganizationId",
                schema: "IdentityDb",
                table: "Branches",
                columns: new[] { "Id", "OrganizationId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_BranchId_OrganizationId",
                schema: "IdentityDb",
                table: "UserSessions",
                columns: new[] { "BranchId", "OrganizationId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserSessions_BranchRequiresOrg",
                schema: "IdentityDb",
                table: "UserSessions",
                sql: "[BranchId] IS NULL OR [OrganizationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignments_BranchId_OrganizationId",
                schema: "IdentityDb",
                table: "UserRoleAssignments",
                columns: new[] { "BranchId", "OrganizationId" });

            migrationBuilder.CreateIndex(
                name: "UX_UserRoleAssignments_Platform",
                schema: "IdentityDb",
                table: "UserRoleAssignments",
                columns: new[] { "UserId", "RoleId", "ProductId" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [OrganizationId] IS NULL AND [BranchId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_UserRoleAssignments_Tenant",
                schema: "IdentityDb",
                table: "UserRoleAssignments",
                columns: new[] { "UserId", "RoleId", "OrganizationId", "BranchId", "ProductId" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [OrganizationId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserRoleAssignments_BranchRequiresOrg",
                schema: "IdentityDb",
                table: "UserRoleAssignments",
                sql: "[BranchId] IS NULL OR [OrganizationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaimAssignments_BranchId_OrganizationId",
                schema: "IdentityDb",
                table: "UserClaimAssignments",
                columns: new[] { "BranchId", "OrganizationId" });

            migrationBuilder.CreateIndex(
                name: "UX_UserClaimAssignments_Platform",
                schema: "IdentityDb",
                table: "UserClaimAssignments",
                columns: new[] { "UserId", "ClaimDefinitionId", "ProductId" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [OrganizationId] IS NULL AND [BranchId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_UserClaimAssignments_Tenant",
                schema: "IdentityDb",
                table: "UserClaimAssignments",
                columns: new[] { "UserId", "ClaimDefinitionId", "OrganizationId", "BranchId", "ProductId" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [OrganizationId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserClaimAssignments_BranchRequiresOrg",
                schema: "IdentityDb",
                table: "UserClaimAssignments",
                sql: "[BranchId] IS NULL OR [OrganizationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationContacts_OrganizationId",
                schema: "IdentityDb",
                table: "OrganizationContacts",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "UX_OrganizationContacts_Primary",
                schema: "IdentityDb",
                table: "OrganizationContacts",
                columns: new[] { "OrganizationId", "IsPrimary" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [IsPrimary] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_LdapGroupRoleMaps_BranchId_OrganizationId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps",
                columns: new[] { "BranchId", "OrganizationId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_LdapGroupRoleMaps_BranchRequiresOrg",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps",
                sql: "[BranchId] IS NULL OR [OrganizationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_ParentBranchId_OrganizationId",
                schema: "IdentityDb",
                table: "Branches",
                columns: new[] { "ParentBranchId", "OrganizationId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Branches_ParentBranchId_OrganizationId",
                schema: "IdentityDb",
                table: "Branches",
                columns: new[] { "ParentBranchId", "OrganizationId" },
                principalSchema: "IdentityDb",
                principalTable: "Branches",
                principalColumns: new[] { "Id", "OrganizationId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LdapGroupRoleMaps_Branches_BranchId_OrganizationId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps",
                columns: new[] { "BranchId", "OrganizationId" },
                principalSchema: "IdentityDb",
                principalTable: "Branches",
                principalColumns: new[] { "Id", "OrganizationId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserClaimAssignments_Branches_BranchId_OrganizationId",
                schema: "IdentityDb",
                table: "UserClaimAssignments",
                columns: new[] { "BranchId", "OrganizationId" },
                principalSchema: "IdentityDb",
                principalTable: "Branches",
                principalColumns: new[] { "Id", "OrganizationId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoleAssignments_Branches_BranchId_OrganizationId",
                schema: "IdentityDb",
                table: "UserRoleAssignments",
                columns: new[] { "BranchId", "OrganizationId" },
                principalSchema: "IdentityDb",
                principalTable: "Branches",
                principalColumns: new[] { "Id", "OrganizationId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSessions_Branches_BranchId_OrganizationId",
                schema: "IdentityDb",
                table: "UserSessions",
                columns: new[] { "BranchId", "OrganizationId" },
                principalSchema: "IdentityDb",
                principalTable: "Branches",
                principalColumns: new[] { "Id", "OrganizationId" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Branches_ParentBranchId_OrganizationId",
                schema: "IdentityDb",
                table: "Branches");

            migrationBuilder.DropForeignKey(
                name: "FK_LdapGroupRoleMaps_Branches_BranchId_OrganizationId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps");

            migrationBuilder.DropForeignKey(
                name: "FK_UserClaimAssignments_Branches_BranchId_OrganizationId",
                schema: "IdentityDb",
                table: "UserClaimAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoleAssignments_Branches_BranchId_OrganizationId",
                schema: "IdentityDb",
                table: "UserRoleAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSessions_Branches_BranchId_OrganizationId",
                schema: "IdentityDb",
                table: "UserSessions");

            migrationBuilder.DropIndex(
                name: "IX_UserSessions_BranchId_OrganizationId",
                schema: "IdentityDb",
                table: "UserSessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserSessions_BranchRequiresOrg",
                schema: "IdentityDb",
                table: "UserSessions");

            migrationBuilder.DropIndex(
                name: "IX_UserRoleAssignments_BranchId_OrganizationId",
                schema: "IdentityDb",
                table: "UserRoleAssignments");

            migrationBuilder.DropIndex(
                name: "UX_UserRoleAssignments_Platform",
                schema: "IdentityDb",
                table: "UserRoleAssignments");

            migrationBuilder.DropIndex(
                name: "UX_UserRoleAssignments_Tenant",
                schema: "IdentityDb",
                table: "UserRoleAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserRoleAssignments_BranchRequiresOrg",
                schema: "IdentityDb",
                table: "UserRoleAssignments");

            migrationBuilder.DropIndex(
                name: "IX_UserClaimAssignments_BranchId_OrganizationId",
                schema: "IdentityDb",
                table: "UserClaimAssignments");

            migrationBuilder.DropIndex(
                name: "UX_UserClaimAssignments_Platform",
                schema: "IdentityDb",
                table: "UserClaimAssignments");

            migrationBuilder.DropIndex(
                name: "UX_UserClaimAssignments_Tenant",
                schema: "IdentityDb",
                table: "UserClaimAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserClaimAssignments_BranchRequiresOrg",
                schema: "IdentityDb",
                table: "UserClaimAssignments");

            migrationBuilder.DropIndex(
                name: "UX_OrganizationContacts_Primary",
                schema: "IdentityDb",
                table: "OrganizationContacts");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationContacts_OrganizationId",
                schema: "IdentityDb",
                table: "OrganizationContacts");

            migrationBuilder.DropIndex(
                name: "IX_LdapGroupRoleMaps_BranchId_OrganizationId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps");

            migrationBuilder.DropCheckConstraint(
                name: "CK_LdapGroupRoleMaps_BranchRequiresOrg",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Branches_Id_OrganizationId",
                schema: "IdentityDb",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Branches_ParentBranchId_OrganizationId",
                schema: "IdentityDb",
                table: "Branches");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_BranchId",
                schema: "IdentityDb",
                table: "UserSessions",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignments_BranchId",
                schema: "IdentityDb",
                table: "UserRoleAssignments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignments_UserId_RoleId_OrganizationId_BranchId_ProductId",
                schema: "IdentityDb",
                table: "UserRoleAssignments",
                columns: new[] { "UserId", "RoleId", "OrganizationId", "BranchId", "ProductId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaimAssignments_BranchId",
                schema: "IdentityDb",
                table: "UserClaimAssignments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaimAssignments_UserId_ClaimDefinitionId_OrganizationId_BranchId_ProductId",
                schema: "IdentityDb",
                table: "UserClaimAssignments",
                columns: new[] { "UserId", "ClaimDefinitionId", "OrganizationId", "BranchId", "ProductId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationContacts_OrganizationId",
                schema: "IdentityDb",
                table: "OrganizationContacts",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LdapGroupRoleMaps_BranchId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_ParentBranchId",
                schema: "IdentityDb",
                table: "Branches",
                column: "ParentBranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Branches_ParentBranchId",
                schema: "IdentityDb",
                table: "Branches",
                column: "ParentBranchId",
                principalSchema: "IdentityDb",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LdapGroupRoleMaps_Branches_BranchId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps",
                column: "BranchId",
                principalSchema: "IdentityDb",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserClaimAssignments_Branches_BranchId",
                schema: "IdentityDb",
                table: "UserClaimAssignments",
                column: "BranchId",
                principalSchema: "IdentityDb",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoleAssignments_Branches_BranchId",
                schema: "IdentityDb",
                table: "UserRoleAssignments",
                column: "BranchId",
                principalSchema: "IdentityDb",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSessions_Branches_BranchId",
                schema: "IdentityDb",
                table: "UserSessions",
                column: "BranchId",
                principalSchema: "IdentityDb",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
