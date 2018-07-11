using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Configuration;
using System.IdentityModel.Claims;
using System.Threading.Tasks;
using System.Web;

namespace MagicMaids
{
	public partial class Startup
	{

        // App config settings
        public static string ClientId = ConfigurationManager.AppSettings["ida:ClientId"];
        public static string ClientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
        public static string Tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        public static string RedirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        public static string AadInstance = ConfigurationManager.AppSettings["ida:AadInstance"];

        // B2C policy identifiers
		public static string SignInPolicyId = ConfigurationManager.AppSettings["ida:SignInPolicyId"];
		public static string EditProfilePolicyId = ConfigurationManager.AppSettings["ida:EditPolicyId"];
		public static string ResetPasswordPolicyId = ConfigurationManager.AppSettings["ida:ResetPasswordPolicyId"];
		public static string NewUserPolicyId = ConfigurationManager.AppSettings["ida:NewUserPolicyId"];

		public static string DefaultPolicy = SignInPolicyId;

		// OWIN auth middleware constants
		public const string ObjectIdElement = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

		// Authorities
		public static string Authority = String.Format(AadInstance, Tenant, DefaultPolicy);

		/*
        * Configure the OWIN middleware 
        */
		public void ConfigureAuth(IAppBuilder app)
		{
			app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

			app.UseCookieAuthentication(new CookieAuthenticationOptions());

			// configure openId Connect middleware for each policy
			app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(EditProfilePolicyId));
			app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(ResetPasswordPolicyId));
			//app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(NewUserPolicyId));
			app.UseOpenIdConnectAuthentication(CreateOptionsFromPolicy(SignInPolicyId));
		}

		private OpenIdConnectAuthenticationOptions CreateOptionsFromPolicy(string policy)
		{
			return new OpenIdConnectAuthenticationOptions
			{
				// for each policy give OWIN policy-specific metadata address and set auth type 
				// to ID of the policy
				MetadataAddress = String.Format(AadInstance, Tenant, policy),
				AuthenticationType = policy,

				// These are standard OpenID Connect parameters, with values pulled from web.config
				ClientId = ClientId,
				RedirectUri = RedirectUri,
				PostLogoutRedirectUri = RedirectUri,

				// Specify the callbacks for each type of notifications
				Notifications = new OpenIdConnectAuthenticationNotifications
				{
					RedirectToIdentityProvider = OnRedirectToIdentityProvider,
					AuthorizationCodeReceived = OnAuthorizationCodeReceived,
					AuthenticationFailed = OnAuthenticationFailed,
				},

				// Specify the claim type that specifies the Name property.
				TokenValidationParameters = new TokenValidationParameters
				{
					NameClaimType = "name"
				},

				// Specify the scope by appending all of the scopes requested into one string (separated by a blank space)
				// {ReadTasksScope} {WriteTasksScope}"
				Scope = $"openid profile offline_access",
				ResponseType = "id_token"
			};
		}

		/*
         *  On each call to Azure AD B2C, check if a policy (e.g. the profile edit or password reset policy) has been specified in the OWIN context.
         *  If so, use that policy when making the call. Also, don't request a code (since it won't be needed).
         */
		private Task OnRedirectToIdentityProvider(RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
		{
			var policy = notification.OwinContext.Get<string>("Policy");

			if (!string.IsNullOrEmpty(policy) && !policy.Equals(DefaultPolicy))
			{
				notification.ProtocolMessage.Scope = OpenIdConnectScope.OpenId;
				notification.ProtocolMessage.ResponseType = OpenIdConnectResponseType.IdToken;
				notification.ProtocolMessage.IssuerAddress = notification.ProtocolMessage.IssuerAddress.ToLower().Replace(DefaultPolicy.ToLower(), policy.ToLower());
			}

			return Task.FromResult(0);
		}

		/*
         * Catch any failures received by the authentication middleware and handle appropriately
         */
		private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
		{
			notification.HandleResponse();

			// Handle the error code that Azure AD B2C throws when trying to reset a password from the login page 
			// because password reset is not supported by a "sign-up or sign-in policy"
			if (notification.ProtocolMessage.ErrorDescription != null && notification.ProtocolMessage.ErrorDescription.Contains("AADB2C90118"))
			{
				// If the user clicked the reset password link, redirect to the reset password route
				notification.Response.Redirect("/Account/ResetPassword");
			}
			else if (notification.Exception.Message == "access_denied")
			{
				LogHelper log = new LogHelper();
				log.Log(LogHelper.LogLevels.Warning, "Access Denied", nameof(OnAuthenticationFailed), notification.Exception, null, null);
				notification.Response.Redirect("/");
			}
			else
			{
				LogHelper log = new LogHelper();
				log.Log(LogHelper.LogLevels.Error, "Error authenticating user", nameof(OnAuthenticationFailed), notification.Exception, null, null);
				notification.Response.Redirect("/pages/Error?message=" + notification.Exception.Message);
			}

			return Task.FromResult(0);
		}


		/*
         * Callback function when an authorization code is received 
         */
		private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedNotification notification)
		{
			// Extract the code from the response notification
			var code = notification.Code;

			string signedInUserID = notification.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;
			TokenCache userTokenCache = new MSALSessionCache(signedInUserID, notification.OwinContext.Environment["System.Web.HttpContextBase"] as HttpContextBase).GetMsalCacheInstance();
			ConfidentialClientApplication cca = new ConfidentialClientApplication(ClientId, Authority, RedirectUri, new ClientCredential(ClientSecret), userTokenCache, null);
			try
			{
				//AuthenticationResult result = await cca.AcquireTokenByAuthorizationCodeAsync(code, Scopes);
			}
			catch (Exception ex)
			{
				//TODO: Handle
				throw;
			}
		}
	}
}
