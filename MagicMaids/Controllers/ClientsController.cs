#region Using
using MagicMaids.DataAccess;
using MagicMaids.EntityModels;
using MagicMaids.Validators;
using MagicMaids.ViewModels;

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Text;
using System.Linq;
using System.Web.Mvc;

using FluentValidation.Mvc;

using NLog;

using AutoMapper;
using MagicMaids.Security;
#endregion

namespace MagicMaids.Controllers
{
	public class ClientsController : BaseController
	{
		#region Fields
		#endregion

		#region Constructor
		public ClientsController(MagicMaidsContext dbContext) : base(dbContext)
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

		public ActionResult ClientBookings()
		{
			return View();
		}
		#endregion

		#region Service Functions
		[HttpPost]
		[ValidateAntiForgeryHeader]
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
					var _results = MMContext.Clients.AsQueryable()
							.Include(nameof(Client.PhysicalAddress));
					if (_results != null)
					{
						if (!String.IsNullOrWhiteSpace(searchCriteria.Name))
						{
							_results = _results.Where(x => x.FirstName.ToLower().StartsWith(searchCriteria.Name.ToLower()) || x.LastName.ToLower().StartsWith(searchCriteria.Name.ToLower()));
						}
						if (!String.IsNullOrWhiteSpace(searchCriteria.Phone))
						{
							_results = _results.Where(x => x.BusinessPhoneNumber.StartsWith(searchCriteria.Phone) ||
													  x.MobileNumber.StartsWith(searchCriteria.Phone) ||
													  x.OtherNumber.StartsWith(searchCriteria.Phone));
						}
						if (!String.IsNullOrWhiteSpace(searchCriteria.Address))
						{
							_results = _results.Where(x => x.PhysicalAddress.AddressLine1.ToLower().Contains(searchCriteria.Address.ToLower()) ||
							                          x.PhysicalAddress.AddressLine2.ToLower().Contains(searchCriteria.Address.ToLower()) ||
							                          x.PhysicalAddress.AddressLine3.ToLower().Contains(searchCriteria.Address.ToLower()) ||
							                          x.PhysicalAddress.State.ToLower().Contains(searchCriteria.Address.ToLower()) ||
							                          x.PhysicalAddress.Country.ToLower().Contains(searchCriteria.Address.ToLower()));
						}
						if (!String.IsNullOrWhiteSpace(searchCriteria.Suburb))
						{
							_results = _results.Where(x => x.PhysicalAddress.Suburb.ToLower().Contains(searchCriteria.Suburb.ToLower()) ||
													  x.PhysicalAddress.PostCode == searchCriteria.Suburb);
						}
						if (!searchCriteria.IncludeInactive)
						{
							_results = _results.Where(x => x.IsActive == true);
						}

						if (!String.IsNullOrWhiteSpace(searchCriteria.Cleaner))
						{
							// todo add sub query
							// https://stackoverflow.com/questions/2066084/in-operator-in-linq?answertab=active#tab-top
							// https://stackoverflow.com/questions/23685375/subquery-with-entity-framework


						}
					};

					var _orderedResults = _results.OrderBy(f => new { f.LastName, f.FirstName })
							   .ToList();

					var _vmResults = Mapper.Map<List<Client>, List<ClientDetailsVM>>(_orderedResults);

					return new JsonNetResult() { Data = new { SearchResults = _vmResults }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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
				_client = MMContext.Clients
						  	.Where(f => f.Id == ClientId)
						  	.Include(nameof(Cleaner.PhysicalAddress))
						  	.Include(nameof(Cleaner.PostalAddress))
							.FirstOrDefault();

				if (_client == null)
				{
					ModelState.AddModelError(string.Empty, $"Customer [{ClientId.ToString()}] not found.  Please try again.");
					return JsonFormResponse();
				}

				_dataItem = new ClientDetailsVM();
				_dataItem.PopulateVM(_client);
				_dataItem.IsNewItem = false;

			}

			return new JsonNetResult() { Data = new { item = _dataItem, selectedFranchise = _selectedFranchise }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpPost]
		[ValidateAntiForgeryHeader]
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

					if (bIsNew)
					{
						_objToUpdate = new Client();

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

						MMContext.Entry(_objToUpdate).State = EntityState.Added;
					}
					else
					{
						_objToUpdate = MMContext.Clients
								 .Where(f => f.Id == _id)
										  .Include(nameof(Cleaner.PhysicalAddress))
										  .Include(nameof(Cleaner.PostalAddress))
										  .FirstOrDefault();

						if (_objToUpdate == null)
						{
							ModelState.AddModelError(string.Empty, $"Customer [{_id.ToString()}] not found.  Please try again.");
							return JsonFormResponse();
						}

						MMContext.Entry(_objToUpdate).CurrentValues.SetValues(dataItem);
						MMContext.Entry(_objToUpdate.PhysicalAddress).CurrentValues.SetValues(dataItem.PhysicalAddress);
						MMContext.Entry(_objToUpdate.PostalAddress).CurrentValues.SetValues(dataItem.PostalAddress);
					}

					MMContext.SaveChanges();

					return JsonSuccessResponse("Customer saved successfully", _objToUpdate);
				}
				catch (DbUpdateConcurrencyException ex)
				{
					var entry = ex.Entries.Single();
					var clientValues = (Client)entry.Entity;
					var databaseEntry = entry.GetDatabaseValues();
					if (databaseEntry == null)
					{
						ModelState.AddModelError(string.Empty, "Unable to save changes. The customer was deleted by another user.");
					}
					else
					{
						var databaseValues = (Client)databaseEntry.ToObject();

						if (databaseValues.FirstName != clientValues.FirstName)
							ModelState.AddModelError("FirstName", "Current database value for customer first name: " + databaseValues.FirstName);

						if (databaseValues.LastName != clientValues.LastName)
							ModelState.AddModelError("LastName", "Current database value for customer surname: " + databaseValues.LastName);

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

		[HttpGet]
		public ActionResult GetClientPaymentMethods(Guid? ClientId)
		{
			if (ClientId == null)
			{
				ModelState.AddModelError(string.Empty, $"Client Id [{ClientId.ToString()}] not provided.  Please try again.");
				return JsonFormResponse();
			}

			List<ClientMethod> _entityList = new List<ClientMethod>();

			MagicMaids.Security.DefaultCrypto _encryption = new MagicMaids.Security.DefaultCrypto();
			var _hash = _encryption.Hash("|" + ClientId.ToString());
			if (String.IsNullOrWhiteSpace(_hash))
			{
				throw new InvalidOperationException("Error decrypting payment details.");
			}

			_entityList = MMContext.ClientMethods
                       .Where(p => p.Validated ==  _hash)
					   .OrderByDescending(p => p.CreatedAt)
					   .ToList();

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
		[ValidateAntiForgeryHeader]
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

					DefaultCrypto _crypto = new DefaultCrypto();
					var _hash = _crypto.Hash("|" + dataItem.ClientId);
					if (String.IsNullOrWhiteSpace(_hash))
					{
						throw new InvalidOperationException("Error encrypting payment details.");
					}

					ClientMethod _objToUpdate = new ClientMethod()
					{
						Details = Crypto.Encrypt(_ccDetails.ToString(), _hash),
						Id = Guid.NewGuid(),
						IsActive = true,
						Validated =  _hash,
					};

					MMContext.Entry(_objToUpdate).State = EntityState.Added;
					MMContext.SaveChanges();

					return JsonSuccessResponse("Customer payment method saved successfully", _objToUpdate);
				}
				catch (DbUpdateConcurrencyException ex)
				{
					var entry = ex.Entries.Single();
					var clientValues = (ClientMethod)entry.Entity;
					var databaseEntry = entry.GetDatabaseValues();
					if (databaseEntry == null)
					{
						ModelState.AddModelError(string.Empty, "Unable to save changes. The payment method was deleted by another user.");
					}
					else
					{
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
		[ValidateAntiForgeryHeader]
		public ActionResult DeletePaymentMethod(Guid? id)
		{
			string _objDesc = "Payment Method";

			if (!id.HasValue)
			{
				ModelState.AddModelError(string.Empty, $"Valid {_objDesc.ToLower()} record not found.");
			}

			try
			{
				var _objToDelete = MMContext.ClientMethods
						.Where(f => f.Id == id.Value)
                        .FirstOrDefault();

				if (_objToDelete != null)
				{
					MMContext.Entry(_objToDelete).State = EntityState.Deleted;
					MMContext.SaveChanges();
				}

				return JsonSuccessResponse($"{_objDesc} deleted successfully", _objToDelete);
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

			_entityList = MMContext.ClientLeave
				   	.Where(p => p.ClientRefId == ClientId)
					   .OrderByDescending(p => p.StartDate)
					.ThenByDescending(p => p.EndDate)
					   .ToList();

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
		[ValidateAntiForgeryHeader]
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

					if (bIsNew)
					{
						_objToUpdate = new ClientLeave();
						_objToUpdate.ClientRefId = formValues.ClientId ;
						_objToUpdate.StartDate = formValues.StartDate;
						_objToUpdate.EndDate = formValues.EndDate;

						MMContext.Entry(_objToUpdate).State = EntityState.Added;
					}
					else
					{
						_objToUpdate = MMContext.ClientLeave
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
					var clientValues = (ClientLeave)entry.Entity;
					var databaseEntry = entry.GetDatabaseValues();
					if (databaseEntry == null)
					{
						ModelState.AddModelError(string.Empty, $"Unable to save changes. The {_objDesc.ToLower()} was deleted by another user.");
					}
					else
					{
						var databaseValues = (ClientLeave)databaseEntry.ToObject();

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
				var objToDelete = MMContext.ClientLeave.FirstOrDefault(l => l.Id == id.Value);
				if (objToDelete != null)
				{
					MMContext.ClientLeave.Remove(objToDelete);
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
