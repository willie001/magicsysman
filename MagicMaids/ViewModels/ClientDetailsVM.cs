#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Attributes;

using MagicMaids.EntityModels;
using MagicMaids.Validators;
#endregion

namespace MagicMaids.ViewModels
{
	public class ClientSearchVM
	{
		#region Properties, Public
		public string Name
		{
			get;
			set;
		}

		public string Address
		{
			get;
			set;
		}

		public string Suburb
		{
			get;
			set;
		}

		public string Phone
		{
			get;
			set;
		}

		public string Cleaner
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

	[Validator(typeof(ClientDetailsValidator))]
	public class ClientDetailsVM : BaseContactVM
	{
		//Primary Client's container view model
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

		public string ClientType
		{
			get;
			set;
		}

		#endregion

		#region Methods, Public
		public void PopulateVM(Client entityModel)
		{
			if (entityModel == null)
				return;

			this.Id = entityModel.Id;
			this.FirstName = entityModel.FirstName;
			this.LastName = entityModel.LastName;
			this.EmailAddress = entityModel.EmailAddress;
			this.BusinessPhoneNumber = entityModel.BusinessPhoneNumber;
			this.OtherNumber = entityModel.OtherNumber;
			this.MobileNumber = entityModel.MobileNumber;
			this.IsActive = entityModel.IsActive;
			this.ClientType = entityModel.ClientType;

			base.FormatContactDetails(entityModel.PhysicalAddress, entityModel.PostalAddress);
		}
		#endregion
	}
}
