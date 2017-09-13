#region Using
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation.Attributes;

using MagicMaids.EntityModels;
using MagicMaids.Validators;
#endregion

namespace MagicMaids.ViewModels
{
	[Validator(typeof(RateValidator))]
	public class RateDetailsVM
	{
		#region Properties, Public
		public Boolean IsNewItem
		{
			get;
			set;
		}

		public Boolean IsActive
		{
			get;
			set;
		}

		public DateTime? ActivationDate
		{
			get;
			set;
		}

		public Guid Id
		{
			get;
			set;
		}

		public string RateCode
		{
			get;
			set;
		}

		[DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
		public decimal RateAmount
		{
			get;
			set;
		}

		public Dictionary<int, string> AvailableRateApplications
		{
			get;
			private set;
		}

		public RateApplicationsSettings SelectedRateApplications
		{
			get;
			set;
		}

		public string RateApplicationHtml
		{
			get;
			private set;
		}

		public Guid? FranchiseId
		{
			get;
			set;
		}
		#endregion

		#region Methods, Public
		public void PopulateVM(Rate entityModel)
		{
			if (entityModel == null)
				return;

			this.Id = entityModel.Id;
			this.RateCode = entityModel.RateCode;
			this.RateAmount = entityModel.RateAmount;
			this.SelectedRateApplications = entityModel.RateApplications;
			this.IsActive = entityModel.IsActive;
			this.ActivationDate = entityModel.ActivationDate;
			this.FranchiseId = entityModel.FranchiseId.HasValue ? entityModel.FranchiseId : null;

			if (AvailableRateApplications == null)
			{
				AvailableRateApplications = new Dictionary<int, string>();
			}
			foreach (var item in Enum.GetValues(typeof(MagicMaids.EntityModels.RateApplicationsSettings)))
			{
				//AvailableRateApplications.Add(0, item);// = (AvailableRateApplications | item);
			}

			FormatApplicationHtml(entityModel);
		}
		#endregion

		#region Methods, Private
		private void FormatApplicationHtml(Rate entityModel)
		{
			if (entityModel == null)
			{
				RateApplicationHtml = string.Empty;
				return;
			}

			System.Text.StringBuilder _output = new System.Text.StringBuilder();

			if (SelectedRateApplications.Equals(RateApplicationsSettings.None))
			{
				RateApplicationHtml =  "<div class=\"label bg-danger-light\">Not set</div>";
			}
			else
			{
				if (SelectedRateApplications.HasFlag(RateApplicationsSettings.Residential))
				{
					if (_output.Length > 0) _output.Append("&nbsp;");
					_output.Append("<div class=\"label bg-success\">Residential</div>");
				}

				if (SelectedRateApplications.HasFlag(RateApplicationsSettings.Commercial))
				{
					if (_output.Length > 0) _output.Append("&nbsp;");
					_output.Append("<div class=\"label bg-primary\">Commercial</div>");
				}

				if (SelectedRateApplications.HasFlag(RateApplicationsSettings.FirstVisit))
				{
					if (_output.Length > 0) _output.Append("&nbsp;");
					_output.Append("<div class=\"label bg-inverse-light\">Initial</div>");
				}

				if (SelectedRateApplications.HasFlag(RateApplicationsSettings.NormalVisit))
				{
					if (_output.Length > 0) _output.Append("&nbsp;");
					_output.Append("<div class=\"label bg-warning\">Standard</div>");
				}

				if (SelectedRateApplications.HasFlag(RateApplicationsSettings.OneHour))
				{
					if (_output.Length > 0) _output.Append("&nbsp;");
					_output.Append("<div class=\"label bg-yellow\">One Hour</div>");
				}

				if (SelectedRateApplications.HasFlag(RateApplicationsSettings.OneOff))
				{
					if (_output.Length > 0) _output.Append("&nbsp;");
					_output.Append("<div class=\"label bg-purple\">One-Off</div>");
				}

				if (SelectedRateApplications.HasFlag(RateApplicationsSettings.Vacancy))
				{
					if (_output.Length > 0) _output.Append("&nbsp;");
					_output.Append("<div class=\"label bg-pink\">Vacancy</div>");
				}

				if (_output.Length == 0) _output.Append("<div class=\"label bg-danger-dark\">Invalid</div>");
			}

			RateApplicationHtml = _output.ToString();
		}
		#endregion
	}
}
