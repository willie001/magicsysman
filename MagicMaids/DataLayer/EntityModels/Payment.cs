#region Using
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace MagicMaids.EntityModels
{
	public enum PaymentTypes
	{
		None = 0,
		Cash,
		CreditCard,
		Invoice
	}

	public class Payment: BaseModel
	{
		#region Properties, Public
		public PaymentTypes PaymentType
		{
			get;
			set;
		}
		#endregion
	}
}
