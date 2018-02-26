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

	[Validator(typeof(ClientLeaveValidator))]
	public class ClientLeaveVM
	{
		//Cleaner leave container view model
		#region Properties, Public
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

		public Guid ClientId
		{
			get;
			set;
		}

		public DateTime StartDate
		{
			get
			{
				return _startDate;
			}
			set
			{
				_startDate = value;
			}
		}

		public String StartDateFormatted
		{
			get
			{
				if (_startDate == null || _startDate.Equals(DateTime.MinValue) || _startDate.Equals(DateTime.MaxValue))
				{
					return String.Empty;
				}

				return _startDate.ToLocalTime().ToString("d MMM yyyy");
			}
		}
		private DateTime _startDate;


		public DateTime EndDate
		{
			get
			{
				return _endDate;
			}
			set
			{
				_endDate = value ;
			}
		}

		public String EndDateFormatted
		{
			get
			{
				if (_endDate == null || _endDate.Equals(DateTime.MinValue) || _endDate.Equals(DateTime.MaxValue))
				{
					return String.Empty;
				}

				return _endDate.ToLocalTime().ToString("d MMM yyyy");
			}
		}
		private DateTime _endDate;

		public String AdvisedDateFormatted
		{
			get
			{
				if (_adviseDate == null || _adviseDate.Equals(DateTime.MinValue) || _adviseDate.Equals(DateTime.MaxValue))
				{
					return String.Empty;
				}

				return _adviseDate.ToLocalTime().ToString("d MMM yyyy");
			}
		}
		private DateTime _adviseDate;
		#endregion

		#region Methods, Public
		public void PopulateVM(Guid? clientId, ClientLeave entityModel)
		{
			if (entityModel == null)
				return;

			if (clientId.Equals(Guid.Empty))
				return;

			this.Id = entityModel.Id;
			this.ClientId = clientId.Value;

			this.StartDate = entityModel.StartDate;
			this.EndDate = entityModel.EndDate;

			this._adviseDate = entityModel.CreatedAt;
		}
		#endregion
	}
}
