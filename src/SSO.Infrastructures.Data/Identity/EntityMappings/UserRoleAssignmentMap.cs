using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class UserRoleAssignmentMap : IEntityTypeConfiguration<UserRoleAssignment>
	{
		public void Configure(EntityTypeBuilder<UserRoleAssignment> builder)
		{
			builder.ToTable("UserRoleAssignments");

			builder.Property(p => p.Id)
				.HasColumnName("Id")
				.HasColumnType("UNIQUEIDENTIFIER")
				.ValueGeneratedOnAdd()
				.IsRequired(true);
			builder.HasKey(e => e.Id);

			builder.Property(e => e.UserId)
				.HasColumnType("UNIQUEIDENTIFIER")
				.HasColumnName("UserId")
				.IsRequired(true);
			builder.Property(e => e.RoleId)
				.HasColumnType("UNIQUEIDENTIFIER")
				.HasColumnName("RoleId")
				.IsRequired(true);
			builder.Property(e => e.OrganizationId)
				.HasColumnType("UNIQUEIDENTIFIER")
				.HasColumnName("OrganizationId")
				.IsRequired(false);
			builder.Property(e => e.BranchId)
				.HasColumnType("UNIQUEIDENTIFIER")
				.HasColumnName("BranchId")
				.IsRequired(false);
			builder.Property(e => e.ProductId)
				.HasColumnType("UNIQUEIDENTIFIER")
				.HasColumnName("ProductId")
				.IsRequired(true);
			builder.Property(e => e.Inheritable)
				.HasColumnType("bit")
				.HasColumnName("Inheritable")
				.IsRequired(true);

			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);

			builder.HasIndex(e => new { e.UserId, e.RoleId, e.OrganizationId, e.BranchId, e.ProductId })
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");

			builder.HasOne(e => e.User)
				.WithMany()
				.HasForeignKey(e => e.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.Role)
				.WithMany()
				.HasForeignKey(e => e.RoleId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.Organization)
				.WithMany()
				.HasForeignKey(e => e.OrganizationId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.Branch)
				.WithMany()
				.HasForeignKey(e => e.BranchId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.Product)
				.WithMany()
				.HasForeignKey(e => e.ProductId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
