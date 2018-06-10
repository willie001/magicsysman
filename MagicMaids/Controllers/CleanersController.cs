#region Using
using MagicMaids.DataAccess;
using MagicMaids.EntityModels;
using MagicMaids.Validators;
using MagicMaids.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using FluentValidation.Mvc;

using AutoMapper;
using System.Data;
using Dapper;
using System.Text;
using NodaTime;
using NodaTime.Extensions;
#endregion

namespace MagicMaids.Controllers
{
	public class CleanersController : BaseController
	{
		// todo - get dependency injection
		//http://www.dotnetcurry.com/aspnet-mvc/1155/aspnet-mvc-repository-pattern-perform-database-operations
		#region Fields
		#endregion

		#region Constructors
		public CleanersController() : base()
		{
			
		}
		#endregion

		#region Properties, Public
		public ActionResult Cleaners()
		{
			return View();
		}

		public ActionResult CleanerDetails()
		{
			return View();
		}

		public ActionResult CleanerAvailability()
		{
			return View();
		}
		#endregion

		#region Service Functions
		[HttpGet]
		public ActionResult GetCleaner(Guid? CleanerId)
		{
			//https://msdn.microsoft.com/en-us/data/jj574232.aspx
			Cleaner _cleaner = null;
			Franchise _franchise = null;
			CleanerDetailsVM _dataItem = null;
			FranchiseSelectViewModel _selectedFranchise = null;
			List<SystemSetting> _settings = new List<SystemSetting>();
					
			if (CleanerId == null)
			{
				// create new item
				_dataItem = new CleanerDetailsVM();
				_dataItem.IsNewItem = true;
				_dataItem.Id = Guid.NewGuid().ToString();
				_dataItem.PhysicalAddress = new UpdateAddressViewModel() { Id = Guid.NewGuid().ToString(), AddressType = AddressTypeSetting.Physical };
				_dataItem.PostalAddress = new UpdateAddressViewModel() { Id = Guid.NewGuid().ToString(), AddressType = AddressTypeSetting.Postal };
				_dataItem.PhysicalAddressRefId = _dataItem.PhysicalAddress.Id;
				_dataItem.PostalAddressRefId = _dataItem.PostalAddress.Id;
			}
			else
			{
				string sql = @"select * from Cleaners C 
							 	inner join Addresses Ph on C.PhysicalAddressRefId = Ph.ID
								inner join Addresses Po on C.PostalAddressRefId = Po.ID
								where C.ID = '" + CleanerId + "'";

				using (DBManager db = new DBManager())
				{
					_cleaner = db.getConnection().Query<Cleaner, Address, Address, Cleaner>(sql, (clnr, phys, post) => {
						clnr.PhysicalAddress = phys;
						clnr.PostalAddress = post;
						return clnr;
					}).SingleOrDefault();

					if (!_cleaner.MasterFranchiseRefId.Equals(Guid.Empty))
					{
						sql = @"select * from Franchises where ID = '" + _cleaner.MasterFranchiseRefId + "'";
						_franchise = db.getConnection().Query<Franchise>(sql).SingleOrDefault();
					}

					sql = "select * from SystemSettings where IsActive = 1";
					_settings = db.getConnection().Query<SystemSetting>(sql).ToList();
				}

				if (_cleaner == null)
				{
					ModelState.AddModelError(string.Empty, $"Cleaner [{CleanerId.ToString()}] not found.  Please try again.");
					return JsonFormResponse();
				}

				_dataItem = new CleanerDetailsVM();
				_dataItem.PopulateVM(_cleaner);
				_dataItem.IsNewItem = false;

				if (_franchise == null)
				{
					ModelState.AddModelError(string.Empty, $"Cleaner's [{CleanerId.ToString()}] Master Franchise not found not found.  Please try again.");
					return JsonFormResponse();
				}

				_selectedFranchise = new FranchiseSelectViewModel();
				_selectedFranchise.PopulateVM(_franchise, _settings);
			}

			return new JsonNetResult() { Data = new { item = _dataItem, selectedFranchise = _selectedFranchise }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpGet]
		public JsonNetResult GetNextCleanerCode()
		{
			Int32 _cleanerCode = 1000; // starting value
			Int32? _match = null;
			using (DBManager db = new DBManager())
			{
				string sql = "select max(cleanerCode) NextCode from Cleaners";
				var rows = db.getConnection().Query(sql).ToList();
				_match = rows[0]?.NextCode;
			}

			if (_match.HasValue)
			{
				_cleanerCode = (Int32)_match + 1;
			}

			return new JsonNetResult() { Data = new { item = _cleanerCode }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpGet]
		public ActionResult GetCleanerTeam(Guid? CleanerId)
		{
			//https://msdn.microsoft.com/en-us/data/jj574232.aspx
			List<TeamMemberDetailsVM> _teamList = new List<TeamMemberDetailsVM>();
			List<CleanerTeam> _team;

			if (CleanerId == null)
			{
				ModelState.AddModelError(string.Empty, $"Cleaner Id [{CleanerId.ToString()}] not provided.  Please try again.");
				return JsonFormResponse();
			}

			string sql = @"select * from CleanerTeam CT 
							 	inner join Addresses Ph on CT.PhysicalAddressRefId = Ph.ID
								inner join Addresses Po on CT.PostalAddressRefId = Po.ID
								where CT.PrimaryCleanerRefId = '" + CleanerId + "'";

			using (DBManager db = new DBManager())
			{
				_team = db.getConnection().Query<CleanerTeam, Address, Address, CleanerTeam>(sql, (clnr, phys, post) => {
					clnr.PhysicalAddress = phys;
					clnr.PostalAddress = post;
					return clnr;
				}).ToList();
			}

			if (_team != null && _team.Count() > 0)
			{
				foreach (var _member in _team)
				{
					var _nextMember = new TeamMemberDetailsVM();
					_nextMember.PopulateVM(CleanerId, _member);
					_teamList.Add(_nextMember);
				}
			}

			return new JsonNetResult() { Data = new { list = _teamList, teamSize = _team.Count() + 1 }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpGet]
		public JsonResult GetFranchiseZonesJson(Guid? FranchiseId)
		{
			var franchiseID = (FranchiseId.HasValue) ? FranchiseId.Value.ToString() : "";
			var item = SettingsController.GetZoneListByFranchise(franchiseID, false); 
			return new JsonNetResult() { Data = new { item }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}


		[HttpPost]
		public ActionResult SaveCleanerDetails(CleanerDetailsVM dataItem)
		{
			//https://stackoverflow.com/questions/13541225/asp-net-mvc-how-to-display-success-confirmation-message-after-server-side-proce

			if (dataItem == null)
			{
				ModelState.AddModelError(string.Empty, "Valid cleaner data not found.");
			}

			if (!dataItem.HasAnyPhoneNumbers)
			{
				ModelState.AddModelError(string.Empty, "Please provide at least one valid phone number.");
			}

			if (!Helpers.IsValidGuid(dataItem.MasterFranchiseRefId))
			{
				ModelState.AddModelError(string.Empty, "Please select cleaner's master franchise.");
			}

			if (!dataItem.HasAnyValidAddress)
			{
				ModelState.AddModelError(string.Empty, "Please ensure at least a valid postal or physical address is supplied.");
			}
			else
			{
				if (dataItem.PostalAddress != null && (dataItem.PostalAddress.IsPartialAddress || dataItem.HasValidPostalAddress))
				{
					var _postalValidator = new AddressValidator();
					var results = _postalValidator.Validate(dataItem.PostalAddress);
					results.AddToModelState(ModelState, "");
				}

				if (dataItem.PhysicalAddress != null && (dataItem.PhysicalAddress.IsPartialAddress || dataItem.HasValidPhysicalAddress))
				{
					var _physValidator = new AddressValidator();
					var results = _physValidator.Validate(dataItem.PhysicalAddress);
					results.AddToModelState(ModelState, "");
				}
			}

			StringBuilder _sql = new StringBuilder();

			if (ModelState.IsValid)
			{
				if (!String.IsNullOrWhiteSpace(dataItem.PrimaryZone) && (dataItem.PrimaryZoneList == null || dataItem.PrimaryZoneList.Count() == 0))
				{
					dataItem.PrimaryZoneList = new List<string>(new string[] { dataItem.PrimaryZone });
				}

				// check zones are valid and active
				if (dataItem.PrimaryZoneList == null || dataItem.PrimaryZoneList.Count() == 0 || dataItem.SecondaryZoneList == null || dataItem.SecondaryZoneList.Count() == 0)
				{
					ModelState.AddModelError(string.Empty, "Primary zone and secondary zones are required.");
				}
				else if (dataItem.PrimaryZoneList.Except(dataItem.SecondaryZoneList).Count() == 0)
				{
					ModelState.AddModelError("", "Secondary zone should not contain the primary zone.");
				}
				else if (!String.IsNullOrWhiteSpace(dataItem.ApprovedZone) && dataItem.PrimaryZoneList.Except(dataItem.ApprovedZoneList).Count() == 0)
				{
					ModelState.AddModelError("", "Approved zone should not contain the primary zone.");
				}
				else
				{

					List<String> _matchList = SettingsController.GetZoneListByFranchise(dataItem.MasterFranchiseRefId, true);

					var _missingItems = dataItem.SecondaryZoneList.Select(x => x.ToLower()).Except(_matchList);
					if (_missingItems.Count() > 0)
					{
						ModelState.AddModelError("", $"The following secondary zones have not been defined for current franchise ({String.Join(",", _missingItems)}).");
					}

					_missingItems = dataItem.ApprovedZoneList.Select(x => x.ToLower()).Except(_matchList);
					if (_missingItems.Count() > 0)
					{
						ModelState.AddModelError("", $"The following approved zones have not been defined for current franchise ({String.Join(",", _missingItems)}).");
					}
				}

				// put list into comma list for saving
				if (ModelState.IsValid)
				{
					// for some reason single item wants text and list wants collection
					// if we use SecondaryZone to bind to ui-select it adds commas.
					// need to revisit !!!
					dataItem.SecondaryZone = String.Join(",", dataItem.SecondaryZoneList);
					if (dataItem.ApprovedZoneList != null)
						dataItem.ApprovedZone = String.Join(",", dataItem.ApprovedZoneList);
				}

			}

			if (ModelState.IsValid)
			{
				String _id = dataItem.Id;
				var bIsNew = (dataItem.IsNewItem);

				//https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/crud
				//https://stackoverflow.com/questions/21286538/asp-net-mvc-5-model-binding-edit-view
				//https://www.mikesdotnetting.com/article/248/mvc-5-with-ef-6-in-visual-basic-updating-related-data

				try
				{
					Cleaner _objToUpdate = null;

					using (DBManager db = new DBManager())
					{
						if (bIsNew)
						{
							_objToUpdate = UpdateCleaner(null, dataItem);
							//var newId = db.Insert(UpdateAuditTracking(_objToUpdate)); 

							if (_objToUpdate.PhysicalAddress != null)
							{
								_sql.Clear();
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
								dataItem.SqlString = _sql.ToString();
								db.getConnection().Execute(_sql.ToString());
							}

							if (_objToUpdate.PostalAddress != null)
							{
								_sql.Clear();
								_sql.Append("Insert into Addresses (Id, CreatedAt, UpdatedAt, UpdatedBy, IsActive, RowVersion, ");
								_sql.Append("AddressType, AddressLine1, AddressLine2, AddressLine3, Suburb, State, PostCode, Country)");
								_sql.Append(" values (");
								_sql.Append($"'{_objToUpdate.PostalAddress.Id}',");
								_sql.Append($"'{_objToUpdate.PostalAddress.CreatedAt.FormatDatabaseDateTime()}',");
								_sql.Append($"'{_objToUpdate.PostalAddress.UpdatedAt.FormatDatabaseDateTime()}',");
								_sql.Append($"'{_objToUpdate.PostalAddress.UpdatedBy}',");
								_sql.Append($"{_objToUpdate.PostalAddress.IsActive},");
								_sql.Append($"'{_objToUpdate.PostalAddress.RowVersion.FormatDatabaseDateTime()}',");
								_sql.Append($"{(int)_objToUpdate.PostalAddress.AddressType},");
								_sql.Append($"'{_objToUpdate.PostalAddress.AddressLine1}',");
								_sql.Append($"'{_objToUpdate.PostalAddress.AddressLine2}',");
								_sql.Append($"'{_objToUpdate.PostalAddress.AddressLine3}',");
								_sql.Append($"'{_objToUpdate.PostalAddress.Suburb}',");
								_sql.Append($"'{_objToUpdate.PostalAddress.State}',");
								_sql.Append($"'{_objToUpdate.PostalAddress.PostCode}',");
								_sql.Append($"'{_objToUpdate.PostalAddress.Country}'");
								_sql.Append(")");
								dataItem.SqlString = _sql.ToString();
								db.getConnection().Execute(_sql.ToString());
							}

							_sql.Clear();
							_sql.Append("Insert into Cleaners (Id, CreatedAt, UpdatedAt, UpdatedBy, IsActive, RowVersion, ");
							_sql.Append("CleanerCode, Initials, FirstName, LastName, Rating, MasterFranchiseRefId, ");
							_sql.Append("EmailAddress, PhysicalAddressRefId, PostalAddressRefId, BusinessPhoneNumber, MobileNumber, OtherNumber, Region, ");
							_sql.Append("GenderFlag, Ironing, PrimaryZone, SecondaryZone, ApprovedZone)");
							_sql.Append(" values (");
							_sql.Append($"'{_objToUpdate.Id}',");
							_sql.Append($"'{_objToUpdate.CreatedAt.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.UpdatedBy}',");
							_sql.Append($"{_objToUpdate.IsActive},");
							_sql.Append($"'{_objToUpdate.RowVersion.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.CleanerCode}',");
							_sql.Append($"'{_objToUpdate.Initials}',");
							_sql.Append($"'{_objToUpdate.FirstName}',");
							_sql.Append($"'{_objToUpdate.LastName}',");
							_sql.Append($"{_objToUpdate.Rating},");
							_sql.Append($"'{_objToUpdate.MasterFranchiseRefId}',");
							_sql.Append($"'{_objToUpdate.EmailAddress}',");
							_sql.Append($"'{_objToUpdate.PhysicalAddressRefId}',");
							_sql.Append($"'{_objToUpdate.PostalAddressRefId}',");
							_sql.Append($"'{_objToUpdate.BusinessPhoneNumber}',");
							_sql.Append($"'{_objToUpdate.MobileNumber}',");
							_sql.Append($"'{_objToUpdate.OtherNumber}',");
							_sql.Append($"'{_objToUpdate.Region}',");
							_sql.Append($"'{_objToUpdate.GenderFlag}',");
							_sql.Append($"'{_objToUpdate.Ironing}',");
							_sql.Append($"'{_objToUpdate.PrimaryZone}',");
							_sql.Append($"'{_objToUpdate.SecondaryZone}',");
							_sql.Append($"'{_objToUpdate.ApprovedZone}'");
							_sql.Append(")");
							dataItem.SqlString = _sql.ToString();
							db.getConnection().Execute(_sql.ToString());
						}
						else
						{
							string sql = @"select * from Cleaners C 
							 	inner join Addresses Ph on C.PhysicalAddressRefId = Ph.ID
								inner join Addresses Po on C.PostalAddressRefId = Po.ID
								where C.ID = '" + _id + "'";

							_objToUpdate = db.getConnection().Query<Cleaner, Address, Address, Cleaner>(sql, (clnr, phys, post) => {
									clnr.PhysicalAddress = phys;
									clnr.PostalAddress = post;
									return clnr;
								}).SingleOrDefault();


							if (_objToUpdate == null)
							{
								ModelState.AddModelError(string.Empty, $"Cleaner [{_id.ToString()}] not found.  Please try again.");
								return JsonFormResponse();
							}

							_objToUpdate = UpdateCleaner(_objToUpdate, dataItem);

							_sql.Clear();
							_sql.Append("Update Cleaners set ");
							_sql.Append($"UpdatedAt = '{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}'");
							_sql.Append($",RowVersion = '{_objToUpdate.RowVersion.FormatDatabaseDateTime()}'");
							_sql.Append($",UpdatedBy = '{_objToUpdate.UpdatedBy}'");
							_sql.Append($",IsActive = {_objToUpdate.IsActive}");
							_sql.Append($",CleanerCode = '{_objToUpdate.CleanerCode}'");
							_sql.Append($",Initials = '{_objToUpdate.Initials}'");
							_sql.Append($",FirstName = '{_objToUpdate.FirstName}'");
							_sql.Append($",LastName = '{_objToUpdate.LastName}'");
							_sql.Append($",Rating = {_objToUpdate.Rating}");
							_sql.Append($",MasterFranchiseRefId = '{_objToUpdate.MasterFranchiseRefId}'");
							_sql.Append($",EmailAddress = '{_objToUpdate.EmailAddress}'");
							_sql.Append($",BusinessPhoneNumber = '{_objToUpdate.BusinessPhoneNumber}'");
							_sql.Append($",MobileNumber = '{_objToUpdate.MobileNumber}'");
							_sql.Append($",OtherNumber = '{_objToUpdate.OtherNumber}'");
							_sql.Append($",Region = '{_objToUpdate.Region}'");
							_sql.Append($",GenderFlag = '{_objToUpdate.GenderFlag}'");
							_sql.Append($",Ironing = '{_objToUpdate.Ironing}'");
							_sql.Append($",PrimaryZone = '{_objToUpdate.PrimaryZone}'");
							_sql.Append($",SecondaryZone = '{_objToUpdate.SecondaryZone}'");
							_sql.Append($",ApprovedZone = '{_objToUpdate.ApprovedZone}'");
							_sql.Append($" where Id = '{_objToUpdate.Id}'");
							dataItem.SqlString = _sql.ToString();
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
							dataItem.SqlString = _sql.ToString();
							db.getConnection().Execute(_sql.ToString());

							_sql.Clear();
							_sql.Append("Update Addresses set ");
							_sql.Append($"UpdatedAt = '{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}'");
							_sql.Append($",RowVersion = '{_objToUpdate.RowVersion.FormatDatabaseDateTime()}'");
							_sql.Append($",UpdatedBy = '{_objToUpdate.UpdatedBy}'");
							_sql.Append($",IsActive = {_objToUpdate.IsActive}");
							_sql.Append($",AddressLine1 = '{_objToUpdate.PostalAddress.AddressLine1}'");
							_sql.Append($",AddressLine2 = '{_objToUpdate.PostalAddress.AddressLine2}'");
							_sql.Append($",AddressLine3 = '{_objToUpdate.PostalAddress.AddressLine3}'");
							_sql.Append($",Suburb = '{_objToUpdate.PostalAddress.Suburb}'");
							_sql.Append($",State = '{_objToUpdate.PostalAddress.State}'");
							_sql.Append($",PostCode = '{_objToUpdate.PostalAddress.PostCode}'");
							_sql.Append($",Country = '{_objToUpdate.PostalAddress.Country}'");
							_sql.Append($" where Id = '{_objToUpdate.PostalAddress.Id}' ");
							dataItem.SqlString = _sql.ToString();
							db.getConnection().Execute(_sql.ToString());
						}
					}
					return JsonSuccessResponse("Cleaner saved successfully", _objToUpdate);
				}
				//catch (DbUpdateConcurrencyException ex)
				//{
				//	var entry = ex.Entries.Single();
				//	var clientValues = (Cleaner)entry.Entity;
				//	var databaseEntry = entry.GetDatabaseValues();
				//	if (databaseEntry == null)
				//	{
				//		ModelState.AddModelError(string.Empty, "Unable to save changes. The cleaner was deleted by another user.");
				//	}
				//	else
				//	{
				//		var databaseValues = (Cleaner)databaseEntry.ToObject();

				//		if (databaseValues.CleanerCode != clientValues.CleanerCode)
				//			ModelState.AddModelError("CleanerCode", "Current database value for cleaner code: " + databaseValues.CleanerCode);

				//		if (databaseValues.FirstName != clientValues.FirstName)
				//			ModelState.AddModelError("FirstName", "Current database value for cleaner first name: " + databaseValues.FirstName);

				//		if (databaseValues.LastName != clientValues.LastName)
				//			ModelState.AddModelError("LastName", "Current database value for cleaner surname: " + databaseValues.LastName);

				//		ModelState.AddModelError(string.Empty, "The record you attempted to edit "
				//			+ "was modified by another user after you got the original value. The edit operation "
				//			+ "was canceled. If you still want to edit this record, click the Save button again.");
				//	}
				//}
				catch (Exception ex)
				{
					//_msg = new InfoViewModel("Error saving cleaner", ex);
					ModelState.AddModelError(string.Empty, $"Error saving cleaner ({ex.Message})");

					LogHelper log = new LogHelper();
					log.Log(LogHelper.LogLevels.Error, "Error saving cleaner", nameof(SaveCleanerDetails), ex, dataItem);
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(ModelState, nameof(SaveCleanerDetails), dataItem);
			}

			return JsonFormResponse();
		}

		private Cleaner UpdateCleaner(Cleaner _objToUpdate, CleanerDetailsVM dataItem)
		{
			
			if (dataItem == null)
			{
				return _objToUpdate;
			}

			if (_objToUpdate == null)
			{
				_objToUpdate = new Cleaner();
			}

			_objToUpdate.CleanerCode = dataItem.CleanerCode;
			_objToUpdate.Initials = dataItem.Initials;
			_objToUpdate.FirstName = dataItem.FirstName;
			_objToUpdate.LastName = dataItem.LastName;
			_objToUpdate.EmailAddress = dataItem.EmailAddress;
			_objToUpdate.IsActive = dataItem.IsActive;
			_objToUpdate.MobileNumber = dataItem.MobileNumber;
			_objToUpdate.OtherNumber = dataItem.OtherNumber;
			_objToUpdate.BusinessPhoneNumber = dataItem.BusinessPhoneNumber;
			_objToUpdate.Region = dataItem.Region;
			_objToUpdate.MasterFranchiseRefId = dataItem.MasterFranchiseRefId.ToString();
			_objToUpdate.Rating = dataItem.Rating;
			_objToUpdate.GenderFlag = dataItem.GenderFlag;
			_objToUpdate.Ironing = dataItem.Ironing;
			_objToUpdate.PrimaryZone = dataItem.PrimaryZone;
			_objToUpdate.SecondaryZone = dataItem.SecondaryZone;
			_objToUpdate.ApprovedZone = dataItem.ApprovedZone;

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


			if (dataItem.PostalAddress == null)
			{
				_objToUpdate.PostalAddress = new Address() { AddressType = AddressTypeSetting.Postal };
				_objToUpdate.PostalAddressRefId = _objToUpdate.PostalAddress.Id;
			}
			else
			{
				if (_objToUpdate.PostalAddress == null)
				{
					_objToUpdate.PostalAddress = new Address() { AddressType = AddressTypeSetting.Postal };
					_objToUpdate.PostalAddress.Id = dataItem.PostalAddress.Id;
					_objToUpdate.PostalAddress.CreatedAt = _objToUpdate.CreatedAt;
				}
				_objToUpdate.PostalAddress.AddressLine1 = dataItem.PostalAddress.AddressLine1;
				_objToUpdate.PostalAddress.AddressLine2 = dataItem.PostalAddress.AddressLine2;
				_objToUpdate.PostalAddress.AddressLine3 = dataItem.PostalAddress.AddressLine3;
				_objToUpdate.PostalAddress.Suburb = dataItem.PostalAddress.Suburb;
				_objToUpdate.PostalAddress.Country = dataItem.PostalAddress.Country;
				_objToUpdate.PostalAddress.IsActive = true;
				_objToUpdate.PostalAddress.PostCode = dataItem.PostalAddress.PostCode;
				_objToUpdate.PostalAddress.State = dataItem.PostalAddress.State;
				_objToUpdate.PostalAddress.UpdatedBy = _objToUpdate.UpdatedBy;
				_objToUpdate.PostalAddress.UpdatedAt = _objToUpdate.UpdatedAt;
				_objToUpdate.PostalAddress.RowVersion = _objToUpdate.RowVersion;
				_objToUpdate.PostalAddressRefId = _objToUpdate.PostalAddress.Id;
			}

			return _objToUpdate;
		}

		private CleanerTeam UpdateCleanerTeam(CleanerTeam _objToUpdate, TeamMemberDetailsVM dataItem)
		{

			if (dataItem == null)
			{
				return _objToUpdate;
			}

			if (_objToUpdate == null)
			{
				_objToUpdate = new CleanerTeam();
			}

			_objToUpdate.FirstName = dataItem.FirstName;
			_objToUpdate.LastName = dataItem.LastName;
			_objToUpdate.EmailAddress = dataItem.EmailAddress;
			_objToUpdate.IsActive = dataItem.IsActive;
			_objToUpdate.MobileNumber = dataItem.MobileNumber;
			_objToUpdate.GenderFlag = dataItem.GenderFlag;
			_objToUpdate.Ironing = dataItem.Ironing;
			_objToUpdate.PrimaryCleanerRefId = dataItem.PrimaryCleanerRefId;

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
					_objToUpdate.PhysicalAddress.Id = Guid.NewGuid().ToString();   // can't use copied address from primary cleaner
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


			if (dataItem.PostalAddress == null)
			{
				_objToUpdate.PostalAddress = new Address() { AddressType = AddressTypeSetting.Postal };
				_objToUpdate.PostalAddressRefId = _objToUpdate.PostalAddress.Id;
			}
			else
			{
				if (_objToUpdate.PostalAddress == null)
				{
					_objToUpdate.PostalAddress = new Address() { AddressType = AddressTypeSetting.Postal };
					_objToUpdate.PostalAddress.Id = Guid.NewGuid().ToString();   // can't use copied address from primary cleaner
					_objToUpdate.PostalAddress.CreatedAt = _objToUpdate.CreatedAt;
				}
				_objToUpdate.PostalAddress.AddressLine1 = dataItem.PostalAddress.AddressLine1;
				_objToUpdate.PostalAddress.AddressLine2 = dataItem.PostalAddress.AddressLine2;
				_objToUpdate.PostalAddress.AddressLine3 = dataItem.PostalAddress.AddressLine3;
				_objToUpdate.PostalAddress.Suburb = dataItem.PostalAddress.Suburb;
				_objToUpdate.PostalAddress.Country = dataItem.PostalAddress.Country;
				_objToUpdate.PostalAddress.IsActive = true;
				_objToUpdate.PostalAddress.PostCode = dataItem.PostalAddress.PostCode;
				_objToUpdate.PostalAddress.State = dataItem.PostalAddress.State;
				_objToUpdate.PostalAddress.UpdatedBy = _objToUpdate.UpdatedBy;
				_objToUpdate.PostalAddress.UpdatedAt = _objToUpdate.UpdatedAt;
				_objToUpdate.PostalAddress.RowVersion = _objToUpdate.RowVersion;
				_objToUpdate.PostalAddressRefId = _objToUpdate.PostalAddress.Id;
			}

			return _objToUpdate;
		}

		[HttpPost]
		public ActionResult SaveTeamMember(TeamMemberDetailsVM dataItem)
		{
			//https://stackoverflow.com/questions/13541225/asp-net-mvc-how-to-display-success-confirmation-message-after-server-side-proce

			if (dataItem == null)
			{
				ModelState.AddModelError(string.Empty, "Valid team member data not found.");
			}

			if (!dataItem.HasAnyPhoneNumbers)
			{
				ModelState.AddModelError(string.Empty, "Please provide at valid mobile number.");
			}

			if (dataItem.PrimaryCleanerRefId.Equals(Guid.Empty))
			{
				ModelState.AddModelError(string.Empty, "Team member does not have a primary cleaner references.");
			}

			if (!dataItem.HasAnyValidAddress)
			{
				ModelState.AddModelError(string.Empty, "Please ensure at least a valid postal or physical address is supplied.");
			}
			else
			{
				if (dataItem.PostalAddress != null && (dataItem.PostalAddress.IsPartialAddress || dataItem.HasValidPostalAddress))
				{
					var _postalValidator = new AddressValidator();
					var results = _postalValidator.Validate(dataItem.PostalAddress);
					results.AddToModelState(ModelState, "");
				}

				if (dataItem.PhysicalAddress != null && (dataItem.PhysicalAddress.IsPartialAddress || dataItem.HasValidPhysicalAddress))
				{
					var _physValidator = new AddressValidator();
					var results = _physValidator.Validate(dataItem.PhysicalAddress);
					results.AddToModelState(ModelState, "");
				}
			}

			StringBuilder _sql = new StringBuilder();
							
			if (ModelState.IsValid)
			{
				String _id = dataItem.Id;
				var bIsNew = (dataItem.IsNewItem);

				//https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/crud
				//https://stackoverflow.com/questions/21286538/asp-net-mvc-5-model-binding-edit-view
				//https://www.mikesdotnetting.com/article/248/mvc-5-with-ef-6-in-visual-basic-updating-related-data

				try
				{
					CleanerTeam _objToUpdate = null;

					using (DBManager db = new DBManager())
					{
						if (bIsNew)
						{
							_objToUpdate = UpdateCleanerTeam(null, dataItem);
							//var newId = db.Insert(_objToUpdate); 

							if (_objToUpdate.PhysicalAddress != null)
							{
								_sql.Clear();
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
								dataItem.SqlString = _sql.ToString();
								db.getConnection().Execute(_sql.ToString());
							}

							if (_objToUpdate.PostalAddress != null)
							{
								_sql.Clear();
								_sql.Append("Insert into Addresses (Id, CreatedAt, UpdatedAt, UpdatedBy, IsActive, RowVersion, ");
								_sql.Append("AddressType, AddressLine1, AddressLine2, AddressLine3, Suburb, State, PostCode, Country)");
								_sql.Append(" values (");
								_sql.Append($"'{_objToUpdate.PostalAddress.Id}',");
								_sql.Append($"'{_objToUpdate.PostalAddress.CreatedAt.FormatDatabaseDateTime()}',");
								_sql.Append($"'{_objToUpdate.PostalAddress.UpdatedAt.FormatDatabaseDateTime()}',");
								_sql.Append($"'{_objToUpdate.PostalAddress.UpdatedBy}',");
								_sql.Append($"{_objToUpdate.PostalAddress.IsActive},");
								_sql.Append($"'{_objToUpdate.PostalAddress.RowVersion.FormatDatabaseDateTime()}',");
								_sql.Append($"{(int)_objToUpdate.PostalAddress.AddressType},");
								_sql.Append($"'{_objToUpdate.PostalAddress.AddressLine1}',");
								_sql.Append($"'{_objToUpdate.PostalAddress.AddressLine2}',");
								_sql.Append($"'{_objToUpdate.PostalAddress.AddressLine3}',");
								_sql.Append($"'{_objToUpdate.PostalAddress.Suburb}',");
								_sql.Append($"'{_objToUpdate.PostalAddress.State}',");
								_sql.Append($"'{_objToUpdate.PostalAddress.PostCode}',");
								_sql.Append($"'{_objToUpdate.PostalAddress.Country}'");
								_sql.Append(")");
								dataItem.SqlString = _sql.ToString();
								db.getConnection().Execute(_sql.ToString());
							}

							_sql.Clear();
							_sql.Append("Insert into CleanerTeam (Id, CreatedAt, UpdatedAt, UpdatedBy, IsActive, RowVersion, ");
							_sql.Append("PrimaryCleanerRefId, FirstName, LastName, ");
							_sql.Append("EmailAddress, PhysicalAddressRefId, PostalAddressRefId, MobileNumber, ");
							_sql.Append("GenderFlag, Ironing)");
							_sql.Append(" values (");
							_sql.Append($"'{_objToUpdate.Id}',");
							_sql.Append($"'{_objToUpdate.CreatedAt.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.UpdatedBy}',");
							_sql.Append($"{_objToUpdate.IsActive},");
							_sql.Append($"'{_objToUpdate.RowVersion.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.PrimaryCleanerRefId}',");
							_sql.Append($"'{_objToUpdate.FirstName}',");
							_sql.Append($"'{_objToUpdate.LastName}',");
							_sql.Append($"'{_objToUpdate.EmailAddress}',");
							_sql.Append($"'{_objToUpdate.PhysicalAddressRefId}',");
							_sql.Append($"'{_objToUpdate.PostalAddressRefId}',");
							_sql.Append($"'{_objToUpdate.MobileNumber}',");
							_sql.Append($"'{_objToUpdate.GenderFlag}',");
							_sql.Append($"'{_objToUpdate.Ironing}'");
							_sql.Append(")");
							dataItem.SqlString = _sql.ToString();
							db.getConnection().Execute(_sql.ToString());
						}
						else
						{
							string sql = @"select * from CleanerTeam C 
							 	inner join Addresses Ph on C.PhysicalAddressRefId = Ph.ID
								inner join Addresses Po on C.PostalAddressRefId = Po.ID
								where C.ID = '" + _id + "'";

							_objToUpdate = db.getConnection().Query<CleanerTeam, Address, Address, CleanerTeam>(sql, (clnr, phys, post) => {
								clnr.PhysicalAddress = phys;
								clnr.PostalAddress = post;
								return clnr;
							}).SingleOrDefault();


							if (_objToUpdate == null)
							{
								ModelState.AddModelError(string.Empty, $"Team member [{_id.ToString()}] not found.  Please try again.");
								return JsonFormResponse();
							}


							// no action on team member yet until it's deleted
							_objToUpdate = UpdateCleanerTeam(_objToUpdate, dataItem);

							_sql.Clear();
							_sql.Append("Update CleanerTeam set ");
							_sql.Append($"UpdatedAt = '{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}'");
							_sql.Append($",RowVersion = '{_objToUpdate.RowVersion.FormatDatabaseDateTime()}'");
							_sql.Append($",UpdatedBy = '{_objToUpdate.UpdatedBy}'");
							_sql.Append($",IsActive = {_objToUpdate.IsActive}");
							_sql.Append($",FirstName = '{_objToUpdate.FirstName}'");
							_sql.Append($",LastName = '{_objToUpdate.LastName}'");
							_sql.Append($",EmailAddress = '{_objToUpdate.EmailAddress}'");
							_sql.Append($",MobileNumber = '{_objToUpdate.MobileNumber}'");
							_sql.Append($",GenderFlag = '{_objToUpdate.GenderFlag}'");
							_sql.Append($",Ironing = {_objToUpdate.Ironing}");
							_sql.Append($" where Id = '{_objToUpdate.Id}'");
							dataItem.SqlString = _sql.ToString();
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
							dataItem.SqlString = _sql.ToString();
							db.getConnection().Execute(_sql.ToString());

							_sql.Clear();
							_sql.Append("Update Addresses set ");
							_sql.Append($"UpdatedAt = '{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}'");
							_sql.Append($",RowVersion = '{_objToUpdate.RowVersion.FormatDatabaseDateTime()}'");
							_sql.Append($",UpdatedBy = '{_objToUpdate.UpdatedBy}'");
							_sql.Append($",IsActive = {_objToUpdate.IsActive}");
							_sql.Append($",AddressLine1 = '{_objToUpdate.PostalAddress.AddressLine1}'");
							_sql.Append($",AddressLine2 = '{_objToUpdate.PostalAddress.AddressLine2}'");
							_sql.Append($",AddressLine3 = '{_objToUpdate.PostalAddress.AddressLine3}'");
							_sql.Append($",Suburb = '{_objToUpdate.PostalAddress.Suburb}'");
							_sql.Append($",State = '{_objToUpdate.PostalAddress.State}'");
							_sql.Append($",PostCode = '{_objToUpdate.PostalAddress.PostCode}'");
							_sql.Append($",Country = '{_objToUpdate.PostalAddress.Country}'");
							_sql.Append($" where Id = '{_objToUpdate.PostalAddress.Id}' ");
							dataItem.SqlString = _sql.ToString();
							db.getConnection().Execute(_sql.ToString());

						}
					}
					return JsonSuccessResponse("Team member saved successfully", _objToUpdate);
				}
				//catch (DbUpdateConcurrencyException ex)
				//{
				//	var entry = ex.Entries.Single();
				//	var clientValues = (Cleaner)entry.Entity;
				//	var databaseEntry = entry.GetDatabaseValues();
				//	if (databaseEntry == null)
				//	{
				//		ModelState.AddModelError(string.Empty, "Unable to save changes. The Team member was deleted by another user.");
				//	}
				//	else
				//	{
				//		var databaseValues = (Cleaner)databaseEntry.ToObject();

				//		if (databaseValues.FirstName != clientValues.FirstName)
				//			ModelState.AddModelError("FirstName", "Current database value for team member first name: " + databaseValues.FirstName);

				//		if (databaseValues.LastName != clientValues.LastName)
				//			ModelState.AddModelError("LastName", "Current database value for team member surname: " + databaseValues.LastName);

				//		ModelState.AddModelError(string.Empty, "The record you attempted to edit "
				//			+ "was modified by another user after you got the original value. The edit operation "
				//			+ "was canceled. If you still want to edit this record, click the Save button again.");
				//	}
				//}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, Helpers.FormatModelError("Error saving team member", ex));

					LogHelper log = new LogHelper();
					log.Log(LogHelper.LogLevels.Error, "Error saving team member", nameof(SaveCleanerDetails), ex, dataItem, Helpers.ParseValidationErrors(ex));
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(ModelState, nameof(SaveTeamMember), dataItem);
			}

			return JsonFormResponse();
		}

		[HttpPost]
		public ActionResult DeleteTeamMember(Guid? CleanerId)
		{
			string _objDesc = "Team Member";

			if (!CleanerId.HasValue)
			{
				ModelState.AddModelError(string.Empty, $"Valid {_objDesc.ToLower()} record not found.");
			}

			try
			{
				using (DBManager db = new DBManager())
				{
					string sql = @"select * from CleanerTeam C 
							 	inner join Addresses Ph on C.PhysicalAddressRefId = Ph.ID
								inner join Addresses Po on C.PostalAddressRefId = Po.ID
								where C.ID = '" + CleanerId + "'";

					var _objToDelete = db.getConnection().Query<CleanerTeam, Address, Address, CleanerTeam>(sql, (clnr, phys, post) => {
						clnr.PhysicalAddress = phys;
						clnr.PostalAddress = post;
						return clnr;
					}).SingleOrDefault();

					sql = @"select * from CleanerRosteredTeam C 
								where C.TeamRefId = '" + CleanerId + "'";

					var _objTeamDelete = db.getConnection().Query<CleanerRosteredTeam>(sql).ToArray();

                   if (_objToDelete == null)
					{
						ModelState.AddModelError(string.Empty, $"Valid {_objDesc.ToLower()} record not found.");
					}

					var _physAddress = _objToDelete.PhysicalAddress;
					var _postAddress = _objToDelete.PostalAddress;

					StringBuilder _sql = new StringBuilder();

					_sql.Append($"Delete from cleanerrosteredteam where TeamRefId = '{_objToDelete.Id}'");
					db.getConnection().Execute(_sql.ToString());

					_sql.Clear();
					_sql.Append($"Delete from cleanerteam where Id = '{_objToDelete.Id}'");
					db.getConnection().Execute(_sql.ToString());

					_sql.Clear();
					_sql.Append($"Delete from Addresses where id = '{_physAddress.Id}'");
					db.getConnection().Execute(_sql.ToString());

					_sql.Clear();
					_sql.Append($"Delete from Addresses where id = '{_postAddress.Id}'");
					db.getConnection().Execute(_sql.ToString());

					return JsonSuccessResponse($"{_objDesc} deleted successfully", _objToDelete);
				}
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, $"Error deleting {_objDesc.ToLower()} ({ex.Message})");

				LogHelper log = new LogHelper();
				log.Log(LogHelper.LogLevels.Error, $"Error deleting {_objDesc.ToLower()}", nameof(CleanerTeam), ex, null);
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(ModelState, nameof(CleanerTeam), null);
			}

			return JsonFormResponse();
		}

		[HttpPost]
		public ActionResult SearchCleaner(CleanerSearchVM searchCriteria)
		{
			if (searchCriteria == null || (searchCriteria.SelectedFranchiseId.Equals(Guid.Empty)
										   && String.IsNullOrWhiteSpace(searchCriteria.Name)
										   && String.IsNullOrWhiteSpace(searchCriteria.Zone)))
			{
				ModelState.AddModelError(string.Empty, $"No search criteria specified.");
			}

			StringBuilder sql = new StringBuilder();
			try
			{
				sql.Append(@"select * from Cleaners C 
							 	inner join Addresses Ph on C.PhysicalAddressRefId = Ph.ID where 1=1");
				if (!searchCriteria.SelectedFranchiseId.Equals(Guid.Empty))
				{
					sql.Append($" and C.MasterFranchiseRefId = '{searchCriteria.SelectedFranchiseId.ToString()}'");
				}

				if (!String.IsNullOrWhiteSpace(searchCriteria.Zone))
				{
					sql.Append($" and (C.PrimaryZone like '%{searchCriteria.Zone}%' || C.SecondaryZone like '%{searchCriteria.Zone.Trim()}%' )");
				}

				if (!String.IsNullOrWhiteSpace(searchCriteria.Name))
				{
					sql.Append($" and concat(C.FirstName, ' ', C.LastName) like '%{searchCriteria.Name.Trim()}%'");
				}

				if (!searchCriteria.IncludeInactive)
				{
					sql.Append(" and C.IsActive = true");
				}

				sql.Append(" order by C.Rating desc, C.LastName, C.FirstName");

				using (DBManager db = new DBManager())
				{
					var _results = db.getConnection().Query<Cleaner, Address, Cleaner>(sql.ToString(), (clnr, phys) => {
						clnr.PhysicalAddress = phys;
						return clnr;
					}).ToList();

					var _vmResults = Mapper.Map<List<Cleaner>, List<CleanerDetailsVM>>(_results);

					return new JsonNetResult() { Data = new { SearchResults = _vmResults }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
				}
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, $"Error performing cleaner search ({ex.Message})");

				LogHelper log = new LogHelper();
				log.Log(LogHelper.LogLevels.Error, "Error performing cleaner search", nameof(SearchCleaner), ex, sql.ToString());
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(ModelState, nameof(SearchCleaner), null);
			}

			return JsonFormResponse();
		}

		[HttpGet]
		public ActionResult GetCleanerRoster(Guid? CleanerId)
		{
			//https://msdn.microsoft.com/en-us/data/jj574232.aspx

			if (CleanerId == null)
			{
				ModelState.AddModelError(string.Empty, $"Cleaner Id [{CleanerId.ToString()}] not provided.  Please try again.");
				return JsonFormResponse();
			}

			StringBuilder sql = new StringBuilder();
			sql.Append("SELECT CR.ID RosterID, CR.PrimaryCleanerRefId, CR.WeekDay, CR.StartTime as TimeOfDayFrom, CR.EndTime as TimeOfDayTo, CR.TeamCount, CT.ID, CT.FirstName, CT.LastName, CRT.IsPrimary ");
			sql.Append(" FROM CleanerRoster CR ");
			sql.Append(" inner JOIN CleanerRosteredTeam CRT on CR.ID = CRT.RosterRefId AND CRT.IsPrimary = 0 ");
			sql.Append(" inner join CleanerTeam CT on CT.ID = CRT.TeamRefId ");
			sql.Append($" WHERE CR.PrimaryCleanerRefId = '{CleanerId}' ");
			sql.Append(" AND CR.IsActive = 1 ");
			sql.Append(" UNION ");
			sql.Append(" SELECT CR.ID as RosterID, CR.PrimaryCleanerRefId, CR.WeekDay, CR.StartTime as TimeOfDayFrom, CR.EndTime as TimeOfDayTo, CR.TeamCount, C.ID, C.FirstName, C.LastName , CRT.IsPrimary ");
			sql.Append(" FROM CleanerRoster CR ");
			sql.Append(" INNER JOIN CleanerRosteredTeam CRT on CR.ID = CRT.RosterRefId AND CRT.IsPrimary = 1");
			sql.Append(" INNER JOIN Cleaners C on C.ID = CRT.TeamRefId AND CRT.IsPrimary = 1 ");
			sql.Append($" WHERE CR.PrimaryCleanerRefId = '{CleanerId}' ");
			sql.Append(" ORDER BY WeekDay, TimeOfDayFrom, TimeOfDayTo");

			using (DBManager db = new DBManager())
			{
				List<CleanerRosterVM> _rosterList = null;
				var _results = db.getConnection().Query<RosterTeamMembersVM>(sql.ToString()).ToList();
				_rosterList = CleanerRosterVM.PopulateCollection(CleanerId, _results);

				return new JsonNetResult() { Data = new { list = _rosterList }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
			}
		}

		[HttpPost]
		public ActionResult SaveCleanerRoster(Guid? CleanerId, List<CleanerRosterVM> dataList)
		{
			//https://stackoverflow.com/questions/13541225/asp-net-mvc-how-to-display-success-confirmation-message-after-server-side-proce

			if (dataList == null || dataList.Count == 0 || !CleanerId.HasValue)
			{
				ModelState.AddModelError(string.Empty, "Valid cleaner roster not found.");
			}

			//ModelState.Clear();
			foreach (var modelValue in ModelState.Values)
			{
				modelValue.Errors.Clear();
			}

			LogHelper.LogDebugDetails("CleanersController.SaveCleanerRoster", LogHelper.GetObjectData(dataList));

			List<CleanerRoster> rosterList = new List<CleanerRoster>();
			CleanerRosteredTeam rosteredTeam;
			CleanerRoster roster;
			foreach (CleanerRosterVM item in dataList)
			{
				var _checkList = new List<String>();

				if (item.IsActive)
				{
					var _start = item.StartTime;
					var _end = item.EndTime;
					var _startTicks = _start.ToMinutes();;
					var _endTicks = _end.ToMinutes();
					var _dateKindFrom = _start.Kind.ToString();
					var _dateKindTo = _end.Kind.ToString();

					if (item.TeamCount <= 0 || item.TeamMembers == null || item.TeamMembers.Count() == 0)
					{
						ModelState.AddModelError("", $"At least 1 team member should be available on {item.Weekday}");
					}

					if (item.StartTime == DateTime.MinValue || item.EndTime == DateTime.MinValue)
					{
						ModelState.AddModelError("", $"Select valid start and end time for {item.Weekday}");
					}
					else if (_endTicks <= _startTicks)
					{
						ModelState.AddModelError("", $"End time must be later than start time for {item.Weekday}");
					}

					if (ModelState.IsValid)
					{
						roster = new CleanerRoster()
						{
							TimeOfDayFrom = _startTicks,
							TimeOfDayTo = _endTicks,
							DateKindFrom = _dateKindFrom,
							DateKindTo = _dateKindTo,
							TeamCount = item.TeamCount,
							Weekday = item.Weekday,
							IsActive = item.IsActive,
							PrimaryCleanerRefId = CleanerId.Value.ToString()

						};
						roster.CleanerRosteredTeam = new List<CleanerRosteredTeam>();
						foreach (var teamMember in item.TeamMembers)
						{
							if (!_checkList.Contains(teamMember.Id.ToString()))
							{
								rosteredTeam = new CleanerRosteredTeam()
								{
									RosterRefId = roster.Id,
									IsPrimary = teamMember.IsPrimary,
									TeamRefId = teamMember.Id
								};
								_checkList.Add(teamMember.Id);
								roster.CleanerRosteredTeam.Add(rosteredTeam);
							}
						}
						rosterList.Add(roster);
					}
				}
			}
			LogHelper.LogDebugDetails("CleanersController.SaveCleanerRoster - input", LogHelper.GetObjectData(dataList));
			LogHelper.LogDebugDetails("CleanersController.SaveCleanerRoster - processed", LogHelper.GetObjectData(rosterList));

			StringBuilder _sql = new StringBuilder();
			if (ModelState.IsValid)
			{
				try
				{
					_sql.Append(@"SELECT * "
							+ "FROM CleanerRosteredTeam  "
							+ "WHERE RosterRefId in ("
							+ $"select Id from CleanerRoster where PrimaryCleanerRefId = '{CleanerId}'"
					             + ")");

					using (DBManager db = new DBManager())
					{
						// first delete the existing roster

						List<CleanerRosteredTeam> _objChildToDelete = db.getConnection().Query<CleanerRosteredTeam>(_sql.ToString()).ToList();
						foreach (var _item in _objChildToDelete)
						{
							_sql.Clear();
							_sql.Append($"delete from CleanerRosteredTeam where TeamRefId = '{_item.TeamRefId}'");
							db.getConnection().Execute(_sql.ToString());
						}

						_sql.Clear();
						_sql.Append($"delete from CleanerRoster where PrimaryCleanerRefId = '{CleanerId}'");
						db.getConnection().Execute(_sql.ToString());

						// insert new roster
						foreach (CleanerRoster _objToInsert in rosterList)
						{
							var _obj = UpdateAuditTracking(_objToInsert);

							_sql.Clear();
							_sql.Append("Insert into CleanerRoster (Id, CreatedAt, UpdatedAt, UpdatedBy, IsActive, RowVersion, ");
							_sql.Append("PrimaryCleanerRefId, WeekDay, StartTime, EndTime, TeamCount)");
							_sql.Append(" values (");
							_sql.Append($"'{_obj.Id}',");
							_sql.Append($"'{_obj.CreatedAt.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_obj.UpdatedAt.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_obj.UpdatedBy}',");
							_sql.Append($"{_obj.IsActive},");
							_sql.Append($"'{_obj.RowVersion.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_obj.PrimaryCleanerRefId}',");
							_sql.Append($"'{_obj.Weekday}',");
							_sql.Append($"{_obj.TimeOfDayFrom},");
							_sql.Append($"{_obj.TimeOfDayTo},");
							_sql.Append($"{_obj.TeamCount}");
							_sql.Append(")");
							db.getConnection().Execute(_sql.ToString());

							foreach (CleanerRosteredTeam _objToInsertChild in _objToInsert.CleanerRosteredTeam)
							{
								var _objRT = UpdateAuditTracking(_objToInsertChild);
								_sql.Clear();
								_sql.Append("Insert into CleanerRosteredTeam (Id, RosterRefId, TeamRefId, IsPrimary)");
								_sql.Append(" values (");
								_sql.Append($"'{_objRT.Id}',");
								_sql.Append($"'{_objRT.RosterRefId}',");
								_sql.Append($"'{_objRT.TeamRefId}',");
								_sql.Append($"{_objRT.IsPrimary}");
								_sql.Append(")");
								db.getConnection().Execute(_sql.ToString());
							}
						}

					}
					return JsonSuccessResponse("Team roster saved successfully", dataList);
				}
				//catch (DbUpdateConcurrencyException ex)
				//{
				//	var entry = ex.Entries.Single();
				//	var clientValues = (CleanerRoster)entry.Entity;
				//	var databaseEntry = entry.GetDatabaseValues();
				//	if (databaseEntry == null)
				//	{
				//		ModelState.AddModelError(string.Empty, "Unable to save changes. The Team roster was deleted by another user.");
				//	}
				//	else
				//	{
				//		var databaseValues = (CleanerRoster)databaseEntry.ToObject();

				//		ModelState.AddModelError(string.Empty, "The record you attempted to edit "
				//			+ "was modified by another user after you got the original value. The edit operation "
				//			+ "was canceled. If you still want to edit this record, click the Save button again.");
				//	}
				//}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, Helpers.FormatModelError("Error saving team roster", ex));

					LogHelper log = new LogHelper();
					log.Log(LogHelper.LogLevels.Error, "Error saving team roster", nameof(SaveCleanerRoster), ex, dataList, Helpers.ParseValidationErrors(ex) + "\n" + _sql.ToString());
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(ModelState, nameof(SaveCleanerRoster), dataList);
			}

			return JsonFormResponse();
		}
		#endregion

		#region Service Functions, Leave Dates
		public ActionResult GetLeaveDates(Guid? CleanerId)
		{
			if (CleanerId == null)
			{
				ModelState.AddModelError(string.Empty, $"Cleaner Id [{CleanerId.ToString()}] not provided.  Please try again.");
				return JsonFormResponse();
			}

			List<CleanerLeave> _entityList = new List<CleanerLeave>();

			using (DBManager db = new DBManager())
			{
				_entityList = db.getConnection().Query<CleanerLeave>($"Select * from CleanerLeave where PrimaryCleanerRefId = '{CleanerId}' order by StartDate desc, EndDate desc").ToList();
			}

			List<CleanerLeaveVM> _editList = new List<CleanerLeaveVM>();
			foreach (CleanerLeave _item in _entityList)
			{
				var _vm = new CleanerLeaveVM();
				_vm.PopulateVM(CleanerId, _item);
				_editList.Add(_vm);
			}

			return new JsonNetResult() { Data = new { list = _editList, nextGuid = Guid.NewGuid() }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}


		[HttpPost]
		public ActionResult SaveLeaveDates(CleanerLeaveVM formValues)
		{
			string _objDesc = "Leave Dates";
			StringBuilder _sql = new StringBuilder();
			  
			if (formValues == null)
			{
				ModelState.AddModelError(string.Empty, $"Valid {_objDesc.ToLower()} data not found.");
			}

			if (ModelState.IsValid)
			{
				if (formValues.StartDate.IsPastDate("CleanersController.SaveLeaveDates"))		// Not sure yet - but UTC stuffs this up and can't add "today" as start date
				{
					ModelState.AddModelError(string.Empty, "Leave start date can not be in the past.");
				}
				else
				{
					if (formValues.EndDate.IsPastDate("CleanersController.SaveLeaveDates"))
					{
						ModelState.AddModelError(string.Empty, "Leave end date can not be in the past.");
					}
				}
			}

			if (ModelState.IsValid)
			{
				String _id = formValues.Id;
				var bIsNew = formValues.IsNewItem;

				try
				{
					CleanerLeave _objToUpdate = null;

					using (DBManager db = new DBManager())
					{
						if (bIsNew)
						{
							_objToUpdate = new CleanerLeave();
							_objToUpdate.PrimaryCleanerRefId = formValues.PrimaryCleanerRefId.ToString();
							_objToUpdate.StartDate = formValues.StartDate.ToUTC();
							_objToUpdate.EndDate = formValues.EndDate.ToUTC();

							_objToUpdate = UpdateAuditTracking(_objToUpdate);

							_sql.Clear();
							_sql.Append("Insert into CleanerLeave (Id, CreatedAt, UpdatedAt, UpdatedBy, IsActive, RowVersion, ");
							_sql.Append("PrimaryCleanerRefId, StartDate, EndDate)");
							_sql.Append(" values (");
							_sql.Append($"'{_objToUpdate.Id}',");
							_sql.Append($"'{_objToUpdate.CreatedAt.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.UpdatedBy}',");
							_sql.Append($"{_objToUpdate.IsActive},");
							_sql.Append($"'{_objToUpdate.RowVersion.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.PrimaryCleanerRefId}',");
							_sql.Append($"'{_objToUpdate.StartDate.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.EndDate.FormatDatabaseDateTime()}'");
							_sql.Append(")");
							formValues.SqlString = _sql.ToString();
							db.getConnection().Execute(_sql.ToString());
						}
						else
						{
							_objToUpdate = db.getConnection().Query<CleanerLeave>($"Select * from CleanerLeave where id = '{_id}'").SingleOrDefault();

							if (_objToUpdate == null)
							{
								ModelState.AddModelError(string.Empty, $"{_objDesc} [{_id.ToString()}] not found.  Please try again.");
								return JsonFormResponse();
							}

							_objToUpdate.StartDate = formValues.StartDate.ToUTC();
							_objToUpdate.EndDate = formValues.EndDate.ToUTC();

							_objToUpdate = UpdateAuditTracking(_objToUpdate);

							_sql.Clear();
							_sql.Append("Update CleanerLeave set ");
							_sql.Append($"UpdatedAt = '{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}'");
							_sql.Append($",RowVersion = '{_objToUpdate.RowVersion.FormatDatabaseDateTime()}'");
							_sql.Append($",UpdatedBy = '{_objToUpdate.UpdatedBy}'");
							_sql.Append($",IsActive = {_objToUpdate.IsActive}");
							_sql.Append($",PrimaryCleanerRefId = '{_objToUpdate.PrimaryCleanerRefId}'");
							_sql.Append($",StartDate = '{_objToUpdate.StartDate.FormatDatabaseDateTime()}'");
							_sql.Append($",EndDate = '{_objToUpdate.EndDate.FormatDatabaseDateTime()}'");
							_sql.Append($" where Id = '{_objToUpdate.Id}'");
							formValues.SqlString = _sql.ToString();
							db.getConnection().Execute(_sql.ToString());
						}
					}
					return JsonSuccessResponse($"{_objDesc} saved successfully", _objToUpdate);
				}
				//catch (DbUpdateConcurrencyException ex)
				//{
				//	var entry = ex.Entries.Single();
				//	var clientValues = (CleanerLeave)entry.Entity;
				//	var databaseEntry = entry.GetDatabaseValues();
				//	if (databaseEntry == null)
				//	{
				//		ModelState.AddModelError(string.Empty, $"Unable to save changes. The {_objDesc.ToLower()} was deleted by another user.");
				//	}
				//	else
				//	{
				//		var databaseValues = (CleanerLeave)databaseEntry.ToObject();

				//		if (databaseValues.StartDate != clientValues.StartDate)
				//		{
				//			ModelState.AddModelError("LeaveStart", "Current database value for start date: " + databaseValues.StartDate);
				//		}

				//		if (databaseValues.EndDate != clientValues.EndDate)
				//		{
				//			ModelState.AddModelError("PostCode", "Current database value for end date: " + databaseValues.EndDate);
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
					log.Log(LogHelper.LogLevels.Error, $"Error saving {_objDesc.ToLower()}", nameof(SaveLeaveDates), ex, formValues, Helpers.ParseValidationErrors(ex));
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(ModelState, nameof(SaveLeaveDates), formValues);
			}

			return JsonFormResponse();
		}

		[HttpPost]
		public ActionResult DeleteLeaveDates(Guid? id)
		{
			string _objDesc = "Leave";

			if (!id.HasValue)
			{
				ModelState.AddModelError(string.Empty, $"Valid {_objDesc.ToLower()} record not found.");
			}

			StringBuilder sql = new StringBuilder();
			try
			{
				using (DBManager db = new DBManager())
				{
					sql.Append($"delete from CleanerLeave where id = '{id.Value.ToString()}'");
					db.getConnection().Execute(sql.ToString());

					return JsonSuccessResponse($"{_objDesc} deleted successfully", "Id="+id.Value.ToString());
				}
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, $"Error deleting {_objDesc.ToLower()} ({ex.Message})");

				LogHelper log = new LogHelper();
				log.Log(LogHelper.LogLevels.Error, $"Error deleting {_objDesc.ToLower()}", nameof(LogEntry), ex, sql.ToString());
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(ModelState, nameof(LogEntry), null);
			}

			return JsonFormResponse();
		}
		#endregion 
	}
}
