#region Using
using System;
using MagicMaids.EntityModels;
#endregion


namespace MagicMaids.ViewModels 
{
	public class BaseContactVM
	{
		#region Properties, Public
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

		public string FormattedContactNumbers
		{
			get
			{
				if (String.IsNullOrWhiteSpace(_formattedContactNumbers) && HasAnyPhoneNumbers)
				{
					FormatContactNumbers();
				}
				return _formattedContactNumbers;
			}
			private set
			{
				_formattedContactNumbers = value;
			}
		}
		private string _formattedContactNumbers;
		#endregion

		#region Methods, Protected
		protected void FormatContactDetails(Address physicalAddress, Address postalAddress)
		{
			if (physicalAddress != null)
			{
				UpdateAddressViewModel _vm = new UpdateAddressViewModel();
				_vm.PopulateVM(physicalAddress);
				this.PhysicalAddress = _vm;
				this.PhysicalAddressRefId = new Guid(physicalAddress.Id);
			}

			if (postalAddress != null)
			{
				UpdateAddressViewModel _vm = new UpdateAddressViewModel();
				_vm.PopulateVM(postalAddress);
				this.PostalAddress = _vm;
				this.PostalAddressRefId = new Guid(postalAddress.Id);
			}

			FormatContactNumbers();
		}
		#endregion

		#region Methods, Private
		private void FormatContactNumbers()
		{
			System.Text.StringBuilder _output = new System.Text.StringBuilder();

			if (!String.IsNullOrWhiteSpace(this.BusinessPhoneNumber))
			{
				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append("<span class='fa fa-phone'></span>&nbsp;");
				_output.Append(this.BusinessPhoneNumber);
			}

			if (!String.IsNullOrWhiteSpace(this.MobileNumber))
			{
				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append("<span class='fa fa-mobile'></span>&nbsp;");
				_output.Append(this.MobileNumber);
			}

			if (!String.IsNullOrWhiteSpace(this.OtherNumber))
			{
				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append("<span class='fa fa-fax'></span>&nbsp;");
				_output.Append(this.OtherNumber);
			}

			FormattedContactNumbers = _output.ToString();
		}
		#endregion
	}
}
