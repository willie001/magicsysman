#region Using
using MagicMaids.DataAccess;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MagicMaids.EntityModels;
using MagicMaids.ViewModels;
using System.Data.Entity;
using MagicMaids.Validators;
using FluentValidation.Mvc;
using System.Data.Entity.Infrastructure;
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
		//private IRepository<Cleaner> repository;
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
							  .Where(f => f.Id == CleanerId )
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
			CleanerDetailsVM _dataItem = null;
			List<TeamMemberDetailsVM> _teamList = new List<TeamMemberDetailsVM>();

			if (CleanerId == null)
			{
				ModelState.AddModelError(string.Empty, $"Cleaner Id [{CleanerId.ToString()}] not provided.  Please try again.");
				return JsonFormResponse();
			}

			//var _team = from c in MMContext.Cleaners
						//join t in MMContext.CleanerTeam
						//   on c.Id equals t.TeamMemberRefId
						//where t.PrimaryCleanerRefId == CleanerId
						//select new { Cleaners = c, CleanerTeam = t };

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


			return new JsonNetResult() { Data = new { list = _teamList, teamSize=_team.Count()+1 }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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
					log.Log(LogLevel.Error, "Error saving cleaner", nameof(SaveCleanerDetails ), ex, dataItem);
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(SaveCleanerDetails), dataItem);
			}

			return JsonFormResponse();
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

			List<CleanerRoster> _rosterList = MMContext.CleanerRoster
							  .Where(f => f.PrimaryCleanerRefId == CleanerId)
							  .ToList();

			return new JsonNetResult() { Data = new { list = _rosterList }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpPost]
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

			foreach(CleanerRosterVM item in dataList)
			{
				if (item.IsActive)
				{
					if (item.TeamCount <= 0)
					{
						ModelState.AddModelError("",$"At least 1 team member should be available on {item.Weekday}");
					}

					if (item.StartTime == DateTime.MinValue || item.EndTime == DateTime.MinValue)
					{
						ModelState.AddModelError("",$"Select valid start and end time for {item.Weekday}");
					}
					else  if (item.EndTime <= item.StartTime)
					{
						ModelState.AddModelError("", $"End time must be later than start time for {item.Weekday}");
					}
				}
			}

			if (ModelState.IsValid)
			{
				try
				{
					// first delete the existing roster
					var _objToDelete = MMContext.CleanerRoster
												.Where(f => f.PrimaryCleanerRefId == CleanerId)
												.ToList();

					foreach(var _item in _objToDelete)
					{
						MMContext.Entry(_item).State = EntityState.Deleted;
					}

					// insert new roster
					CleanerRoster _objToInsert = null;

					foreach(CleanerRosterVM _item in dataList )
					{
						if (_item.TeamCount > 0)
						{
							_objToInsert = new CleanerRoster();

							_objToInsert.Weekday = _item.Weekday;
							_objToInsert.StartTime = _item.StartTime;
							_objToInsert.EndTime = _item.EndTime;
							_objToInsert.TeamCount = _item.TeamCount;
							_objToInsert.PrimaryCleanerRefId = CleanerId.Value;
							_objToInsert.IsActive = _item.IsActive;
							MMContext.Entry(_objToInsert).State = EntityState.Added;
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
	}
}
