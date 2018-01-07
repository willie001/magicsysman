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
			RuleFor(x => x.MetroRegion).Length(3, 100).WithName("Metro region");

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
			RuleFor(x => x.Suburb).Length(5, 100);
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

			RuleFor(x => x.SuburbName).Length(5, 250).WithName("Suburb name");
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
			RuleFor(x => x.FirstName).Length(3, 100).WithName("First name");
			RuleFor(x => x.LastName).Length(3, 100).WithName("Last name");

			RuleFor(x => x.Region).Length(3, 100).WithName("Region");
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

			RuleFor(x => x.FirstName).Length(3, 100).WithName("First name");
			RuleFor(x => x.LastName).Length(3, 100).WithName("Last name");

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
}
