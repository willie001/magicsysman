#region Using
using System;

using FluentValidation.Attributes;

using MagicMaids.EntityModels;
using MagicMaids.Validators;
#endregion

namespace MagicMaids.ViewModels
{
	public class UpdateAddressViewModel
	{
		#region Fields
		private string _state;
		private string _country;
		#endregion

		#region Properties,Public
		public Boolean IsNewItem
		{
			get;
			set;
		}

		public Guid Id
		{
			get;
			set;
		}

		public AddressTypeSetting AddressType
		{
			get;
			set;
		}

		public string AddressLine1
		{
			get;
			set;
		}

		public string AddressLine2
		{
			get;
			set;
		}

		public string AddressLine3
		{
			get;
			set;
		}

		public string Suburb
		{
			get;
			set;
		}

		public string State
		{
			get
			{
				if (String.IsNullOrWhiteSpace(_state))
				{
					return "WA";
				}
				return _state;
			}
			set
			{
				_state = value;
			}
		}

		public string PostCode
		{
			get;
			set;
		}

		public string Country
		{
			get
			{
				if (String.IsNullOrWhiteSpace(_country))
				{
					return "Australia";
				}
				return _country;
			}
			set
			{
				_country = value;
			}
		}

		public bool IsValidAddress
		{
			get
			{
				if (String.IsNullOrWhiteSpace(AddressLine1)
				   && String.IsNullOrWhiteSpace(Suburb)
				   && String.IsNullOrWhiteSpace(PostCode))
				{
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		public bool IsPartialAddress
		{
			get
			{
				if (!String.IsNullOrWhiteSpace(AddressLine1)
				   || !String.IsNullOrWhiteSpace(Suburb)
				   || !String.IsNullOrWhiteSpace(PostCode))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public string FormattedAddress
		{
			get;
			private set;
		}

		#endregion

		#region Methods, Public
		public void PopulateVM(Address entityModel)
		{
			if (entityModel == null)
				return;

			this.Id = entityModel.Id;
			this.AddressLine1 = entityModel.AddressLine1;
			this.AddressLine2 = entityModel.AddressLine2;
			this.AddressLine3 = entityModel.AddressLine3;
			this.AddressType = entityModel.AddressType;
			this.Suburb = entityModel.Suburb;
			this.State = entityModel.State;
			this.Country = entityModel.Country;
			this.PostCode = entityModel.PostCode;

			FormatAddress(entityModel);
		}
		#endregion

		#region Methods, Private
		private void FormatAddress(Address entityModel)
		{
			if (entityModel == null)
			{
				FormattedAddress = string.Empty;
				return;
			}

			System.Text.StringBuilder _output = new System.Text.StringBuilder();

			if (!String.IsNullOrWhiteSpace(entityModel.AddressLine1))
			{
				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append(entityModel.AddressLine1);
			}

			if (!String.IsNullOrWhiteSpace(entityModel.AddressLine2))
			{
				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append(entityModel.AddressLine2);
			}

			if (!String.IsNullOrWhiteSpace(entityModel.AddressLine3))
			{
				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append(entityModel.AddressLine3);
			}

			if (!String.IsNullOrWhiteSpace(entityModel.Suburb))
			{
				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append(entityModel.Suburb);
			}

			if (!String.IsNullOrWhiteSpace(entityModel.State))
			{
				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append(entityModel.State);
			}

			if (!String.IsNullOrWhiteSpace(entityModel.PostCode))
			{
				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append(entityModel.PostCode);
			}

			if (!String.IsNullOrWhiteSpace(entityModel.Country))
			{
				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append(entityModel.Country);
			}

			FormattedAddress  = _output.ToString();
		}
		#endregion
	}
}
