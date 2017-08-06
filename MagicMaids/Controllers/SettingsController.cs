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
#endregion

namespace MagicMaids.Controllers
{
	public partial class SettingsController : BaseController 
	{
		#region Constructor
		public SettingsController(MagicMaidsContext dbContext): base(dbContext)
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
		public ActionResult Rates()
		{
			bool _chkShowDisabled = false;

			//if (_rateRepo == null)
			//	_rateRepo = new RatesRepository();

			//List<Rate> _itemList = _rateRepo.GetAll(_chkShowDisabled).ToList<Rate>();

			//return View(_itemList);

			return View();
		}

		[OutputCache(CacheProfile = "CacheForDemo")]
		public ActionResult RateManage()
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

		[OutputCache(CacheProfile = "CacheForDemo")]
		public ActionResult Postcodes()
		{
			return View();
		}

		public ActionResult MasterSettings()
		{
			return View();
		}
        #endregion

        #region Service Functions
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

			//List<UpdateSettingsViewModel> _editSettings = new List<UpdateSettingsViewModel>();
			//foreach(SystemSetting _item in _settings)
			//{
			//	_editSettings.Add(new UpdateSettingsViewModel(_item));
			//}

			return Json(new { list = _settings }, JsonRequestBehavior.AllowGet);
		}

		//[Bind(Include = "SettingValue")] 
		[HttpPost]
		public ActionResult SaveSettings(SystemSetting setting)
		{
            //https://stackoverflow.com/questions/13541225/asp-net-mvc-how-to-display-success-confirmation-message-after-server-side-proce
           
            if (setting == null)
            {
                ModelState.AddModelError(string.Empty, "Valid setting not found.");
            }

			setting.UpdatedAt = DateTime.Now;
			setting.RowVersion = DateTime.Now;
			if (setting.CreatedAt.Year < 1950)
				setting.CreatedAt = DateTime.Now;
			
			LogHelper log2 = new LogHelper(LogManager.GetCurrentClassLogger());
			log2.Log(LogLevel.Info, "<XXXXXX> 1 - " + setting.UpdatedAt.ToString(), nameof(SaveSettings), null, null);

			if (ModelState.IsValid)
			{
				log2.Log(LogLevel.Info, "<XXXXXX> 2", nameof(SaveSettings), null, null);

				Guid _id = setting.Id;
                // get original rowversion before updating model
                var rowVersion = setting.RowVersion;

				SystemSetting _objToUpdate = MMContext.DefaultSettings.Find(_id);
				if (_objToUpdate == null)
				{
					ModelState.AddModelError(string.Empty, $"Setting [{_id.ToString()}] not found.  Please try again.");
					return JsonFormResponse();
				}
				//log2.Log(LogLevel.Info, "<XXXXXX> 3", nameof(SaveSettings), null, null);
                
				if (TryUpdateModel<SystemSetting>(_objToUpdate))
				{
					//log2.Log(LogLevel.Info, "<XXXXXX> 4", nameof(SaveSettings), null, null);
					//https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/crud
					_objToUpdate.UpdatedAt = DateTime.Now;
					_objToUpdate.UpdatedBy = HttpContext.User.Identity.Name;
					_objToUpdate.RowVersion = DateTime.Now;

					try
                    {
                        MMContext.Entry(_objToUpdate).State = EntityState.Modified;
						MMContext.Entry(_objToUpdate).OriginalValues["RowVersion"] = rowVersion;
						MMContext.SaveChanges();

						log2.Log(LogLevel.Info, "<XXXXXX> 5", nameof(SaveSettings), null, null);
						return JsonSuccessResponse("Setting saved successfully");
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

                            if (databaseValues.SettingName != clientValues.SettingName)
                                ModelState.AddModelError("SettingName", "Current value: " + databaseValues.SettingName);

                            if (databaseValues.SettingValue != clientValues.SettingValue)
                                ModelState.AddModelError("SettingValue", "Current value: " + databaseValues.SettingValue);

                            if (databaseValues.CodeIdentifier != clientValues.CodeIdentifier)
                                ModelState.AddModelError("CodeIdentifier", "Current value: " + databaseValues.CodeIdentifier);

                            ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                + "was modified by another user after you got the original value. The "
                                + "edit operation was canceled and the current values in the database "
                                + "have been displayed. If you still want to edit this record, click "
                                + "the Save button again.");

                            setting.RowVersion = databaseValues.RowVersion;
                        }
                    }
                    catch (RetryLimitExceededException /* dex */)
                    {
                        //Log the error (uncomment dex variable name and add a line here to write a log.
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                    }
                    catch (Exception ex)
                    {
                        //_msg = new InfoViewModel("Error saving settings", ex);
                        ModelState.AddModelError(string.Empty, $"Error saving setting ({ex.Message})");

                        LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
                        log.Log(LogLevel.Error, "Error saving setting", nameof(SaveSettings), ex, setting);
                    }
				}
                
            }

			if (!ModelState.IsValid)
			{
				log2.Log(LogLevel.Info, "<XXXXXX> 7", nameof(SaveSettings), null, null);
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(SaveSettings), setting);
			}
			log2.Log(LogLevel.Info, "<XXXXXX> 8", nameof(SaveSettings), null, null);
            return JsonFormResponse();
        }
		#endregion
	}
}
