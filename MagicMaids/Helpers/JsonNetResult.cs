using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace MagicMaids
{
	public class JsonNetResult : JsonResult
	{
		public JsonNetResult()
		{
			IsValid = true;
		}

		public bool IsValid
		{
			get;
			set;
		}

		public override void ExecuteResult(ControllerContext context)
		{
			HttpResponseBase response = context.HttpContext.Response;
			response.ContentType = "application/json";
			if (ContentEncoding != null)
				response.ContentEncoding = ContentEncoding;
			
			if (Data != null)
			{
				JsonTextWriter writer = new JsonTextWriter(response.Output) { Formatting = Formatting.Indented };
				JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings());
				serializer.Serialize(writer, Data);
				writer.Flush();
			}
		}
	}
}
