using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.ProductEnablements.Entity;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class ProductEnablementMap : IEntityTypeConfiguration<ProductEnablement>
	{
		public void Configure(EntityTypeBuilder<ProductEnablement> builder)
		{
			builder.ToTable("ProductEnablements");

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
			builder.Property(e => e.ProductId)
				.HasColumnType("UNIQUEIDENTIFIER")
				.HasColumnName("ProductId")
				.IsRequired(true);

			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);

			builder.HasIndex(e => new { e.OrganizationId, e.ProductId })
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");

			builder.HasOne(e => e.Organization)
				.WithMany(o => o.ProductEnablements)
				.HasForeignKey(e => e.OrganizationId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.Product)
				.WithMany(p => p.ProductEnablements)
				.HasForeignKey(e => e.ProductId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
