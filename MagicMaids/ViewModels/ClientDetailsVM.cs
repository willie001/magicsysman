#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Attributes;

using MagicMaids.EntityModels;
using MagicMaids.Formatters;
using MagicMaids.Security;
using MagicMaids.Validators;
using NodaTime;
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

			this.Id = new Guid(entityModel.Id);
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

		public string CardNumber
		{
			get
			{
				return FormattedCardNumber;
			}
			set
			{
				_cardNumber = value;
				_cardNumberPart1 = (_cardNumber.Length >=4) ? _cardNumber.Substring(0, 4) : "";
				_cardNumberPart2 = (_cardNumber.Length >= 8) ? _cardNumber.Substring(4, 4) : "";
				_cardNumberPart3 = (_cardNumber.Length >= 12) ? _cardNumber.Substring(8, 4) : "";
				_cardNumberPart4 = (_cardNumber.Length >= 15) ? _cardNumber.Substring(12) : "";
			}
		}
		private string _cardNumber;

		public string CardNumberPart1
		{
			get
			{
				return _cardNumberPart1;
			}
		}
		private string _cardNumberPart1="";

		public string CardNumberPart2
		{
			get
			{
				return _cardNumberPart2;
			}
		}
		private string _cardNumberPart2="";

		public string CardNumberPart3
		{
			get
			{
				return _cardNumberPart3;
			}
		}
		private string _cardNumberPart3="";

		public string CardNumberPart4
		{
			get
			{
				return _cardNumberPart4;
			}
		}
		private string _cardNumberPart4="";

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
			get;
			set;
			//{
			//	var card = Helpers.GetCardTypeFromNumber($"{CardNumberPart1}{CardNumberPart2}{CardNumberPart3}{CardNumberPart4}");
	  //          switch(card)
			//	{
			//		case Helpers.CreditCardType.AmericanExpress:
			//			return "American Express";
			//		default:
			//			return card.ToString();
						        
			//	}
			//}
		}

		public string ClientReferenceCode
		{
			get;
			set;
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

			Id = new Guid(entityModel.Id);
			IsNewItem = false;
			string[] items = Crypto.Decrypt(entityModel.Details, passphrase).Split('|');

			ClientId = Guid.Parse(items[1]);
			CardCVV = items[2];
			ExpiryYear = items[3];
			ExpiryMonth = items[4];
			CardName = items[5];
			_cardNumberPart3 = items[6];
			_cardNumberPart4 = items[7];
			_cardNumberPart1 = items[8];
			_cardNumberPart2 = items[9];

			CardType = items[10];
			ClientReferenceCode = items[11];
		}
		#endregion
	}

	public class ClientPaymentReferenceUpdateVM
	{
		//Primary Client's container view model
		#region Properties, Public
		public Guid Id
		{
			get;
			set;
		}

		public string ClientReferenceCode
		{
			get;
			set;
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

				return DateTimeWrapper.FormatClientDate(_startDate);
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

				return DateTimeWrapper.FormatClientDate(_endDate);
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

				return DateTimeWrapper.FormatClientDate(_adviseDate) ;
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

			this.Id = new Guid(entityModel.Id);
			this.ClientId = clientId.Value;

			this.StartDate = entityModel.StartDate;
			this.EndDate = entityModel.EndDate;

			this._adviseDate = entityModel.CreatedAt;
		}
		#endregion
	}
}
