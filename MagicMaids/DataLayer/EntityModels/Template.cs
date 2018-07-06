#region Using
using System;
#endregion


namespace MagicMaids.EntityModels
{
	public enum TemplateTypeSetting
	{
		Document,
		Email
	}

	[Flags]
	public enum TemplateAudienceSetting
	{
		Cleaners = 1,
		MasterFranchises = 2,
		Customers = 4
	}

	public class Template : BaseModel
	{

		#region Properties, Public
		public string TemplateName
		{
			get;
			set;
		}

		public string TemplateDescription
		{
			get;
			set;
		}

		public string LimitToCountry
		{
			get;
			set;
		}

		public TemplateTypeSetting TemplateType
		{
			get;
			set;
		}

		public TemplateAudienceSetting TemplateAudience
		{
			get;
			set;
		}

		public TemplateContent TemplateContent
		{
			get;
			set;
		}

		private EmailSchedule _emailSchedule;
		public EmailSchedule EmailSchedule
		{
			get
			{
				if (_emailSchedule == null)
				{
					_emailSchedule = new EmailSchedule();
					_emailSchedule.Schedule = ScheduleSetting.None;
				}
				return _emailSchedule;
			}
			set
			{
				_emailSchedule = value;	
			}
		}

		private string _templateAudienceHtml;
		public string TemplateAudienceHtml
		{
			get
			{
				if (!String.IsNullOrWhiteSpace(_templateAudienceHtml))
				{
					return _templateAudienceHtml;
				}

				System.Text.StringBuilder _output = new System.Text.StringBuilder();

				if (TemplateAudience.Equals(RateApplicationsSettings.None))
				{
					_templateAudienceHtml = string.Empty;
					return "<div class=\"label bg-danger-light\">Not set</div>";
				}
				else
				{
					if (TemplateAudience.HasFlag(TemplateAudienceSetting.Customers))
					{
						if (_output.Length > 0) _output.Append("&nbsp;");
						_output.Append("<div class=\"label bg-success\">Customers</div>");
					}

					if (TemplateAudience.HasFlag(TemplateAudienceSetting.MasterFranchises))
					{
						if (_output.Length > 0) _output.Append("&nbsp;");
						_output.Append("<div class=\"label bg-primary\">Master Franchises</div>");
					}

					if (TemplateAudience.HasFlag(TemplateAudienceSetting.Cleaners))
					{
						if (_output.Length > 0) _output.Append("&nbsp;");
						_output.Append("<div class=\"label bg-pink\">Cleaners</div>");
					}

					if (_output.Length == 0) _output.Append("<div class=\"label bg-danger-dark\">Invalid</div>");
				}

				_templateAudienceHtml = _output.ToString();
				return _templateAudienceHtml;
			}
		}
		#endregion
	}
}
