using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.LdapGroupRoleMaps.Entity;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class LdapGroupRoleMapMap : IEntityTypeConfiguration<LdapGroupRoleMap>
	{
		public void Configure(EntityTypeBuilder<LdapGroupRoleMap> builder)
		{
			builder.ToTable("LdapGroupRoleMaps", t =>
			{
				t.HasCheckConstraint(
					"CK_LdapGroupRoleMaps_BranchRequiresOrg",
					"[BranchId] IS NULL OR [OrganizationId] IS NOT NULL");
			});

			builder.Property(p => p.Id)
				.HasColumnName("Id")
				.HasColumnType("UNIQUEIDENTIFIER")
				.ValueGeneratedOnAdd()
				.IsRequired(true);
			builder.HasKey(e => e.Id);

			builder.Property(e => e.OrganizationId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(true);
			builder.Property(e => e.GroupIdentifier).HasColumnType("nvarchar(512)").IsRequired(true);
			builder.Property(e => e.RoleId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(true);
			builder.Property(e => e.ProductId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(true);
			builder.Property(e => e.BranchId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(false);

			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);

			builder.HasIndex(e => new { e.OrganizationId, e.GroupIdentifier, e.RoleId, e.ProductId })
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");

			builder.HasOne(e => e.Organization)
				.WithMany(o => o.LdapGroupRoleMaps)
				.HasForeignKey(e => e.OrganizationId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.Role)
				.WithMany(r => r.LdapGroupRoleMaps)
				.HasForeignKey(e => e.RoleId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.Product)
				.WithMany(p => p.LdapGroupRoleMaps)
				.HasForeignKey(e => e.ProductId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.Branch)
				.WithMany(b => b.LdapGroupRoleMaps)
				.HasForeignKey(e => new { e.BranchId, e.OrganizationId })
				.HasPrincipalKey(b => new { b.Id, b.OrganizationId })
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
