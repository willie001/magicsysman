﻿#region Using
using System;

using FluentValidation.Attributes;

using MagicMaids.EntityModels;
using MagicMaids.Validators;
#endregion

namespace MagicMaids.ViewModels
{
	[Validator(typeof(FranchiseValidator))]
	public class UpdateFranchisesViewModel
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

		public Guid Id
		{
			get;
			set;
		}

		public DateTime RowVersion
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

		public decimal? ManagementFeePercentage
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

		public Guid PhysicalAddressRefId
		{
			get;
			set;
		}

		public UpdateAddressViewModel PhysicalAddress
		{
			get;
			set;
		}

		public Guid PostalAddressRefId
		{
			get;
			set;
		}

		public UpdateAddressViewModel PostalAddress
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
				if (PhysicalAddress == null && PostalAddress == null)
					return false;

				if (HasValidPostalAddress || HasValidPhysicalAddress)
					return true;
				else
					return false;
			}
		}

		public bool HasValidPostalAddress
		{
			get
			{
				if (PostalAddress == null)
				{
					return false;
				}

				return PostalAddress.IsValidAddress;
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
			this.ManagementFeePercentage = entityModel.ManagementFeePercentage;
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
			if (entityModel.PostalAddress != null)
			{
				UpdateAddressViewModel _vm = new UpdateAddressViewModel();
				_vm.PopulateVM(entityModel.PostalAddress);
				this.PostalAddress = _vm;
			}
			this.PostalAddressRefId = entityModel.PostalAddressRefId;
			this.RowVersion = entityModel.RowVersion;

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