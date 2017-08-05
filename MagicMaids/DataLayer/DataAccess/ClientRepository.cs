#region Using
using System;
using System.Collections.Generic;
using System.Data.Entity;

using MagicMaids.EntityModels;

#endregion

namespace MagicMaids.DataAccess
{
	public class ClientRepository : DbContext
	{
		#region Constructors
		public ClientRepository()
			: base(nameOrConnectionString: "MagicMaidsDBConn")
		{

		}
		#endregion

		#region Methods, Public
		public DbSet<Client> Clients
		{
			get;
			set;
		}
		//public override IEnumerable<Client> GetAll(bool includeActive, bool includeDisabled)
		//{
		//	var _clientList = new List<Client>();

		//	var _item = new Client() { Title = "Mr", GivenName = "Joe", Surname = "Bloggs", EmailAddress = "joe@test.com" };
		//	_item.PhysicalAddress = new Address() {
		//		AddressLine1 = "15 Somestreet", 
		//				Suburb = "Perth", PostCode = "600", State = "WA",
		//				AddressType = AddressTypeSetting.Physical 
		//	};
		//	_item.PostalAddress = new Address() {AddressType = AddressTypeSetting.Postal};
		//	_item.PhoneNumber = "9999 9999";
		//	_item.MobileNumber = "0400 000 000";
		//	_clientList.Add(_item);

		//	_item = new Client() { Title = "Mrs", GivenName = "Suzy", Surname = "Smith", EmailAddress = "suzy@test.com" };
		//	_item.PhysicalAddress = new Address()
		//	{
		//		AddressLine1 = "15 Somestreet", 
		//				Suburb = "Perth", PostCode = "600", State = "WA",
		//				AddressType = AddressTypeSetting.Physical
		//	};
		//	_item.PostalAddress = new Address() { AddressType = AddressTypeSetting.Postal};
		//	_item.PhoneNumber = "9999 9999";
		//	_item.MobileNumber = "0400 000 000";
		//	_clientList.Add(_item);

		//	_item =  new Client() { Title = "Mrs", GivenName = "Mary", Surname = "Brent", EmailAddress = "mary@test.com" };
		//	_item.PhysicalAddress = new Address()
		//	{
		//		AddressLine1 = "15 Somestreet", 
		//							Suburb = "Perth", PostCode = "600", State = "WA",
		//							AddressType = AddressTypeSetting.Physical
		//	};
		//	_item.PhoneNumber = "9999 9999";
		//	_item.PostalAddress = new Address() { AddressType = AddressTypeSetting.Postal};
		//	_item.MobileNumber = "0400 000 000";
		//	_clientList.Add(_item);

		//	_item = new Client() { Title = "Mr", GivenName = "Carl", Surname = "Fouche", EmailAddress = "carl@test.com" };
		//	_item.PhysicalAddress = new Address()
		//	{
		//		AddressLine1 = "15 Somestreet", 
		//										Suburb = "Perth", PostCode = "600", State = "WA",
		//										AddressType = AddressTypeSetting.Physical
		//	};
		//	_item.PostalAddress = new Address() { AddressType = AddressTypeSetting.Postal};
		//	_item.PhoneNumber = "9999 9999";
		//	_item.MobileNumber = "0400 000 000";
		//	_clientList.Add(_item);

		//	return _clientList;

		//	//return base.GetAll(includeDisabled);
		//}
		#endregion
	}
}
