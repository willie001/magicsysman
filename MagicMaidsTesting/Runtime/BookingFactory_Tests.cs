using System;
using System.Collections.Generic;
using MagicMaids;
using MagicMaids.ViewModels;
using NUnit.Framework;

namespace MagicMaidsTesting
{
	[TestFixture()]
	public class BookingFactory_Tests
	{
		SearchVM criteria = new SearchVM()
		{
			Suburb = "Leederville"
		};

		UpdateAddressViewModel address = new UpdateAddressViewModel()
		{
			AddressLine1 = "Street 123",
			Suburb = "Test Suburb",
			PostCode = "6000"
		};

		//[Test()]
		//public void BookingFactory_FirstDay_Test()
		//{
		//	List<CleanerJobMatchVM> list = new List<CleanerJobMatchVM>();
		//	CleanerJobMatchVM _item = new CleanerJobMatchVM()
		//	{
		//		PreviousJobLocation = "",
		//		PhysicalAddress = address,
		//		NextJobLocation = "Follow Address"
		//	};
		//	list.Add(_item);


		//	BookingFactory factory = new BookingFactory(list, criteria);
		//	Assert.AreEqual(factory.FormatStyleForHome(_item), NamedColours.FirstJobColor, "First job colour not matching");
		//}

		//[Test()]
		//public void Booking_Week_Colour()
		//{
		//	var _year = DateTime.Now.Year;
		//	var _hitMonday = false;
		//	var _expected = NamedColours.WeeksOdd;
		//	for (int i = 1; i <= 10; i++)
		//	{
		//		var _test = new DateTime(_year, 1, i);
		//		var _day = _test.DayOfWeek;
		//		if (!_hitMonday && _day == DayOfWeek.Monday)
		//		{
		//			_expected = NamedColours.WeeksEven;
		//			_hitMonday = true;
		//		}

		//		Assert.AreEqual(_expected, _test.WeekYearStyle(), $"Week {i} expected to be {_expected}");
		//	}
		//}
	}
}
