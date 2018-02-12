#region Using
using MagicMaids.DataAccess;
using MagicMaids.EntityModels;
using MagicMaids.Validators;
using MagicMaids.ViewModels;

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;

using FluentValidation.Mvc;

using NLog;

using AutoMapper;
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
		public CleanersController(MagicMaidsContext dbContext) : base(dbContext)
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
			CleanerDetailsVM _dataItem = null;
			FranchiseSelectViewModel _selectedFranchise = null;

			if (CleanerId == null)
			{
				// create new item
				_dataItem = new CleanerDetailsVM();
				_dataItem.IsNewItem = true;
				_dataItem.PhysicalAddress = new UpdateAddressViewModel() { AddressType = AddressTypeSetting.Physical };
				_dataItem.PostalAddress = new UpdateAddressViewModel() { AddressType = AddressTypeSetting.Postal };
			}
			else
			{
				_cleaner = MMContext.Cleaners
						  	.Where(f => f.Id == CleanerId)
						  	.Include(nameof(Cleaner.PhysicalAddress))
						  	.Include(nameof(Cleaner.PostalAddress))
							.FirstOrDefault();

				if (_cleaner == null)
				{
					ModelState.AddModelError(string.Empty, $"Cleaner [{CleanerId.ToString()}] not found.  Please try again.");
					return JsonFormResponse();
				}

				_dataItem = new CleanerDetailsVM();
				_dataItem.PopulateVM(_cleaner);
				_dataItem.IsNewItem = false;

				if (!_dataItem.MasterFranchiseRefId.Equals(Guid.Empty))
				{
					var _franchise = MMContext.Franchises
						  .Where(f => f.Id == _dataItem.MasterFranchiseRefId)
						  .FirstOrDefault();

					if (_franchise == null)
					{
						ModelState.AddModelError(string.Empty, $"Cleaner's [{CleanerId.ToString()}] Master Franchise not found not found.  Please try again.");
						return JsonFormResponse();
					}

					List<SystemSetting> _settings = new List<SystemSetting>();
					_settings = MMContext.DefaultSettings
							 .Where(p => p.IsActive == true)
							 .ToList();

					_selectedFranchise = new FranchiseSelectViewModel();
					_selectedFranchise.PopulateVM(_franchise, _settings);
				}
			}

			return new JsonNetResult() { Data = new { item = _dataItem, selectedFranchise = _selectedFranchise }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpGet]
		public JsonNetResult GetNextCleanerCode()
		{
			Int32 _cleanerCode = 1000; // starting value

			Int32? _match = MMContext.Cleaners
								  .Max(x => (Int32?)x.CleanerCode);

			if (!_match.HasValue)
			{
				_cleanerCode = 1000;
			}
			else
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

			if (CleanerId == null)
			{
				ModelState.AddModelError(string.Empty, $"Cleaner Id [{CleanerId.ToString()}] not provided.  Please try again.");
				return JsonFormResponse();
			}

			var _team = MMContext.CleanerTeam
							  .Where(f => f.PrimaryCleanerRefId == CleanerId)
							  .Include(nameof(CleanerTeam.PhysicalAddress))
							  .Include(nameof(CleanerTeam.PostalAddress))
							  .ToList();

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
			return new JsonNetResult() { Data = new { item = SettingsController.GetZoneListByFranchise(FranchiseId, false) }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}


		[HttpPost]
		[ValidateAntiForgeryHeader]
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

			if (dataItem.MasterFranchiseRefId.Equals(Guid.Empty))
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
				else if (dataItem.PrimaryZoneList.Except(dataItem.ApprovedZoneList).Count() == 0)
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
				Guid _id = dataItem.Id;
				var bIsNew = (dataItem.IsNewItem);

				//https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/crud
				//https://stackoverflow.com/questions/21286538/asp-net-mvc-5-model-binding-edit-view
				//https://www.mikesdotnetting.com/article/248/mvc-5-with-ef-6-in-visual-basic-updating-related-data

				try
				{
					Cleaner _objToUpdate = null;

					if (bIsNew)
					{
						_objToUpdate = new Cleaner();

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
						_objToUpdate.MasterFranchiseRefId = dataItem.MasterFranchiseRefId;
						_objToUpdate.Rating = dataItem.Rating;
						_objToUpdate.GenderFlag = dataItem.GenderFlag;
						_objToUpdate.Ironing = dataItem.Ironing;

						_objToUpdate.PhysicalAddress = new Address() { AddressType = AddressTypeSetting.Physical };
						_objToUpdate.PostalAddress = new Address() { AddressType = AddressTypeSetting.Postal };
						_objToUpdate.PostalAddressRefId = _objToUpdate.PostalAddress.Id;
						_objToUpdate.PhysicalAddressRefId = _objToUpdate.PhysicalAddress.Id;

						if (dataItem.PhysicalAddress != null)
						{
							_objToUpdate.PhysicalAddress.AddressLine1 = dataItem.PhysicalAddress.AddressLine1;
							_objToUpdate.PhysicalAddress.AddressLine2 = dataItem.PhysicalAddress.AddressLine2;
							_objToUpdate.PhysicalAddress.AddressLine3 = dataItem.PhysicalAddress.AddressLine3;
							_objToUpdate.PhysicalAddress.Suburb = dataItem.PhysicalAddress.Suburb;
							_objToUpdate.PhysicalAddress.Country = dataItem.PhysicalAddress.Country;
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
						_objToUpdate = MMContext.Cleaners
								 .Where(f => f.Id == _id)
										  .Include(nameof(Cleaner.PhysicalAddress))
										  .Include(nameof(Cleaner.PostalAddress))
										  .FirstOrDefault();

						if (_objToUpdate == null)
						{
							ModelState.AddModelError(string.Empty, $"Cleaner [{_id.ToString()}] not found.  Please try again.");
							return JsonFormResponse();
						}

						MMContext.Entry(_objToUpdate).CurrentValues.SetValues(dataItem);
						MMContext.Entry(_objToUpdate.PhysicalAddress).CurrentValues.SetValues(dataItem.PhysicalAddress);
						MMContext.Entry(_objToUpdate.PostalAddress).CurrentValues.SetValues(dataItem.PostalAddress);
					}

					MMContext.SaveChanges();

					return JsonSuccessResponse("Cleaner saved successfully", _objToUpdate);
				}
				catch (DbUpdateConcurrencyException ex)
				{
					var entry = ex.Entries.Single();
					var clientValues = (Cleaner)entry.Entity;
					var databaseEntry = entry.GetDatabaseValues();
					if (databaseEntry == null)
					{
						ModelState.AddModelError(string.Empty, "Unable to save changes. The cleaner was deleted by another user.");
					}
					else
					{
						var databaseValues = (Cleaner)databaseEntry.ToObject();

						if (databaseValues.CleanerCode != clientValues.CleanerCode)
							ModelState.AddModelError("CleanerCode", "Current database value for cleaner code: " + databaseValues.CleanerCode);

						if (databaseValues.FirstName != clientValues.FirstName)
							ModelState.AddModelError("FirstName", "Current database value for cleaner first name: " + databaseValues.FirstName);

						if (databaseValues.LastName != clientValues.LastName)
							ModelState.AddModelError("LastName", "Current database value for cleaner surname: " + databaseValues.LastName);

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
					//_msg = new InfoViewModel("Error saving cleaner", ex);
					ModelState.AddModelError(string.Empty, $"Error saving cleaner ({ex.Message})");

					LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
					log.Log(LogLevel.Error, "Error saving cleaner", nameof(SaveCleanerDetails), ex, dataItem);
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(SaveCleanerDetails), dataItem);
			}

			return JsonFormResponse();
		}

		[HttpPost]
		[ValidateAntiForgeryHeader]
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

			if (ModelState.IsValid)
			{
				Guid _id = dataItem.Id;
				var bIsNew = (dataItem.IsNewItem);

				//https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/crud
				//https://stackoverflow.com/questions/21286538/asp-net-mvc-5-model-binding-edit-view
				//https://www.mikesdotnetting.com/article/248/mvc-5-with-ef-6-in-visual-basic-updating-related-data

				try
				{
					CleanerTeam _objToUpdate = null;

					if (bIsNew)
					{
						_objToUpdate = new CleanerTeam();

						_objToUpdate.FirstName = dataItem.FirstName;
						_objToUpdate.LastName = dataItem.LastName;
						_objToUpdate.EmailAddress = dataItem.EmailAddress;
						_objToUpdate.IsActive = dataItem.IsActive;
						_objToUpdate.MobileNumber = dataItem.MobileNumber;
						_objToUpdate.GenderFlag = dataItem.GenderFlag;
						_objToUpdate.Ironing = dataItem.Ironing;
						_objToUpdate.PrimaryCleanerRefId = dataItem.PrimaryCleanerRefId;

						_objToUpdate.PhysicalAddress = new Address() { AddressType = AddressTypeSetting.Physical };
						_objToUpdate.PostalAddress = new Address() { AddressType = AddressTypeSetting.Postal };
						_objToUpdate.PostalAddressRefId = _objToUpdate.PostalAddress.Id;
						_objToUpdate.PhysicalAddressRefId = _objToUpdate.PhysicalAddress.Id;

						if (dataItem.PhysicalAddress != null)
						{
							_objToUpdate.PhysicalAddress.AddressLine1 = dataItem.PhysicalAddress.AddressLine1;
							_objToUpdate.PhysicalAddress.AddressLine2 = dataItem.PhysicalAddress.AddressLine2;
							_objToUpdate.PhysicalAddress.AddressLine3 = dataItem.PhysicalAddress.AddressLine3;
							_objToUpdate.PhysicalAddress.Suburb = dataItem.PhysicalAddress.Suburb;
							_objToUpdate.PhysicalAddress.Country = dataItem.PhysicalAddress.Country;
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
						_objToUpdate = MMContext.CleanerTeam
								 .Where(f => f.Id == _id)
										  .Include(nameof(CleanerTeam.PhysicalAddress))
										  .Include(nameof(CleanerTeam.PostalAddress))
										  .FirstOrDefault();

						if (_objToUpdate == null)
						{
							ModelState.AddModelError(string.Empty, $"Team member [{_id.ToString()}] not found.  Please try again.");
							return JsonFormResponse();
						}

						// no action on team member yet until it's deleted

						MMContext.Entry(_objToUpdate).CurrentValues.SetValues(dataItem);
						MMContext.Entry(_objToUpdate.PhysicalAddress).CurrentValues.SetValues(dataItem.PhysicalAddress);
						MMContext.Entry(_objToUpdate.PostalAddress).CurrentValues.SetValues(dataItem.PostalAddress);
					}

					MMContext.SaveChanges();

					return JsonSuccessResponse("Team member saved successfully", _objToUpdate);
				}
				catch (DbUpdateConcurrencyException ex)
				{
					var entry = ex.Entries.Single();
					var clientValues = (Cleaner)entry.Entity;
					var databaseEntry = entry.GetDatabaseValues();
					if (databaseEntry == null)
					{
						ModelState.AddModelError(string.Empty, "Unable to save changes. The Team member was deleted by another user.");
					}
					else
					{
						var databaseValues = (Cleaner)databaseEntry.ToObject();

						if (databaseValues.FirstName != clientValues.FirstName)
							ModelState.AddModelError("FirstName", "Current database value for team member first name: " + databaseValues.FirstName);

						if (databaseValues.LastName != clientValues.LastName)
							ModelState.AddModelError("LastName", "Current database value for team member surname: " + databaseValues.LastName);

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
					ModelState.AddModelError(string.Empty, Helpers.FormatModelError("Error saving team member", ex));

					LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
					log.Log(LogLevel.Error, "Error saving team member", nameof(SaveCleanerDetails), ex, dataItem, Helpers.ParseValidationErrors(ex));
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(SaveTeamMember), dataItem);
			}

			return JsonFormResponse();
		}

		[HttpPost]
		[ValidateAntiForgeryHeader]
		public ActionResult DeleteTeamMember(Guid? CleanerId)
		{
			string _objDesc = "Team Member";

			if (!CleanerId.HasValue)
			{
				ModelState.AddModelError(string.Empty, $"Valid {_objDesc.ToLower()} record not found.");
			}

			try
			{
				var _objToDelete = MMContext.CleanerTeam
						.Where(f => f.Id == CleanerId)
							.Include(nameof(CleanerTeam.PhysicalAddress))
						  	.Include(nameof(CleanerTeam.PostalAddress))
						  .FirstOrDefault();


				if (_objToDelete == null)
				{
					ModelState.AddModelError(string.Empty, $"Valid {_objDesc.ToLower()} record not found.");
				}

				var _physAddress = _objToDelete.PhysicalAddress;
				var _postAddress = _objToDelete.PostalAddress;

				MMContext.Addresses.Remove(_physAddress);
				MMContext.Addresses.Remove(_postAddress);

				MMContext.Entry(_objToDelete).State = EntityState.Deleted;
				MMContext.SaveChanges();

				return JsonSuccessResponse($"{_objDesc} deleted successfully", _objToDelete);
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, $"Error deleting {_objDesc.ToLower()} ({ex.Message})");

				LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
				log.Log(LogLevel.Error, $"Error deleting {_objDesc.ToLower()}", nameof(CleanerTeam), ex, null);
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(CleanerTeam), null);
			}

			return JsonFormResponse();
		}

		[HttpPost]
		[ValidateAntiForgeryHeader]
		public ActionResult SearchCleaner(CleanerSearchVM searchCriteria)
		{
			if (searchCriteria == null || (searchCriteria.SelectedFranchiseId.Equals(Guid.Empty)
										   && String.IsNullOrWhiteSpace(searchCriteria.Name)
										   && String.IsNullOrWhiteSpace(searchCriteria.Zone)))
			{
				ModelState.AddModelError(string.Empty, $"No search criteria specified.");
			}

			try
			{
				var _results = MMContext.Cleaners
						.Include(nameof(Cleaner.PhysicalAddress))
					.Where(f => searchCriteria.SelectedFranchiseId.Equals(Guid.Empty) || f.MasterFranchiseRefId == searchCriteria.SelectedFranchiseId)
					.Where(f => (searchCriteria.Name == null || searchCriteria.Name.Trim() == string.Empty) || (f.FirstName + " " + f.LastName).Contains(searchCriteria.Name))
					.Where(f => (searchCriteria.Zone == null || searchCriteria.Zone.Trim() == string.Empty)
						   || ("," + f.PrimaryZone + ",").Contains("," + searchCriteria.Zone + ",")
						   || ("," + f.SecondaryZone + ",").Contains("," + searchCriteria.Zone + ","))
					.Where(f => (searchCriteria.IncludeInactive == false && f.IsActive == true) || searchCriteria.IncludeInactive == true)
					.OrderByDescending(f => f.Rating).ThenBy(f => new { f.LastName, f.FirstName })
				  	.ToList();

				var _vmResults = Mapper.Map<List<Cleaner>, List<CleanerDetailsVM>>(_results);

				return new JsonNetResult() { Data = new { SearchResults = _vmResults }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, $"Error performing cleaner search ({ex.Message})");

				LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
				log.Log(LogLevel.Error, "Error performing cleaner search", nameof(SearchCleaner), ex, null);
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(SearchCleaner), null);
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

			string query = "SELECT CR.ID as RosterID, CR.PrimaryCleanerRefId, CR.WeekDay, CR.StartTime, CR.EndTime, CR.TeamCount,"
				+ "CT.ID, CT.FirstName, CT.LastName, CRT.IsPrimary "
				+ "FROM CleanerRoster CR "
				+ "inner JOIN CleanerRosteredTeam CRT on CR.ID = CRT.RosterRefId AND CRT.IsPrimary = 0 "
				+ "inner JOIN CleanerTeam CT on CT.ID = CRT.TeamRefId "
				+ $"WHERE CR.PrimaryCleanerRefId = '{CleanerId}' "
				+ "AND CR.IsActive = 1 "
				+ "UNION "
				+ "SELECT CR.ID as RosterID, CR.PrimaryCleanerRefId, CR.WeekDay, CR.StartTime, CR.EndTime, CR.TeamCount, "
				+ "C.ID, C.FirstName, C.LastName , CRT.IsPrimary "
				+ "FROM CleanerRoster CR "
				+ "INNER JOIN CleanerRosteredTeam CRT on CR.ID = CRT.RosterRefId AND CRT.IsPrimary = 1 "
				+ "INNER JOIN Cleaners C on C.ID = CRT.TeamRefId AND CRT.IsPrimary = 1 "
				+ $"WHERE CR.PrimaryCleanerRefId = '{CleanerId}' "
				+ "ORDER BY WeekDay, StartTime, EndTime";

			var _results = MMContext.Database.SqlQuery<RosterTeamMembersVM>(query).ToList();
			List<CleanerRosterVM> _rosterList = CleanerRosterVM.PopulateCollection(CleanerId, _results);

			return new JsonNetResult() { Data = new { list = _rosterList }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpPost]
		[ValidateAntiForgeryHeader]
		public ActionResult SaveCleanerRoster(Guid? CleanerId, List<CleanerRosterVM> dataList)
		{
			string _objDesc = "Cleaner Roster";

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

			List<CleanerRoster> rosterList = new List<CleanerRoster>();
			CleanerRosteredTeam rosteredTeam;
			CleanerRoster roster;
			foreach (CleanerRosterVM item in dataList)
			{
				var _checkList = new List<String>();

				if (item.IsActive)
				{
					if (item.TeamCount <= 0 || item.TeamMembers == null || item.TeamMembers.Count() == 0)
					{
						ModelState.AddModelError("", $"At least 1 team member should be available on {item.Weekday}");
					}

					if (item.StartTime == DateTime.MinValue || item.EndTime == DateTime.MinValue)
					{
						ModelState.AddModelError("", $"Select valid start and end time for {item.Weekday}");
					}
					else if (item.EndTime <= item.StartTime)
					{
						ModelState.AddModelError("", $"End time must be later than start time for {item.Weekday}");
					}

					if (ModelState.IsValid)
					{
						roster = new CleanerRoster()
						{
							StartTime = item.StartTime,
							EndTime = item.EndTime,
							TeamCount = item.TeamCount,
							Weekday = item.Weekday,
							IsActive = item.IsActive,
							PrimaryCleanerRefId = CleanerId.Value

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
								_checkList.Add(teamMember.Id.ToString());
								roster.CleanerRosteredTeam.Add(rosteredTeam);
							}
						}
						rosterList.Add(roster);
					}
				}
			}

			if (ModelState.IsValid)
			{
				try
				{
					// first delete the existing roster
					string query = "SELECT * "
						+ "FROM CleanerRosteredTeam  "
						+ "WHERE RosterRefId in ("
						+ $"select Id from CleanerRoster where PrimaryCleanerRefId = '{CleanerId}'"
						+ ")";
					var _objChildToDelete = MMContext.Database.SqlQuery<CleanerRosteredTeam>(query).ToList();

					foreach (var _item in _objChildToDelete)
					{
						MMContext.Entry(_item).State = EntityState.Deleted;
					}

					var _objToDelete = MMContext.CleanerRoster
												.Where(f => f.PrimaryCleanerRefId == CleanerId)
												.ToList();
					foreach (var _item in _objToDelete)
					{
						MMContext.Entry(_item).State = EntityState.Deleted;
					}


					// insert new roster
					foreach (CleanerRoster _objToInsert in rosterList)
					{
						MMContext.Entry(_objToInsert).State = EntityState.Added;

						foreach (CleanerRosteredTeam _objToInsertChild in _objToInsert.CleanerRosteredTeam)
						{
							MMContext.Entry(_objToInsertChild).State = EntityState.Added;
						}
					}

					MMContext.SaveChanges();

					return JsonSuccessResponse("Team roster saved successfully", dataList);
				}
				catch (DbUpdateConcurrencyException ex)
				{
					var entry = ex.Entries.Single();
					var clientValues = (CleanerRoster)entry.Entity;
					var databaseEntry = entry.GetDatabaseValues();
					if (databaseEntry == null)
					{
						ModelState.AddModelError(string.Empty, "Unable to save changes. The Team roster was deleted by another user.");
					}
					else
					{
						var databaseValues = (CleanerRoster)databaseEntry.ToObject();

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
					ModelState.AddModelError(string.Empty, Helpers.FormatModelError("Error saving team roster", ex));

					LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
					log.Log(LogLevel.Error, "Error saving team roster", nameof(SaveCleanerRoster), ex, dataList, Helpers.ParseValidationErrors(ex));
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(SaveCleanerRoster), null);
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

			_entityList = MMContext.CleanerLeave
				   	.Where(p => p.PrimaryCleanerRefId == CleanerId)
                   	.OrderByDescending(p => p.StartDate)
					.ThenByDescending(p => p.EndDate) 
				   	.ToList();

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
		[ValidateAntiForgeryHeader]
		public ActionResult SaveLeaveDates(CleanerLeaveVM formValues)
		{
			string _objDesc = "Leave Dates";

			if (formValues == null)
			{
				ModelState.AddModelError(string.Empty, $"Valid {_objDesc.ToLower()} data not found.");
			}

			if (ModelState.IsValid)
			{
				Guid _id = formValues.Id;
				var bIsNew = formValues.IsNewItem;

				try
				{
					CleanerLeave _objToUpdate = null;

					if (bIsNew)
					{
						_objToUpdate = new CleanerLeave();
						_objToUpdate.PrimaryCleanerRefId = formValues.PrimaryCleanerRefId;
						_objToUpdate.StartDate = formValues.StartDate;
						_objToUpdate.EndDate  = formValues.EndDate;

						MMContext.Entry(_objToUpdate).State = EntityState.Added;
					}
					else
					{
						_objToUpdate = MMContext.CleanerLeave
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
					var clientValues = (CleanerLeave)entry.Entity;
					var databaseEntry = entry.GetDatabaseValues();
					if (databaseEntry == null)
					{
						ModelState.AddModelError(string.Empty, $"Unable to save changes. The {_objDesc.ToLower()} was deleted by another user.");
					}
					else
					{
						var databaseValues = (CleanerLeave)databaseEntry.ToObject();

						if (databaseValues.StartDate != clientValues.StartDate)
						{
							ModelState.AddModelError("LeaveStart", "Current database value for start date: " + databaseValues.StartDate);
						}

						if (databaseValues.EndDate != clientValues.EndDate)
						{
							ModelState.AddModelError("PostCode", "Current database value for end date: " + databaseValues.EndDate);
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
					log.Log(LogLevel.Error, $"Error saving {_objDesc.ToLower()}", nameof(SaveLeaveDates ), ex, formValues, Helpers.ParseValidationErrors(ex));
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(SaveLeaveDates), formValues);
			}

			return JsonFormResponse();
		}

		[HttpPost]
		[ValidateAntiForgeryHeader]
		public ActionResult DeleteLeaveDates(Guid? id)
		{
			string _objDesc = "Leave";

			if (!id.HasValue)
			{
				ModelState.AddModelError(string.Empty, $"Valid {_objDesc.ToLower()} record not found.");
			}

			try
			{
				var objToDelete = MMContext.CleanerLeave.FirstOrDefault(l => l.Id == id.Value);
				if (objToDelete != null)
				{
					MMContext.CleanerLeave.Remove(objToDelete);
					MMContext.SaveChanges();
				}

				return JsonSuccessResponse($"{_objDesc} deleted successfully", objToDelete);
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, $"Error deleting {_objDesc.ToLower()} ({ex.Message})");

				LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
				log.Log(LogLevel.Error, $"Error deleting {_objDesc.ToLower()}", nameof(LogEntry), ex, null);
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(LogEntry), null);
			}

			return JsonFormResponse();
		}
		#endregion 
	}
}
