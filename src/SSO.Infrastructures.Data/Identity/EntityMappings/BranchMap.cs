using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.Branches.Entity;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class BranchMap : IEntityTypeConfiguration<Branch>
	{
		public void Configure(EntityTypeBuilder<Branch> builder)
		{
			builder.ToTable("Branches");

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
			builder.Property(e => e.ParentBranchId)
				.HasColumnType("UNIQUEIDENTIFIER")
				.HasColumnName("ParentBranchId")
				.IsRequired(false);
			builder.Property(e => e.Name)
				.HasColumnType("NVARCHAR(128)")
				.HasColumnName("Name")
				.IsRequired(true);
			builder.Property(e => e.Code)
				.HasColumnType("NVARCHAR(64)")
				.HasColumnName("Code")
				.IsRequired(true);
			builder.Property(e => e.LegalName)
				.HasColumnType("NVARCHAR(256)")
				.HasColumnName("LegalName")
				.IsRequired(false);
			builder.Property(e => e.TradeName)
				.HasColumnType("NVARCHAR(128)")
				.HasColumnName("TradeName")
				.IsRequired(false);
			builder.Property(e => e.TaxId)
				.HasColumnType("NVARCHAR(18)")
				.HasColumnName("TaxId")
				.IsRequired(false);
			builder.Property(e => e.Segment)
				.HasColumnType("NVARCHAR(64)")
				.HasColumnName("Segment")
				.IsRequired(false);
			builder.Property(e => e.Description)
				.HasColumnType("NVARCHAR(1000)")
				.HasColumnName("Description")
				.IsRequired(false);
			builder.Property(e => e.PostalCode)
				.HasColumnType("NVARCHAR(16)")
				.HasColumnName("PostalCode")
				.IsRequired(false);
			builder.Property(e => e.Street)
				.HasColumnType("NVARCHAR(256)")
				.HasColumnName("Street")
				.IsRequired(false);
			builder.Property(e => e.Number)
				.HasColumnType("NVARCHAR(32)")
				.HasColumnName("Number")
				.IsRequired(false);
			builder.Property(e => e.Complement)
				.HasColumnType("NVARCHAR(128)")
				.HasColumnName("Complement")
				.IsRequired(false);
			builder.Property(e => e.City)
				.HasColumnType("NVARCHAR(128)")
				.HasColumnName("City")
				.IsRequired(false);
			builder.Property(e => e.State)
				.HasColumnType("NVARCHAR(2)")
				.HasColumnName("State")
				.IsRequired(false);

			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);

			builder.HasIndex(e => new { e.OrganizationId, e.Code })
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");

			builder.HasOne(e => e.Organization)
				.WithMany(o => o.Branches)
				.HasForeignKey(e => e.OrganizationId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.ParentBranch)
				.WithMany(b => b.ChildBranches)
				.HasForeignKey(e => e.ParentBranchId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
