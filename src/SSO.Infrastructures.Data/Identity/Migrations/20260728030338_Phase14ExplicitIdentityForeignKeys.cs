using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSO.Infrastructures.Data.Identity.Migrations
{
    /// <inheritdoc />
    public partial class Phase14ExplicitIdentityForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fail fast if orphan Guids exist before AddForeignKey (bug 00012).
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM [IdentityDb].[Memberships] m LEFT JOIN [IdentityDb].[Organizations] o ON m.[OrganizationId] = o.[Id] WHERE o.[Id] IS NULL)
    THROW 50001, 'Orphan Memberships.OrganizationId — clean data before Phase14 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[Memberships] m LEFT JOIN [IdentityDb].[AspNetUsers] u ON m.[UserId] = u.[Id] WHERE u.[Id] IS NULL)
    THROW 50001, 'Orphan Memberships.UserId — clean data before Phase14 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[Branches] b LEFT JOIN [IdentityDb].[Organizations] o ON b.[OrganizationId] = o.[Id] WHERE o.[Id] IS NULL)
    THROW 50001, 'Orphan Branches.OrganizationId — clean data before Phase14 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[Branches] b LEFT JOIN [IdentityDb].[Branches] p ON b.[ParentBranchId] = p.[Id] WHERE b.[ParentBranchId] IS NOT NULL AND p.[Id] IS NULL)
    THROW 50001, 'Orphan Branches.ParentBranchId — clean data before Phase14 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[RolePermissions] rp LEFT JOIN [IdentityDb].[AuthRoles] r ON rp.[RoleId] = r.[Id] WHERE r.[Id] IS NULL)
    THROW 50001, 'Orphan RolePermissions.RoleId — clean data before Phase14 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[RolePermissions] rp LEFT JOIN [IdentityDb].[Permissions] p ON rp.[PermissionId] = p.[Id] WHERE p.[Id] IS NULL)
    THROW 50001, 'Orphan RolePermissions.PermissionId — clean data before Phase14 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[OrganizationInvites] i LEFT JOIN [IdentityDb].[Organizations] o ON i.[OrganizationId] = o.[Id] WHERE o.[Id] IS NULL)
    THROW 50001, 'Orphan OrganizationInvites.OrganizationId — clean data before Phase14 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[OrganizationInvites] i LEFT JOIN [IdentityDb].[AspNetUsers] u ON i.[InvitedByUserId] = u.[Id] WHERE u.[Id] IS NULL)
    THROW 50001, 'Orphan OrganizationInvites.InvitedByUserId — clean data before Phase14 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[OrganizationInvites] i LEFT JOIN [IdentityDb].[AspNetUsers] u ON i.[AcceptedUserId] = u.[Id] WHERE i.[AcceptedUserId] IS NOT NULL AND u.[Id] IS NULL)
    THROW 50001, 'Orphan OrganizationInvites.AcceptedUserId — clean data before Phase14 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[UserRoleAssignments] a LEFT JOIN [IdentityDb].[AspNetUsers] u ON a.[UserId] = u.[Id] WHERE u.[Id] IS NULL)
    THROW 50001, 'Orphan UserRoleAssignments.UserId — clean data before Phase14 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[UserRoleAssignments] a LEFT JOIN [IdentityDb].[AuthRoles] r ON a.[RoleId] = r.[Id] WHERE r.[Id] IS NULL)
    THROW 50001, 'Orphan UserRoleAssignments.RoleId — clean data before Phase14 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[UserRoleAssignments] a LEFT JOIN [IdentityDb].[Products] p ON a.[ProductId] = p.[Id] WHERE p.[Id] IS NULL)
    THROW 50001, 'Orphan UserRoleAssignments.ProductId — clean data before Phase14 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[UserRoleAssignments] a LEFT JOIN [IdentityDb].[Organizations] o ON a.[OrganizationId] = o.[Id] WHERE a.[OrganizationId] IS NOT NULL AND o.[Id] IS NULL)
    THROW 50001, 'Orphan UserRoleAssignments.OrganizationId — clean data before Phase14 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[UserRoleAssignments] a LEFT JOIN [IdentityDb].[Branches] b ON a.[BranchId] = b.[Id] WHERE a.[BranchId] IS NOT NULL AND b.[Id] IS NULL)
    THROW 50001, 'Orphan UserRoleAssignments.BranchId — clean data before Phase14 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[MenuItems] m LEFT JOIN [IdentityDb].[Products] p ON m.[ProductId] = p.[Id] WHERE p.[Id] IS NULL)
    THROW 50001, 'Orphan MenuItems.ProductId — clean data before Phase14 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[ClientProductBindings] c LEFT JOIN [IdentityDb].[Products] p ON c.[ProductId] = p.[Id] WHERE p.[Id] IS NULL)
    THROW 50001, 'Orphan ClientProductBindings.ProductId — clean data before Phase14 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[UserSessions] s LEFT JOIN [IdentityDb].[AspNetUsers] u ON s.[UserId] = u.[Id] WHERE u.[Id] IS NULL)
    THROW 50001, 'Orphan UserSessions.UserId — clean data before Phase14 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[UserSessions] s LEFT JOIN [IdentityDb].[Organizations] o ON s.[OrganizationId] = o.[Id] WHERE s.[OrganizationId] IS NOT NULL AND o.[Id] IS NULL)
    THROW 50001, 'Orphan UserSessions.OrganizationId — clean data before Phase14 FKs.', 1;
IF EXISTS (SELECT 1 FROM [IdentityDb].[UserSessions] s LEFT JOIN [IdentityDb].[Branches] b ON s.[BranchId] = b.[Id] WHERE s.[BranchId] IS NOT NULL AND b.[Id] IS NULL)
    THROW 50001, 'Orphan UserSessions.BranchId — clean data before Phase14 FKs.', 1;
");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_BranchId",
                schema: "IdentityDb",
                table: "UserSessions",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_OrganizationId",
                schema: "IdentityDb",
                table: "UserSessions",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignments_BranchId",
                schema: "IdentityDb",
                table: "UserRoleAssignments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignments_OrganizationId",
                schema: "IdentityDb",
                table: "UserRoleAssignments",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignments_ProductId",
                schema: "IdentityDb",
                table: "UserRoleAssignments",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleAssignments_RoleId",
                schema: "IdentityDb",
                table: "UserRoleAssignments",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaimAssignments_BranchId",
                schema: "IdentityDb",
                table: "UserClaimAssignments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaimAssignments_ClaimDefinitionId",
                schema: "IdentityDb",
                table: "UserClaimAssignments",
                column: "ClaimDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaimAssignments_OrganizationId",
                schema: "IdentityDb",
                table: "UserClaimAssignments",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaimAssignments_ProductId",
                schema: "IdentityDb",
                table: "UserClaimAssignments",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                schema: "IdentityDb",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationInvites_AcceptedUserId",
                schema: "IdentityDb",
                table: "OrganizationInvites",
                column: "AcceptedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationInvites_InvitedByUserId",
                schema: "IdentityDb",
                table: "OrganizationInvites",
                column: "InvitedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_OrganizationId",
                schema: "IdentityDb",
                table: "Memberships",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LdapGroupRoleMaps_BranchId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_LdapGroupRoleMaps_ProductId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_LdapGroupRoleMaps_RoleId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalIdentityProviders_OrganizationId",
                schema: "IdentityDb",
                table: "ExternalIdentityProviders",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientProductBindings_ProductId",
                schema: "IdentityDb",
                table: "ClientProductBindings",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimDefinitions_ProductId",
                schema: "IdentityDb",
                table: "ClaimDefinitions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_ParentBranchId",
                schema: "IdentityDb",
                table: "Branches",
                column: "ParentBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthRoleClaims_ClaimDefinitionId",
                schema: "IdentityDb",
                table: "AuthRoleClaims",
                column: "ClaimDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuthRoleClaims_AuthRoles_RoleId",
                schema: "IdentityDb",
                table: "AuthRoleClaims",
                column: "RoleId",
                principalSchema: "IdentityDb",
                principalTable: "AuthRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AuthRoleClaims_ClaimDefinitions_ClaimDefinitionId",
                schema: "IdentityDb",
                table: "AuthRoleClaims",
                column: "ClaimDefinitionId",
                principalSchema: "IdentityDb",
                principalTable: "ClaimDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_Branches_Organizations_OrganizationId",
                schema: "IdentityDb",
                table: "Branches",
                column: "OrganizationId",
                principalSchema: "IdentityDb",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClaimDefinitions_Products_ProductId",
                schema: "IdentityDb",
                table: "ClaimDefinitions",
                column: "ProductId",
                principalSchema: "IdentityDb",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientProductBindings_Products_ProductId",
                schema: "IdentityDb",
                table: "ClientProductBindings",
                column: "ProductId",
                principalSchema: "IdentityDb",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalIdentityProviders_Organizations_OrganizationId",
                schema: "IdentityDb",
                table: "ExternalIdentityProviders",
                column: "OrganizationId",
                principalSchema: "IdentityDb",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LdapGroupRoleMaps_AuthRoles_RoleId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps",
                column: "RoleId",
                principalSchema: "IdentityDb",
                principalTable: "AuthRoles",
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
                name: "FK_LdapGroupRoleMaps_Organizations_OrganizationId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps",
                column: "OrganizationId",
                principalSchema: "IdentityDb",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LdapGroupRoleMaps_Products_ProductId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps",
                column: "ProductId",
                principalSchema: "IdentityDb",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_AspNetUsers_UserId",
                schema: "IdentityDb",
                table: "Memberships",
                column: "UserId",
                principalSchema: "IdentityDb",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Organizations_OrganizationId",
                schema: "IdentityDb",
                table: "Memberships",
                column: "OrganizationId",
                principalSchema: "IdentityDb",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_Products_ProductId",
                schema: "IdentityDb",
                table: "MenuItems",
                column: "ProductId",
                principalSchema: "IdentityDb",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationInvites_AspNetUsers_AcceptedUserId",
                schema: "IdentityDb",
                table: "OrganizationInvites",
                column: "AcceptedUserId",
                principalSchema: "IdentityDb",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationInvites_AspNetUsers_InvitedByUserId",
                schema: "IdentityDb",
                table: "OrganizationInvites",
                column: "InvitedByUserId",
                principalSchema: "IdentityDb",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationInvites_Organizations_OrganizationId",
                schema: "IdentityDb",
                table: "OrganizationInvites",
                column: "OrganizationId",
                principalSchema: "IdentityDb",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_AuthRoles_RoleId",
                schema: "IdentityDb",
                table: "RolePermissions",
                column: "RoleId",
                principalSchema: "IdentityDb",
                principalTable: "AuthRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                schema: "IdentityDb",
                table: "RolePermissions",
                column: "PermissionId",
                principalSchema: "IdentityDb",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserClaimAssignments_AspNetUsers_UserId",
                schema: "IdentityDb",
                table: "UserClaimAssignments",
                column: "UserId",
                principalSchema: "IdentityDb",
                principalTable: "AspNetUsers",
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
                name: "FK_UserClaimAssignments_ClaimDefinitions_ClaimDefinitionId",
                schema: "IdentityDb",
                table: "UserClaimAssignments",
                column: "ClaimDefinitionId",
                principalSchema: "IdentityDb",
                principalTable: "ClaimDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserClaimAssignments_Organizations_OrganizationId",
                schema: "IdentityDb",
                table: "UserClaimAssignments",
                column: "OrganizationId",
                principalSchema: "IdentityDb",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserClaimAssignments_Products_ProductId",
                schema: "IdentityDb",
                table: "UserClaimAssignments",
                column: "ProductId",
                principalSchema: "IdentityDb",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoleAssignments_AspNetUsers_UserId",
                schema: "IdentityDb",
                table: "UserRoleAssignments",
                column: "UserId",
                principalSchema: "IdentityDb",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoleAssignments_AuthRoles_RoleId",
                schema: "IdentityDb",
                table: "UserRoleAssignments",
                column: "RoleId",
                principalSchema: "IdentityDb",
                principalTable: "AuthRoles",
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
                name: "FK_UserRoleAssignments_Organizations_OrganizationId",
                schema: "IdentityDb",
                table: "UserRoleAssignments",
                column: "OrganizationId",
                principalSchema: "IdentityDb",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoleAssignments_Products_ProductId",
                schema: "IdentityDb",
                table: "UserRoleAssignments",
                column: "ProductId",
                principalSchema: "IdentityDb",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSessions_AspNetUsers_UserId",
                schema: "IdentityDb",
                table: "UserSessions",
                column: "UserId",
                principalSchema: "IdentityDb",
                principalTable: "AspNetUsers",
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

            migrationBuilder.AddForeignKey(
                name: "FK_UserSessions_Organizations_OrganizationId",
                schema: "IdentityDb",
                table: "UserSessions",
                column: "OrganizationId",
                principalSchema: "IdentityDb",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthRoleClaims_AuthRoles_RoleId",
                schema: "IdentityDb",
                table: "AuthRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AuthRoleClaims_ClaimDefinitions_ClaimDefinitionId",
                schema: "IdentityDb",
                table: "AuthRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Branches_ParentBranchId",
                schema: "IdentityDb",
                table: "Branches");

            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Organizations_OrganizationId",
                schema: "IdentityDb",
                table: "Branches");

            migrationBuilder.DropForeignKey(
                name: "FK_ClaimDefinitions_Products_ProductId",
                schema: "IdentityDb",
                table: "ClaimDefinitions");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientProductBindings_Products_ProductId",
                schema: "IdentityDb",
                table: "ClientProductBindings");

            migrationBuilder.DropForeignKey(
                name: "FK_ExternalIdentityProviders_Organizations_OrganizationId",
                schema: "IdentityDb",
                table: "ExternalIdentityProviders");

            migrationBuilder.DropForeignKey(
                name: "FK_LdapGroupRoleMaps_AuthRoles_RoleId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps");

            migrationBuilder.DropForeignKey(
                name: "FK_LdapGroupRoleMaps_Branches_BranchId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps");

            migrationBuilder.DropForeignKey(
                name: "FK_LdapGroupRoleMaps_Organizations_OrganizationId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps");

            migrationBuilder.DropForeignKey(
                name: "FK_LdapGroupRoleMaps_Products_ProductId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps");

            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_AspNetUsers_UserId",
                schema: "IdentityDb",
                table: "Memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Organizations_OrganizationId",
                schema: "IdentityDb",
                table: "Memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_Products_ProductId",
                schema: "IdentityDb",
                table: "MenuItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationInvites_AspNetUsers_AcceptedUserId",
                schema: "IdentityDb",
                table: "OrganizationInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationInvites_AspNetUsers_InvitedByUserId",
                schema: "IdentityDb",
                table: "OrganizationInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationInvites_Organizations_OrganizationId",
                schema: "IdentityDb",
                table: "OrganizationInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_AuthRoles_RoleId",
                schema: "IdentityDb",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                schema: "IdentityDb",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserClaimAssignments_AspNetUsers_UserId",
                schema: "IdentityDb",
                table: "UserClaimAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserClaimAssignments_Branches_BranchId",
                schema: "IdentityDb",
                table: "UserClaimAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserClaimAssignments_ClaimDefinitions_ClaimDefinitionId",
                schema: "IdentityDb",
                table: "UserClaimAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserClaimAssignments_Organizations_OrganizationId",
                schema: "IdentityDb",
                table: "UserClaimAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserClaimAssignments_Products_ProductId",
                schema: "IdentityDb",
                table: "UserClaimAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoleAssignments_AspNetUsers_UserId",
                schema: "IdentityDb",
                table: "UserRoleAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoleAssignments_AuthRoles_RoleId",
                schema: "IdentityDb",
                table: "UserRoleAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoleAssignments_Branches_BranchId",
                schema: "IdentityDb",
                table: "UserRoleAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoleAssignments_Organizations_OrganizationId",
                schema: "IdentityDb",
                table: "UserRoleAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoleAssignments_Products_ProductId",
                schema: "IdentityDb",
                table: "UserRoleAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSessions_AspNetUsers_UserId",
                schema: "IdentityDb",
                table: "UserSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSessions_Branches_BranchId",
                schema: "IdentityDb",
                table: "UserSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSessions_Organizations_OrganizationId",
                schema: "IdentityDb",
                table: "UserSessions");

            migrationBuilder.DropIndex(
                name: "IX_UserSessions_BranchId",
                schema: "IdentityDb",
                table: "UserSessions");

            migrationBuilder.DropIndex(
                name: "IX_UserSessions_OrganizationId",
                schema: "IdentityDb",
                table: "UserSessions");

            migrationBuilder.DropIndex(
                name: "IX_UserRoleAssignments_BranchId",
                schema: "IdentityDb",
                table: "UserRoleAssignments");

            migrationBuilder.DropIndex(
                name: "IX_UserRoleAssignments_OrganizationId",
                schema: "IdentityDb",
                table: "UserRoleAssignments");

            migrationBuilder.DropIndex(
                name: "IX_UserRoleAssignments_ProductId",
                schema: "IdentityDb",
                table: "UserRoleAssignments");

            migrationBuilder.DropIndex(
                name: "IX_UserRoleAssignments_RoleId",
                schema: "IdentityDb",
                table: "UserRoleAssignments");

            migrationBuilder.DropIndex(
                name: "IX_UserClaimAssignments_BranchId",
                schema: "IdentityDb",
                table: "UserClaimAssignments");

            migrationBuilder.DropIndex(
                name: "IX_UserClaimAssignments_ClaimDefinitionId",
                schema: "IdentityDb",
                table: "UserClaimAssignments");

            migrationBuilder.DropIndex(
                name: "IX_UserClaimAssignments_OrganizationId",
                schema: "IdentityDb",
                table: "UserClaimAssignments");

            migrationBuilder.DropIndex(
                name: "IX_UserClaimAssignments_ProductId",
                schema: "IdentityDb",
                table: "UserClaimAssignments");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_PermissionId",
                schema: "IdentityDb",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationInvites_AcceptedUserId",
                schema: "IdentityDb",
                table: "OrganizationInvites");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationInvites_InvitedByUserId",
                schema: "IdentityDb",
                table: "OrganizationInvites");

            migrationBuilder.DropIndex(
                name: "IX_Memberships_OrganizationId",
                schema: "IdentityDb",
                table: "Memberships");

            migrationBuilder.DropIndex(
                name: "IX_LdapGroupRoleMaps_BranchId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps");

            migrationBuilder.DropIndex(
                name: "IX_LdapGroupRoleMaps_ProductId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps");

            migrationBuilder.DropIndex(
                name: "IX_LdapGroupRoleMaps_RoleId",
                schema: "IdentityDb",
                table: "LdapGroupRoleMaps");

            migrationBuilder.DropIndex(
                name: "IX_ExternalIdentityProviders_OrganizationId",
                schema: "IdentityDb",
                table: "ExternalIdentityProviders");

            migrationBuilder.DropIndex(
                name: "IX_ClientProductBindings_ProductId",
                schema: "IdentityDb",
                table: "ClientProductBindings");

            migrationBuilder.DropIndex(
                name: "IX_ClaimDefinitions_ProductId",
                schema: "IdentityDb",
                table: "ClaimDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_Branches_ParentBranchId",
                schema: "IdentityDb",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_AuthRoleClaims_ClaimDefinitionId",
                schema: "IdentityDb",
                table: "AuthRoleClaims");
        }
    }
}
