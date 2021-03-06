﻿#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using MagicMaids.EntityModels ;
using MagicMaids.ViewModels; 
using MagicMaids.Validators;

using FluentValidation.Mvc;
using MagicMaids.DataAccess;
using LazyCache;
using System.Data;
using Dapper;
using System.Text;
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

			StringBuilder sql = new StringBuilder(@"select * from Franchises C 
							 	inner join Addresses Ph on C.PhysicalAddressRefId = Ph.ID");

			if (!incDisabled.HasValue || incDisabled == 0)
			{
				sql.Append(" where C.IsActive = 1");
			}
				
			using (DBManager db = new DBManager())
			{
				_data = db.getConnection().Query<Franchise, Address, Franchise>(sql.ToString(), (fr, phys) => {
					fr.PhysicalAddress = phys;
					return fr;
				}).ToList();
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
			List<FranchiseSelectViewModel> _listFranchises = new List<FranchiseSelectViewModel>();

			IAppCache cache = new CachingService();
			_listFranchises = cache.GetOrAdd("Active_Franchises", () => GetActiveFranchisesPrivate(), new TimeSpan(8, 0, 0));

			if (_listFranchises == null || _listFranchises.Count == 0)
			{
				LogHelper log = new LogHelper();
				log.Log(LogHelper.LogLevels.Warning, "Error loading active franchises - Franchise cache will be reset and attempted again", nameof(GetActiveFranchises));

				cache.Remove("Active_Franchises");
				_listFranchises = cache.GetOrAdd("Active_Franchises", () => GetActiveFranchisesPrivate(), new TimeSpan(8, 0, 0));
			}

			if (_listFranchises == null || _listFranchises.Count == 0)
			{
				throw new ApplicationException($"Active franchise list could not be loaded.");
			}

			return new JsonNetResult() { Data = new { list = _listFranchises }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		private List<FranchiseSelectViewModel> GetActiveFranchisesPrivate()
		{
			List<FranchiseSelectViewModel> _listFranchises = new List<FranchiseSelectViewModel>();
			List<Franchise> _data = new List<Franchise>();

			using (DBManager db = new DBManager())
			{
				StringBuilder sql = new StringBuilder(@"select * from Franchises C where IsActive = true");

				_data = db.getConnection().Query<Franchise>(sql.ToString()).ToList();

				List<SystemSetting> _settings = db.getConnection().Query<SystemSetting>("Select * from SystemSettings where IsActive = true").ToList();
				foreach (Franchise _item in _data)
				{
					var _vm = new FranchiseSelectViewModel();
					_vm.PopulateVM(_item, _settings);
					_listFranchises.Add(_vm);
				}
			}	

			return _listFranchises;
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
				_dataItem.Id = Guid.NewGuid().ToString();
				_dataItem.IsNewItem = true;
				_dataItem.PhysicalAddress = new UpdateAddressViewModel() { Id = Guid.NewGuid().ToString(), AddressType = AddressTypeSetting.Physical };
				_dataItem.PhysicalAddressRefId = _dataItem.PhysicalAddress.Id;
			}
			else
			{
				string sql = @"select * from Franchises C 
							 	inner join Addresses Ph on C.PhysicalAddressRefId = Ph.ID
								where C.Id = '" + FranchiseId + "'";

				using (DBManager db = new DBManager())
				{
					_franchise = db.getConnection().Query<Franchise, Address, Franchise>(sql, (clnt, phys) => {
						clnt.PhysicalAddress = phys;
						return clnt;
					}).SingleOrDefault();

				}
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
			using (DBManager db = new DBManager())
			{
				StringBuilder _sql = new StringBuilder();
				_sql.Append("Select * from SystemSettings where IsActive = 1");
				_settings = db.getConnection().Query<SystemSetting>(_sql.ToString()).ToList();

				_sql.Clear();
				_sql.Append($"select * from Franchises where ID = '{FranchiseId}'");
				_franchise = db.getConnection().Query<Franchise>(_sql.ToString()).SingleOrDefault();

				if (_franchise == null)
				{
					ModelState.AddModelError(string.Empty, $"Franchise [{FranchiseId.ToString()}] not found.  Please try again.");
					return JsonFormResponse();
				}

				_dataItem = new FranchiseSettingsVM();
				_dataItem.PopulateVM(_franchise, _settings);

				return new JsonNetResult() { Data = new { item = _dataItem }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
			}
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
				ModelState.AddModelError(string.Empty, "Please ensure at least a valid physical address is supplied.");
			}
			else
			{
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
				String _id = dataItem.Id;
				var bIsNew = (dataItem.IsNewItem );

				//https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/crud
				//https://stackoverflow.com/questions/21286538/asp-net-mvc-5-model-binding-edit-view
				//https://www.mikesdotnetting.com/article/248/mvc-5-with-ef-6-in-visual-basic-updating-related-data

				try
				{
					Franchise _objToUpdate = null;

					using (DBManager db = new DBManager())
					{
						if (bIsNew)
						{
							_objToUpdate = UpdateFranchise(null, dataItem);

							StringBuilder _sql = new StringBuilder();
							if (_objToUpdate.PhysicalAddress != null)
							{
								_sql.Append("Insert into Addresses (Id, CreatedAt, UpdatedAt, UpdatedBy, IsActive, RowVersion, ");
								_sql.Append("AddressType, AddressLine1, AddressLine2, AddressLine3, Suburb, State, PostCode, Country)");
								_sql.Append(" values (");
								_sql.Append($"'{_objToUpdate.PhysicalAddress.Id}',");
								_sql.Append($"'{_objToUpdate.PhysicalAddress.CreatedAt.FormatDatabaseDateTime()}',");
								_sql.Append($"'{_objToUpdate.PhysicalAddress.UpdatedAt.FormatDatabaseDateTime()}',");
								_sql.Append($"'{_objToUpdate.PhysicalAddress.UpdatedBy}',");
								_sql.Append($"{_objToUpdate.PhysicalAddress.IsActive},");
								_sql.Append($"'{_objToUpdate.PhysicalAddress.RowVersion.FormatDatabaseDateTime()}',");
								_sql.Append($"{(int)_objToUpdate.PhysicalAddress.AddressType},");
								_sql.Append($"'{_objToUpdate.PhysicalAddress.AddressLine1}',");
								_sql.Append($"'{_objToUpdate.PhysicalAddress.AddressLine2}',");
								_sql.Append($"'{_objToUpdate.PhysicalAddress.AddressLine3}',");
								_sql.Append($"'{_objToUpdate.PhysicalAddress.Suburb}',");
								_sql.Append($"'{_objToUpdate.PhysicalAddress.State}',");
								_sql.Append($"'{_objToUpdate.PhysicalAddress.PostCode}',");
								_sql.Append($"'{_objToUpdate.PhysicalAddress.Country}'");
								_sql.Append(")");
								db.getConnection().Execute(_sql.ToString());
							}

							_sql.Clear();
							_sql.Append("Insert into Franchises (Id, CreatedAt, UpdatedAt, UpdatedBy, IsActive, RowVersion, ");
							_sql.Append("Name, TradingName, MasterFranchiseCode, EmailAddress, PhysicalAddressRefId, ");
							_sql.Append("BusinessPhoneNumber, MobileNumber, OtherNumber, CodeOfConductURL, ManagementFeePercentage, RoyaltyFeePercentage, MetroRegion)");
							_sql.Append(" values (");
							_sql.Append($"'{_objToUpdate.Id}',");
							_sql.Append($"'{_objToUpdate.CreatedAt.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.UpdatedBy}',");
							_sql.Append($"{_objToUpdate.IsActive},");
							_sql.Append($"'{_objToUpdate.RowVersion.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.Name}',");
							_sql.Append($"'{_objToUpdate.TradingName}',");
							_sql.Append($"'{_objToUpdate.MasterFranchiseCode}',");
							_sql.Append($"'{_objToUpdate.EmailAddress}',");
							_sql.Append($"'{_objToUpdate.PhysicalAddressRefId}',");
							_sql.Append($"'{_objToUpdate.BusinessPhoneNumber}',");
							_sql.Append($"'{_objToUpdate.MobileNumber}',");
							_sql.Append($"'{_objToUpdate.OtherNumber}',");
							_sql.Append($"'{_objToUpdate.CodeOfConductURL}',");
							if (_objToUpdate.ManagementFeePercentage == null)
							{
								_sql.Append($"null,");

							}
							else
							{
								_sql.Append($"{_objToUpdate.ManagementFeePercentage},");
							}
							if (_objToUpdate.RoyaltyFeePercentage == null)
							{
								_sql.Append($"null,");

							}
							else
							{
								_sql.Append($"{_objToUpdate.RoyaltyFeePercentage},");
							}
							_sql.Append($"'{_objToUpdate.MetroRegion}'");
							_sql.Append(")");
							db.getConnection().Execute(_sql.ToString());
							//var newId = db.Insert<Franchise>(UpdateAuditTracking(_objToUpdate));
						}
						else
						{
							string sql = @"select * from Franchises C 
							 	inner join Addresses Ph on C.PhysicalAddressRefId = Ph.ID
								where C.ID = '" + _id + "'";

							_objToUpdate = db.getConnection().Query<Franchise, Address, Franchise>(sql, (clnt, phys) => {
								clnt.PhysicalAddress = phys;
								return clnt;
							}).SingleOrDefault();

							if (_objToUpdate == null)
							{
								ModelState.AddModelError(string.Empty, $"Franchise [{_id.ToString()}] not found.  Please try again.");
								return JsonFormResponse();
							}

							StringBuilder _sql = new StringBuilder();
							_objToUpdate = UpdateFranchise(_objToUpdate, dataItem);

							_sql.Append("Update Franchises set ");
							_sql.Append($"UpdatedAt = '{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}'");
							_sql.Append($",RowVersion = '{_objToUpdate.RowVersion.FormatDatabaseDateTime()}'");
							_sql.Append($",UpdatedBy = '{_objToUpdate.UpdatedBy}'");
							_sql.Append($",IsActive = {_objToUpdate.IsActive}");
							_sql.Append($",Name = '{_objToUpdate.Name}'");
							_sql.Append($",TradingName = '{_objToUpdate.TradingName}'");
							_sql.Append($",MasterFranchiseCode = '{_objToUpdate.MasterFranchiseCode}'");
							_sql.Append($",EmailAddress = '{_objToUpdate.EmailAddress}'");
							_sql.Append($",BusinessPhoneNumber = '{_objToUpdate.BusinessPhoneNumber}'");
							_sql.Append($",MobileNumber = '{_objToUpdate.MobileNumber}'");
							_sql.Append($",OtherNumber = '{_objToUpdate.OtherNumber}'");
							_sql.Append($",CodeOfConductURL = '{_objToUpdate.CodeOfConductURL}'");
							if (_objToUpdate.ManagementFeePercentage != null)
							{
								_sql.Append($",ManagementFeePercentage = {_objToUpdate.ManagementFeePercentage}");
							}
							if (_objToUpdate.RoyaltyFeePercentage != null)
							{
								_sql.Append($",RoyaltyFeePercentage = {_objToUpdate.RoyaltyFeePercentage}");
							}
							_sql.Append($",MetroRegion = '{_objToUpdate.MetroRegion}'");
							_sql.Append($" where Id = '{_objToUpdate.Id}'");
							db.getConnection().Execute(_sql.ToString());

							_sql.Clear();
							_sql.Append("Update Addresses set ");
							_sql.Append($"UpdatedAt = '{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}'");
							_sql.Append($",RowVersion = '{_objToUpdate.RowVersion.FormatDatabaseDateTime()}'");
							_sql.Append($",UpdatedBy = '{_objToUpdate.UpdatedBy}'");
							_sql.Append($",IsActive = {_objToUpdate.IsActive}");
							_sql.Append($",AddressLine1 = '{_objToUpdate.PhysicalAddress.AddressLine1}'");
							_sql.Append($",AddressLine2 = '{_objToUpdate.PhysicalAddress.AddressLine2}'");
							_sql.Append($",AddressLine3 = '{_objToUpdate.PhysicalAddress.AddressLine3}'");
							_sql.Append($",Suburb = '{_objToUpdate.PhysicalAddress.Suburb}'");
							_sql.Append($",State = '{_objToUpdate.PhysicalAddress.State}'");
							_sql.Append($",PostCode = '{_objToUpdate.PhysicalAddress.PostCode}'");
							_sql.Append($",Country = '{_objToUpdate.PhysicalAddress.Country}'");
							_sql.Append($" where Id = '{_objToUpdate.PhysicalAddress.Id}' ");
							db.getConnection().Execute(_sql.ToString());
						}

						IAppCache cache = new CachingService();
						cache.Remove("Active_Franchises");

						return JsonSuccessResponse("Franchise saved successfully", _objToUpdate);
					}
				}
				//catch (DbUpdateConcurrencyException ex)
				//{
				//	var entry = ex.Entries.Single();
				//	var clientValues = (Franchise)entry.Entity;
				//	var databaseEntry = entry.GetDatabaseValues();
				//	if (databaseEntry == null)
				//	{
				//		ModelState.AddModelError(string.Empty, "Unable to save changes. The franchise was deleted by another user.");
				//	}
				//	else
				//	{
				//		var databaseValues = (Franchise)databaseEntry.ToObject();

				//		if (databaseValues.Name  != clientValues.Name)
				//			ModelState.AddModelError("Name", "Current database value for franchise name: " + databaseValues.Name);

				//		if (databaseValues.TradingName != clientValues.TradingName)
				//			ModelState.AddModelError("TradingName", "Current database value for trading name: " + databaseValues.TradingName);

				//		ModelState.AddModelError(string.Empty, "The record you attempted to edit "
				//			+ "was modified by another user after you got the original value. The edit operation "
				//			+ "was canceled. If you still want to edit this record, click the Save button again.");
				//	}
				//}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, Helpers.FormatModelError("Error saving franchise", ex));

					LogHelper log = new LogHelper();
					log.Log(LogHelper.LogLevels.Error, "Error saving franchise", nameof(SaveFranchise), ex, dataItem, Helpers.ParseValidationErrors(ex));
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(ModelState, nameof(SaveFranchise ), dataItem);
			}

			return JsonFormResponse();
		}

		private Franchise UpdateFranchise(Franchise _objToUpdate, UpdateFranchisesViewModel dataItem)
		{

			if (dataItem == null)
			{
				return _objToUpdate;
			}

			if (_objToUpdate == null)
			{
				_objToUpdate = new Franchise();
			}

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

			_objToUpdate = UpdateAuditTracking(_objToUpdate);

			if (dataItem.PhysicalAddress == null)
			{
				_objToUpdate.PhysicalAddress = new Address() { AddressType = AddressTypeSetting.Physical };
				_objToUpdate.PhysicalAddressRefId = _objToUpdate.PhysicalAddress.Id;
			}
			else
			{
				if (_objToUpdate.PhysicalAddress == null)
				{
					_objToUpdate.PhysicalAddress = new Address() { AddressType = AddressTypeSetting.Physical };
					_objToUpdate.PhysicalAddress.Id = dataItem.PhysicalAddress.Id;
					_objToUpdate.PhysicalAddress.CreatedAt = _objToUpdate.CreatedAt;
				}
				_objToUpdate.PhysicalAddress.AddressLine1 = dataItem.PhysicalAddress.AddressLine1;
				_objToUpdate.PhysicalAddress.AddressLine2 = dataItem.PhysicalAddress.AddressLine2;
				_objToUpdate.PhysicalAddress.AddressLine3 = dataItem.PhysicalAddress.AddressLine3;
				_objToUpdate.PhysicalAddress.Suburb = dataItem.PhysicalAddress.Suburb;
				_objToUpdate.PhysicalAddress.Country = dataItem.PhysicalAddress.Country;
				_objToUpdate.PhysicalAddress.IsActive = true;
				_objToUpdate.PhysicalAddress.PostCode = dataItem.PhysicalAddress.PostCode;
				_objToUpdate.PhysicalAddress.State = dataItem.PhysicalAddress.State;
				_objToUpdate.PhysicalAddress.UpdatedBy = _objToUpdate.UpdatedBy;
				_objToUpdate.PhysicalAddress.UpdatedAt = _objToUpdate.UpdatedAt;
				_objToUpdate.PhysicalAddress.RowVersion = _objToUpdate.RowVersion;
				_objToUpdate.PhysicalAddressRefId = _objToUpdate.PhysicalAddress.Id;
			}

			return _objToUpdate;
		}

		[HttpPost]
		public ActionResult SaveFranchiseSettings(FranchiseSettingsVM dataItem)
		{
			//https://stackoverflow.com/questions/13541225/asp-net-mvc-how-to-display-success-confirmation-message-after-server-side-proce

			if (dataItem == null || !Helpers.IsValidGuid(dataItem.Id))
			{
				ModelState.AddModelError(string.Empty, "Valid franchise settings not found. Ensure a valid franchise is selected.");
			}

			if (ModelState.IsValid)
			{
				String _id = dataItem.Id;

				try
				{
					Franchise _objToUpdate = null;
					StringBuilder _sql = new StringBuilder();
						
					using (DBManager db = new DBManager())
					{
						_sql.Append($"select * from Franchises where Id = '{_id}'");
						_objToUpdate = db.getConnection().Query<Franchise>(_sql.ToString()).SingleOrDefault();

						if (_objToUpdate == null)
						{
							ModelState.AddModelError(string.Empty, $"Franchise [{_id.ToString()}] not found.  Please try again.");
							return JsonFormResponse();
						}

						_objToUpdate.ManagementFeePercentage = dataItem.ManagementFeePercentage;
						_objToUpdate.RoyaltyFeePercentage = dataItem.RoyaltyFeePercentage;

						_sql.Clear();
						_sql.Append("Update Franchises set ");
						_sql.Append($"UpdatedAt = '{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}'");
						_sql.Append($",RowVersion = '{_objToUpdate.RowVersion.FormatDatabaseDateTime()}'");
						_sql.Append($",UpdatedBy = '{_objToUpdate.UpdatedBy}'");
						_sql.Append($",ManagementFeePercentage = {_objToUpdate.ManagementFeePercentage}");
						_sql.Append($",RoyaltyFeePercentage = {_objToUpdate.RoyaltyFeePercentage}");
						_sql.Append($" where Id = '{_objToUpdate.Id}' ");
						db.getConnection().Execute(_sql.ToString());
					}
					return JsonSuccessResponse("Franchise settings saved successfully", _objToUpdate);
				}
				//catch (DbUpdateConcurrencyException ex)
				//{
				//	var entry = ex.Entries.Single();
				//	var clientValues = (Franchise)entry.Entity;
				//	var databaseEntry = entry.GetDatabaseValues();
				//	if (databaseEntry == null)
				//	{
				//		ModelState.AddModelError(string.Empty, "Unable to save changes. The franchise was deleted by another user.");
				//	}
				//	else
				//	{
				//		var databaseValues = (Franchise)databaseEntry.ToObject();

				//		if (databaseValues.RoyaltyFeePercentage != clientValues.RoyaltyFeePercentage)
				//			ModelState.AddModelError("RoyaltyFeePercentage", "Current database value for franchise royalty fee: " + databaseValues.RoyaltyFeePercentage);

				//		if (databaseValues.ManagementFeePercentage != clientValues.ManagementFeePercentage)
				//			ModelState.AddModelError("TradingName", "Current database value for franchise management fee: " + databaseValues.ManagementFeePercentage);

				//		ModelState.AddModelError(string.Empty, "The record you attempted to edit "
				//			+ "was modified by another user after you got the original value. The edit operation "
				//			+ "was canceled. If you still want to edit this record, click the Save button again.");
				//	}
				//}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, Helpers.FormatModelError("Error saving franchise settings", ex));

					LogHelper log = new LogHelper();
					log.Log(LogHelper.LogLevels.Error, "Error saving franchise settings", nameof(SaveFranchise), ex, dataItem, Helpers.ParseValidationErrors(ex));
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(ModelState, nameof(SaveFranchiseSettings), dataItem);
			}

			return JsonFormResponse();
		}

		#endregion
	}
}
