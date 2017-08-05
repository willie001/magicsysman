#region Using
using System;
using System.ComponentModel.DataAnnotations.Schema;

using FluentValidation;
using NLog;
#endregion

namespace MagicMaids.EntityModels
{
	public enum AddressTypeSetting
	{
		Postal,
		Physical
	}

	[Table("Addresses")]
	public class Address : BaseModel
	{
		#region Fields
		private string _state;
		private string _country;
		#endregion 

		#region Properties, Public
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
			get{
				if (String.IsNullOrWhiteSpace(_state))
				{
					return "WA";
				}
				return _state;
			}
			set{
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
			get{
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

		private string _formattedAddress;
		public string FormattedAddress
		{
			get
			{
				if (!String.IsNullOrWhiteSpace(_formattedAddress))
				{
					return _formattedAddress;
				}

				System.Text.StringBuilder _output = new System.Text.StringBuilder();

				if (!String.IsNullOrWhiteSpace(AddressLine1))
				{
					if (_output.Length > 0) _output.Append("<br/>");
					_output.Append(AddressLine1);	
				}

				if (!String.IsNullOrWhiteSpace(AddressLine2))
				{
					if (_output.Length > 0) _output.Append("<br/>");
					_output.Append(AddressLine2);	
				}

				if (!String.IsNullOrWhiteSpace(AddressLine3))
				{
					if (_output.Length > 0) _output.Append("<br/>");
					_output.Append(AddressLine3);	
				}

				if (!String.IsNullOrWhiteSpace(Suburb))
				{
					if (_output.Length > 0) _output.Append("<br/>");
					_output.Append(Suburb);	
				}

				if (!String.IsNullOrWhiteSpace(State))
				{
					if (_output.Length > 0) _output.Append("<br/>");
					_output.Append(State);	
				}

				if (!String.IsNullOrWhiteSpace(PostCode))
				{
					if (_output.Length > 0) _output.Append("<br/>");
					_output.Append(PostCode);	
				}

				if (!String.IsNullOrWhiteSpace(Country))
				{
					if (_output.Length > 0) _output.Append("<br/>");
					_output.Append(Country);	
				}

				_formattedAddress = _output.ToString();
				return _formattedAddress;
			}
		}

		#endregion
	}

	//https://github.com/JeremySkinner/FluentValidation/wiki/c.-Built-In-Validators
	public class AddressValidator : AbstractValidator<Address>
	{
		public AddressValidator()
		{
			try
			{
				RuleFor(x => x.Id).NotNull();

				RuleFor(x => x.AddressLine1).NotEmpty().WithMessage(x => $"{x.AddressType} 1st address line is required.");
				RuleFor(x => x.Suburb).NotEmpty().WithMessage(x => $"{x.AddressType} Suburb is required.");
				RuleFor(x => x.State).NotEmpty().WithMessage(x => $"{x.AddressType} State is required.");
				RuleFor(x => x.PostCode).NotEmpty().WithMessage(x => $"{x.AddressType} Post code is required.");
				RuleFor(x => x.Country).NotEmpty().WithMessage(x => $"{x.AddressType} Country is required.");

				RuleFor(x => x.AddressLine1).Length(5, 250).WithName("1st address line");
				RuleFor(x => x.Suburb).Length(5, 100);
				RuleFor(x => x.State).Length(2, 20);
				RuleFor(x => x.PostCode).Length(2, 5).WithName("Post code");
				RuleFor(x => x.Country).Length(5, 250);

				RuleFor(x => x.AddressLine2).MaximumLength(250).WithName("2nd address line");
				RuleFor(x => x.AddressLine3).MaximumLength(250).WithName("3rd address line");
			}
			catch(Exception ex)
			{
				var logger = LogManager.GetCurrentClassLogger();
				logger.Log(LogLevel.Error, ex, "Address Validation Error");
			}
		}
	}
}

