﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;

namespace MagicMaids.Controllers
{
    public class Account : Controller
    {
		/*
         *  Called when requesting to sign in
         */
		public void SignIn()
		{
			if (!Request.IsAuthenticated)
			{
				// to execute policy simply trigger OWIN challenge
				HttpContext.GetOwinContext().Authentication.Challenge(
					new AuthenticationProperties(){ RedirectUri = "/"}, Startup.SignInPolicyId );
				return;
			}

			Response.Redirect("/");
		}

		/*
         *  Called when requesting to sign Up
         */
		public void NewUser()
		{
			if (!Request.IsAuthenticated)
			{
				// to execute policy simply trigger OWIN challenge
				HttpContext.GetOwinContext().Authentication.Challenge(
					new AuthenticationProperties() { RedirectUri = "/" }, Startup.NewUserPolicyId);
				return;
			}

			Response.Redirect("/");
		}

		/*
         *  Called when requesting to edit a profile
         */
		public void EditProfile()
		{
			if (Request.IsAuthenticated)
			{
				// Let the middleware know you are trying to use the edit profile policy (see OnRedirectToIdentityProvider in Startup.Auth.cs)
				HttpContext.GetOwinContext().Set("Policy", Startup.EditProfilePolicyId);

				// Set the page to redirect to after editing the profile
				var authenticationProperties = new AuthenticationProperties { RedirectUri = "/" };
				HttpContext.GetOwinContext().Authentication.Challenge(authenticationProperties);

				return;
			}

			Response.Redirect("/");

		}

		/*
         *  Called when requesting to reset a password
         */
		public void ResetPassword()
		{
			// Let the middleware know you are trying to use the reset password policy (see OnRedirectToIdentityProvider in Startup.Auth.cs)
			HttpContext.GetOwinContext().Set("Policy", Startup.ResetPasswordPolicyId);

			// Set the page to redirect to after changing passwords
			var authenticationProperties = new AuthenticationProperties { RedirectUri = "/" };
			HttpContext.GetOwinContext().Authentication.Challenge(authenticationProperties);

			return;
		}

		/*
         *  Called when requesting to sign out
         */
		public void SignOut()
		{
			// To sign out the user, you should issue an OpenIDConnect sign out request.
			if (Request.IsAuthenticated)
			{
				IEnumerable<AuthenticationDescription> authTypes = HttpContext.GetOwinContext().Authentication.GetAuthenticationTypes();
				HttpContext.GetOwinContext().Authentication.SignOut(authTypes.Select(t => t.AuthenticationType).ToArray());
				Request.GetOwinContext().Authentication.GetAuthenticationTypes();
			}
		}
    }
}
