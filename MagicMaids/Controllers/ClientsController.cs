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

using NLog;

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
					using (IDbConnection db = MagicMaidsInitialiser.getConnection())
					{
						StringBuilder sql = new StringBuilder(@"select * from Clients C 
							 	inner join Addresses Ph on C.PhysicalAddressRefId = Ph.ID where 1=1");
						
						if (!String.IsNullOrWhiteSpace(searchCriteria.Name))
						{
							sql.Append($" and (C.FirstName like '{searchCriteria.Name}%' or C.LastName like '{searchCriteria.Name}%')" );
						}

						if (!String.IsNullOrWhiteSpace(searchCriteria.Phone))
						{
							sql.Append($" and (C.BusinessPhoneNumber like '{searchCriteria.Phone}%' or C.MobileNumber like '{searchCriteria.Phone}%' or C.OtherNumber like '{searchCriteria.Phone}%')");
						}

						if (!String.IsNullOrWhiteSpace(searchCriteria.Address))
						{
							sql.Append($" and (Ph.AddressLine1 like '%{searchCriteria.Address}%'");
							sql.Append($" or Ph.AddressLine2 like '{searchCriteria.Address}%'");
							sql.Append($" or Ph.AddressLine3 like '{searchCriteria.Address}%'");
							sql.Append($" or Ph.State like '{searchCriteria.Address}%'");
							sql.Append($" or Ph.Country like '{searchCriteria.Address}%')");
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
							// todo add sub query
							// https://stackoverflow.com/questions/2066084/in-operator-in-linq?answertab=active#tab-top
							// https://stackoverflow.com/questions/23685375/subquery-with-entity-framework
						}

						sql.Append(" order by C.LastName, C.FirstName ");

						var _orderedResults = db.Query<Client, Address, Client>(sql.ToString(), (clnr, phys) => {
							clnr.PhysicalAddress = phys;
							return clnr;
						}).ToList();

						var _vmResults = Mapper.Map<List<Client>, List<ClientDetailsVM>>(_orderedResults);

						return new JsonNetResult() { Data = new { SearchResults = _vmResults }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
					}
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, $"Error performing customer search ({ex.Message})");

					LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
					log.Log(LogLevel.Error, "Error performing customer search", nameof(SearchClient), ex, null);
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(SearchClient), null);
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
				_dataItem.IsNewItem = true;
				_dataItem.IsActive = true;
				_dataItem.PhysicalAddress = new UpdateAddressViewModel() { AddressType = AddressTypeSetting.Physical };
				_dataItem.PostalAddress = new UpdateAddressViewModel() { AddressType = AddressTypeSetting.Postal };
			}
			else
			{
				using (IDbConnection db = MagicMaidsInitialiser.getConnection())
				{
					string sql = @"select * from Clients C 
							 	inner join Addresses Ph on C.PhysicalAddressRefId = Ph.ID
								inner join Addresses Po on C.PostalAddressRefId = Po.ID
								where C.ID = '" + ClientId + "'";

					_client = db.Query<Client, Address, Address, Client>(sql, (cl, phys, post) => {
						cl.PhysicalAddress = phys;
						cl.PostalAddress = post;
						return cl;
					}).SingleOrDefault();

					if (_client == null)
					{
						ModelState.AddModelError(string.Empty, $"Customer [{ClientId.ToString()}] not found.  Please try again.");
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
					Client _objToUpdate = null;

					using (IDbConnection db = MagicMaidsInitialiser.getConnection())
					{
						if (bIsNew)
						{
							_objToUpdate = UpdateClient(null, dataItem);
							var newId = db.Insert(_objToUpdate); 
						}
						else
						{
							string sql = @"select * from Clients C 
							 	inner join Addresses Ph on C.PhysicalAddressRefId = Ph.ID
								inner join Addresses Po on C.PostalAddressRefId = Po.ID
								where C.ID = '" + _id + "'";

							_objToUpdate = db.Query<Client, Address, Address, Client>(sql, (clnt, phys, post) => {
								clnt.PhysicalAddress = phys;
								clnt.PostalAddress = post;
								return clnt;
							}).SingleOrDefault();

							if (_objToUpdate == null)
							{
								ModelState.AddModelError(string.Empty, $"Customer [{_id.ToString()}] not found.  Please try again.");
								return JsonFormResponse();
							}


							_objToUpdate = UpdateClient(_objToUpdate, dataItem);

							db.Update(dataItem);
							db.Update(dataItem.PhysicalAddress);
							db.Update(dataItem.PostalAddress);
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

					LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
					log.Log(LogLevel.Error, "Error saving customer", nameof(SaveClientDetails), ex, dataItem);
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(SaveClientDetails), dataItem);
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

			return UpdateAuditTracking(_objToUpdate);
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

			using (IDbConnection db = MagicMaidsInitialiser.getConnection())
			{
				_entityList = db.Query<ClientMethod>($"Select * from Methods where Validated = '{_hash}' order by CreatedAt").ToList();
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

					using (IDbConnection db = MagicMaidsInitialiser.getConnection())
					{
						var newId = db.Insert(UpdateAuditTracking(_objToUpdate)); 
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

					LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
					log.Log(LogLevel.Error, "Error saving payment method", nameof(SaveClientDetails), ex, dataItem);
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(SaveClientPaymentMethod), dataItem);
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
					using (IDbConnection db = MagicMaidsInitialiser.getConnection())
					{
						_objToUpdate = db.Get<ClientMethod>(new { Id = dataItem.Id });

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

						db.Update(UpdateAuditTracking(_objToUpdate));
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

					LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
					log.Log(LogLevel.Error, "Error saving payment method", nameof(UpdateRefCode), ex, dataItem);
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(UpdateRefCode), dataItem);
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
				using (IDbConnection db = MagicMaidsInitialiser.getConnection())
				{
					
					db.Delete<ClientMethod>(id);
					return JsonSuccessResponse($"{_objDesc} deleted successfully", "Id = " + id);
				}
			}
			catch(Exception ex)
			{
				ModelState.AddModelError(string.Empty, $"Error deleting {_objDesc.ToLower()} ({ex.Message})");

				LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
				log.Log(LogLevel.Error, $"Error deleting {_objDesc.ToLower()}", nameof(LogEntry), ex, null);
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(DeletePaymentMethod), null);
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

			using (IDbConnection db = MagicMaidsInitialiser.getConnection())
			{
				_entityList = db.Query<ClientLeave>($"Select * from ClientLeave where ClientRefId = '{ClientId}' order by StartDate desc, EndDate desc").ToList();
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
				Guid _id = formValues.Id;
				var bIsNew = formValues.IsNewItem;

				try
				{
					ClientLeave _objToUpdate = null;

					using (IDbConnection db = MagicMaidsInitialiser.getConnection())
					{
						if (bIsNew)
						{
							_objToUpdate = new ClientLeave();
							_objToUpdate.ClientRefId = formValues.ClientId.ToString();
							_objToUpdate.StartDate = formValues.StartDate;
							_objToUpdate.EndDate = formValues.EndDate;

							var newId = db.Insert(UpdateAuditTracking(_objToUpdate));
						}
						else
						{
							_objToUpdate = db.Get<ClientLeave>(new {Id = _id});
							if (_objToUpdate == null)
							{
								ModelState.AddModelError(string.Empty, $"{_objDesc} [{_id.ToString()}] not found.  Please try again.");
								return JsonFormResponse();
							}

							db.Update(UpdateAuditTracking(_objToUpdate));
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

					LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
					log.Log(LogLevel.Error, $"Error saving {_objDesc.ToLower()}", nameof(SaveLeaveDates), ex, formValues, Helpers.ParseValidationErrors(ex));
				}
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(SaveLeaveDates), formValues);
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

			try
			{
				using (IDbConnection db = MagicMaidsInitialiser.getConnection())
				{
					db.Delete<ClientLeave>(new {Id = id.Value});
					return JsonSuccessResponse($"{_objDesc} deleted successfully", "Id=" + id.Value);
				}
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
