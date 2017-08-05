#region Using
using System;
using System.Collections.Generic;
using System.Data.Entity;

using MagicMaids.EntityModels;

#endregion

namespace MagicMaids.DataAccess
{
	public class TemplateRepository: DbContext
	{
		#region Fields

		#endregion

		#region Constructors
		public TemplateRepository()
			: base(nameOrConnectionString: "MagicMaidsDBConn")
		{

		}
		#endregion

		#region Properties, Public
		public DbSet<Template> Templates
		{
			get;
			set;
		}

		//public override IEnumerable<Template> GetAll(bool includeDisabled)
		//{
		//	var _tempList = new List<Template>();

		//	var _temp = new Template()
		//	{
		//		TemplateName = "Price Change Letter",
		//		TemplateDescription = "Generic letter to advise all of price changes",
		//		LimitToCountry = "",
		//		TemplateType = TemplateTypeSetting.Document,
		//		TemplateAudience = (TemplateAudienceSetting.Cleaners | TemplateAudienceSetting.MasterFranchises | TemplateAudienceSetting.Customers),
		//		TemplateContent = new TemplateContent() 
		//	};
		//	_tempList.Add(_temp);


		//	_temp = new Template()
		//	{
		//		TemplateName = "Disclosure Document",
		//		TemplateDescription = "Business diclosure statement",
		//		LimitToCountry = "",
		//		TemplateType = TemplateTypeSetting.Document,
		//		TemplateAudience = (TemplateAudienceSetting.Customers),
		//		TemplateContent = new TemplateContent()
		//	};
		//	_tempList.Add(_temp);

		//	_temp = new Template()
		//	{
		//		TemplateName = "Price change email",
		//		TemplateDescription = "Customer centric price change email",
		//		LimitToCountry = "Australia",
		//		TemplateType = TemplateTypeSetting.Email,
		//		TemplateAudience = (TemplateAudienceSetting.Customers),
		//		TemplateContent = new TemplateContent() 
		//	};
		//	_tempList.Add(_temp);

		//	_temp = new Template()
		//	{
		//		TemplateName = "Cleaner leave dates",
		//		TemplateDescription = "Customer advisement of cleaner leave dates",
		//		LimitToCountry = "",
		//		TemplateType = TemplateTypeSetting.Email,
		//		TemplateAudience = (TemplateAudienceSetting.Customers),
		//		TemplateContent = new TemplateContent()
		//	};
		//	_tempList.Add(_temp);

		//	_temp = new Template()
		//	{
		//		TemplateName = "Daily confirmation email",
		//		TemplateDescription = "Customer daily confirmation email",
		//		LimitToCountry = "",
		//		TemplateType = TemplateTypeSetting.Email,
		//		TemplateAudience = (TemplateAudienceSetting.Customers),
		//		TemplateContent = new TemplateContent(),
		//		EmailSchedule = new EmailSchedule() { Schedule = ScheduleSetting.Daily, NextScheduled = DateTime.Now }	
		//	};
		//	_tempList.Add(_temp);

		//	return _tempList;
		//	//return base.GetAll(includeDisabled)
		//}
		#endregion
	}
}
