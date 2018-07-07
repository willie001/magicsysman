using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Web;
using NodaTime;
using NodaTime.Extensions;

namespace MagicMaids
{
	public static class Extensions
	{
		public static string UserName(this ClaimsPrincipal user)
		{
			if (user == null)
			{
				return "guest";
			}

			var displayName = user.FindFirst(ClaimsPrincipal.Current.Identities.First().NameClaimType);
			var currentUser = displayName != null ? displayName.Value : string.Empty;

			if (String.IsNullOrWhiteSpace(currentUser))
				return "guest";

			return currentUser;
		}


	}

}
