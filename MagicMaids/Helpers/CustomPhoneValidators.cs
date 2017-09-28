﻿#region Using
using System;
using System.Collections.Generic;
using System.Linq;

using FluentValidation.Validators;
#endregion

namespace MagicMaids.Validators 
{
	public class PhoneNumberValidator: PropertyValidator 
	{
		#region Fields
		private bool IsMobile = false;
		#endregion

		#region Constructors
		public PhoneNumberValidator(bool isMobile)
			: base("{PropertyName} is not a valid phone number.")
		{
			IsMobile = isMobile;
		}
		#endregion

		#region Methods, Protected
		protected override bool IsValid(PropertyValidatorContext context)
		{
			if (context.PropertyValue == null)
				return true;
			
			var _phone = Helpers.DumbParseDigitsOnly(context.PropertyValue.ToString());
			if (String.IsNullOrWhiteSpace(_phone))
			{
				return false;
			}

			// strips out the internaltional area code if present.

			if (_phone.StartsWith("61", StringComparison.InvariantCulture))
			{
				_phone = _phone.Substring(2); 	
			}

			if (_phone.Length == 9)
				_phone = $"0{_phone}"; // maybe the first 0 dropped by user or replacing international code

			if (_phone.Length == 10)
			{
				// check index of first 2 chars for area codes
				var _area = _phone.Substring(0, 2);

				if (IsMobile && (_area.StartsWith("04", StringComparison.InvariantCulture) || _area.StartsWith("05", StringComparison.InvariantCulture) ) )
				{
					return true;
				}

				if (!IsMobile)
				{
					var _areas = "01,02,03,04,05,07,08";
					List<string> _areaCodes = _areas.Split(',').ToList();
					if (_areaCodes.Contains(_area))
					{
						return true;
					}

					// check for 1300 / 1800 numbers (need more validation)
					if (_phone.StartsWith("1", StringComparison.InvariantCulture))
					{
						return true;
					}
				}
			}
			else  //mobile number has to be 10
			{
				if (!IsMobile)
					return true;
			}

			return false;
		}
		#endregion
	}
}
