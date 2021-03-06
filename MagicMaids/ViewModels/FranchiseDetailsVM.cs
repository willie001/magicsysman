﻿#region Using
using System;
using System.Collections.Generic;
using FluentValidation.Attributes;

using MagicMaids.EntityModels;
using MagicMaids.Validators;
#endregion

namespace MagicMaids.ViewModels
{
	public class FranchiseSelectViewModel: BaseViewModel 
	{
		#region Properties, Public
		public String Id
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public decimal? ManagementFeePercentage
		{
			get;
			set;
		}
		#endregion

		#region Methods, Public
		public void PopulateVM(Franchise entityModel, List<SystemSetting> defaultSettings)
		{
			if (entityModel == null)
				return;

			this.Id = entityModel.Id.ToString();
			this.Name = entityModel.Name;
			this.ManagementFeePercentage = entityModel.ManagementFeePercentage;

			if (!this.ManagementFeePercentage.HasValue)
			{
				Decimal settingValue;
				foreach (SystemSetting item in defaultSettings)
				{
					switch (item.CodeIdentifier)
					{
						case "MANAGE_FEE_PERC":
							decimal.TryParse(item.SettingValue, out settingValue);
							this.ManagementFeePercentage = settingValue;
							break;
						//case "ROYALTY_FEE_PERC":
							//decimal.TryParse(item.SettingValue, out settingValue);
							//break;
						default:
							break;
					}
				}
			}
		}
		#endregion
	}

	[Validator(typeof(FranchiseDetailsValidator))]
	public class UpdateFranchisesViewModel: BaseViewModel
	{
		#region Properties, Public
		public String Id
		{
			get;
			set;
		}
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

		public string MasterFranchiseCode
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public string TradingName
		{
			get;
			set;
		}

		public string EmailAddress
		{
			get;
			set;
		}

		public string BusinessPhoneNumber
		{
			get;
			set;
		}

		public string MobileNumber
		{
			get;
			set;
		}

		public string OtherNumber
		{
			get;
			set;
		}

		public string CodeOfConductURL
		{
			get;
			set;
		}

		public string MetroRegion
		{
			get;
			set;
		}

		public string FormattedContactNumbers
		{
			get;
			private set;
		}

		public String PhysicalAddressRefId
		{
			get;
			set;
		}

		public UpdateAddressViewModel PhysicalAddress
		{
			get;
			set;
		}

		public bool HasAnyPhoneNumbers
		{
			get
			{
				if (String.IsNullOrWhiteSpace(BusinessPhoneNumber) &&
				   String.IsNullOrWhiteSpace(MobileNumber) &&
				   String.IsNullOrWhiteSpace(OtherNumber))
				{
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		public bool HasAnyValidAddress
		{
			get
			{
				if (PhysicalAddress == null)
					return false;

				if (HasValidPhysicalAddress)
					return true;
				else
					return false;
			}
		}

		public bool HasValidPhysicalAddress
		{
			get
			{
				if (PhysicalAddress == null)
				{
					return false;
				}

				return PhysicalAddress.IsValidAddress;
			}
		}
		#endregion

		#region Methods, Public
		public void PopulateVM(Franchise entityModel)
		{
			if (entityModel == null)
				return;

			this.Id = entityModel.Id;
			this.MasterFranchiseCode = entityModel.MasterFranchiseCode ;
			this.Name = entityModel.Name;
			this.TradingName = entityModel.TradingName ;
			this.EmailAddress = entityModel.EmailAddress ;
			this.BusinessPhoneNumber = entityModel.BusinessPhoneNumber;
			this.OtherNumber = entityModel.OtherNumber ;
			this.MobileNumber = entityModel.MobileNumber ;
			this.CodeOfConductURL  = entityModel.CodeOfConductURL;
			this.MetroRegion  = entityModel.MetroRegion ;
			this.PhysicalAddressRefId  = entityModel.PhysicalAddressRefId ;
			this.IsActive = entityModel.IsActive;

			if (entityModel.PhysicalAddress != null)
			{
				UpdateAddressViewModel _vm = new UpdateAddressViewModel();
				_vm.PopulateVM(entityModel.PhysicalAddress);
				this.PhysicalAddress = _vm;
			}
			this.PhysicalAddressRefId = entityModel.PhysicalAddressRefId;

			FormatContactNumbers(entityModel);
		}
		#endregion

		#region Methods, Private
		private void FormatContactNumbers(Franchise entityModel)
		{
			if (entityModel == null)
			{
				FormattedContactNumbers  = string.Empty;
				return;
			}

			System.Text.StringBuilder _output = new System.Text.StringBuilder();

			if (!String.IsNullOrWhiteSpace(entityModel.BusinessPhoneNumber))
			{
				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append("<span class='fa fa-phone'></span>&nbsp;");
				_output.Append(entityModel.BusinessPhoneNumber);
			}

			if (!String.IsNullOrWhiteSpace(entityModel.MobileNumber))
			{
				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append("<span class='fa fa-mobile'></span>&nbsp;");
				_output.Append(entityModel.MobileNumber);
			}

			if (!String.IsNullOrWhiteSpace(entityModel.OtherNumber))
			{
				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append("<span class='fa fa-fax'></span>&nbsp;");
				_output.Append(entityModel.OtherNumber);
			}

			FormattedContactNumbers  = _output.ToString();
		}
		#endregion 
	}
}
