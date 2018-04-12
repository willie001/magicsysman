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

	public class RateListVM
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

		public List<object> SelectedRates
		{
			get;
			private set;
		}

		public RateApplicationsSettings SelectedRatesValue
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

			Id = new Guid(entityModel.Id);
			RateCode = entityModel.RateCode;
			RateAmount = entityModel.RateAmount;
			IsActive = entityModel.IsActive;
			ActivationDate = entityModel.ActivationDate;
			FranchiseId = (Helpers.IsValidGuid(entityModel.FranchiseId)) ? new Guid(entityModel.FranchiseId) : Guid.Empty;

			SelectedRatesValue = entityModel.RateApplications;

			SelectedRates = new List<object>();
			if (entityModel.RateApplications.HasFlag(RateApplicationsSettings.Commercial))
				SelectedRates.Add(new { 
					id = (int)RateApplicationsSettings.Commercial,
					name = RateApplicationsSettings.Commercial.ToString()
				});

			if (entityModel.RateApplications.HasFlag(RateApplicationsSettings.InitialVisit))
				SelectedRates.Add(new
				{
					id = (int)RateApplicationsSettings.InitialVisit,
					name = RateApplicationsSettings.InitialVisit.ToString()
				});

			if (entityModel.RateApplications.HasFlag(RateApplicationsSettings.StandardVisit))
				SelectedRates.Add(new
				{
					id = (int)RateApplicationsSettings.StandardVisit,
					name = RateApplicationsSettings.StandardVisit.ToString()
				});

			if (entityModel.RateApplications.HasFlag(RateApplicationsSettings.OneHour))
				SelectedRates.Add(new
				{
					id = (int)RateApplicationsSettings.OneHour,
					name = RateApplicationsSettings.OneHour.ToString()
				});

			if (entityModel.RateApplications.HasFlag(RateApplicationsSettings.OneOff))
				SelectedRates.Add(new
				{
					id = (int)RateApplicationsSettings.OneOff,
					name = RateApplicationsSettings.OneOff.ToString()
				});

			if (entityModel.RateApplications.HasFlag(RateApplicationsSettings.Residential))
				SelectedRates.Add(new
				{
					id = (int)RateApplicationsSettings.Residential,
					name = RateApplicationsSettings.Residential.ToString()
				});

			if (entityModel.RateApplications.HasFlag(RateApplicationsSettings.Vacate))
				SelectedRates.Add(new
				{
					id = (int)RateApplicationsSettings.Vacate,
					name = RateApplicationsSettings.Vacate.ToString()
				});

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

			if (entityModel.RateApplications.Equals(RateApplicationsSettings.None))
			{
				RateApplicationHtml =  "<div class=\"label bg-danger-light\">Not set</div>";
			}
			else
			{
				if (entityModel.RateApplications.HasFlag(RateApplicationsSettings.Residential))
				{
					if (_output.Length > 0) _output.Append("&nbsp;");
					_output.Append("<div class=\"label bg-success\">Residential</div>");
				}

				if (entityModel.RateApplications.HasFlag(RateApplicationsSettings.Commercial))
				{
					if (_output.Length > 0) _output.Append("&nbsp;");
					_output.Append("<div class=\"label bg-primary\">Commercial</div>");
				}

				if (entityModel.RateApplications.HasFlag(RateApplicationsSettings.InitialVisit))
				{
					if (_output.Length > 0) _output.Append("&nbsp;");
					_output.Append("<div class=\"label bg-inverse-light\">Initial</div>");
				}

				if (entityModel.RateApplications.HasFlag(RateApplicationsSettings.StandardVisit))
				{
					if (_output.Length > 0) _output.Append("&nbsp;");
					_output.Append("<div class=\"label bg-warning\">Standard</div>");
				}

				if (entityModel.RateApplications.HasFlag(RateApplicationsSettings.OneHour))
				{
					if (_output.Length > 0) _output.Append("&nbsp;");
					_output.Append("<div class=\"label bg-yellow\">One Hour</div>");
				}

				if (entityModel.RateApplications.HasFlag(RateApplicationsSettings.OneOff))
				{
					if (_output.Length > 0) _output.Append("&nbsp;");
					_output.Append("<div class=\"label bg-purple\">One-Off</div>");
				}

				if (entityModel.RateApplications.HasFlag(RateApplicationsSettings.Vacate))
				{
					if (_output.Length > 0) _output.Append("&nbsp;");
					_output.Append("<div class=\"label bg-pink\">Vacate</div>");
				}

				if (_output.Length == 0) _output.Append("<div class=\"label bg-danger-dark\">Invalid</div>");
			}

			RateApplicationHtml = _output.ToString();
		}
		#endregion
	}

	public class SelectedRateItem
	{
		public int Id
		{
			get;
			set;
		}

		public String Name
		{
			get;
			set;
		}
	}

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

		public String SelectedRatesJson
		{
			get;
			set;
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

			this.Id = new Guid(entityModel.Id);
			this.RateCode = entityModel.RateCode;
			this.RateAmount = entityModel.RateAmount;
			this.IsActive = entityModel.IsActive;
			this.ActivationDate = entityModel.ActivationDate;
			this.FranchiseId = (Helpers.IsValidGuid(entityModel.FranchiseId)) ? new Guid(entityModel.FranchiseId) : Guid.Empty;
		}
		#endregion
	}
}
