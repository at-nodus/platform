using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.UserClaimAssignments.Entity;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class UserClaimAssignmentMap : IEntityTypeConfiguration<UserClaimAssignment>
	{
		public void Configure(EntityTypeBuilder<UserClaimAssignment> builder)
		{
			builder.ToTable("UserClaimAssignments", t =>
			{
				t.HasCheckConstraint(
					"CK_UserClaimAssignments_BranchRequiresOrg",
					"[BranchId] IS NULL OR [OrganizationId] IS NOT NULL");
			});

			builder.Property(p => p.Id)
				.HasColumnName("Id")
				.HasColumnType("UNIQUEIDENTIFIER")
				.ValueGeneratedOnAdd()
				.IsRequired(true);
			builder.HasKey(e => e.Id);

			builder.Property(e => e.UserId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(true);
			builder.Property(e => e.ClaimDefinitionId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(true);
			builder.Property(e => e.Value).HasColumnType("nvarchar(512)").IsRequired(true);
			builder.Property(e => e.OrganizationId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(false);
			builder.Property(e => e.BranchId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(false);
			builder.Property(e => e.ProductId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(true);
			builder.Property(e => e.Inheritable).HasColumnType("bit").IsRequired(true);

			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);

			builder.HasIndex(e => new { e.UserId, e.ClaimDefinitionId, e.OrganizationId, e.BranchId, e.ProductId })
				.IsUnique()
				.HasFilter("[IsDeleted] = 0 AND [OrganizationId] IS NOT NULL")
				.HasDatabaseName("UX_UserClaimAssignments_Tenant");

			builder.HasIndex(e => new { e.UserId, e.ClaimDefinitionId, e.ProductId })
				.IsUnique()
				.HasFilter("[IsDeleted] = 0 AND [OrganizationId] IS NULL AND [BranchId] IS NULL")
				.HasDatabaseName("UX_UserClaimAssignments_Platform");

			builder.HasOne(e => e.User)
				.WithMany(u => u.UserClaimAssignments)
				.HasForeignKey(e => e.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.ClaimDefinition)
				.WithMany(c => c.UserClaimAssignments)
				.HasForeignKey(e => e.ClaimDefinitionId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.Organization)
				.WithMany(o => o.UserClaimAssignments)
				.HasForeignKey(e => e.OrganizationId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.Branch)
				.WithMany(b => b.UserClaimAssignments)
				.HasForeignKey(e => new { e.BranchId, e.OrganizationId })
				.HasPrincipalKey(b => new { b.Id, b.OrganizationId })
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.Product)
				.WithMany(p => p.UserClaimAssignments)
				.HasForeignKey(e => e.ProductId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
