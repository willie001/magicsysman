#region Using
using System;
using System.Collections.Generic;
using System.Data.Entity;

using MagicMaids.EntityModels;

#endregion

namespace MagicMaids.DataAccess
{
	public class CleanerRepository : DbContext
	{
		#region Constructors
		public CleanerRepository()
			: base(nameOrConnectionString: "MagicMaidsDBConn")
		{

		}
		#endregion

		#region Methods, Public
		public DbSet<Cleaner> Cleaners
		{
			get;
			set;
		}
		//public override IEnumerable<Cleaner> GetAll(bool includeActive, bool includeDisabled)
		//{
		//	var _cleanerList = new List<Cleaner>();

		//	var _item = new Cleaner() { Title = "Mr", GivenName = "Joe", Surname = "Bloggs", EmailAddress = "joe@test.com", Zone="5" };
		//	_item.PhysicalAddress = new Address() { AddressLine1 = "15 Somestreet", 
		//			Suburb = "Perth", PostCode = "600", State = "WA",
		//			AddressType = AddressTypeSetting.Physical };
		//	_item.PhoneNumber = "9999 9999";
		//	_item.MobileNumber = "0400 000 000";
		//	_cleanerList.Add(_item);

		//	_item = new Cleaner() { Title = "Mrs", GivenName = "Suzy", Surname = "Smith", EmailAddress = "suzy@test.com", Zone="29" };
		//	_item.PhoneNumber = "9999 9999";
		//	_item.MobileNumber = "0400 000 000";
		//	_item.PhysicalAddress = new Address()
		//		{
		//			AddressLine1 = "15 Somestreet", 
		//			Suburb = "Perth", PostCode = "600", State = "WA",
		//			AddressType = AddressTypeSetting.Physical };
		//	_cleanerList.Add(_item);

		//	_item =  new Cleaner() { Title = "Mrs", GivenName = "Mary", Surname = "Brent", EmailAddress = "mary@test.com", Zone="33" };
		//	_item.PhoneNumber = "9999 9999";
		//	_item.MobileNumber = "0400 000 000";
		//	_item.PhysicalAddress = new Address()
		//		{
		//			AddressLine1 = "15 Somestreet", 
		//			Suburb = "Perth", PostCode = "600", State = "WA",
		//			AddressType = AddressTypeSetting.Physical };
		//	_cleanerList.Add(_item);

		//	_item = new Cleaner() { Title = "Mr", GivenName = "Carl", Surname = "Fouche", EmailAddress = "carl@test.com", Zone="1" };
		//	_item.PhoneNumber = "9999 9999";
		//	_item.MobileNumber = "0400 000 000";
		//	_item.PhysicalAddress = new Address()
		//		{
		//			AddressLine1 = "15 Somestreet", 
		//			Suburb = "Perth", PostCode = "600", State = "WA",
		//			AddressType = AddressTypeSetting.Physical };
		//	_cleanerList.Add(_item);

		//	return _cleanerList;

		//	//return base.GetAll(includeDisabled);
		//}
		#endregion
	}
}
