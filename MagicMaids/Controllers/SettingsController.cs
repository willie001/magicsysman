#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using MagicMaids.DataAccess;
using MagicMaids.EntityModels;
using MagicMaids.ViewModels;

using NLog;

using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using Newtonsoft.Json;
using MagicMaids.Validators;
using FluentValidation.Results;
#endregion

namespace MagicMaids.Controllers
{
	public partial class SettingsController : BaseController
	{
		#region Constructor
		public SettingsController(MagicMaidsContext dbContext) : base(dbContext)
		{
		}
		#endregion

		#region Method, Public
		[OutputCache(CacheProfile = "CacheForDemo")]
		public ActionResult Index()
		{
			return View();
		}

		[OutputCache(CacheProfile = "CacheForDemo")]
		public ActionResult Templates()
		{
			bool _chkShowDisabled = false;

			//if (_tempRepo == null)
			//	_tempRepo = new TemplateRepository();

			//List<Template> _itemList = _tempRepo.GetAll(_chkShowDisabled).ToList<Template>();

			//return View(_itemList);

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
			bool _chkShowDisabled = false;

			//if (_userRepo == null)
			//{
			//	_userRepo = new UserAccountRepository();
			//}

			//List<UserAccount> _itemList = _userRepo.GetAll(_chkShowDisabled).ToList<UserAccount>();

			//return View(_itemList);

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
			List<SystemSetting> _settings = new List<SystemSetting>();

			if (incDisabled != null && incDisabled == 1)
			{
				_settings = MMContext.DefaultSettings.ToList();
			}
			else
			{
				_settings = MMContext.DefaultSettings
					 .Where(p => p.IsActive == true)
					 .ToList();
			}

			List<UpdateSettingsViewModel> _editSettings = new List<UpdateSettingsViewModel>();
			foreach (SystemSetting _item in _settings)
			{
				var _vm = new UpdateSettingsViewModel();
				_vm.PopulateVM(_item);
				_editSettings.Add(_vm);
			}

			return new JsonNetResult() { Data = new { list = _editSettings }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpPost]
		[ValidateAntiForgeryHeader]
		public ActionResult SaveSettings(UpdateSettingsViewModel setting)
		{
			//https://stackoverflow.com/questions/13541225/asp-net-mvc-how-to-display-success-confirmation-message-after-server-side-proce

			if (setting == null)
			{
				ModelState.AddModelError(string.Empty, "Valid setting not found.");
			}

			if (ModelState.IsValid)
			{
				Guid _id = setting.Id;

				SystemSetting _objToUpdate = MMContext.DefaultSettings.Find(_id);
				if (_objToUpdate == null)
				{
					ModelState.AddModelError(string.Empty, $"Setting [{_id.ToString()}] not found.  Please try again.");
					return JsonFormResponse();
				}
				//log2.Log(LogLevel.Info, "<XXXXXX> 3", nameof(SaveSettings), null, null);

				if (TryUpdateModel<SystemSetting>(_objToUpdate))
				{
					try
					{
						MMContext.Entry(_objToUpdate).State = EntityState.Modified;
						MMContext.SaveChanges();

						return JsonSuccessResponse("Setting saved successfully", _objToUpdate);
					}
					catch (DbUpdateConcurrencyException ex)
					{
						var entry = ex.Entries.Single();
						var clientValues = (SystemSetting)entry.Entity;
						var databaseEntry = entry.GetDatabaseValues();
						if (databaseEntry == null)
						{
							ModelState.AddModelError(string.Empty, "Unable to save changes. The system setting was deleted by another user.");
						}
						else
						{
							var databaseValues = (SystemSetting)databaseEntry.ToObject();

							ModelState.AddModelError(string.Empty, "The record you attempted to edit "
								+ "was modified by another user after you got the original value. The "
								+ "edit operation was canceled and the current values in the database "
								+ "have been displayed. If you still want to edit this record, click "
								+ "the Save button again.");

							if (databaseValues.SettingName != clientValues.SettingName)
								ModelState.AddModelError("SettingName", "Current database value for setting name: " + databaseValues.SettingName);

							if (databaseValues.SettingValue != clientValues.SettingValue)
								ModelState.AddModelError("SettingValue", "Current database value for setting value: " + databaseValues.SettingValue);

							if (databaseValues.CodeIdentifier != clientValues.CodeIdentifier)
								ModelState.AddModelError("CodeIdentifier", "Current database value for code identifier: " + databaseValues.CodeIdentifier);
						}
					}
					catch (RetryLimitExceededException /* dex */)
					{
						//Log the error (uncomment dex variable name and add a line here to write a log.
						ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
					}
					catch (Exception ex)
					{
						ModelState.AddModelError(string.Empty, Helpers.FormatModelError("Error saving setting", ex));

						LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
						log.Log(LogLevel.Error, "Error saving setting", nameof(SaveSettings), ex, setting, Helpers.ParseValidationErrors(ex));
					}
				}

			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(SaveSettings), setting);
			}

			return JsonFormResponse();
		}
		#endregion

		#region Service Functions, Postcodes
		public JsonResult GetPostcodes(Guid? FranchiseId)
		{
			List<SuburbZone> _entityList = new List<SuburbZone>();

			_entityList = MMContext.SuburbZones
				   .Where(p => p.FranchiseId == FranchiseId)
				   .ToList();

			List<UpdateSuburbZonesVM> _editList = new List<UpdateSuburbZonesVM>();
			foreach (SuburbZone _item in _entityList)
			{
				var _vm = new UpdateSuburbZonesVM();
				_vm.PopulateVM(_item);
				_editList.Add(_vm);
			}

			return new JsonNetResult() { Data = new { list = _editList, nextGuid = Guid.NewGuid() }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}


		[HttpPost]
		[ValidateAntiForgeryHeader]
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
				Guid _id = formValues.Id;
				var bIsNew = formValues.IsNewItem;

				try
				{
					SuburbZone _objToUpdate = null;

					if (bIsNew)
					{
						_objToUpdate = new SuburbZone();
						_objToUpdate.SuburbName = formValues.SuburbName;
						_objToUpdate.PostCode = formValues.PostCode;
						_objToUpdate.Zone = formValues.Zone;
						_objToUpdate.LinkedZones = formValues.LinkedZones;
						_objToUpdate.FranchiseId = formValues.FranchiseId.HasValue ? formValues.FranchiseId : null;

						MMContext.Entry(_objToUpdate).State = EntityState.Added;
					}
					else
					{
						_objToUpdate = MMContext.SuburbZones
								 .Where(f => f.Id == _id)
										  .FirstOrDefault();

						if (_objToUpdate == null)
						{
							ModelState.AddModelError(string.Empty, $"{_objDesc} [{_id.ToString()}] not found.  Please try again.");
							return JsonFormResponse();
						}

						MMContext.Entry(_objToUpdate).CurrentValues.SetValues(formValues);
					}

					MMContext.SaveChanges();

					return JsonSuccessResponse($"{_objDesc} saved successfully", _objToUpdate);
				}
				catch (DbUpdateConcurrencyException ex)
				{
					var entry = ex.Entries.Single();
					var clientValues = (SuburbZone)entry.Entity;
					var databaseEntry = entry.GetDatabaseValues();
					if (databaseEntry == null)
					{
						ModelState.AddModelError(string.Empty, $"Unable to save changes. The {_objDesc.ToLower()} was deleted by another user.");
					}
					else
					{
						var databaseValues = (SuburbZone)databaseEntry.ToObject();

						if (databaseValues.SuburbName != clientValues.SuburbName)
						{
							ModelState.AddModelError("SuburbName", "Current database value for suburb name: " + databaseValues.SuburbName);
						}

						if (databaseValues.PostCode != clientValues.PostCode)
						{
							ModelState.AddModelError("PostCode", "Current database value for post code: " + databaseValues.PostCode);
						}

						ModelState.AddModelError(string.Empty, "The record you attempted to edit "
							+ "was modified by another user after you got the original value. The edit operation "
							+ "was canceled. If you still want to edit this record, click the Save button again.");
					}
				}
				catch (RetryLimitExceededException /* dex */)
				{
					//Log the error (uncomment dex variable name and add a line here to write a log.
					ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, Helpers.FormatModelError($"Error saving {_objDesc.ToLower()}", ex));

					LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
					log.Log(LogLevel.Error, $"Error saving {_objDesc.ToLower()}", nameof(SavePostCodes), ex, formValues, Helpers.ParseValidationErrors(ex));
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(SavePostCodes), formValues);
			}

			return JsonFormResponse();
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
			List<Rate> _entityList = new List<Rate>();

			_entityList = MMContext.Rates
				   .Where(p => p.FranchiseId == FranchiseId)
				   .ToList();

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
		[ValidateAntiForgeryHeader]
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
				Guid _id = formValues.Id;
				var bIsNew = formValues.IsNewItem;

				try
				{
					Rate _objToUpdate = null;

					if (bIsNew)
					{
						_objToUpdate = new Rate();
						_objToUpdate.RateCode = formValues.RateCode;
						_objToUpdate.RateAmount = formValues.RateAmount;
						_objToUpdate.RateApplications = (RateApplicationsSettings)_selection;
						_objToUpdate.IsActive = formValues.IsActive;
						_objToUpdate.FranchiseId = formValues.FranchiseId.HasValue ? formValues.FranchiseId : null;

						MMContext.Entry(_objToUpdate).State = EntityState.Added;
					}
					else
					{
						_objToUpdate = MMContext.Rates
								 .Where(f => f.Id == _id)
										  .FirstOrDefault();

						if (_objToUpdate == null)
						{
							ModelState.AddModelError(string.Empty, $"{_objDesc} [{_id.ToString()}] not found.  Please try again.");
							return JsonFormResponse();
						}

						MMContext.Entry(_objToUpdate).CurrentValues.SetValues(formValues);
						_objToUpdate.RateApplications = (RateApplicationsSettings)_selection;
					}

					MMContext.SaveChanges();

					return JsonSuccessResponse($"{_objDesc} saved successfully", _objToUpdate);
				}
				catch (DbUpdateConcurrencyException ex)
				{
					var entry = ex.Entries.Single();
					var clientValues = (Rate)entry.Entity;
					var databaseEntry = entry.GetDatabaseValues();
					if (databaseEntry == null)
					{
						ModelState.AddModelError(string.Empty, $"Unable to save changes. The {_objDesc.ToLower()} was deleted by another user.");
					}
					else
					{
						var databaseValues = (Rate)databaseEntry.ToObject();

						if (databaseValues.RateCode != clientValues.RateCode)
						{
							ModelState.AddModelError("RateCode", "Current database value for rate code: " + databaseValues.RateCode);
						}

						if (databaseValues.RateAmount != clientValues.RateAmount)
						{
							ModelState.AddModelError("RateAmount", "Current database value for rate amount: " + databaseValues.RateAmount);
						}

						ModelState.AddModelError(string.Empty, "The record you attempted to edit "
							+ "was modified by another user after you got the original value. The edit operation "
							+ "was canceled. If you still want to edit this record, click the Save button again.");
					}
				}
				catch (RetryLimitExceededException /* dex */)
				{
					//Log the error (uncomment dex variable name and add a line here to write a log.
					ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, Helpers.FormatModelError($"Error saving {_objDesc.ToLower()}", ex));

					LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
					log.Log(LogLevel.Error, $"Error saving {_objDesc.ToLower()}", nameof(Rate), ex, formValues, Helpers.ParseValidationErrors(ex));
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(Rate), formValues);
			}

			return JsonFormResponse();
		}
		#endregion

		#region Methods, Static
		public static List<string> GetZoneListByFranchise(Guid? FranchiseId, Boolean toLower)
		{
			MagicMaidsContext MMContext = new MagicMaidsContext();

			List<String> _zoneList = new List<String>();
			if (FranchiseId.HasValue)
			{
				_zoneList = MMContext.SuburbZones
							.Where(p => (p.FranchiseId == FranchiseId))
							.Select(p => p.Zone + "," + p.LinkedZones)
							.ToList();
			}

			// load system default list
			if (_zoneList.Count == 0)
			{
				_zoneList = MMContext.SuburbZones
						 	.Where(p => (!p.FranchiseId.HasValue))
							.Select(p => p.Zone + "," + p.LinkedZones)
							.ToList();
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
		#endregion 
	}
}
