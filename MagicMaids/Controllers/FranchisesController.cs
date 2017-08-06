#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using MagicMaids.EntityModels;

using NLog;

using System.Data.Entity;
using System.Data.Entity.Infrastructure;

using FluentValidation.Mvc;
#endregion

namespace MagicMaids.Controllers
{

    public partial class SettingsController : BaseController
	{
		#region Methods, Public
		public ActionResult Franchises()
		{
			return View();
		}

		public ActionResult FranchiseRegister()
		{
			return View();
		}

		#endregion

		#region Service Functions
		[HttpGet]
		public JsonResult GetFranchises(int? incDisabled)
		{
			List<Franchise> _data = new List<Franchise>();

			if (incDisabled != null && incDisabled == 1)
			{
				_data = MMContext.Franchises
					 .Include(nameof(Franchise.PhysicalAddress))
					 .Include(nameof(Franchise.PostalAddress))
					 .ToList();
			}
			else
			{
				_data = MMContext.Franchises
		             .Include(nameof(Franchise.PhysicalAddress))
					 .Include(nameof(Franchise.PostalAddress))
					 .Where(p => p.IsActive == true)
					 .ToList();
			}

			return Json(new { list = _data }, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public ActionResult GetFranchise(Guid? Id)
		{
			//https://msdn.microsoft.com/en-us/data/jj574232.aspx
			Franchise _franchise = null;
			if (Id == null)
			{
				// create new item
				_franchise = new Franchise();
				_franchise.PhysicalAddress = new Address() { AddressType = AddressTypeSetting.Physical };
				_franchise.PostalAddress = new Address() { AddressType = AddressTypeSetting.Postal };
				_franchise.PostalAddressRefId = _franchise.PostalAddress.Id;
				_franchise.PhysicalAddressRefId = _franchise.PhysicalAddress.Id;
			}
			else
			{
				_franchise = MMContext.Franchises
									  .Where(f => f.Id == Id)
									  .Include(nameof(Franchise.PhysicalAddress))
									  .Include(nameof(Franchise.PostalAddress))
				                      .FirstOrDefault();
				                      //.Find(Id);
				if (_franchise == null)
				{
					ModelState.AddModelError(string.Empty, $"Franchise [{Id.ToString()}] not found.  Please try again.");
					return JsonFormResponse();
				}
			}

			return Json(new { item = _franchise }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public ActionResult SaveFranchise(Franchise dataItem)
		{
			//https://stackoverflow.com/questions/13541225/asp-net-mvc-how-to-display-success-confirmation-message-after-server-side-proce

			if (dataItem == null)
			{
				ModelState.AddModelError(string.Empty, "Valid franchise data not found.");
			}

			dataItem.UpdatedAt = DateTime.Now;
			dataItem.RowVersion = DateTime.Now;
			if (dataItem.CreatedAt.Year < 1950)
				dataItem.CreatedAt = DateTime.Now;

			if (!dataItem.HasAnyValidAddress)
			{
				ModelState.AddModelError(string.Empty, "Please ensure at least a valid postal or physical address is supplied.");
			}
			else
			{
					if (dataItem.PostalAddress != null && (dataItem.PostalAddress.IsPartialAddress || dataItem.HasValidPostalAddress ))
					{
						var _postalValidator = new AddressValidator();
						var results = _postalValidator.Validate(dataItem.PostalAddress);
						results.AddToModelState(ModelState, "");
					}

					if (dataItem.PhysicalAddress != null && (dataItem.PhysicalAddress.IsPartialAddress || dataItem.HasValidPhysicalAddress) )
					{
						var _physValidator = new AddressValidator();
						var results = _physValidator.Validate(dataItem.PhysicalAddress);
						results.AddToModelState(ModelState, "");
					}
			}

			if (!dataItem.HasAnyPhoneNumbers)
			{
				ModelState.AddModelError(string.Empty, "Please provide at least one valid phone number.");
			}

			if (ModelState.IsValid)
			{
				// get original rowversion before updating model
				var rowVersion = dataItem.RowVersion;
				var bIsNew = (rowVersion.Year < 1990 || dataItem.CreatedAt.Year < 1990);

				if (TryUpdateModel<Franchise>(dataItem))
				{
					//https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/crud
					//https://stackoverflow.com/questions/21286538/asp-net-mvc-5-model-binding-edit-view
					//https://www.mikesdotnetting.com/article/248/mvc-5-with-ef-6-in-visual-basic-updating-related-data
					dataItem.UpdatedAt = DateTime.Now;
					dataItem.UpdatedBy = HttpContext.User.Identity.Name;
					dataItem.RowVersion = DateTime.Now;
					if (bIsNew)
					{
						dataItem.CreatedAt = dataItem.UpdatedAt;
					}

					if (dataItem.PhysicalAddress != null)
					{
						dataItem.PhysicalAddress.UpdatedAt = dataItem.UpdatedAt;
						dataItem.PhysicalAddress.UpdatedBy = dataItem.UpdatedBy;
						dataItem.PhysicalAddress.RowVersion = dataItem.RowVersion;
						if (bIsNew)
						{
							dataItem.PhysicalAddress.CreatedAt = dataItem.UpdatedAt;
						}
					}

					if (dataItem.PostalAddress != null)
					{
						dataItem.PostalAddress.UpdatedAt = dataItem.UpdatedAt;
						dataItem.PostalAddress.UpdatedBy = dataItem.UpdatedBy;
						dataItem.PostalAddress.RowVersion = dataItem.RowVersion;
						if (bIsNew)
						{
							dataItem.PostalAddress.CreatedAt = dataItem.UpdatedAt;
						}
					}

					try
					{
						if (bIsNew)
						{
							MMContext.Franchises.Add(dataItem);
						}
						else
						{
							MMContext.Entry(dataItem).State = EntityState.Modified;
							MMContext.Entry(dataItem).OriginalValues["RowVersion"] = rowVersion;

							if (dataItem.PhysicalAddress != null)
							{
								MMContext.Entry(dataItem.PhysicalAddress).State = EntityState.Modified;
								MMContext.Entry(dataItem.PhysicalAddress).OriginalValues["RowVersion"] = rowVersion;
							}

							if (dataItem.PostalAddress  != null)
							{
								MMContext.Entry(dataItem.PostalAddress).State = EntityState.Modified;
								MMContext.Entry(dataItem.PostalAddress).OriginalValues["RowVersion"] = rowVersion;
							}
						}

						MMContext.SaveChanges();

						return JsonSuccessResponse("Franchise saved successfully", dataItem);
					}
					catch (DbUpdateConcurrencyException ex)
					{
						var entry = ex.Entries.Single();
						var clientValues = (Franchise)entry.Entity;
						var databaseEntry = entry.GetDatabaseValues();
						if (databaseEntry == null)
						{
							ModelState.AddModelError(string.Empty, "Unable to save changes. The franchise was deleted by another user.");
						}
						else
						{
							var databaseValues = (Franchise)databaseEntry.ToObject();

							if (databaseValues.Name  != clientValues.Name)
								ModelState.AddModelError("Name", "Current value: " + databaseValues.Name);

							if (databaseValues.TradingName != clientValues.TradingName)
								ModelState.AddModelError("TradingName", "Current value: " + databaseValues.TradingName);

							ModelState.AddModelError(string.Empty, "The record you attempted to edit "
								+ "was modified by another user after you got the original value. The "
								+ "edit operation was canceled and the current values in the database "
								+ "have been displayed. If you still want to edit this record, click "
								+ "the Save button again.");

							dataItem.RowVersion = databaseValues.RowVersion;
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
						ModelState.AddModelError(string.Empty, $"Error saving franchise ({ex.Message})");

						LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
						log.Log(LogLevel.Error, "Error saving franchise", nameof(SaveFranchise), ex, dataItem);
					}
				}

			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(SaveFranchise ), dataItem);
			}

			return JsonFormResponse();
		}


		#endregion
	}
}
