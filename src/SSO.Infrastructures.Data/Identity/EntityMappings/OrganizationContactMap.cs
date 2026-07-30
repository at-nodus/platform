using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.OrganizationContacts.Entity;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class OrganizationContactMap : IEntityTypeConfiguration<OrganizationContact>
	{
		public void Configure(EntityTypeBuilder<OrganizationContact> builder)
		{
			builder.ToTable("OrganizationContacts");

			builder.Property(p => p.Id)
				.HasColumnName("Id")
				.HasColumnType("UNIQUEIDENTIFIER")
				.ValueGeneratedOnAdd()
				.IsRequired(true);
			builder.HasKey(e => e.Id);

			builder.Property(e => e.OrganizationId)
				.HasColumnType("UNIQUEIDENTIFIER")
				.HasColumnName("OrganizationId")
				.IsRequired(true);
			builder.Property(e => e.Name)
				.HasColumnType("NVARCHAR(200)")
				.HasColumnName("Name")
				.IsRequired(true);
			builder.Property(e => e.Email)
				.HasColumnType("NVARCHAR(256)")
				.HasColumnName("Email")
				.IsRequired(false);
			builder.Property(e => e.Phone)
				.HasColumnType("NVARCHAR(64)")
				.HasColumnName("Phone")
				.IsRequired(false);
			builder.Property(e => e.Title)
				.HasColumnType("NVARCHAR(128)")
				.HasColumnName("Title")
				.IsRequired(false);
			builder.Property(e => e.IsPrimary)
				.HasColumnType("bit")
				.HasColumnName("IsPrimary")
				.IsRequired(true);

			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);

			builder.HasIndex(e => e.OrganizationId);

			builder.HasOne(e => e.Organization)
				.WithMany(o => o.OrganizationContacts)
				.HasForeignKey(e => e.OrganizationId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
