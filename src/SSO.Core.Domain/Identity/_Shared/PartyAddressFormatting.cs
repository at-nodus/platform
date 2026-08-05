using System;
using System.Collections.Generic;
using System.Linq;

namespace SSO.Core.Domain.Identity._Shared
{
	public static class PartyAddressFormatting
	{
		public static string FormatAddress(
			string postalCode,
			string street,
			string number,
			string complement,
			string city,
			string state)
		{
			var line1Parts = new List<string>();
			if (!string.IsNullOrWhiteSpace(street))
			{
				line1Parts.Add(street.Trim());
			}

			if (!string.IsNullOrWhiteSpace(number))
			{
				line1Parts.Add(number.Trim());
			}

			if (!string.IsNullOrWhiteSpace(complement))
			{
				line1Parts.Add(complement.Trim());
			}

			var localityParts = new List<string>();
			if (!string.IsNullOrWhiteSpace(city) && !string.IsNullOrWhiteSpace(state))
			{
				localityParts.Add($"{city.Trim()}/{state.Trim().ToUpperInvariant()}");
			}
			else if (!string.IsNullOrWhiteSpace(city))
			{
				localityParts.Add(city.Trim());
			}
			else if (!string.IsNullOrWhiteSpace(state))
			{
				localityParts.Add(state.Trim().ToUpperInvariant());
			}

			if (!string.IsNullOrWhiteSpace(postalCode))
			{
				localityParts.Add(postalCode.Trim());
			}

			var segments = new List<string>();
			if (line1Parts.Count > 0)
			{
				segments.Add(string.Join(", ", line1Parts));
			}

			if (localityParts.Count > 0)
			{
				segments.Add(string.Join(" · ", localityParts));
			}

			return segments.Count == 0 ? string.Empty : string.Join(" · ", segments);
		}

		public static bool HasAnyAddress(params string[] values) =>
			values.Any(v => !string.IsNullOrWhiteSpace(v));

		public static string DigitsOnly(string? value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return string.Empty;
			}

			return string.Concat(value.Where(char.IsDigit));
		}

		public static IOrderedEnumerable<T> OrderBranchesMatrizThenTaxId<T>(
			IEnumerable<T> source,
			Func<T, Guid?> parentBranchId,
			Func<T, string?> taxId,
			Func<T, string> name) =>
			source
				.OrderBy(x => parentBranchId(x).HasValue ? 1 : 0)
				.ThenBy(x => DigitsOnly(taxId(x)))
				.ThenBy(x => name(x), StringComparer.OrdinalIgnoreCase);
	}
}
