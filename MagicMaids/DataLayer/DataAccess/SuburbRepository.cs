#region Using
using System;
using System.Collections.Generic;
using System.Data.Entity;

using MagicMaids.EntityModels;

#endregion

namespace MagicMaids.DataAccess
{
	public class SuburbRepository: DbContext
	{
		#region Constructors
		public SuburbRepository()
			: base(nameOrConnectionString: "MagicMaidsDBConn")
		{

		}
		#endregion

		#region Methods, Public
		public DbSet<Suburb> Suburbs
		{
			get;
			set;
		}

		//public override IEnumerable<Suburb> GetAll(bool includeDisabled)
		//{
		//	var _suburbList = new List<Suburb>()
		//	{
		//		new Suburb("2,3")
		//		{
		//			SuburbName = "Banksia Grove",
		//			PostCode="6031",
		//			ZoneID=1
		//		},
		//		new Suburb("1,3")
		//		{
		//			SuburbName = "Tapping",
		//			PostCode = "6032",
		//			ZoneID = 2
		//		},
		//		new Suburb("2")
		//		{
		//			SuburbName = "Carramar",
		//			PostCode = "6032",
		//			ZoneID = 3
		//		},
		//		new Suburb()
		//		{
		//			SuburbName = "Joondalup",
		//			PostCode = "6033",
		//			ZoneID = 4
		//		}
		//	};

		//	return _suburbList;

		//	//return base.GetAll(includeDisabled);
		//}
		#endregion
	}
}
