#region Using
using System;
using System.Collections.Generic;
using System.Data.Entity;

using MagicMaids.EntityModels;
#endregion

namespace MagicMaids.DataAccess
{
	public class RatesRepository : DbContext
	{
		#region Constructors
		public RatesRepository()
			: base(nameOrConnectionString: "MagicMaidsDBConn")
		{

		}
		#endregion

		#region Methods, Public
		public DbSet<Rate> Rates
		{
			get;
			set;
		}
		//public override IEnumerable<Rate> GetAll(bool includeDisabled)
		//{
		//	var _ratesList = new List<Rate>();

		//	var _rate = new Rate()
		//	{
		//		RateCode = "A",
		//		RateAmount = 32.00m
		//	};

		//	_rate.RateApplications = (RateApplicationsSettings.Residential | RateApplicationsSettings.NormalVisit);
		//	_ratesList.Add(_rate);

		//	_rate = new Rate()
		//	{
		//		RateCode = "B",
		//		RateAmount = 38.00m
		//	};

		//	_rate.RateApplications = (RateApplicationsSettings.Commercial | RateApplicationsSettings.OneOff |
		//							 RateApplicationsSettings.Vacancy | RateApplicationsSettings.FirstVisit);
		//	_ratesList.Add(_rate);

		//	_rate = new Rate()
		//	{
		//		RateCode = "C",
		//		RateAmount = 46.00m
		//	};

		//	_rate.RateApplications = (RateApplicationsSettings.OneHour);
		//	_ratesList.Add(_rate);

		//	_rate = new Rate()
		//	{
		//		RateCode = "D",
		//		RateAmount = 10.00m,
		//		IsActive = false

		//	};

		//	_rate.RateApplications = RateApplicationsSettings.None;
		//	_ratesList.Add(_rate);

		//	return _ratesList;

		//	//return base.GetAll(includeDisabled)
		//}
		#endregion

	}
}
