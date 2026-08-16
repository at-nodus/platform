using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenIddict.EntityFrameworkCore.Models;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	internal static class OpenIddictClientRelationshipExtensions
	{
		public static void HasOpenIddictClient<TEntity>(this EntityTypeBuilder<TEntity> builder)
			where TEntity : class
		{
			builder.HasOne<OpenIddictEntityFrameworkCoreApplication<Guid>>()
				.WithMany()
				.HasForeignKey("ClientId")
				.HasPrincipalKey(a => a.ClientId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
