#region Using
using MagicMaids.DataAccess;
using MagicMaids.EntityModels;
using MagicMaids.Validators;
using MagicMaids.ViewModels;

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Web.Mvc;

using FluentValidation.Mvc;

using AutoMapper;
using MagicMaids.Security;
using System.Data;
using Dapper;
#endregion

namespace MagicMaids.Controllers
{
	public class ClientsController : BaseController
	{
		#region Fields
		#endregion

		#region Constructor
		public ClientsController() : base()
		{
		}
		#endregion

		#region Method, Public
		public ActionResult Clients()
		{
			return View();
		}

		public ActionResult ClientDetails()
		{
			return View();
		}
		#endregion

		#region Service Functions
		[HttpPost]
		public ActionResult SearchClient(ClientSearchVM searchCriteria)
		{
			if (searchCriteria == null || (String.IsNullOrWhiteSpace(searchCriteria.Name)
										   && String.IsNullOrWhiteSpace(searchCriteria.Address)
										   && String.IsNullOrWhiteSpace(searchCriteria.Cleaner)
										   && String.IsNullOrWhiteSpace(searchCriteria.Phone)
										   && String.IsNullOrWhiteSpace(searchCriteria.Suburb)))
			{
				ModelState.AddModelError(string.Empty, $"No search criteria specified.");
			}

			if (ModelState.IsValid)

			{

				try
				{
					using (DBManager db = new DBManager())
					{
						StringBuilder sql = new StringBuilder(@"select * from Clients C 
							 	inner join Addresses Ph on C.PhysicalAddressRefId = Ph.ID where 1=1");
						
						if (!String.IsNullOrWhiteSpace(searchCriteria.Name))
						{
							var nameList = searchCriteria.Name.Split(' ').ToList();
							if (nameList.Count == 1)
							{
								sql.Append($" and (C.FirstName like '{searchCriteria.Name}%' or C.LastName like '{searchCriteria.Name}%')");
							}
							else if (nameList.Count == 2)
							{
								sql.Append($" and (C.FirstName like '{nameList[0].Trim()}%' and C.LastName like '{nameList[1].Trim()}%')");
							}
							else
							{
								sql.Append($" and (");
								var addOr = false;
								foreach (var name in nameList)
								{
									if (addOr)
									{
										sql.Append(" or ");
									}
									sql.Append($"C.FirstName like '{name.Trim()}%'  or C.LastName like '{name.Trim()}%' ");
									addOr = true;
								}
								sql.Append($")");
							}
						}

						if (!String.IsNullOrWhiteSpace(searchCriteria.Phone))
						{
							sql.Append($" and (C.BusinessPhoneNumber like '{searchCriteria.Phone}%' or C.MobileNumber like '{searchCriteria.Phone}%' or C.OtherNumber like '{searchCriteria.Phone}%')");
						}

						if (!String.IsNullOrWhiteSpace(searchCriteria.Address))
						{
                            sql.Append($" and (CONCAT_WS(' ', Ph.AddressLine1, Ph.AddressLine2, Ph.AddressLine3, Ph.State, Ph.Country) LIKE '%{searchCriteria.Address}%')");
                        }

						if (!String.IsNullOrWhiteSpace(searchCriteria.Suburb))
						{
							sql.Append($" and (Ph.Suburb like '%{searchCriteria.Suburb}%'");
							sql.Append($" or Ph.Suburb like '{searchCriteria.Suburb}%')");
						}

						if (!searchCriteria.IncludeInactive)
						{
							sql.Append($" and Ph.IsActive = 1");
						}

						if (!String.IsNullOrWhiteSpace(searchCriteria.Cleaner))
						{
                            sql.Append($" and (C.ID in (SELECT jb.ClientRefId FROM cleaners c INNER JOIN jobbooking jb ON c.ID = jb.PrimaryCleanerRefId WHERE CONCAT_WS(' ', c.FirstName, c.LastName) LIKE '%{searchCriteria.Cleaner}%' GROUP BY jb.ClientRefId))");
						}

						sql.Append(" order by C.LastName, C.FirstName ");

						var _orderedResults = db.getConnection().Query<Client, Address, Client>(sql.ToString(), (client, physicalAddress) => {
                            client.PhysicalAddress = physicalAddress;
							return client;
						}).ToList();

						var _vmResults = Mapper.Map<List<Client>, List<ClientDetailsVM>>(_orderedResults);

						return new JsonNetResult() { Data = new { SearchResults = _vmResults }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
					}
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, $"Error performing customer search ({ex.Message})");

					LogHelper log = new LogHelper();
					log.Log(LogHelper.LogLevels.Error, "Error performing customer search", nameof(SearchClient), ex, null);
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(ModelState, nameof(SearchClient), null);
			}

			return JsonFormResponse();
		}

		[HttpGet]
		public ActionResult GetClient(Guid? ClientId)
		{
			//https://msdn.microsoft.com/en-us/data/jj574232.aspx
			Client _client = null;
			ClientDetailsVM _dataItem = null;
			FranchiseSelectViewModel _selectedFranchise = null;

			if (ClientId == null)
			{
				// create new item
				_dataItem = new ClientDetailsVM();
				_dataItem.Id = Guid.NewGuid().ToString();
				_dataItem.IsNewItem = true;
				_dataItem.IsActive = true;
				_dataItem.PhysicalAddress = new UpdateAddressViewModel() { Id = Guid.NewGuid().ToString(), AddressType = AddressTypeSetting.Physical };
				_dataItem.PhysicalAddressRefId = _dataItem.PhysicalAddress.Id;
			}
			else
			{
				using (DBManager db = new DBManager())
				{
					string sql = @"select * from Clients C 
							 	inner join Addresses Ph on C.PhysicalAddressRefId = Ph.ID
								where C.ID = '" + ClientId + "'";

					_client = db.getConnection().Query<Client, Address, Client>(sql, (cl, phys) => {
						cl.PhysicalAddress = phys;
						return cl;
					}).SingleOrDefault();

					if (_client == null)
					{
						ModelState.AddModelError(string.Empty, $"Customer [{ClientId.ToString()}] not found. Please try again.");
						return JsonFormResponse();
					}
				}

				_dataItem = new ClientDetailsVM();
				_dataItem.PopulateVM(_client);
				_dataItem.IsNewItem = false;

			}

			return new JsonNetResult() { Data = new { item = _dataItem, selectedFranchise = _selectedFranchise }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpPost]
		public ActionResult SaveClientDetails(ClientDetailsVM dataItem)
		{
			//https://stackoverflow.com/questions/13541225/asp-net-mvc-how-to-display-success-confirmation-message-after-server-side-proce

			if (dataItem == null)
			{
				ModelState.AddModelError(string.Empty, "Valid customer data not found.");
			}

			if (!dataItem.HasAnyPhoneNumbers)
			{
				ModelState.AddModelError(string.Empty, "Please provide at least one valid phone number.");
			}

			if (!dataItem.HasAnyValidAddress)
			{
				ModelState.AddModelError(string.Empty, "Please ensure at least a valid physical address is supplied.");
			}
			else
			{
				if (dataItem.PhysicalAddress != null && (dataItem.PhysicalAddress.IsPartialAddress || dataItem.HasValidPhysicalAddress))
				{
					var _physValidator = new AddressValidator();
					var results = _physValidator.Validate(dataItem.PhysicalAddress);
					results.AddToModelState(ModelState, "");
				}
			}

			if (ModelState.IsValid)
			{
				String _id = dataItem.Id;
				var bIsNew = (dataItem.IsNewItem);

				try
				{
					Client _objToUpdate = null;

					using (DBManager db = new DBManager())
					{
						if (bIsNew)
						{
							_objToUpdate = UpdateClient(null, dataItem);

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
							_sql.Append("Insert into Clients (Id, CreatedAt, UpdatedAt, UpdatedBy, IsActive, RowVersion, ");
							_sql.Append("FirstName, LastName,  ");
							_sql.Append("EmailAddress, PhysicalAddressRefId, BusinessPhoneNumber, MobileNumber, OtherNumber, ");
							_sql.Append("ClientType)");
							_sql.Append(" values (");
							_sql.Append($"'{_objToUpdate.Id}',");
							_sql.Append($"'{_objToUpdate.CreatedAt.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.UpdatedBy}',");
							_sql.Append($"{_objToUpdate.IsActive},");
							_sql.Append($"'{_objToUpdate.RowVersion.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.FirstName}',");
							_sql.Append($"'{_objToUpdate.LastName}',");
							_sql.Append($"'{_objToUpdate.EmailAddress}',");
							_sql.Append($"'{_objToUpdate.PhysicalAddressRefId}',");
							_sql.Append($"'{_objToUpdate.BusinessPhoneNumber}',");
							_sql.Append($"'{_objToUpdate.MobileNumber}',");
							_sql.Append($"'{_objToUpdate.OtherNumber}',");
							_sql.Append($"'{_objToUpdate.ClientType}'");
							_sql.Append(")");
							db.getConnection().Execute(_sql.ToString());
						}
						else
						{
							string sql = @"select * from Clients C 
							 	inner join Addresses Ph on C.PhysicalAddressRefId = Ph.ID
								where C.ID = '" + _id + "'";

							_objToUpdate = db.getConnection().Query<Client, Address, Client>(sql, (clnt, phys) => {
								clnt.PhysicalAddress = phys;
								return clnt;
							}).SingleOrDefault();

							if (_objToUpdate == null)
							{
								ModelState.AddModelError(string.Empty, $"Customer [{_id.ToString()}] not found.  Please try again.");
								return JsonFormResponse();
							}

							StringBuilder _sql = new StringBuilder();
							_objToUpdate = UpdateClient(_objToUpdate, dataItem);

							_sql.Append("Update Clients set ");
							_sql.Append($"UpdatedAt = '{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}'");
							_sql.Append($",RowVersion = '{_objToUpdate.RowVersion.FormatDatabaseDateTime()}'");
							_sql.Append($",UpdatedBy = '{_objToUpdate.UpdatedBy}'");
							_sql.Append($",IsActive = {_objToUpdate.IsActive}");
							_sql.Append($",FirstName = '{_objToUpdate.FirstName}'");
							_sql.Append($",LastName = '{_objToUpdate.LastName}'");
							_sql.Append($",EmailAddress = '{_objToUpdate.EmailAddress}'");
							_sql.Append($",BusinessPhoneNumber = '{_objToUpdate.BusinessPhoneNumber}'");
							_sql.Append($",MobileNumber = '{_objToUpdate.MobileNumber}'");
							_sql.Append($",OtherNumber = '{_objToUpdate.OtherNumber}'");
							_sql.Append($",ClientType = '{_objToUpdate.ClientType}'");
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
					}

					return JsonSuccessResponse("Customer saved successfully", _objToUpdate);
				}
				//catch (DbUpdateConcurrencyException ex)
				//{
				//	var entry = ex.Entries.Single();
				//	var clientValues = (Client)entry.Entity;
				//	var databaseEntry = entry.GetDatabaseValues();
				//	if (databaseEntry == null)
				//	{
				//		ModelState.AddModelError(string.Empty, "Unable to save changes. The customer was deleted by another user.");
				//	}
				//	else
				//	{
				//		var databaseValues = (Client)databaseEntry.ToObject();

				//		if (databaseValues.FirstName != clientValues.FirstName)
				//			ModelState.AddModelError("FirstName", "Current database value for customer first name: " + databaseValues.FirstName);

				//		if (databaseValues.LastName != clientValues.LastName)
				//			ModelState.AddModelError("LastName", "Current database value for customer surname: " + databaseValues.LastName);

				//		ModelState.AddModelError(string.Empty, "The record you attempted to edit "
				//			+ "was modified by another user after you got the original value. The edit operation "
				//			+ "was canceled. If you still want to edit this record, click the Save button again.");
				//	}
				//}
				catch (Exception ex)
				{
					//_msg = new InfoViewModel("Error saving cleaner", ex);
					ModelState.AddModelError(string.Empty, $"Error saving customer ({ex.Message})");

					LogHelper log = new LogHelper();
					log.Log(LogHelper.LogLevels.Error, "Error saving customer", nameof(SaveClientDetails), ex, dataItem);
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(ModelState, nameof(SaveClientDetails), dataItem);
			}

			return JsonFormResponse();
		}

		private Client UpdateClient(Client _objToUpdate, ClientDetailsVM dataItem)
		{

			if (dataItem == null)
			{
				return _objToUpdate;
			}

			if (_objToUpdate == null)
			{
				_objToUpdate = new Client();
			}

			_objToUpdate.FirstName = dataItem.FirstName;
			_objToUpdate.LastName = dataItem.LastName;
			_objToUpdate.EmailAddress = dataItem.EmailAddress;
			_objToUpdate.IsActive = dataItem.IsActive;
			_objToUpdate.MobileNumber = dataItem.MobileNumber;
			_objToUpdate.OtherNumber = dataItem.OtherNumber;
			_objToUpdate.BusinessPhoneNumber = dataItem.BusinessPhoneNumber;
			_objToUpdate.ClientType = dataItem.ClientType;

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

		private JobBooking UpdateBooking(JobBooking _objToUpdate, JobBookingsVM dataItem)
		{

			if (dataItem == null)
			{
				return _objToUpdate;
			}

			if (_objToUpdate == null)
			{
				_objToUpdate = new JobBooking();
			}

			_objToUpdate.ClientRefId = dataItem.ClientId;
			_objToUpdate.PrimaryCleanerRefId = dataItem.CleanerId;
			_objToUpdate.StartTime = dataItem.StartTimeForControl.ToMinutes();
			_objToUpdate.EndTime = dataItem.EndTimeForControl.ToMinutes();
			_objToUpdate.IsActive = true;
            //_objToUpdate.JobDate = dataItem.JobDateUTC.Value;
            _objToUpdate.JobDate = dataItem.JobDate.ToUTC();
            _objToUpdate.JobStatus = BookingStatus.CONFIRMED;
			_objToUpdate.JobSuburb = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(dataItem.JobSuburb.ToLower());
			_objToUpdate.JobType = dataItem.JobType;
			_objToUpdate.WeekDay = dataItem.WeekDay;
			_objToUpdate.TeamSize = dataItem.TeamSize;

			_objToUpdate = UpdateAuditTracking(_objToUpdate);

			return _objToUpdate;
		}

		public ActionResult GetJobBookings(Guid? ClientId)
		{
			if (ClientId == null)
			{
				ModelState.AddModelError(string.Empty, $"Client Id [{ClientId.ToString()}] not provided.  Please try again.");
				return JsonFormResponse();
			}

			List<JobBookingsVM> _editList = new List<JobBookingsVM>();

			using (DBManager db = new DBManager())
			{
				StringBuilder _sql = new StringBuilder();
				_sql.Append("Select J.*, ClientRefId as ClientId, PrimaryCleanerRefId as CleanerId, JobDate as JobDateUTC, ");
				_sql.Append("(");
				_sql.Append("select group_concat(CleanerName) as TeamList from ( ");
				_sql.Append("select concat(FirstName, ' ', LastName) as CleanerName, CR.PrimaryCleanerRefId, WeekDay ");
				_sql.Append("from CleanerRoster CR ");
				_sql.Append("inner join CleanerRosteredTeam CRT on CRT.RosterRefId = CR.Id ");
				_sql.Append("inner join CleanerTeam CT on CT.Id = CRT.TeamRefId ");
				_sql.Append("where CRT.IsPrimary = 0 ");
				_sql.Append("group by FirstName, LastName, CR.PrimaryCleanerRefId, WeekDay ");
				_sql.Append("UNION ");
				_sql.Append("select concat(FirstName, ' ', LastName) as CleanerName, CR.PrimaryCleanerRefId, WeekDay ");
				_sql.Append("from CleanerRoster CR ");
				_sql.Append("inner join CleanerRosteredTeam CRT on CRT.RosterRefId = CR.Id ");
				_sql.Append("inner join Cleaners CT on CT.Id = CRT.TeamRefId ");
				_sql.Append("where CRT.IsPrimary = 1 ");
				_sql.Append("group by FirstName, LastName, CR.PrimaryCleanerRefId, WeekDay ");
				_sql.Append(") as TeamList where TeamList.PrimaryCleanerRefId = J.PrimaryCleanerRefId ");
				_sql.Append("and upper(TeamList.WeekDay) = upper(J.WeekDay) ");
				_sql.Append(") as CleanerTeam ");
				_sql.Append("from JobBooking J ");
				_sql.Append($"where J.ClientRefId = '{ClientId}' ");
				_sql.Append("order by JobDate desc, StartTime");

				_editList = db.getConnection().Query<JobBookingsVM>(_sql.ToString()).ToList();
			}

			return new JsonNetResult() { Data = new { list = _editList, nextGuid = Guid.NewGuid() }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}


		public ActionResult RefreshBookingTimes(JobBookingsVM booking)
		{
			return new JsonNetResult() { Data = new { item = booking }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpPost]
		public ActionResult SaveClientBooking(JobBookingsVM dataItem)
		{
			if (dataItem == null)
			{
				ModelState.AddModelError(string.Empty, "Valid client booking not found.");
			}

			var originalStart = dataItem.StartTime;
			var originalEnd = dataItem.EndTime;
			var newStart = dataItem.StartTimeForControl.ToMinutes();
			var newEnd = dataItem.EndTimeForControl.ToMinutes();

			if (originalStart > newStart || originalEnd < newEnd)
			{
				ModelState.AddModelError(string.Empty, "Booking times do not fall within selected available gap for cleaner.");
			}

			if (ModelState.IsValid)
			{
				var bIsNew = (dataItem.IsNewItem);

				try
				{
					JobBooking _objToUpdate = null;
					using (DBManager db = new DBManager())
					{
						if (bIsNew)
						{
							_objToUpdate = UpdateBooking(null, dataItem);
							StringBuilder _sql = new StringBuilder();

							_sql.Append("Insert into JobBooking (Id, CreatedAt, UpdatedAt, UpdatedBy, IsActive, RowVersion, ");
							_sql.Append("PrimaryCleanerRefId, ClientRefId, JobType, JobStatus, JobDate, WeekDay, StartTime, EndTime, JobSuburb, TeamSize)");
							_sql.Append(" values (");
							_sql.Append($"'{_objToUpdate.Id}',");
							_sql.Append($"'{_objToUpdate.CreatedAt.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.UpdatedBy}',");
							_sql.Append($"{_objToUpdate.IsActive},");
							_sql.Append($"'{_objToUpdate.RowVersion.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.PrimaryCleanerRefId}',");
							_sql.Append($"'{_objToUpdate.ClientRefId}',");
							_sql.Append($"'{_objToUpdate.JobType}',");
							_sql.Append($"'{_objToUpdate.JobStatus}',");
							_sql.Append($"'{_objToUpdate.JobDate.FormatDatabaseDate()}',");
							_sql.Append($"'{_objToUpdate.WeekDay}',");
							_sql.Append($"{_objToUpdate.StartTime},");
							_sql.Append($"{_objToUpdate.EndTime},");
							_sql.Append($"'{_objToUpdate.JobSuburb}',");
							_sql.Append($"{_objToUpdate.TeamSize}");
							_sql.Append(")");

							db.getConnection().Execute(_sql.ToString());
						}
						else
						{

						}
					}

					Extensions.ClearJobMatchCookies();

					return JsonSuccessResponse("Customer booking saved successfully", _objToUpdate);
				}
				catch (Exception ex)
				{
					//_msg = new InfoViewModel("Error saving cleaner", ex);
					ModelState.AddModelError(string.Empty, $"Error saving customer booking ({ex.Message})");

					LogHelper log = new LogHelper();
					log.Log(LogHelper.LogLevels.Error, "Error saving customer booking", nameof(SaveClientBooking), ex, dataItem);
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(ModelState, nameof(SaveClientBooking), dataItem);
			}

			return JsonFormResponse();
		}

		[HttpPost]
		public ActionResult DeleteJobBooking(Guid? id)
		{
			string _objDesc = "Job Booking";

			if (!id.HasValue)
			{
				ModelState.AddModelError(string.Empty, $"Valid {_objDesc.ToLower()} record not found.");
			}

			StringBuilder sql = new StringBuilder();
			try
			{
				using (DBManager db = new DBManager())
				{
					sql.Append($"delete from JobBooking where id = '{id.Value.ToString()}'");
					db.getConnection().Execute(sql.ToString());

					return JsonSuccessResponse($"{_objDesc} deleted successfully", "Id=" + id.Value.ToString());
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

		[HttpGet]
		public ActionResult GetClientPaymentMethods(Guid? ClientId)
		{
			if (ClientId == null)
			{
				ModelState.AddModelError(string.Empty, $"Client Id [{ClientId.ToString()}] not provided.  Please try again.");
				return JsonFormResponse();
			}

			List<ClientMethod> _entityList = new List<ClientMethod>();

			DefaultCrypto _encryption = new DefaultCrypto();
			var _hash = _encryption.Hash("|" + ClientId.ToString());
			if (String.IsNullOrWhiteSpace(_hash))
			{
				throw new InvalidOperationException("Error decrypting payment details.");
			}

			using (DBManager db = new DBManager())
			{
				_entityList = db.getConnection().Query<ClientMethod>($"Select * from Methods where Validated = '{_hash}' order by CreatedAt desc").ToList();
			}

			List<ClientPaymentMethodVM> _editList = new List<ClientPaymentMethodVM>();
			foreach (ClientMethod _item in _entityList)
			{
				var _vm = new ClientPaymentMethodVM();
				_vm.PopulateVM(_item, _hash);
				_editList.Add(_vm);
			}

			return new JsonNetResult() { Data = new { list = _editList }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpPost]
		public ActionResult SaveClientPaymentMethod(ClientPaymentMethodVM dataItem)
		{
			if (dataItem == null)
			{
				ModelState.AddModelError(string.Empty, "Valid client payment method not found.");
			}

			if (ModelState.IsValid)
			{
				try
				{
					StringBuilder _ccDetails = new StringBuilder();
					_ccDetails.Append("|").Append(dataItem.ClientId);
					_ccDetails.Append("|").Append(dataItem.CardCVV);
					_ccDetails.Append("|").Append(dataItem.ExpiryYear);
					_ccDetails.Append("|").Append(dataItem.ExpiryMonth);
					_ccDetails.Append("|").Append(dataItem.CardName);
					_ccDetails.Append("|").Append(dataItem.CardNumberPart3);
					_ccDetails.Append("|").Append(dataItem.CardNumberPart4);
					_ccDetails.Append("|").Append(dataItem.CardNumberPart1);
					_ccDetails.Append("|").Append(dataItem.CardNumberPart2);
					_ccDetails.Append("|").Append(dataItem.CardType);
					_ccDetails.Append("|").Append(dataItem.ClientReferenceCode);

					DefaultCrypto _crypto = new DefaultCrypto();
					var _hash = _crypto.Hash("|" + dataItem.ClientId);
					if (String.IsNullOrWhiteSpace(_hash))
					{
						throw new InvalidOperationException("Error encrypting payment details.");
					}

					ClientMethod _objToUpdate = new ClientMethod()
					{
						Details = Crypto.Encrypt(_ccDetails.ToString(), _hash),
						Id = Guid.NewGuid().ToString(),
						IsActive = true,
						Validated =  _hash,
					};
					_objToUpdate = UpdateAuditTracking(_objToUpdate);

					using (DBManager db = new DBManager())
					{
						StringBuilder _sql = new StringBuilder();
						_sql.Append("Insert into Methods (Id, CreatedAt, UpdatedAt, UpdatedBy, IsActive, RowVersion, ");
						_sql.Append("Details, Validated)");
						_sql.Append(" values (");
						_sql.Append($"'{_objToUpdate.Id}',");
						_sql.Append($"'{_objToUpdate.CreatedAt.FormatDatabaseDateTime()}',");
						_sql.Append($"'{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}',");
						_sql.Append($"'{_objToUpdate.UpdatedBy}',");
						_sql.Append($"{_objToUpdate.IsActive},");
						_sql.Append($"'{_objToUpdate.RowVersion.FormatDatabaseDateTime()}',");
						_sql.Append($"'{_objToUpdate.Details}',");
						_sql.Append($"'{_objToUpdate.Validated}'");
						_sql.Append(")");
						db.getConnection().Execute(_sql.ToString());
					}
					return JsonSuccessResponse("Customer payment method saved successfully", _objToUpdate);

				}
				//catch (DbUpdateConcurrencyException ex)
				//{
				//	var entry = ex.Entries.Single();
				//	var clientValues = (ClientMethod)entry.Entity;
				//	var databaseEntry = entry.GetDatabaseValues();
				//	if (databaseEntry == null)
				//	{
				//		ModelState.AddModelError(string.Empty, "Unable to save changes. The payment method was deleted by another user.");
				//	}
				//	else
				//	{
				//		ModelState.AddModelError(string.Empty, "The record you attempted to edit "
				//			+ "was modified by another user after you got the original value. The edit operation "
				//			+ "was canceled. If you still want to edit this record, click the Save button again.");
				//	}
				//}
				catch (Exception ex)
				{
					//_msg = new InfoViewModel("Error saving cleaner", ex);
					ModelState.AddModelError(string.Empty, $"Error saving payment method ({ex.Message})");

					LogHelper log = new LogHelper();
					log.Log(LogHelper.LogLevels.Error, "Error saving payment method", nameof(SaveClientDetails), ex, dataItem);
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(ModelState, nameof(SaveClientPaymentMethod), dataItem);
			}

			return JsonFormResponse();
		}

		[HttpPost]
		public ActionResult UpdateRefCode(ClientPaymentReferenceUpdateVM dataItem)
		{
			if (dataItem == null)
			{
				ModelState.AddModelError(string.Empty, "Valid client payment method not found.");
			}

			if (ModelState.IsValid)
			{
				ClientMethod _objToUpdate = null;
				try
				{
					using (DBManager db = new DBManager())
					{
						string sql = @"select * from Methods C  where C.ID = '" + dataItem.Id + "'";
						_objToUpdate = db.getConnection().Query<ClientMethod>(sql).SingleOrDefault();

						if (_objToUpdate == null)
						{
							ModelState.AddModelError(string.Empty, $"Payment method [{dataItem.Id.ToString()}] not found.  Please try again.");
							return JsonFormResponse();
						}

						ClientPaymentMethodVM _payment = new ClientPaymentMethodVM();
						_payment.PopulateVM(_objToUpdate, _objToUpdate.Validated);

						StringBuilder _ccDetails = new StringBuilder();
						_ccDetails.Append("|").Append(_payment.ClientId);
						_ccDetails.Append("|").Append(_payment.CardCVV);
						_ccDetails.Append("|").Append(_payment.ExpiryYear);
						_ccDetails.Append("|").Append(_payment.ExpiryMonth);
						_ccDetails.Append("|").Append(_payment.CardName);
						_ccDetails.Append("|").Append(_payment.CardNumberPart3);
						_ccDetails.Append("|").Append(_payment.CardNumberPart4);
						_ccDetails.Append("|").Append(_payment.CardNumberPart1);
						_ccDetails.Append("|").Append(_payment.CardNumberPart2);
						_ccDetails.Append("|").Append(_payment.CardType);
						_ccDetails.Append("|").Append(dataItem.ClientReferenceCode);

						DefaultCrypto _crypto = new DefaultCrypto();
						var _hash = _crypto.Hash("|" + _payment.ClientId);
						if (String.IsNullOrWhiteSpace(_hash))
						{
							throw new InvalidOperationException("Error encrypting payment details.");
						}

						_objToUpdate.Details = Crypto.Encrypt(_ccDetails.ToString(), _hash);
						_objToUpdate = UpdateAuditTracking(_objToUpdate);

						StringBuilder _sql = new StringBuilder();
						_sql.Append("Update Methods set ");
						_sql.Append($"UpdatedAt = '{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}'");
						_sql.Append($",RowVersion = '{_objToUpdate.RowVersion.FormatDatabaseDateTime()}'");
						_sql.Append($",UpdatedBy = '{_objToUpdate.UpdatedBy}'");
						_sql.Append($",IsActive = {_objToUpdate.IsActive}");
						_sql.Append($",Details = '{_objToUpdate.Details}'");
						_sql.Append($",Validated = '{_objToUpdate.Validated}'");
						_sql.Append($" where Id = '{_objToUpdate.Id}'");
						db.getConnection().Execute(_sql.ToString());
					}

					return JsonSuccessResponse("Payment method saved successfully", _objToUpdate);

				}
				//catch (DbUpdateConcurrencyException ex)
				//{
				//	var entry = ex.Entries.Single();
				//	var clientValues = (ClientMethod)entry.Entity;
				//	var databaseEntry = entry.GetDatabaseValues();
				//	if (databaseEntry == null)
				//	{
				//		ModelState.AddModelError(string.Empty, "Unable to save changes. The payment method was deleted by another user.");
				//	}
				//	else
				//	{
				//		ModelState.AddModelError(string.Empty, "The record you attempted to edit "
				//			+ "was modified by another user after you got the original value. The edit operation "
				//			+ "was canceled. If you still want to edit this record, click the Save button again.");
				//	}
				//}
				catch (Exception ex)
				{
					//_msg = new InfoViewModel("Error saving cleaner", ex);
					ModelState.AddModelError(string.Empty, $"Error saving payment method ({ex.Message})");

					LogHelper log = new LogHelper();
					log.Log(LogHelper.LogLevels.Error, "Error saving payment method", nameof(UpdateRefCode), ex, dataItem);
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(ModelState, nameof(UpdateRefCode), dataItem);
			}

			return JsonFormResponse();
		}

		[HttpPost]
		public ActionResult DeletePaymentMethod(Guid? id)
		{
			string _objDesc = "Payment Method";

			if (!id.HasValue)
			{
				ModelState.AddModelError(string.Empty, $"Valid {_objDesc.ToLower()} record not found.");
			}

			try
			{
				using (DBManager db = new DBManager())
				{
					String _sql = $"delete from methods where id = '{id}'";
					db.getConnection().Execute(_sql);
					return JsonSuccessResponse($"{_objDesc} deleted successfully", "Id = " + id);
				}
			}
			catch(Exception ex)
			{
				ModelState.AddModelError(string.Empty, $"Error deleting {_objDesc.ToLower()} ({ex.Message})");

				LogHelper log = new LogHelper();
				log.Log(LogHelper.LogLevels.Error, $"Error deleting {_objDesc.ToLower()}", nameof(LogEntry), ex, null);
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(ModelState, nameof(DeletePaymentMethod), null);
			}

			return JsonFormResponse();
		}

		#endregion

		#region Service Functions, Leave Dates
		public ActionResult GetLeaveDates(Guid? ClientId)
		{
			if (ClientId == null)
			{
				ModelState.AddModelError(string.Empty, $"Client Id [{ClientId.ToString()}] not provided.  Please try again.");
				return JsonFormResponse();
			}

			List<ClientLeave> _entityList = new List<ClientLeave>();

			using (DBManager db = new DBManager())
			{
				_entityList = db.getConnection().Query<ClientLeave>($"Select * from ClientLeave where ClientRefId = '{ClientId}' order by StartDate desc, EndDate desc").ToList();
			}

			List<ClientLeaveVM> _editList = new List<ClientLeaveVM>();
			foreach (ClientLeave _item in _entityList)
			{
				var _vm = new ClientLeaveVM();
				_vm.PopulateVM(ClientId, _item);
				_editList.Add(_vm);
			}

			return new JsonNetResult() { Data = new { list = _editList, nextGuid = Guid.NewGuid() }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}


		[HttpPost]
		public ActionResult SaveLeaveDates(ClientLeaveVM  formValues)
		{
			string _objDesc = "Customer Leave Dates";

			if (formValues == null)
			{
				ModelState.AddModelError(string.Empty, $"Valid {_objDesc.ToLower()} data not found.");
			}

			if (ModelState.IsValid)
			{
				if (formValues.StartDate.IsPastDate("ClientsController.SaveLeaveDates"))       // Not sure yet - but UTC stuffs this up and can't add "today" as start date
				{
					ModelState.AddModelError(string.Empty, "Leave start date can not be in the past.");
				}
				else
				{
					if (formValues.EndDate.IsPastDate("ClientsController.SaveLeaveDates"))
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
					ClientLeave _objToUpdate = null;

					using (DBManager db = new DBManager())
					{
						if (bIsNew)
						{
							_objToUpdate = new ClientLeave();
							_objToUpdate.ClientRefId = formValues.ClientId;
							_objToUpdate.StartDate = formValues.StartDate.ToUTC();
							_objToUpdate.EndDate = formValues.EndDate.ToUTC();

							_objToUpdate = UpdateAuditTracking(_objToUpdate);

							StringBuilder _sql = new StringBuilder();
							_sql.Append("Insert into ClientLeave (Id, CreatedAt, UpdatedAt, UpdatedBy, IsActive, RowVersion, ");
							_sql.Append("ClientRefId, StartDate, EndDate)");
							_sql.Append(" values (");
							_sql.Append($"'{_objToUpdate.Id}',");
							_sql.Append($"'{_objToUpdate.CreatedAt.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.UpdatedBy}',");
							_sql.Append($"{_objToUpdate.IsActive},");
							_sql.Append($"'{_objToUpdate.RowVersion.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.ClientRefId}',");
							_sql.Append($"'{_objToUpdate.StartDate.FormatDatabaseDateTime()}',");
							_sql.Append($"'{_objToUpdate.EndDate.FormatDatabaseDateTime()}'");
							_sql.Append(")");
							db.getConnection().Execute(_sql.ToString());
						}
						else
						{
							string sql = @"select * from ClientLeave C where C.ID = '" + _id + "'";

							_objToUpdate = db.getConnection().Query<ClientLeave>(sql).SingleOrDefault();

							if (_objToUpdate == null)
							{
								ModelState.AddModelError(string.Empty, $"{_objDesc} [{_id.ToString()}] not found.  Please try again.");
								return JsonFormResponse();
							}

							_objToUpdate.StartDate = formValues.StartDate.ToUTC();
							_objToUpdate.EndDate = formValues.EndDate.ToUTC();
							_objToUpdate = UpdateAuditTracking(_objToUpdate);

							StringBuilder _sql = new StringBuilder();
							_sql.Append("Update ClientLeave set ");
							_sql.Append($"UpdatedAt = '{_objToUpdate.UpdatedAt.FormatDatabaseDateTime()}'");
							_sql.Append($",RowVersion = '{_objToUpdate.RowVersion.FormatDatabaseDateTime()}'");
							_sql.Append($",UpdatedBy = '{_objToUpdate.UpdatedBy}'");
							_sql.Append($",IsActive = {_objToUpdate.IsActive}");
							_sql.Append($",StartDate = '{_objToUpdate.StartDate.FormatDatabaseDateTime()}'");
							_sql.Append($",EndDate = '{_objToUpdate.EndDate.FormatDatabaseDateTime()}'");
							_sql.Append($" where Id = '{_objToUpdate.Id}'");
							db.getConnection().Execute(_sql.ToString());
						}

						return JsonSuccessResponse($"{_objDesc} saved successfully", _objToUpdate);
					}
				}
				//catch (DbUpdateConcurrencyException ex)
				//{
				//	var entry = ex.Entries.Single();
				//	var clientValues = (ClientLeave)entry.Entity;
				//	var databaseEntry = entry.GetDatabaseValues();
				//	if (databaseEntry == null)
				//	{
				//		ModelState.AddModelError(string.Empty, $"Unable to save changes. The {_objDesc.ToLower()} was deleted by another user.");
				//	}
				//	else
				//	{
				//		var databaseValues = (ClientLeave)databaseEntry.ToObject();

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
					sql.Append($"delete from ClientLeave where id = '{id.Value.ToString()}'");
					db.getConnection().Execute(sql.ToString());

					return JsonSuccessResponse($"{_objDesc} deleted successfully", "Id=" + id.Value.ToString());
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
