using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace MagicMaids
{
	[Serializable]
	public class RequestWrapper
	{
		private HttpRequest _request;
		private HttpCookieCollection _cookies;
		private HttpBrowserCapabilities _browser;
		private NameValueCollection _headers;
		private NameValueCollection _params;
		private NameValueCollection _queryString;
		private NameValueCollection _serverVars;

		public RequestWrapper(HttpRequest request)
		{
			_request = request;

			ApplicationPath = _request.ApplicationPath;
			_browser = _request.Browser;
			_cookies = _request.Cookies;
			ContentType = _request.ContentType;
			_headers = _request.Headers;
			IsAuthenticated = _request.IsAuthenticated;
			IsLocal = _request.IsLocal;
			IsSecureConnection = _request.IsSecureConnection;
			_params = _request.Params;
			_queryString = _request.QueryString;
			_serverVars = _request.ServerVariables;
			UserAgent = _request.UserAgent;
		}

		public String  ApplicationPath
		{
			get;
			set;
		}

		public SortedDictionary<string, string> Browser
		{
			get
			{
				SortedDictionary<string, string> _return = new SortedDictionary<string, string>();
				if (_browser != null)
				{
					foreach (string key in _browser.Capabilities.Keys)
					{
						_return.Add(key, _browser.Capabilities[key].ToString());
					}
				}

				return _return;
			}
		}

		public SortedDictionary<string,string> Cookies
		{
			get
			{
				SortedDictionary<string, string> _return = new SortedDictionary<string, string>();
				if (_cookies != null)
				{
					foreach (string key in _cookies.AllKeys)
					{
						_return.Add(key, _cookies[key].Value);
					}
				}

				return _return;
			}
		}

		public String ContentType
		{
			get;
			set;
		}

		public SortedDictionary<string, string> Headers
		{
			get
			{
				SortedDictionary<string, string> _return = new SortedDictionary<string, string>();
				if (_headers != null)
				{
					foreach (string key in _headers.AllKeys)
					{
						_return.Add(key, _headers[key]);
					}
				}

				return _return;
			}
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

		public SortedDictionary<string, string> Params
		{
			get
			{
				SortedDictionary<string, string> _return = new SortedDictionary<string, string>();
				if (_params != null)
				{
					foreach (string key in _params.AllKeys)
					{
						_return.Add(key, _params[key]);
					}
				}

				return _return;
			}
		}

		public SortedDictionary<string, string> QueryString
		{
			get
			{
				SortedDictionary<string, string> _return = new SortedDictionary<string, string>();
				if (_queryString != null)
				{
					foreach (string key in _queryString.AllKeys)
					{
						_return.Add(key, _queryString[key]);
					}
				}

				return _return;
			}
		}
		public SortedDictionary<string, string> ServerVariables
		{
			get
			{
				SortedDictionary<string, string> _return = new SortedDictionary<string, string>();
				if (_serverVars  != null)
				{
					foreach (string key in _serverVars.AllKeys)
					{
						_return.Add(key, _serverVars[key]);
					}
				}

				return _return;
			}
		}

		public String UserAgent
		{
			get;
			set;
		}
	}
}
