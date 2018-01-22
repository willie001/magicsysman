#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Attributes;

using MagicMaids.EntityModels;
using MagicMaids.Formatters;
using MagicMaids.Security;
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

	[Validator(typeof(ClientPaymentMethodValidator))]
	public class ClientPaymentMethodVM
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

		public string FormattedCardNumber
		{
			get
			{
				return String.Format(new CreditCardNumberFormatter(), "{0}", $"{this.CardNumberPart1},{this.CardNumberPart2},{this.CardNumberPart3},{this.CardNumberPart4}");
			}
		}

		public string FormattedExpiryDate
		{
			get
			{
				return $"{ExpiryMonth}/{ExpiryYear}";
			}
		}

		public string CardNumberPart1
		{
			get;
			set;
		}

		public string CardNumberPart2
		{
			get;
			set;
		}

		public string CardNumberPart3
		{
			get;
			set;
		}

		public string CardNumberPart4
		{
			get;
			set;
		}

		public string CardName
		{
			get;
			set;
		}

		public string CardCVV
		{
			get;
			set;
		}

		public string ExpiryMonth
		{
			get;
			set;
		}

		public string ExpiryYear
		{
			get;
			set;
		}

		public string CardType
		{
			get
			{
				var card = Helpers.GetCardTypeFromNumber($"{CardNumberPart1}{CardNumberPart2}{CardNumberPart3}{CardNumberPart4}");
	            switch(card)
				{
					case Helpers.CreditCardType.AmericanExpress:
						return "American Express";
					default:
						return card.ToString();
						        
				}
			}
		}

		public Guid ClientId
		{
			get;
			set;
		}

		#endregion

		#region Methods, Public
		public void PopulateVM(ClientMethod entityModel, string passphrase)
		{
			if (entityModel == null)
				return;

			this.Id = entityModel.Id;
			this.IsNewItem = false;
			string[] items = Crypto.Decrypt(entityModel.Details, passphrase).Split('|');

			this.ClientId = Guid.Parse(items[1]);
			this.CardCVV = items[2];
			this.ExpiryYear = items[3];
			this.ExpiryMonth = items[4];
			this.CardName = items[5];
			this.CardNumberPart3 = items[6];
			this.CardNumberPart4 = items[7];
			this.CardNumberPart1 = items[8];
			this.CardNumberPart2 = items[9];
		}
		#endregion
	}
}
