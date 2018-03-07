#region Using
using System;

using MagicMaids;
using MagicMaids.ViewModels;

using FluentValidation;
using System.Collections.Generic;
using System.Linq;
#endregion
namespace MagicMaids.Validators
{
	//https://github.com/JeremySkinner/FluentValidation/wiki/c.-Built-In-Validators
	//http://nodogmablog.bryanhogan.net/2015/04/complex-model-validation-using-fluent-validation/
	public class SettingsValidator : AbstractValidator<UpdateSettingsViewModel>
	{
		public SettingsValidator()
		{
			RuleFor(x => x.SettingName).NotEmpty().WithMessage("Default setting name is required.");
			RuleFor(x => x.SettingValue).NotEmpty().WithMessage("Default value is required.");
			RuleFor(x => x.CodeIdentifier).NotEmpty().WithMessage("The Code Identifier is required.");

			RuleFor(x => x.SettingName).Length(5, 40).WithName("Default name");
			RuleFor(x => x.SettingValue).Length(1, 50).WithName("Default value");
		}
	}

	public class FranchiseDetailsValidator : AbstractValidator<UpdateFranchisesViewModel>
	{
		public FranchiseDetailsValidator()
		{
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
			RuleFor(x => x.MetroRegion).Length(2, 100).WithName("Metro region");

			RuleFor(x => x.BusinessPhoneNumber).SetValidator(new PhoneNumberValidator(false)).WithMessage("Primary business number is not a valid phone number.");
			RuleFor(x => x.MobileNumber).SetValidator(new PhoneNumberValidator(true)).WithMessage("Mobile number is not a valid number.");
			RuleFor(x => x.OtherNumber).SetValidator(new PhoneNumberValidator(false)).WithMessage("Alternative contact number is not a valid number.");

			RuleFor(x => x.EmailAddress).EmailAddress().WithMessage("Email address is not a valid email address.");
		}
	}

	public class FranchiseSettingsValidator : AbstractValidator<FranchiseSettingsVM>
	{
		public FranchiseSettingsValidator()
		{
			RuleFor(x => x.ManagementFeePercentage).GreaterThan(0).WithName("Management fee");
			RuleFor(x => x.ManagementFeePercentage).LessThanOrEqualTo(100).WithName("Management fee");
			RuleFor(x => x.RoyaltyFeePercentage).GreaterThan(0).WithName("Royalty fee");
			RuleFor(x => x.RoyaltyFeePercentage).LessThanOrEqualTo(100).WithName("Royalty fee");
		}
	}

	public class AddressValidator : AbstractValidator<UpdateAddressViewModel>
	{
		public AddressValidator()
		{
			RuleFor(x => x.Id).NotNull();

			RuleFor(x => x.AddressLine1).NotEmpty().WithMessage(x => $"{x.AddressType} 1st address line is required.");
			RuleFor(x => x.Suburb).NotEmpty().WithMessage(x => $"{x.AddressType} Suburb is required.");
			RuleFor(x => x.State).NotEmpty().WithMessage(x => $"{x.AddressType} State is required.");
			RuleFor(x => x.PostCode).NotEmpty().WithMessage(x => $"{x.AddressType} Post code is required.");
			RuleFor(x => x.Country).NotEmpty().WithMessage(x => $"{x.AddressType} Country is required.");

			RuleFor(x => x.AddressLine1).Length(5, 250).WithName("1st address line");
			RuleFor(x => x.Suburb).Length(4, 100);
			RuleFor(x => x.State).Length(2, 20);
			RuleFor(x => x.PostCode).Length(2, 5).WithName("Post code");
			RuleFor(x => x.Country).Length(5, 250);

			RuleFor(x => x.AddressLine2).MaximumLength(250).WithName("2nd address line");
			RuleFor(x => x.AddressLine3).MaximumLength(250).WithName("3rd address line");
		}
	}

	public class SuburbZoneValidator : AbstractValidator<UpdateSuburbZonesVM>
	{
		private readonly IEnumerable<UpdateSuburbZonesVM> _zones;

		public SuburbZoneValidator()
		{
			RuleFor(x => x.SuburbName).NotEmpty().WithMessage("Suburb name is required.");
			RuleFor(x => x.PostCode).NotEmpty().WithMessage("Post code is required.");
			RuleFor(x => x.LinkedZones).NotEmpty().WithMessage("At least one linked zone must be provided.");

			RuleFor(x => x.SuburbName).Length(4, 250).WithName("Suburb name");
			RuleFor(x => x.PostCode).Length(2, 5).WithName("Post code");
			RuleFor(x => x.Zone).Length(1, 10).WithName("Zone Id");
			RuleFor(x => x.LinkedZones).Length(1, 500).WithName("Linked zone list");
		}

		//public SuburbZoneValidator(List<UpdateSuburbZonesVM> zones)
		//{
		//	_zones = zones;

		//	RuleFor(x => x.SuburbName).Must(UniqueEntity).WithMessage("Zone name and postcode must be unique per franchise.");
		//	//RuleFor(x => x.SuburbName).SetValidator(new UniqueZoneValidator(_zones)).WithMessage("Zone name and postcode must be unique per franchise.");
		//}

		//private bool UniqueEntity(UpdateSuburbZonesVM selectedItem, string name)
		//{
		//	var selectedItems = _zones.SingleOrDefault(x => 
		//	                                           !x.Equals(selectedItem) ||
		//	                                           (x.FranchiseId.Equals(selectedItem.FranchiseId) && 
		//	                                            	(x.PostCode.Equals(selectedItem.PostCode, StringComparison.InvariantCultureIgnoreCase) || 
		//	                                             	x.SuburbName.Equals(selectedItem.SuburbName, StringComparison.InvariantCultureIgnoreCase) )
  //                                                     ));
		//	//ProjecteDataContext _db = new ProjecteDataContext();
		//	//var dbCategory = _db.Categories
		//	//					.Where(x => x.Name.ToLower() == name.ToLower())
		//	//					.SingleOrDefault();

		//	if (selectedItems == null)
		//		return true;
		//	else
		//		return false;
		//}
	}

	public class RateValidator : AbstractValidator<RateDetailsVM>
	{
		public RateValidator()
		{
			RuleFor(x => x.RateCode).NotEmpty().WithMessage("Rate Description is required.");
			RuleFor(x => x.RateAmount).NotEmpty().WithMessage("Rate Amount is required.");

			RuleFor(x => x.RateCode).Length(3, 50).WithName("Rate Description");
			RuleFor(x => x.RateAmount).GreaterThan(0).WithMessage("Rate Amount");
		}
	}

	public class CleanerDetailsValidator : AbstractValidator<CleanerDetailsVM>
	{
		public CleanerDetailsValidator()
		{
			RuleFor(x => x.CleanerCode).NotEmpty().GreaterThanOrEqualTo(1000).WithMessage("Cleaner code is required and must start from 1000.");

			RuleFor(x => x.Initials).NotEmpty().WithMessage("Initials are required.");
			RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required.");
			RuleFor(x => x.LastName).NotEmpty().WithMessage("Surname is required.");
			RuleFor(x => x.Region).NotEmpty().WithMessage("Region is required.");
			RuleFor(x => x.EmailAddress).NotEmpty().WithMessage("Valid email address is required.");

			RuleFor(x => x.Initials).Length(1, 5).WithName("Initials");
			RuleFor(x => x.FirstName).Length(2, 100).WithName("First name");
			RuleFor(x => x.LastName).Length(2, 100).WithName("Last name");

			RuleFor(x => x.Region).Length(2, 100).WithName("Region");
			RuleFor(x => x.PrimaryZone).Length(1, 10).WithMessage("Primary zone must be a valid number");

			RuleFor(x => x.BusinessPhoneNumber).SetValidator(new PhoneNumberValidator(false)).WithMessage("Primary business number is not a valid phone number.");
			RuleFor(x => x.MobileNumber).SetValidator(new PhoneNumberValidator(true)).WithMessage("Mobile number is not a valid number.");
			RuleFor(x => x.OtherNumber).SetValidator(new PhoneNumberValidator(false)).WithMessage("Alternative contact number is not a valid number.");

			RuleFor(x => x.EmailAddress).EmailAddress().WithMessage("Email address is not a valid email address.");

			When(x => x.Rating.HasValue, () =>
			{
				RuleFor(x => x.Rating).InclusiveBetween(1, 6).WithMessage("Rating must be between 1 and 6.");
			});
		}
	}

	public class CleanerLeaveValidator : AbstractValidator<CleanerLeaveVM>
	{
		public CleanerLeaveValidator()
		{
			RuleFor(x => x.StartDate).NotEmpty().WithMessage("Start date is required");
			RuleFor(x => x.EndDate).NotEmpty().WithMessage("End date is required");

			When(x => (x.StartDate.Year > 1950), () =>
			{
				RuleFor(x => x.StartDate).GreaterThan(DateTime.Now).WithMessage("Leave start date must be in the future.");
				RuleFor(x => x.EndDate)
					.NotEmpty()
					.Must((x, EndDate) => EndDate >= x.StartDate)
					.WithMessage("Leave end date must be after the start date.");
			});

			When(x => (x.EndDate.Year > 1950), () =>
			{
				RuleFor(x => x.EndDate).GreaterThan(DateTime.Now).WithMessage("Leave end date must be in the future.");
				RuleFor(x => x.StartDate)
					.NotEmpty()
					.Must((x, StartDate) => StartDate <= x.EndDate)
					.WithMessage("Leave start date must be before the end date.");
			});

		}
	}


	public class TeamMemberDetailsValidator : AbstractValidator<TeamMemberDetailsVM>
	{
		public TeamMemberDetailsValidator()
		{
			RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required.");
			RuleFor(x => x.LastName).NotEmpty().WithMessage("Surname is required.");
			RuleFor(x => x.EmailAddress).NotEmpty().WithMessage("Valid email address is required.");

			RuleFor(x => x.FirstName).Length(2, 100).WithName("First name");
			RuleFor(x => x.LastName).Length(2, 100).WithName("Last name");

			RuleFor(x => x.MobileNumber).SetValidator(new PhoneNumberValidator(true)).WithMessage("Mobile number is not a valid number.");

			RuleFor(x => x.EmailAddress).EmailAddress().WithMessage("Email address is not a valid email address.");
		}
	}

	public class CleanerRosterValidator : AbstractValidator<CleanerRosterVM>
	{
		public CleanerRosterValidator()
		{
			When(x => x.IsActive, () =>
		  	{
			  	RuleFor(x => x.TeamCount).GreaterThan(0).WithMessage(x => $"At least one team member must be available on {x.Weekday}");
				RuleFor(x => x.StartTime).GreaterThan(DateTime.MinValue).WithMessage(x => $"Select start time for {x.Weekday}");
				RuleFor(x => x.EndTime).GreaterThan(DateTime.MinValue).WithMessage(x => $"Select end time for {x.Weekday}");
				RuleFor(x => x.StartTime).GreaterThan(x => x.EndTime).WithMessage(x => $"Start time must be greater than end time for {x.Weekday}");
	        });
		}
	}

	public class ClientDetailsValidator : AbstractValidator<ClientDetailsVM>
	{
		public ClientDetailsValidator()
		{
			RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required.");
			RuleFor(x => x.LastName).NotEmpty().WithMessage("Surname is required.");
			RuleFor(x => x.EmailAddress).NotEmpty().WithMessage("Valid email address is required.");
			RuleFor(x => x.ClientType).NotEmpty().WithMessage("Client classification is required.");

			RuleFor(x => x.FirstName).Length(2, 100).WithName("First name");
			RuleFor(x => x.LastName).Length(2, 100).WithName("Last name");

			RuleFor(x => x.BusinessPhoneNumber).SetValidator(new PhoneNumberValidator(false)).WithMessage("Primary business number is not a valid phone number.");
			RuleFor(x => x.MobileNumber).SetValidator(new PhoneNumberValidator(true)).WithMessage("Mobile number is not a valid number.");
			RuleFor(x => x.OtherNumber).SetValidator(new PhoneNumberValidator(false)).WithMessage("Alternative contact number is not a valid number.");

			RuleFor(x => x.EmailAddress).EmailAddress().WithMessage("Email address is not a valid email address.");

		}
	}

	public class ClientLeaveValidator : AbstractValidator<ClientLeaveVM>
	{
		// todo merge with CleanerLeaveValidator
		public ClientLeaveValidator()
		{
			RuleFor(x => x.StartDate).NotEmpty().WithMessage("Start date is required");
			RuleFor(x => x.EndDate).NotEmpty().WithMessage("End date is required");

			When(x => (x.StartDate.Year > 1950), () =>
			{
				RuleFor(x => x.StartDate).GreaterThan(DateTime.Now).WithMessage("Leave start date must be in the future.");
				RuleFor(x => x.EndDate)
					.NotEmpty()
					.Must((x, EndDate) => EndDate >= x.StartDate)
					.WithMessage("Leave end date must be after the start date.");
			});

			When(x => (x.EndDate.Year > 1950), () =>
			{
				RuleFor(x => x.EndDate).GreaterThan(DateTime.Now).WithMessage("Leave end date must be in the future.");
				RuleFor(x => x.StartDate)
					.NotEmpty()
					.Must((x, StartDate) => StartDate <= x.EndDate)
					.WithMessage("Leave start date must be before the end date.");
			});

		}
	}

	public class ClientPaymentMethodValidator : AbstractValidator<ClientPaymentMethodVM>
	{
		public ClientPaymentMethodValidator()
		{
			RuleFor(x => x.CardName).NotEmpty().WithMessage("Card name is required.");
			RuleFor(x => x).Must((x, s) => IsCardnumberValid(x)).WithMessage("Card number is not valid.");
			RuleFor(x => x.CardCVV).Must((x, s) => IsCvvValid(x.CardCVV)).WithMessage("Card CVV is not valid.");
			RuleFor(x => x.ExpiryMonth).Must((x, s) => IsExpiryValid(x)).WithMessage("Card expiry is not valid.");
			RuleFor(x => x.ExpiryYear).Must((x, s) => IsExpiryValid(x)).WithMessage("Card expiry is not valid.");
		}

		private bool IsCardnumberValid(ClientPaymentMethodVM c)
		{
			var values = new string[] { c.CardNumberPart1, c.CardNumberPart2, c.CardNumberPart3, c.CardNumberPart4 };

			if (values.Any(v => String.IsNullOrWhiteSpace(v)))
			{
				return false;
			}

			if (values.Any(v => !Helpers.IsValidNumeric(v)))
			{
				return false;
			}

			var cardNum = String.Concat(values.ToList());
			var cardType = Helpers.GetCardTypeFromNumber($"{values[0]}{values[1]}{values[2]}{values[3]}");
			var cardLen = cardNum.Length;

			bool isValid = false;
			switch(cardType)
			{
				case Helpers.CreditCardType.AmericanExpress:
					isValid = ((values[0].StartsWith("34") || values[0].StartsWith("37")) && (cardLen == 15));
					break;
				case Helpers.CreditCardType.Visa:
					isValid = (values[0].StartsWith("4") && (cardLen == 13 || cardLen == 16 || cardLen == 19));
					break;
				case Helpers.CreditCardType.MasterCard:
					isValid = ((values[0].StartsWith("51") || values[0].StartsWith("52")
					         || values[0].StartsWith("53") || values[0].StartsWith("54")
					         || values[0].StartsWith("2") || values[0].StartsWith("55")) && (cardLen == 16));
					break;
				case Helpers.CreditCardType.DinersClub:
					isValid = ((values[0].StartsWith("30") || values[0].StartsWith("36") 
					            || values[0].StartsWith("38") || values[0].StartsWith("39")) && (cardLen >= 14 && cardLen <= 19));
					break;
				case Helpers.CreditCardType.Discover:
					isValid = ((values[0].StartsWith("6011") || values[0].StartsWith("64") || values[0].StartsWith("65")) && (cardLen >= 16 && cardLen <= 19));
					break;
				default:
					isValid = false;
					break;
			}

			if (!isValid)
			{
				return false;
			}

			if (!PassesLuhnTest(cardNum))
			{
				return false;
			}

			return true;
		}

		// *****************************************************************************************
		//  PassesLuhnTest Method
		//
		/// <summary>
		///     </summary>
		/// <param name="cardNumber">
		///     The card number.</param>
		/// <returns>
		///     <c>true</c> if XXXX, <c>false</c> otherwise.</returns>
		//
		// *****************************************************************************************
		private  Boolean PassesLuhnTest(String cardNumber)
		{
			//Clean the card number- remove dashes and spaces
			cardNumber = cardNumber.Replace("-", "").Replace(" ", "");

			//Convert card number into digits array
			Int32[] digits = new int[cardNumber.Length];
			for (Int32 len = 0; len < cardNumber.Length; len++)
			{
				digits[len] = Int32.Parse(cardNumber.Substring(len, 1));
			}

			//Luhn Algorithm
			//Adapted from code availabe on Wikipedia at
			//http://en.wikipedia.org/wiki/Luhn_algorithm
			Int32 sum = 0;
			Boolean alt = false;
			for (Int32 i = digits.Length - 1; i >= 0; i--)
			{
				Int32 curDigit = digits[i];
				if (alt)
				{
					curDigit *= 2;
					if (curDigit > 9)
					{
						curDigit -= 9;
					}
				}
				sum += curDigit;
				alt = !alt;
			}

			//If Mod 10 equals 0, the number is good and this will return true
			return sum % 10 == 0;
		}

		private bool IsExpiryValid(ClientPaymentMethodVM c)
		{
			var values = new string[] { c.ExpiryMonth, c.ExpiryYear };

			if (values.Any(v => String.IsNullOrWhiteSpace(v)))
			{
				return false;
			}

			if (values.Any(v => !Helpers.IsValidNumeric(v)))
			{
				return false;
			}

			Int32 _year = Helpers.ToInt32(c.ExpiryYear);
			Int32 _month = Helpers.ToInt32(c.ExpiryMonth);

			if (_month <= 1 && _month > 12)
			{
				return false;
			}

			if (_year < DateTime.Now.Year)
			{
				return false;
			}

			// we have a valid month/year, but make sure when you combine them that the expiry date is not old
				
			DateTime expiryDate = DateTime.Parse(String.Format("{0}/{1}/{2}", DateTime.DaysInMonth(_year,_month), _month, _year));
			if (expiryDate.Date < DateTime.UtcNow.Date)
			{
				if (expiryDate.Year < DateTime.UtcNow.Year)
				{
					return false;		// past year
				}
				else
				{
					return false;		// past month
				}
			}
			else if (expiryDate.Year > (DateTime.UtcNow.Year + 100))
			{
				return false;	//too far in the future
			}

			return true;
		}

		private bool IsCvvValid(String cvv)
		{
			if (String.IsNullOrWhiteSpace(cvv))
			{
				// temporary disable mandatory check
				return true;   // false;
			}

			if (cvv.Length != 3)
			{
				return false;
			}

			if (!Helpers.IsValidNumeric(cvv))
			{
				return false;
			}

			return true;
		}
	}

	public class SearchCleanerMatch : AbstractValidator<SearchVM>
	{
		public SearchCleanerMatch()
		{
			RuleFor(x => x.Suburb).NotEmpty().WithMessage("Suburb is required.");
			RuleFor(x => x.Suburb).Length(3, 100);

			When(x => (x.OneOffJob == false), () =>
			{
				RuleFor(x => x.ServiceDay).NotEmpty().WithMessage("Preferred service day is required.");
			});

			When(x => (x.OneOffJob == true), () =>
			{
				RuleFor(x => x.ServiceDate).GreaterThanOrEqualTo(DateTime.Now.AddDays(-1)).WithMessage("Service date can't be in the past.");
				RuleFor(x => x.ServiceDate).LessThanOrEqualTo(DateTime.Now.AddDays(SystemSettings.BookingsDaysAllowed)).WithMessage($"Services can't be booked more than {SystemSettings.BookingsDaysAllowed} days in advance.");
			});

			RuleFor(x => x.ServiceLength).NotEmpty().WithMessage("Service duration is required.");
			RuleFor(x => x.ServiceLength).GreaterThan(0).LessThanOrEqualTo(SystemSettings.WorkSessionMaxHours).WithMessage($"Service duration can not exceed {SystemSettings.WorkSessionMaxHours} hours.");
		}

		private bool IsJobSelected(SearchVM c)
		{
			if (c.OneOffJob || c.FortnightlyJob || c.WeeklyJob)
			{
				return true;
			}

			return false;
		}
	}
}
