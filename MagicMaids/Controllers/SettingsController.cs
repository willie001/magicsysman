#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using MagicMaids.DataAccess;
using MagicMaids.EntityModels;
using MagicMaids.ViewModels;

using System.Globalization;
using Newtonsoft.Json;
using MagicMaids.Validators;
using FluentValidation.Results;
using LazyCache;
using MySql.Data.MySqlClient;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using Dapper;
#endregion

namespace MagicMaids.Controllers
{
	public partial class SettingsController : BaseController
	{
		#region Constructor
		public SettingsController() : base()
		{
		}
		#endregion

		#region Method, Public
		public ActionResult ServerVars()
		{
			return View();
		}

		[OutputCache(CacheProfile = "CacheForDemo")]
		public ActionResult Index()
		{
			return View();
		}

		[OutputCache(CacheProfile = "CacheForDemo")]
		public ActionResult Templates()
		{
			return View();
		}

		[OutputCache(CacheProfile = "CacheForDemo")]
		public ActionResult TemplateManage()
		{
			return View();
		}

		[OutputCache(CacheProfile = "CacheForDemo")]
		public ActionResult UserAccounts()
		{
			return View();
		}

		[OutputCache(CacheProfile = "CacheForDemo")]
		public ActionResult UserAccountDetails()
		{
			return View();
		}

		public ActionResult MasterSettings()
		{
			return View();
		}
		#endregion

		#region Service Functions, Settings
		public JsonResult GetSettings(int? incDisabled)
		{
			List<UpdateSettingsViewModel> _editSettings = new List<UpdateSettingsViewModel>();

			IAppCache cache = new CachingService();
			if (incDisabled != null && incDisabled == 1)
			{
				_editSettings = cache.GetOrAdd("System_Settings_All", () => GetSettingsPrivate(incDisabled), new TimeSpan(1, 0, 0));
			}
			else
			{
				_editSettings = cache.GetOrAdd("System_Settings_Active", () => GetSettingsPrivate(incDisabled), new TimeSpan(1, 0, 0));
			}

			return new JsonNetResult() { Data = new { list = _editSettings }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		private List<UpdateSettingsViewModel> GetSettingsPrivate(int? incDisabled)
		{
			List<SystemSetting> _settings = new List<SystemSetting>();

			using (DBManager db = new DBManager())
			{
				if (incDisabled != null && incDisabled == 1)
				{
					_settings = db.getConnection().GetList<SystemSetting>().ToList();
				}
				else
				{
					_settings = db.getConnection().GetList<SystemSetting>(new { IsActive = 1 }).ToList();
				}
			}

			List<UpdateSettingsViewModel> _editSettings = new List<UpdateSettingsViewModel>();
			foreach (SystemSetting _item in _settings)
			{
				var _vm = new UpdateSettingsViewModel();
				_vm.PopulateVM(_item);
				_editSettings.Add(_vm);
			}

			return _editSettings;
		}

		[HttpPost]
		public ActionResult SaveSettings(UpdateSettingsViewModel setting)
		{
			//https://stackoverflow.com/questions/13541225/asp-net-mvc-how-to-display-success-confirmation-message-after-server-side-proce

			if (setting == null)
			{
				ModelState.AddModelError(string.Empty, "Valid setting not found.");
			}

			if (ModelState.IsValid)
			{
				String _id = setting.Id;

				using (DBManager db = new DBManager())
				{
					SystemSetting _objToUpdate = db.getConnection().Get<SystemSetting>(_id); 
					if (_objToUpdate == null)
					{
						ModelState.AddModelError(string.Empty, $"Setting [{_id.ToString()}] not found.  Please try again.");
						return JsonFormResponse();
					}

					if (TryUpdateModel<SystemSetting>(_objToUpdate))
					{
						try
						{
							StringBuilder _sql = new StringBuilder();
							_sql.Append("Update SystemSettings set ");
							_sql.Append($"UpdatedAt = '{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}'");
							_sql.Append($",RowVersion = '{_objToUpdate.RowVersion.FormatDatabaseDateTime()}'");
							_sql.Append($",UpdatedBy = '{_objToUpdate.UpdatedBy}'");
							_sql.Append($",IsActive = {_objToUpdate.IsActive}");
							_sql.Append($",SettingName = '{_objToUpdate.SettingName}'");
							_sql.Append($",SettingValue = '{_objToUpdate.SettingValue}'");
							_sql.Append($",CodeIdentifier = '{_objToUpdate.CodeIdentifier}'");
							_sql.Append($" where Id = '{_objToUpdate.Id}'");
							db.getConnection().Execute(_sql.ToString());

							SystemSettings.Reset();

							IAppCache cache = new CachingService();
							cache.Remove("System_Settings_All");
							cache.Remove("System_Settings_Active");

							return JsonSuccessResponse("Setting saved successfully", _objToUpdate);
						}
						//catch (DbUpdateConcurrencyException ex)
						//{
						//	var entry = ex.Entries.Single();
						//	var clientValues = (SystemSetting)entry.Entity;
						//	var databaseEntry = entry.GetDatabaseValues();
						//	if (databaseEntry == null)
						//	{
						//		ModelState.AddModelError(string.Empty, "Unable to save changes. The system setting was deleted by another user.");
						//	}
						//	else
						//	{
						//		var databaseValues = (SystemSetting)databaseEntry.ToObject();

						//		ModelState.AddModelError(string.Empty, "The record you attempted to edit "
						//			+ "was modified by another user after you got the original value. The "
						//			+ "edit operation was canceled and the current values in the database "
						//			+ "have been displayed. If you still want to edit this record, click "
						//			+ "the Save button again.");

						//		if (databaseValues.SettingName != clientValues.SettingName)
						//			ModelState.AddModelError("SettingName", "Current database value for setting name: " + databaseValues.SettingName);

						//		if (databaseValues.SettingValue != clientValues.SettingValue)
						//			ModelState.AddModelError("SettingValue", "Current database value for setting value: " + databaseValues.SettingValue);

						//		if (databaseValues.CodeIdentifier != clientValues.CodeIdentifier)
						//			ModelState.AddModelError("CodeIdentifier", "Current database value for code identifier: " + databaseValues.CodeIdentifier);
						//	}
						//}
						catch (Exception ex)
						{
							ModelState.AddModelError(string.Empty, Helpers.FormatModelError("Error saving setting", ex));

							LogHelper log = new LogHelper();
							log.Log(LogHelper.LogLevels.Error, "Error saving setting", nameof(SaveSettings), ex, setting, Helpers.ParseValidationErrors(ex));
						}
					}
				}

			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(ModelState, nameof(SaveSettings), setting);
			}

			return JsonFormResponse();
		}
		#endregion

		#region Service Functions, Postcodes
		public JsonResult GetPostcodes(Guid? FranchiseId)
		{
			List<UpdateSuburbZonesVM> _editList = new List<UpdateSuburbZonesVM>();

			IAppCache cache = new CachingService();
			var cacheName = "Postcodes";
			if (FranchiseId.HasValue && FranchiseId != null)
				cacheName += $"_{FranchiseId}";
			_editList = cache.GetOrAdd(cacheName, () => GetPostcodesPrivate(FranchiseId), new TimeSpan(1, 0, 0));

			return new JsonNetResult() { Data = new { list = _editList, nextGuid = Guid.NewGuid() }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		private List<UpdateSuburbZonesVM> GetPostcodesPrivate(Guid? FranchiseId)
		{
			List<SuburbZone> _entityList = new List<SuburbZone>();

			using (DBManager db = new DBManager())
			{
				string _where = "where FranchiseId ";
				if (Helpers.IsValidGuid(FranchiseId))
				{
					_where += " = '" + FranchiseId.Value.ToString() + "'";
				}
				else
				{
					_where += " is null or LENGTH(FranchiseId)=0 or FranchiseId like '00000%'";
				}
				_entityList = db.getConnection().GetList<SuburbZone>(_where).ToList();
			}
			List<UpdateSuburbZonesVM> _editList = new List<UpdateSuburbZonesVM>();
			foreach (SuburbZone _item in _entityList)
			{
				var _vm = new UpdateSuburbZonesVM();
				_vm.PopulateVM(_item);
				_editList.Add(_vm);
			}

			return _editList;
		}


		[HttpPost]
		public ActionResult SavePostCodes(UpdateSuburbZonesVM formValues)
		{
			string _objDesc = "Suburb/zone";

			if (formValues == null)
			{
				ModelState.AddModelError(string.Empty, $"Valid {_objDesc.ToLower()} data not found.");
			}

			if (ModelState.IsValid)
			{
				// validate for unique suburb / zone for 
				//SuburbZoneValidator validator = new SuburbZoneValidator(formValues.OtherZones);
				//ValidationResult results = validator.Validate(formValues);
				//if (!results.IsValid)
				//{
				//	ModelState.AddModelError(string.Empty, results.Errors.FirstOrDefault().ErrorMessage);
				//}

			}

			if (ModelState.IsValid)
			{
				String _id = formValues.Id;
				var bIsNew = formValues.IsNewItem;

				try
				{
					SuburbZone _objToUpdate = null;

					using (DBManager db = new DBManager())
					{
						if (bIsNew)
						{
							_objToUpdate = UpdateAuditTracking(UpdateSettings(null, formValues));
							StringBuilder _sql = new StringBuilder();
							_sql.Append("Insert into SuburbZones (Id, CreatedAt, UpdatedAt, UpdatedBy, IsActive,");
							_sql.Append("RowVersion, SuburbName, PostCode, Zone, LinkedZones, FranchiseId)");
							_sql.Append(" values (");
							_sql.Append($"'{_objToUpdate.Id}',");
							_sql.Append($"'{_objToUpdate.CreatedAt.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.UpdatedBy}',");
							_sql.Append($"{_objToUpdate.IsActive},");
							_sql.Append($"'{_objToUpdate.RowVersion.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.SuburbName}',");
							_sql.Append($"'{_objToUpdate.PostCode}',");
							_sql.Append($"'{_objToUpdate.Zone}',");
							_sql.Append($"'{_objToUpdate.LinkedZones}',");
							_sql.Append($"'{_objToUpdate.FranchiseId}'");
							_sql.Append(")");

							db.getConnection().Execute(_sql.ToString());
							//https://github.com/ericdc1/Dapper.SimpleCRUD/blob/master/Dapper.SimpleCRUD/SimpleCRUD.cs#L320
							//var _key = db.Insert<SuburbZone>(UpdateAuditTracking(_objToUpdate));
						}
						else
						{
							_objToUpdate = db.getConnection().Get<SuburbZone>(_id);

							if (_objToUpdate == null)
							{
								ModelState.AddModelError(string.Empty, $"{_objDesc} [{_id.ToString()}] not found.  Please try again.");
								return JsonFormResponse();
							}

							_objToUpdate = UpdateAuditTracking(UpdateSettings(_objToUpdate, formValues));

							StringBuilder _sql = new StringBuilder();
							_sql.Append("Update SuburbZones set ");
							_sql.Append($"UpdatedAt = '{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}'");
							_sql.Append($",RowVersion = '{_objToUpdate.RowVersion.FormatDatabaseDateTime()}'");
							_sql.Append($",UpdatedBy = '{_objToUpdate.UpdatedBy}'");
							_sql.Append($",IsActive = {_objToUpdate.IsActive}");
							_sql.Append($",SuburbName = '{_objToUpdate.SuburbName}'");
							_sql.Append($",PostCode = '{_objToUpdate.PostCode}'");
							_sql.Append($",Zone = '{_objToUpdate.Zone}'");
							_sql.Append($",LinkedZones = '{_objToUpdate.LinkedZones}'");
							_sql.Append($",FranchiseId = '{_objToUpdate.FranchiseId}'");
							_sql.Append($" where Id = '{_objToUpdate.Id}'");
							db.getConnection().Execute(_sql.ToString());
							    
						}

						IAppCache cache = new CachingService();
						var cacheName = "Postcodes";
						if (Helpers.IsValidGuid(_objToUpdate.FranchiseId))
							cacheName += $"_{_objToUpdate.FranchiseId}";
						cache.Remove(cacheName);
					}

					return JsonSuccessResponse($"{_objDesc} saved successfully", _objToUpdate);
				}
				//catch (DbUpdateConcurrencyException ex)
				//{
				//	var entry = ex.Entries.Single();
				//	var clientValues = (SuburbZone)entry.Entity;
				//	var databaseEntry = entry.GetDatabaseValues();
				//	if (databaseEntry == null)
				//	{
				//		ModelState.AddModelError(string.Empty, $"Unable to save changes. The {_objDesc.ToLower()} was deleted by another user.");
				//	}
				//	else
				//	{
				//		var databaseValues = (SuburbZone)databaseEntry.ToObject();

				//		if (databaseValues.SuburbName != clientValues.SuburbName)
				//		{
				//			ModelState.AddModelError("SuburbName", "Current database value for suburb name: " + databaseValues.SuburbName);
				//		}

				//		if (databaseValues.PostCode != clientValues.PostCode)
				//		{
				//			ModelState.AddModelError("PostCode", "Current database value for post code: " + databaseValues.PostCode);
				//		}

				//		ModelState.AddModelError(string.Empty, "The record you attempted to edit "
				//			+ "was modified by another user after you got the original value. The edit operation "
				//			+ "was canceled. If you still want to edit this record, click the Save button again.");
				//	}
				//}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, Helpers.FormatModelError($"Error saving {_objDesc.ToLower()}", ex));

					LogHelper log = new LogHelper();
					log.Log(LogHelper.LogLevels.Error, $"Error saving {_objDesc.ToLower()}", nameof(SavePostCodes), ex, formValues, Helpers.ParseValidationErrors(ex));
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(ModelState, nameof(SavePostCodes), formValues);
			}

			return JsonFormResponse();
		}

		private SuburbZone UpdateSettings(SuburbZone _objToUpdate, UpdateSuburbZonesVM dataItem)
		{

			if (dataItem == null)
			{
				return _objToUpdate;
			}

			if (_objToUpdate == null)
			{
				_objToUpdate = new SuburbZone();
			}

			_objToUpdate.SuburbName = dataItem.SuburbName;
			_objToUpdate.PostCode = dataItem.PostCode;
			_objToUpdate.Zone = dataItem.Zone;
			_objToUpdate.LinkedZones = dataItem.LinkedZones;
			_objToUpdate.FranchiseId = dataItem.FranchiseId;

			return _objToUpdate;
		}
		#endregion

		#region Service Functions, Rates
		[HttpGet]
		public JsonResult GetRateTypesJson(RateApplicationsSettings contextSelection = RateApplicationsSettings.None)
		{
			var enumVals = new List<object>();

			foreach (var item in Enum.GetValues(typeof(RateApplicationsSettings)))
			{
				var _val = Enum.Parse(typeof(RateApplicationsSettings), item.ToString());

				if (contextSelection == RateApplicationsSettings.None || (((int)item & (int)contextSelection) != (int)item))
				{
					enumVals.Add(new
					{
						id = (int)item,
						name = item.ToString()
					});
				}
			}

			return new JsonNetResult() { Data = new { item = enumVals }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		public JsonResult GetRates(Guid? FranchiseId)
		{
			List<RateListVM> currentRates = GetRatesFromContext(FranchiseId);
			RateApplicationsSettings allSelections = RateApplicationsSettings.None;
			foreach (RateListVM _item in currentRates)
			{
				allSelections |= _item.SelectedRatesValue;
			}

			// remove flag if not ONLY none is set
			// ** NOT needed because None is automatically removed.
			// ** I'm leaving this here to document removal step :)
			if (allSelections != RateApplicationsSettings.None)
			{
				allSelections &= ~RateApplicationsSettings.None;
			}
			return new JsonNetResult() { Data = new { list = currentRates, nextGuid = Guid.NewGuid(), contextSelections = allSelections }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		private List<RateListVM> GetRatesFromContext(Guid? FranchiseId)
		{
			List<RateListVM> _showList = new List<RateListVM>();

			IAppCache cache = new CachingService();
			var cacheName = "Rates";
			if (FranchiseId.HasValue && FranchiseId != null)
				cacheName += $"_{FranchiseId}";
			_showList = cache.GetOrAdd(cacheName, () => GetRatesPrivate(FranchiseId), new TimeSpan(1, 0, 0));

			return _showList;
		}

		private List<RateListVM> GetRatesPrivate(Guid? franchiseId)
		{
			List<Rate> _entityList = new List<Rate>();

			using (DBManager db = new DBManager())
			{
				if (franchiseId.HasValue && Helpers.IsValidGuid(franchiseId))
				{
					_entityList = db.getConnection().GetList<Rate>(new { franchiseId = franchiseId.Value.ToString() }).ToList();
				}
				else
				{
					_entityList = db.getConnection().GetList<Rate>().ToList();
				}
			}

			List<RateListVM> _showList = new List<RateListVM>();
			foreach (Rate _item in _entityList)
			{
				var _vm = new RateListVM();
				_vm.PopulateVM(_item);
				_showList.Add(_vm);
			}

			return _showList;
		}
		[HttpPost]
		public ActionResult SaveRate(RateDetailsVM formValues)
		{
			string _objDesc = "Rate";
			int _selection = 0;

			if (formValues == null)
			{
				ModelState.AddModelError(string.Empty, $"Valid {_objDesc.ToLower()} data not found.");
			}

			if (String.IsNullOrWhiteSpace(formValues.SelectedRatesJson))
			{
				ModelState.AddModelError(string.Empty, $"Valid {_objDesc.ToLower()} selections not found.");
			}
			else
			{
				List<SelectedRateItem> _selectionList = JsonConvert.DeserializeObject<List<SelectedRateItem>>(formValues.SelectedRatesJson);
				foreach (SelectedRateItem _item in _selectionList)
				{
					_selection += _item.Id;
				}
			}

			if (ModelState.IsValid)
			{
				String _id = formValues.Id;
				var bIsNew = formValues.IsNewItem;

				try
				{
					Rate _objToUpdate = null;

					using (DBManager db = new DBManager())
					{
						if (bIsNew)
						{
							_objToUpdate = UpdateRates(null, formValues, _selection);
							StringBuilder _sql = new StringBuilder();
							_sql.Append("Insert into Rates (Id, CreatedAt, UpdatedAt, UpdatedBy, IsActive, RowVersion, ");
							_sql.Append("RateCode, RateAmount, RateApplications, FranchiseId)");
							_sql.Append(" values (");
							_sql.Append($"'{_objToUpdate.Id}',");
							_sql.Append($"'{_objToUpdate.CreatedAt.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.UpdatedBy}',");
							_sql.Append($"{_objToUpdate.IsActive},");
							_sql.Append($"'{_objToUpdate.RowVersion.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.RateCode}',");
							_sql.Append($"{_objToUpdate.RateAmount},");
							_sql.Append($"{(int)_objToUpdate.RateApplications},");
							_sql.Append($"'{_objToUpdate.FranchiseId}'");
							_sql.Append(")");
							db.getConnection().Execute(_sql.ToString());

							//var newId = db.Insert<Rate>(_objToUpdate);
						}
						else
						{
							_objToUpdate = db.getConnection().Get<Rate>(_id);

							if (_objToUpdate == null)
							{
								ModelState.AddModelError(string.Empty, $"{_objDesc} [{_id.ToString()}] not found.  Please try again.");
								return JsonFormResponse();
							}

							_objToUpdate = UpdateRates(_objToUpdate, formValues, _selection);
							db.getConnection().Update(_objToUpdate);
						}

						IAppCache cache = new CachingService();
						var cacheName = "Rates";
						if (Helpers.IsValidGuid(_objToUpdate.FranchiseId))
							cacheName += $"_{_objToUpdate.FranchiseId.ToString()}";
						cache.Remove(cacheName);
					}
					return JsonSuccessResponse($"{_objDesc} saved successfully", _objToUpdate);
				}
				//catch (DbUpdateConcurrencyException ex)
				//{
				//	var entry = ex.Entries.Single();
				//	var clientValues = (Rate)entry.Entity;
				//	var databaseEntry = entry.GetDatabaseValues();
				//	if (databaseEntry == null)
				//	{
				//		ModelState.AddModelError(string.Empty, $"Unable to save changes. The {_objDesc.ToLower()} was deleted by another user.");
				//	}
				//	else
				//	{
				//		var databaseValues = (Rate)databaseEntry.ToObject();

				//		if (databaseValues.RateCode != clientValues.RateCode)
				//		{
				//			ModelState.AddModelError("RateCode", "Current database value for rate code: " + databaseValues.RateCode);
				//		}

				//		if (databaseValues.RateAmount != clientValues.RateAmount)
				//		{
				//			ModelState.AddModelError("RateAmount", "Current database value for rate amount: " + databaseValues.RateAmount);
				//		}

				//		ModelState.AddModelError(string.Empty, "The record you attempted to edit "
				//			+ "was modified by another user after you got the original value. The edit operation "
				//			+ "was canceled. If you still want to edit this record, click the Save button again.");
				//	}
				//}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, Helpers.FormatModelError($"Error saving {_objDesc.ToLower()}", ex));

					LogHelper log = new LogHelper();
					log.Log(LogHelper.LogLevels.Error, $"Error saving {_objDesc.ToLower()}", nameof(Rate), ex, formValues, Helpers.ParseValidationErrors(ex));
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(ModelState, nameof(Rate), formValues);
			}

			return JsonFormResponse();
		}

		private Rate UpdateRates(Rate _objToUpdate, RateDetailsVM dataItem, int selection)
		{

			if (dataItem == null)
			{
				return _objToUpdate;
			}

			if (_objToUpdate == null)
			{
				_objToUpdate = new Rate();
			}

			_objToUpdate.RateCode = dataItem.RateCode;
			_objToUpdate.RateAmount = dataItem.RateAmount;
			_objToUpdate.RateApplications = (RateApplicationsSettings)selection;
			_objToUpdate.IsActive = dataItem.IsActive;
			_objToUpdate.FranchiseId = dataItem.FranchiseId;
			_objToUpdate.ActivationDate = DateTime.Now.ToUTC();

			return UpdateAuditTracking(_objToUpdate);
		}
		#endregion

		#region Methods, Static
		public static List<string> GetZoneListByFranchise(String FranchiseId, Boolean toLower)
		{
			List<String> _zoneList = new List<String>();
			using (DBManager db = new DBManager())
			{
				if (Helpers.IsValidGuid(FranchiseId))
				{
					_zoneList = db.getConnection().Query<String>($"select concat(Zone,',',LinkedZones) from SuburbZones where FranchiseId='{FranchiseId}'").ToList();
				}

				// load system default list
				if (_zoneList.Count == 0)
				{
					_zoneList = db.getConnection().Query<String>($"select concat(Zone,',',LinkedZones) from SuburbZones where FranchiseId is not null").ToList();
				}
			}

			var _zoneCSV = String.Join(",", _zoneList);
			if (toLower)
			{
				_zoneList = _zoneCSV.ToLower().Split(new char[] { ',', ';' })
											  .Distinct()
											  .ToList();
			}
			else
			{
				_zoneList = _zoneCSV.Split(new char[] { ',', ';' })
											  .Distinct()
											  .ToList();
			}

			_zoneList.Sort();

			return _zoneList;
		}

		public static List<string> GetZoneListBySuburb(string Suburb)
		{
			if (String.IsNullOrWhiteSpace(Suburb))
			{
				return new List<string>();
			}

			List<String> _zoneList = new List<String>();
			using (DBManager db = new DBManager())
			{
				_zoneList = db.getConnection().Query<String>($"select Zone+','+LinkedZones from SuburbZones where SuburbName like '%{Suburb}%' or PostCode = '{Suburb}'").ToList();

				// load system default list
				if (_zoneList.Count == 0)
				{
					_zoneList = db.getConnection().Query<String>($"select Zone+','+LinkedZones from SuburbZones where FranchiseId is not null").ToList();
				}
			}


			var _zoneCSV = String.Join(",", _zoneList);
			_zoneList = _zoneCSV.Split(new char[] { ',', ';' })
						  .Distinct()
						  .ToList();

			_zoneList.Sort();

			return _zoneList;
		}
		#endregion 
	}
}
