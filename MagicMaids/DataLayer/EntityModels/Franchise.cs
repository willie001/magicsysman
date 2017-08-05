#region Using
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using FluentValidation;
using FluentValidation.Attributes;
using NLog;
#endregion

namespace MagicMaids.EntityModels
{
	[Validator(typeof(FranchiseValidator))]
	[Table("Franchises")]
	public class Franchise : BaseModel
	{
		#region Properties, Public
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

		private string _formattedContactNumbers;
		public string FormattedContactNumbers
		{
			get
			{
				if (!String.IsNullOrWhiteSpace(_formattedContactNumbers))
				{
					return _formattedContactNumbers;
				}

				System.Text.StringBuilder _output = new System.Text.StringBuilder();

				if (!String.IsNullOrWhiteSpace(BusinessPhoneNumber))
				{
					if (_output.Length > 0) _output.Append("<br/>");
					_output.Append("<span class='fa fa-phone'></span>&nbsp;");
					_output.Append(BusinessPhoneNumber);
				}

				if (!String.IsNullOrWhiteSpace(MobileNumber))
				{
					if (_output.Length > 0) _output.Append("<br/>");
					_output.Append("<span class='fa fa-mobile'></span>&nbsp;");
					_output.Append(MobileNumber);
				}

				if (!String.IsNullOrWhiteSpace(OtherNumber))
				{
					if (_output.Length > 0) _output.Append("<br/>");
					_output.Append("<span class='fa fa-fax'></span>&nbsp;");
					_output.Append(OtherNumber);
				}

				_formattedContactNumbers = _output.ToString();
				return _formattedContactNumbers;
			}
		}
		#endregion

		#region Properties, Foreign Key
		public Guid PhysicalAddressRefId
		{
			get;
			set;
		}

		[ForeignKey("PhysicalAddressRefId")]
		public  Address PhysicalAddress
		{
			get;
			set;
		}

		public Guid PostalAddressRefId
		{
			get;
			set;
		}

		[ForeignKey("PostalAddressRefId")]
		public  Address PostalAddress
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
	}

	//https://github.com/JeremySkinner/FluentValidation/wiki/c.-Built-In-Validators
	public class FranchiseValidator : AbstractValidator<Franchise>
	{
		public FranchiseValidator()
		{
			try
			{
				RuleFor(x => x.Id).NotNull();

				RuleFor(x => x.Name).NotEmpty().WithMessage("Franchise name is required.");
				RuleFor(x => x.TradingName).NotEmpty().WithMessage("Trading name is required.");
				RuleFor(x => x.MasterFranchiseCode).NotEmpty().WithMessage("Franchise code is required.");
				RuleFor(x => x.EmailAddress).NotEmpty().WithMessage("Valid email address is required.");
				RuleFor(x => x.CodeOfConductURL).NotEmpty().WithMessage("Code of conduct is required.");
				RuleFor(x => x.MetroRegion).NotEmpty().WithMessage("Metropolitan region is required.");

				RuleFor(x => x.Name).Length(5, 50).WithName("Franchise name");
				RuleFor(x => x.TradingName).Length(5, 50).WithName("Trading name");
				RuleFor(x => x.MasterFranchiseCode).Length(5, 20).WithName("Franchise code");
				RuleFor(x => x.CodeOfConductURL).Length(0, 500).WithName("Code of conduct");
				RuleFor(x => x.MetroRegion).Length(5, 100).WithName("Metro region");

				RuleFor(x => x.ManagementFeePercentage).GreaterThan(0).WithName("Management fee");
				RuleFor(x => x.ManagementFeePercentage).LessThanOrEqualTo(100).WithName("Management fee");

				RuleFor(x => x.BusinessPhoneNumber).SetValidator(new PhoneNumberValidator(false)).WithMessage("Primary business number is not a valid phone number.");
				RuleFor(x => x.MobileNumber).SetValidator(new PhoneNumberValidator(true)).WithMessage("Mobile number is not a valid number.");
				RuleFor(x => x.OtherNumber).SetValidator(new PhoneNumberValidator(false)).WithMessage("Alternative contact number is not a valid number.");

				RuleFor(x => x.EmailAddress).EmailAddress().WithMessage("Email address is not a valid email address.");
			}
			catch(Exception ex)
			{
				var logger = LogManager.GetCurrentClassLogger();
				logger.Log(LogLevel.Error, ex, "Franchise Validation Error");
			}
		}
	}
}
