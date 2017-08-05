#region Using
using System;
using System.Collections.Generic;
using System.Data.Entity;

using MagicMaids.EntityModels;

#endregion

namespace MagicMaids.DataAccess
{
	public class UserAccountRepository: DbContext
	{
		#region Constructors
		public UserAccountRepository()
			: base(nameOrConnectionString: "MagicMaidsDBConn")
		{

		}
		#endregion

		#region Methods, Public
		public DbSet<UserAccount> UserAccounts
		{
			get;
			set;
		}

		//public override IEnumerable<UserAccount> GetAll(bool includeDisabled)
		//{
		//	var _list = new List<UserAccount>()
		//	{
		//		new UserAccount()
		//		{
		//			UserName = "joe",
		//			FullName = "Joe Boss",
		//			AccountType = UserAccountTypes.Franchisor
		//		},
		//		new UserAccount()
		//		{
		//			UserName = "ben",
		//			FullName = "Ben Office",
		//			AccountType = UserAccountTypes.Staff
		//		},
		//		new UserAccount()
		//		{
		//			UserName = "mary",
		//			FullName = "Mary Operator",
		//			AccountType = UserAccountTypes.Cleaner
		//		},
		//	};

		//	return _list;

		//	//return base.GetAll(includeDisabled);
		//}
		#endregion
	}
}
