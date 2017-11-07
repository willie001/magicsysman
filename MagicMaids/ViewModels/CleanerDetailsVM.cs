#region Using
using System;
using System.Collections.Generic;
using FluentValidation.Attributes;

using MagicMaids.EntityModels;
using MagicMaids.Validators;
#endregion

namespace MagicMaids.ViewModels
{
	[Validator(typeof(CleanerDetailsValidator))]
	public class CleanerDetailsVM: BaseContactVM
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

		public Guid MasterFranchiseRefId
		{
			get;
			set;
		}

		public Int32 CleanerCode
		{
			get;
			set;
		}

		public string Initials
		{
			get;
			set;
		}

		public string FirstName
		{
			get;
			set;
		}

		public string LastName
		{
			get;
			set;
		}

		public string Region
		{
			get;
			set;
		}

		public Int32? Rating
		{
			get;
			set;
		}

		public String GenderFlag
		{
			get;
			set;
		}

		public Boolean Ironing
		{
			get;
			set;
		}

		public List<TeamMemberDetailsVM> TeamMembers
		{
			get;
			set;
		}
		#endregion

		#region Methods, Public
		public void PopulateVM(Cleaner entityModel)
		{
			if (entityModel == null)
				return;

			this.Id = entityModel.Id;
			this.CleanerCode = entityModel.CleanerCode;
			this.Initials = entityModel.Initials;
			this.FirstName = entityModel.FirstName;
			this.LastName = entityModel.LastName;
			this.EmailAddress = entityModel.EmailAddress;
			this.BusinessPhoneNumber = entityModel.BusinessPhoneNumber;
			this.OtherNumber = entityModel.OtherNumber;
			this.MobileNumber = entityModel.MobileNumber;
			this.Region = entityModel.Region;
			this.Rating = entityModel.Rating;
			this.MasterFranchiseRefId = entityModel.MasterFranchiseRefId;
			this.IsActive = entityModel.IsActive;
			this.Ironing = entityModel.Ironing;
			this.GenderFlag = entityModel.GenderFlag;

			base.FormatContactDetails(entityModel.PhysicalAddress, entityModel.PostalAddress);
		}
		#endregion
	}

	[Validator(typeof(TeamMemberDetailsValidator))]
	public class TeamMemberDetailsVM : BaseContactVM
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

		public Guid PrimaryCleanerRefId
		{
			get;
			set;
		}

		public string FirstName
		{
			get;
			set;
		}

		public string LastName
		{
			get;
			set;
		}

		public String GenderFlag
		{
			get;
			set;
		}

		public Boolean Ironing
		{
			get;
			set;
		}

		#endregion

		#region Methods, Public
		public void PopulateVM(Guid? cleanerId, CleanerTeam entityModel)
		{
			if (entityModel == null)
				return;

			if (cleanerId.Equals(Guid.Empty))
				return;

			this.Id = entityModel.Id;
			this.FirstName = entityModel.FirstName;
			this.LastName = entityModel.LastName;
			this.EmailAddress = entityModel.EmailAddress;
			this.MobileNumber = entityModel.MobileNumber;
			this.IsActive = entityModel.IsActive;
			this.Ironing = entityModel.Ironing;
			this.GenderFlag = entityModel.GenderFlag;
			this.PrimaryCleanerRefId = cleanerId.Value;

			base.FormatContactDetails(entityModel.PhysicalAddress, entityModel.PostalAddress);
		}
		#endregion
	}

	public class CleanerSearchVM
	{
		#region Properties, Public
		public Guid SelectedFranchiseId
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public string Zone
		{
			get;
			set;
		}

		public Boolean IncludeInactive
		{
			get;
			set;
		}
		#endregion
	}
}
