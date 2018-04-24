#region Using
using System;
using System.Collections.Generic;
using FluentValidation.Attributes;

using MagicMaids.EntityModels;
using MagicMaids.Validators;
#endregion

namespace MagicMaids.ViewModels
{
	[Validator(typeof(FranchiseSettingsValidator))]
	public class FranchiseSettingsVM
	{
		#region Properties, Public
		public String Id
		{
			get;
			set;
		}

		public String Name
		{
			get
			{
				return franchiseName;
			}
		}
		private String franchiseName;

		public Boolean HasCustomRoyaltyValues
		{
			get;
			set;
		}

		public Boolean HasCustomManagementValues
		{
			get;
			set;
		}


		public decimal CurrentManagementFeePercentage
		{
			get
			{
				if (ManagementFeePercentage.HasValue)
				{
					return ManagementFeePercentage.Value;
				}
				else
				{
					return DefaultManagementFeePercentage;
				}
			}
		}

		public decimal CurrentRoyaltyFeePercentage
		{
			get
			{
				if (RoyaltyFeePercentage.HasValue)
				{
					return RoyaltyFeePercentage.Value;
				}
				else
				{
					return DefaultRoyaltyFeePercentage;
				}
			}
		}
		public decimal? ManagementFeePercentage
		{
			get;
			set;
		}

		public decimal? RoyaltyFeePercentage
		{
			get;
			set;
		}

		public decimal DefaultManagementFeePercentage
		{
			get
			{
				return defaultManagementFeePercentage;
			}
		}
		private decimal defaultManagementFeePercentage;

		public decimal DefaultRoyaltyFeePercentage
		{
			get
			{
				return defaultRoyaltyFeePercentage;
			}
		}
		private decimal defaultRoyaltyFeePercentage;
		#endregion

		#region Methods, Public
		public void PopulateVM(Franchise entityModel, List<SystemSetting> defaultSettings)
		{
			if (entityModel == null || defaultSettings == null)
				return;

			this.Id = entityModel.Id;
			this.franchiseName = entityModel.Name;
			this.ManagementFeePercentage = entityModel.ManagementFeePercentage;
			this.RoyaltyFeePercentage = entityModel.RoyaltyFeePercentage;

			this.HasCustomRoyaltyValues = this.RoyaltyFeePercentage.HasValue;
			this.HasCustomManagementValues = this.ManagementFeePercentage.HasValue;

			foreach (SystemSetting item in defaultSettings)
			{
				switch(item.CodeIdentifier)
				{
					case "MANAGE_FEE_PERC":
						decimal.TryParse(item.SettingValue, out defaultManagementFeePercentage);
						break;
					case "ROYALTY_FEE_PERC":
						decimal.TryParse(item.SettingValue, out defaultRoyaltyFeePercentage);
						break;
					default:
						break;
				}
			}
		}
		#endregion
	}
}
