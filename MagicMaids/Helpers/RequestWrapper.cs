using System;
using System.Collections.Specialized;
using System.Web;

namespace MagicMaids
{
	[Serializable]
	public class RequestWrapper
	{
		private HttpRequest _request;
		public RequestWrapper(HttpRequest request)
		{
			_request = request;

			ApplicationPath = _request.ApplicationPath;
			//Browser = _request.Browser.Capabilities.ToList;
			Cookies = _request.Cookies;
			ContentType = _request.ContentType;
			Headers = _request.Headers;
			IsAuthenticated = _request.IsAuthenticated;
			IsLocal = _request.IsLocal;
			IsSecureConnection = _request.IsSecureConnection;
			//Params = _request.Params;
			QueryString = _request.QueryString;
			ServerVariables = _request.ServerVariables;
			UserAgent = _request.UserAgent;
		}

		public String  ApplicationPath
		{
			get;
			set;
		}

		public HttpBrowserCapabilities Browser
		{
			get;
			set;
		}

		public HttpCookieCollection Cookies
		{
			get;
			set;
		}

		public String ContentType
		{
			get;
			set;
		}

		public NameValueCollection Headers
		{
			get;
			set;
		}

		public bool? IsAuthenticated
		{
			get;
			set;
		}

		public bool? IsLocal
		{
			get;
			set;
		}

		public bool? IsSecureConnection
		{
			get;
			set;
		}

		public NameValueCollection Params
		{
			get;
			set;
		}

		public NameValueCollection QueryString
		{
			get;
			set;
		}

		public NameValueCollection ServerVariables
		{
			get;
			set;
		}

		public String UserAgent
		{
			get;
			set;
		}
	}
}
