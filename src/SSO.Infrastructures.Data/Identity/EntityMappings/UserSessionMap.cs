using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.UserSessions.Entity;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class UserSessionMap : IEntityTypeConfiguration<UserSession>
	{
		public void Configure(EntityTypeBuilder<UserSession> builder)
		{
			builder.ToTable("UserSessions", t =>
			{
				t.HasCheckConstraint(
					"CK_UserSessions_BranchRequiresOrg",
					"[BranchId] IS NULL OR [OrganizationId] IS NOT NULL");
			});

			builder.Property(p => p.Id)
				.HasColumnName("Id")
				.HasColumnType("UNIQUEIDENTIFIER")
				.ValueGeneratedOnAdd()
				.IsRequired(true);
			builder.HasKey(e => e.Id);

			builder.Property(e => e.UserId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(true);
			builder.Property(e => e.ClientId).HasColumnType("nvarchar(100)").IsRequired(true);
			builder.Property(e => e.OrganizationId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(false);
			builder.Property(e => e.BranchId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(false);
			builder.Property(e => e.LastSeenAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.RevokedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.RevokeReason).HasColumnType("nvarchar(256)").IsRequired(false);

			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);

			builder.HasIndex(e => e.UserId);
			builder.HasIndex(e => new { e.UserId, e.RevokedAt });

			builder.HasOne(e => e.User)
				.WithMany(u => u.UserSessions)
				.HasForeignKey(e => e.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.Organization)
				.WithMany(o => o.UserSessions)
				.HasForeignKey(e => e.OrganizationId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.Branch)
				.WithMany(b => b.UserSessions)
				.HasForeignKey(e => new { e.BranchId, e.OrganizationId })
				.HasPrincipalKey(b => new { b.Id, b.OrganizationId })
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOpenIddictClient();
		}
	}
}
