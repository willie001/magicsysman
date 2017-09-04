#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using MagicMaids.EntityModels ;
using MagicMaids.ViewModels; 
using MagicMaids.Validators;

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
		public JsonNetResult  GetFranchises(int? incDisabled)
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

			List<UpdateFranchisesViewModel> _editFranchises = new List<UpdateFranchisesViewModel>();
			foreach (Franchise _item in _data)
			{
				var _vm = new UpdateFranchisesViewModel();
				_vm.PopulateVM(_item);
				_editFranchises.Add(_vm);
			}
			return new JsonNetResult() { Data = new { list = _editFranchises }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpGet]
		public JsonNetResult GetActiveFranchises()
		{
			List<Franchise> _data = new List<Franchise>();

		 	_data = MMContext.Franchises
			 	.Where(p => p.IsActive == true)
				.OrderBy(x => x.Name)
				.ToList();

			List<FranchiseSelectViewModel> _listFranchises = new List<FranchiseSelectViewModel>();
			foreach (Franchise _item in _data)
			{
				var _vm = new FranchiseSelectViewModel();
				_vm.PopulateVM(_item);
				_listFranchises.Add(_vm);
			}
			return new JsonNetResult() { Data = new { list = _listFranchises }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpGet]
		public ActionResult GetFranchise(Guid? FranchiseId)
		{
			//https://msdn.microsoft.com/en-us/data/jj574232.aspx
			Franchise _franchise = null;
			UpdateFranchisesViewModel _dataItem = null;

			if (FranchiseId == null)
			{
				// create new item
				_dataItem = new UpdateFranchisesViewModel();
				_dataItem.IsNewItem = true;
				_dataItem.PhysicalAddress = new UpdateAddressViewModel() { AddressType = AddressTypeSetting.Physical };
				_dataItem.PostalAddress = new UpdateAddressViewModel() { AddressType = AddressTypeSetting.Postal };
			}
			else
			{
				_franchise = MMContext.Franchises
									  .Where(f => f.Id == FranchiseId)
									  .Include(nameof(Franchise.PhysicalAddress))
									  .Include(nameof(Franchise.PostalAddress))
				                      .FirstOrDefault();
				if (_franchise == null)
				{
					ModelState.AddModelError(string.Empty, $"Franchise [{FranchiseId.ToString()}] not found.  Please try again.");
					return JsonFormResponse();
				}

				_dataItem = new UpdateFranchisesViewModel();
				_dataItem.PopulateVM(_franchise);
				_dataItem.IsNewItem = false;
			}

			return new JsonNetResult() { Data = new { item = _dataItem }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpGet]
		public ActionResult GetFranchiseSettings(Guid? FranchiseId)
		{
			if (FranchiseId == null)
			{
				ModelState.AddModelError(string.Empty, $"Franchise ID not provided.  Unable to load franchise settings.");
				return JsonFormResponse();
			}

			Franchise _franchise = null;
			FranchiseSettingsVM _dataItem = null;

			List<SystemSetting> _settings = new List<SystemSetting>();
			_settings = MMContext.DefaultSettings
					 .Where(p => p.IsActive == true)
					 .ToList();

			_franchise = MMContext.Franchises
								  .Where(f => f.Id == FranchiseId)
								  .FirstOrDefault();
			if (_franchise == null)
			{
				ModelState.AddModelError(string.Empty, $"Franchise [{FranchiseId.ToString()}] not found.  Please try again.");
				return JsonFormResponse();
			}

			_dataItem = new FranchiseSettingsVM();
			_dataItem.PopulateVM(_franchise, _settings);

			return new JsonNetResult() { Data = new { item = _dataItem }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpPost]
		public ActionResult SaveFranchise(UpdateFranchisesViewModel dataItem)
		{
			//https://stackoverflow.com/questions/13541225/asp-net-mvc-how-to-display-success-confirmation-message-after-server-side-proce

			if (dataItem == null)
			{
				ModelState.AddModelError(string.Empty, "Valid franchise data not found.");
			}

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
				Guid _id = dataItem.Id;
				var bIsNew = (dataItem.IsNewItem );

				//https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/crud
				//https://stackoverflow.com/questions/21286538/asp-net-mvc-5-model-binding-edit-view
				//https://www.mikesdotnetting.com/article/248/mvc-5-with-ef-6-in-visual-basic-updating-related-data

				try
				{
					Franchise _objToUpdate = null;

					if (bIsNew)
					{
						_objToUpdate = new Franchise();
						_objToUpdate.PhysicalAddress = new Address() { AddressType = AddressTypeSetting.Physical };
						_objToUpdate.PostalAddress = new Address() { AddressType = AddressTypeSetting.Postal };
						_objToUpdate.PostalAddressRefId = _objToUpdate.PostalAddress.Id;
						_objToUpdate.PhysicalAddressRefId = _objToUpdate.PhysicalAddress.Id;

						_objToUpdate.BusinessPhoneNumber = dataItem.BusinessPhoneNumber;
						_objToUpdate.CodeOfConductURL = dataItem.CodeOfConductURL;
						_objToUpdate.EmailAddress = dataItem.EmailAddress;
						_objToUpdate.IsActive = dataItem.IsActive;
						_objToUpdate.MasterFranchiseCode = dataItem.MasterFranchiseCode;
						_objToUpdate.MetroRegion = dataItem.MetroRegion;
						_objToUpdate.MobileNumber = dataItem.MobileNumber;
						_objToUpdate.Name = dataItem.Name;
						_objToUpdate.OtherNumber = dataItem.OtherNumber;
						_objToUpdate.TradingName = dataItem.TradingName;
						_objToUpdate.Name = dataItem.Name;


						if (dataItem.PhysicalAddress != null)
						{
							_objToUpdate.PhysicalAddress.AddressLine1 = dataItem.PhysicalAddress.AddressLine1;
							_objToUpdate.PhysicalAddress.AddressLine2 = dataItem.PhysicalAddress.AddressLine2;
							_objToUpdate.PhysicalAddress.AddressLine3 = dataItem.PhysicalAddress.AddressLine3;
							_objToUpdate.PhysicalAddress.Suburb = dataItem.PhysicalAddress.Suburb;
							_objToUpdate.PhysicalAddress.Country = dataItem.PhysicalAddress.Country ;
							_objToUpdate.PhysicalAddress.IsActive = true;
							_objToUpdate.PhysicalAddress.PostCode = dataItem.PhysicalAddress.PostCode;
							_objToUpdate.PhysicalAddress.State = dataItem.PhysicalAddress.State;
						}

						if (dataItem.PostalAddress != null)
						{
							_objToUpdate.PostalAddress.AddressLine1 = dataItem.PostalAddress.AddressLine1;
							_objToUpdate.PostalAddress.AddressLine2 = dataItem.PostalAddress.AddressLine2;
							_objToUpdate.PostalAddress.AddressLine3 = dataItem.PostalAddress.AddressLine3;
							_objToUpdate.PostalAddress.Suburb = dataItem.PostalAddress.Suburb;
							_objToUpdate.PostalAddress.Country = dataItem.PostalAddress.Country;
							_objToUpdate.PostalAddress.IsActive = true;
							_objToUpdate.PostalAddress.PostCode = dataItem.PostalAddress.PostCode;
							_objToUpdate.PostalAddress.State = dataItem.PostalAddress.State;
						}

						MMContext.Entry(_objToUpdate).State = EntityState.Added;
					}
					else
					{
						_objToUpdate = MMContext.Franchises
								 .Where(f => f.Id == _id)
										  .Include(nameof(Franchise.PhysicalAddress))
										  .Include(nameof(Franchise.PostalAddress))
										  .FirstOrDefault();

						if (_objToUpdate == null)
						{
							ModelState.AddModelError(string.Empty, $"Franchise [{_id.ToString()}] not found.  Please try again.");
							return JsonFormResponse();
						}

						MMContext.Entry(_objToUpdate).CurrentValues.SetValues(dataItem);
						MMContext.Entry(_objToUpdate.PhysicalAddress).CurrentValues.SetValues(dataItem.PhysicalAddress);
						MMContext.Entry(_objToUpdate.PostalAddress).CurrentValues.SetValues(dataItem.PostalAddress);
					}

					MMContext.SaveChanges();

					return JsonSuccessResponse("Franchise saved successfully", _objToUpdate);
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
							ModelState.AddModelError("Name", "Current database value for franchise name: " + databaseValues.Name);

						if (databaseValues.TradingName != clientValues.TradingName)
							ModelState.AddModelError("TradingName", "Current database value for trading name: " + databaseValues.TradingName);

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
					//_msg = new InfoViewModel("Error saving franchises", ex);
					ModelState.AddModelError(string.Empty, $"Error saving franchise ({ex.Message})");

					LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
					log.Log(LogLevel.Error, "Error saving franchise", nameof(SaveFranchise), ex, dataItem);
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(SaveFranchise ), dataItem);
			}

			return JsonFormResponse();
		}

		[HttpPost]
		public ActionResult SaveFranchiseSettings(FranchiseSettingsVM dataItem)
		{
			//https://stackoverflow.com/questions/13541225/asp-net-mvc-how-to-display-success-confirmation-message-after-server-side-proce

			if (dataItem == null || dataItem.Id.ToString().Contains("00000000-0000-0000-0000-000000000000"))
			{
				ModelState.AddModelError(string.Empty, "Valid franchise settings not found. Ensure a valid franchise is selected.");
			}

			if (ModelState.IsValid)
			{
				Guid _id = dataItem.Id;

				try
				{
					Franchise _objToUpdate = null;

					_objToUpdate = MMContext.Franchises
							 .Where(f => f.Id == _id)
									  .FirstOrDefault();

					if (_objToUpdate == null)
					{
						ModelState.AddModelError(string.Empty, $"Franchise [{_id.ToString()}] not found.  Please try again.");
						return JsonFormResponse();
					}

					MMContext.Entry(_objToUpdate).CurrentValues.SetValues(dataItem);
					MMContext.Entry(_objToUpdate).Property("Name").IsModified = false;

					MMContext.SaveChanges();

					return JsonSuccessResponse("Franchise settings saved successfully", _objToUpdate);
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

						if (databaseValues.RoyaltyFeePercentage != clientValues.RoyaltyFeePercentage)
							ModelState.AddModelError("RoyaltyFeePercentage", "Current database value for franchise royalty fee: " + databaseValues.RoyaltyFeePercentage);

						if (databaseValues.ManagementFeePercentage != clientValues.ManagementFeePercentage)
							ModelState.AddModelError("TradingName", "Current database value for franchise management fee: " + databaseValues.ManagementFeePercentage);

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
					//_msg = new InfoViewModel("Error saving franchises", ex);
					ModelState.AddModelError(string.Empty, $"Error saving franchise settings ({ex.Message})");

					LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
					log.Log(LogLevel.Error, "Error saving franchise settings", nameof(SaveFranchise), ex, dataItem);
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(SaveFranchiseSettings), dataItem);
			}

			return JsonFormResponse();
		}

		#endregion
	}
}
