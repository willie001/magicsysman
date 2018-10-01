(function() {
    'use strict';

    angular
    	.module("magicclients",[])
    	.controller('MasterClientController', MasterClientController)
    	.controller('MasterClientHeaderController', MasterClientHeaderController)
    	.controller('ClientSearchController', ClientSearchController)
    	.controller('ClientDetailsController', ClientDetailsController)
    	.controller('ClientPaymentController', ClientPaymentController)
    	.controller('ClientLeaveController', ClientLeaveController)
    	.controller('ClientBookingController', ClientBookingController);

    MasterClientController.$inject = ['$scope','savedJobBookingFactory'];
    MasterClientHeaderController.$inject = ['$scope','$location','$route','$state'];
    ClientSearchController.$inject = ['$scope', '$http', 'HandleBusySpinner', 'ShowUserMessages','DTOptionsBuilder','$cookies', 'moment','manageTimeZoneCookie','savedJobBookingFactory'];
    ClientDetailsController.$inject = ['$scope','$filter', '$http','$q','$location','$rootScope', '$state', 'HandleBusySpinner', 'ShowUserMessages','savedJobBookingFactory'];
    ClientPaymentController.$inject = ['$scope','$filter', '$http','$q','$location','$rootScope', '$state', 'HandleBusySpinner', 'ShowUserMessages'];
    ClientLeaveController.$inject = ['$scope','$filter','$http', 'HandleBusySpinner', 'ShowUserMessages','editableOptions', 'editableThemes'];
	ClientBookingController.$inject = ['$document','$scope','$filter', '$http','$q','$location','$rootScope', '$state', 'HandleBusySpinner', 'ShowUserMessages','savedJobBookingFactory','$timeout'];
    
    function MasterClientController($scope, savedJobBookingFactory)
    {
   		var vm = this;
   		vm.ClientId = $scope.ClientId;

    	// A parent controller is required where we use tabbing with messaging being sent from the child.
    	$scope.userMessages = [];
		$scope.userMessageType = [];

		$scope.isLoadingResults;
		$scope.DataRecordStatus = { IsNewDataRecord: true};

		$scope.selectedCleaner = savedJobBookingFactory.getCleaner();
		$scope.selectedCleanerJob = savedJobBookingFactory.getJob();
		//console.log("<Cleaner PICKED -  job> - " + angular.toJson($scope.selectedCleaner));
		
        $scope.cancelJobBooking = function() {
			savedJobBookingFactory.set(null,null);
			location.reload();
        };
    }

    function MasterClientHeaderController($scope, $location, $route, $state)
    {
		$scope.addNewClient = function() {
			$scope.userMessages = [];
			$scope.userMessageType = [];

			$scope.DataRecordStatus = { IsNewDataRecord: true};

			$state.go($state.current, {ClientId: null}, {reload: true});
		};
    }

    /***********************/
	/*** CLIENT SEARCH   ***/
	/***********************/
	function ClientSearchController($scope, $http, HandleBusySpinner, ShowUserMessages, DTOptionsBuilder, $cookies, moment, manageTimeZoneCookie, savedJobBookingFactory)
	{
		var vm = this;
		var panelName = "panelClientResults";
		manageTimeZoneCookie.set($cookies, moment, location);

		vm.Search = {};
		vm.SeachResults = {};
		vm.hasSearched = false;
		vm.Search.IncludeInactive = true;

		// track selected cleaner and job for booking underway
		vm.selectedCleanerJob = {};
		vm.selectedCleaner = {};

		$scope.userMessages = [];
		$scope.userMessageType = [];

		activate();

		function activate() {
			manageTimeZoneCookie.set($cookies, moment, location);

			HandleBusySpinner.start($scope, panelName);
			$scope.panelClientSearch = false;

			$scope.dtOptions =  DTOptionsBuilder.newOptions().withOption('order', [5, 'desc']);

			vm.selectedCleaner = savedJobBookingFactory.getCleaner();
			vm.selectedCleanerJob = savedJobBookingFactory.getJob();
			//console.log("<Cleaner PICKED -  cleaner> - " + angular.toJson(vm.selectedCleaner));
			//console.log("<JOB PICKED -  job> - " + angular.toJson(vm.selectedCleanerJob));
		}

		$scope.cancelJobBooking = function() {
			savedJobBookingFactory.set(null,null);
			location.reload();
        };

		$scope.clearForm = function() {
				vm.Search = {};
				vm.SearchResults = {};
				vm.hasSearched = false;
				$scope.panelClientSearch = false;
			}

		$scope.searchClients = function() {
				ShowUserMessages.clear($scope);

				//console.log("<CLIENT Search> - " + angular.toJson(vm.Search));
				vm.hasSearched = true;

	            HandleBusySpinner.start($scope, panelName);

				$http.post('/clients/searchclient/', vm.Search).success(function (response) {
					//console.log("<CLIENT Search Results> - " + angular.toJson(response));
					if (!response.IsValid && response.IsValid !== undefined)
					{
						HandleBusySpinner.stop($scope, panelName);
	            		ShowUserMessages.show($scope, response, "Error performing client search.");
	            		vm.hasSearched = false;
					}
					else
					{
						$scope.panelClientSearch = true;
						vm.SearchResults = response.SearchResults;
						HandleBusySpinner.stop($scope, panelName);
					}
	                
				}).error(function (error) {
	        		//console.log("<CLIENT Search Errors> - " + angular.toJson(error));
	        		HandleBusySpinner.stop($scope, panelName);
	            	ShowUserMessages.show($scope, error, "Error performing client search.");
	            	vm.hasSearched = false;
	        	});   	
			}
	}

    /**********************/
	/*** CLIENT DETAILS ***/
	/**********************/
	function ClientDetailsController($scope, $filter, $http, $q, $location, $rootScope, $state, HandleBusySpinner, ShowUserMessages, savedJobBookingFactory)
	{
		var vm = this;
		var _physicalType = 0;
		var panelName = "panelClientDetails";
		var Id = $scope.ClientId;

		if ($rootScope.childMessage)
		{
			ShowUserMessages.show($scope, $rootScope.childMessage, "Error updating details.");
            $rootScope.childMessage = null; 	
		}

		vm.client = {};
		vm.addressTypes = null;

		activate();

		function activate() {
        	HandleBusySpinner.start($scope, panelName);
        	$http.get('/clients/getclient/?ClientId=' + Id)
                .success(function (data) {
                	//console.log("<CLIENT> - " + angular.toJson(data.item));
                	//console.log("<FRANCHISE> - " + angular.toJson(data.selectedFranchise));
                	vm.client = data.item;
                	$scope.ClientId = vm.client.Id;
                	$scope.DataRecordStatus.IsNewDataRecord = data.item.IsNewItem;

                }).error(function(err) {
                	
                }).finally(function() {
                });

			$http.get('/addresses/getaddresstypesjson')
                .success(function (data) {
                	//console.log('<ADDRESS TYPES> ' + angular.toJson(data.item));
                	vm.addressTypes = data.item;

                	let result = $filter('filter')(vm.addressTypes, {name:'Physical'})[0];
                	_physicalType = result.id;

                }).error(function(err) {
                	
                }).finally(function() {
                	HandleBusySpinner.stop($scope, panelName);
                });
		}

		vm.submitted = false;
      	vm.validateInput = function(name, type) {
        	var input = vm.clientForm[name];
    		return (input.$dirty || vm.submitted) && input.$error[type];
      	};

		vm.saveData = function() {
			//console.log("<CLIENT SAVE> - " + angular.toJson(vm.client));
      		$scope.submitted = true;

			//console.log("<CLIENT Data> - " + angular.toJson(vm.client));
			if (vm.clientForm.$valid) {
                HandleBusySpinner.start($scope, panelName);
	
            	return $http.post('/clients/saveclientdetails', vm.client).success(function (response) {
            		HandleBusySpinner.stop($scope, panelName);
            		$scope.submitted = false;
	                	
        			// Add your success stuff here
                	ShowUserMessages.show($scope, response, "Error updating details.");
                	if (response.IsValid)
                	{
                		vm.client = response.DataItem;
	                	$scope.ClientId = vm.client.Id;
	                	$scope.FranchiseId = vm.client.MasterFranchiseRefId;

	        			if ($scope.DataRecordStatus.IsNewDataRecord)
	                	{
	            			$scope.DataRecordStatus.IsNewDataRecord = false;
	                	}

	                	activate();
	                	$rootScope.childMessage = response;
	                	$state.go("app.client_details", { "ClientId": $scope.ClientId });
                	}

            	}).error(function (error) {
            		console.log("<CLIENT ERROR> - " + angular.toJson(error));
            		HandleBusySpinner.stop($scope, panelName);
            		$scope.submitted = false;
                	ShowUserMessages.show($scope, error, "Error updating details.");

            	});
        	}
        	else
        	{
        		//console.log("<XX> - " + angular.toJson(vm.clientForm.$error));
            	$scope.submitted = false;
            	ShowUserMessages.show($scope, "Error updating customer details - please review validation errors", "Error updating details.");
        		return false;
        	}
		}
	}

	/**********************/
	/*** CLIENT PAYMENT ***/
	/**********************/
	function ClientPaymentController($scope, $filter, $http, $q, $location, $rootScope, $state, HandleBusySpinner, ShowUserMessages)
	{
		var vm = this;
		var panelName = "panelClientPayment";
		var ClientId = $scope.ClientId;
		vm.Cards = {};

		if ($rootScope.childMessage)
		{
			ShowUserMessages.show($scope, $rootScope.childMessage, "Error updating details.");
            $rootScope.childMessage = null; 	
		}

		activate();

		function activate() {
			var date = new Date();
			vm.cardYears = [];
			for(var i=0; i<=5; i++)
			{
				vm.cardYears.push(date.getFullYear()+i);
			}

			vm.client = {
		        CardNumber: '',
		        creditCardFormattedValue: '',
		        CardType: ''
	        }

			if (!ClientId)
				return;

			HandleBusySpinner.start($scope, panelName);
        	$http.get('/clients/getclientpaymentmethods/?ClientId=' + ClientId)
                .success(function (data) {
                	vm.Cards = data.list;
                	//console.log("<CLIENT> - " + angular.toJson(vm.Cards));
                	$scope.ClientId = ClientId;

                }).error(function(err) {
                	
                }).finally(function() {
                	HandleBusySpinner.stop($scope, panelName);
                });

		}

		/* Cleave.JS card mask formatting */

	 	$scope.onCreditCardValueChange = function(formattedValue) {
        	vm.client.creditCardFormattedValue = formattedValue;
    	};

	 	$scope.onCreditCardTypeChanged = function(type) {
   		 	vm.client.CardType = type;
    	};

	 	$scope.options = {
	        creditCard: {
	            creditCard: true,
	            onCreditCardTypeChanged: $scope.onCreditCardTypeChanged
	        }
	    };
	    /* Cleave.JS card mask formatting */
		
		$scope.deleteEntry = function(id, ix) {
			HandleBusySpinner.start($scope, panelName);
			if (confirm('Are you sure you want to delete the payment method?')) {
				return $http.post('/clients/deletepaymentmethod/?id=' + id).success(function (response) {
                		// Add your success stuff here
        				ShowUserMessages.show($scope, response, "Error deleting payment method.");

        				if (response.IsValid)
                		{	
	        				if (ix)
							{
								vm.Cards.splice(ix, 1);
							}
							else
							{
								activate();
							}
						}

        			}).error(function (error) {
        				ShowUserMessages.show($scope, error, "Error deleting payment method.");

        			}).finally(function() {
        				HandleBusySpinner.stop($scope, panelName);
        			});
			}
		}

		vm.submitted = false;
      	vm.validateInput = function(name, type) {
        	var input = vm.clientForm[name];
    		return (input.$dirty || vm.submitted) && input.$error[type];
      	};



		vm.saveData = function() {
			//console.log("<CLIENT PAYMENT SAVE> - " + angular.toJson(vm.client));
      		$scope.submitted = true;

			if (vm.clientForm.$valid) {

                HandleBusySpinner.start($scope, panelName);
				vm.client.ClientId = ClientId;
            	return $http.post('/clients/saveclientpaymentmethod', vm.client).success(function (response) {
                	// Add your success stuff here
                	HandleBusySpinner.stop($scope, panelName);
                	$scope.submitted = false;
                	ShowUserMessages.show($scope, response, "Error updating payment method details.");
                	if (response.IsValid)
            		{	
                		vm.client = response.DataItem;
                		activate();
					}

            	}).error(function (error) {
            		HandleBusySpinner.stop($scope, panelName);
            		$scope.submitted = false;
                	ShowUserMessages.show($scope, error, "Error updating payment method details.");

            	});
        	}
        	else
        	{
        		//console.log("<XX> - " + angular.toJson(vm.client.$error));
            	$scope.submitted = false;
            	ShowUserMessages.show($scope, "Error updating payment method details - please review validation errors", "Error updating payment method details.");
        		return false;
        	}
		}

		vm.saveCustomerRef = function(data, id) {

			angular.extend(data, {
					Id: id
				});

       		return $http.post('/clients/updateRefCode', data).success(function (response) {
                // Add your success stuff here
       			ShowUserMessages.show($scope, response, "Error updating customer reference.");
       			if (response.IsValid)
        		{	
           			activate();
				}

            }).error(function (error) {

                ShowUserMessages.show($scope, error, "Error updating customer reference.");

            });
        }
	}

	/**************************/
	/***    CLIENT LEAVE    ***/
	/**************************/
	function ClientLeaveController($scope, $filter, $http, HandleBusySpinner, ShowUserMessages, editableOptions, editableThemes)
	{
		var vm = this;
		var panelName = "panelClientDetails";
		var ClientId = $scope.ClientId;

		activate();

		function activate()
		{
			vm.listOfLeave = [];

			editableOptions.theme = 'bs3';

          	editableThemes.bs3.inputClass = 'input-sm';
          	editableThemes.bs3.buttonsClass = 'btn-sm';
          	editableThemes.bs3.submitTpl = '<button type="submit" class="btn btn-success"><span class="fa fa-check"></span></button>';
            editableThemes.bs3.cancelTpl = '<button type="button" class="btn btn-default" ng-click="$form.$cancel()">'+
                                           '<span class="fa fa-times text-muted"></span>'+
                                         '</button>';

			loadLeaveDates();

		}

		function loadLeaveDates()
		{
			if (!ClientId)
				return;

			HandleBusySpinner.start($scope, panelName);
			
			$http.get('/clients/getleavedates?ClientId='+ClientId)
                .success(function (data) {
                	vm.listOfLeave = data.list;
                	vm.nextNewGuid = data.nextGuid;

                }).error(function(err) {

                }).finally(function() {

                	//console.log("<LEAVE loaded PRE> - " + angular.toJson(vm.listOfLeave));
					angular.forEach(vm.listOfLeave, function(value, key) {
						value.StartDate = new Date(value.StartDate);
						value.EndDate = new Date(value.EndDate);
						
					});
                	//console.log("<LEAVE loaded POST> - " + angular.toJson(vm.listOfLeave));
                	HandleBusySpinner.stop($scope, panelName);
			
                });
		};

		vm.addData = function() {
          	vm.inserted = {
          	  Id: vm.nextNewGuid,
	          StartDate: '',
	          EndDate: '',
	          IsNewItem: true
	        };

            vm.listOfLeave.unshift(vm.inserted);
      	};

		vm.validateData = function(data, colName) {
			//console.log("<LEAVE validate> - " + angular.toJson(data));
			if (data.length == 0) {
              return colName + ' is mandatory';
            }
      	};

      	$scope.isFutureLeave = function(endDate) {
      		var today = new Date().getTime(),
        		idate = new Date(endDate).getTime();

    		return (today - idate) < 0 ? true : false;
      	};

      	vm.removeLeave = function(id, ix) {
			HandleBusySpinner.start($scope, panelName);
			if (confirm('Are you sure you want to delete the leave dates?')) {
	       			return $http.post('/clients/DeleteLeaveDates/?id=' + id).success(function (response) {
                		// Add your success stuff here
        				ShowUserMessages.show($scope, response, "Error deleting leave dates.");

        				if (response.IsValid)
                		{	
	        				if (ix)
							{
								vm.listOfLeave.splice(ix, 1);
							}
							else
							{
								loadLeaveDates();
							}
						}

        			}).error(function (error) {
        				ShowUserMessages.show($scope, error, "Error deleting leave dates.");

        			}).finally(function() {
        				HandleBusySpinner.stop($scope, panelName);
        			});
	        	}
			

		}

      	vm.saveData = function(data, id, isNew) {
			//console.log("<LEAVE SAVE> - " + angular.toJson(data));
          	angular.extend(data, {
					ClientId: ClientId,
					Id: id,
					IsNewItem: isNew
				});

       		return $http.post('/clients/saveleavedates', data).success(function (response) {
                // Add your success stuff here
            	//console.log("<LEAVE response post> - " + angular.toJson(response));
       			ShowUserMessages.show($scope, response, "Error updating leave dates.");
       			if (response.IsValid)
        		{	
           			loadLeaveDates();	
				}

            }).error(function (error) {

                ShowUserMessages.show($scope, error, "Error updating leave dates.");
				
            });
        }

	}

	/***************************/
	/*** CLIENT JOB BOOKINGS ***/
	/***************************/
	function ClientBookingController($document, $scope, $filter, $http, $q, $location, $rootScope, $state, HandleBusySpinner, ShowUserMessages, savedJobBookingFactory, $timeout)
	{
		var vm = this;
		var ClientId = $scope.ClientId;
		var panelName = "panelClientDetails";
		var panelBookingInfo = "panelBookingInfo";
		vm.JobBookings = {};
		vm.isMeridian = true;
    	vm.hrStep = 1;
    	vm.minStep = 5;

		if ($rootScope.childMessage)
		{
			ShowUserMessages.show($scope, $rootScope.childMessage, "Error updating details.");
            $rootScope.childMessage = null; 	
		}

		activate();

		vm.selectedCleaner = savedJobBookingFactory.getCleaner();
		vm.selectedCleanerJob = savedJobBookingFactory.getJob();
		vm.searchCriteria = savedJobBookingFactory.getCriteria();

		if (vm.selectedCleaner)
		{
			vm.minSelection = vm.selectedCleanerJob.StartTimeForControl;
		}
		if (vm.selectedCleanerJob)
		{
			vm.maxSelection = vm.selectedCleanerJob.EndTimeForControl;
		}

		//console.log("<Cleaner PICKED -  job> - " + angular.toJson(vm.selectedCleaner));
		//console.log("<Job PICKED -  job> - " + angular.toJson(vm.selectedCleanerJob));
		//console.log("<Criteria PICKED -  job> - " + angular.toJson(vm.searchCriteria));

		function removeDisabledInTimepicker() {
	     	return $timeout(function () {
         		$document.find('.uib-time input').removeAttr('disabled');
	     	}, 0);
	   	}

		removeDisabledInTimepicker();

		function activate() {
			if (!ClientId)
				return;

			HandleBusySpinner.start($scope, panelName);
		}

		vm.saveData = function() {
			vm.selectedCleanerJob.ClientId = $scope.ClientId;
			vm.selectedCleanerJob.TeamSize = vm.selectedCleaner.TeamSize;
			vm.selectedCleanerJob.JobSuburb  = vm.searchCriteria.Suburb;
      		//console.log("<JOB Data> - " + angular.toJson(vm.selectedCleaner));
		 	$scope.submitted = true;

            HandleBusySpinner.start($scope, panelName);

        	return $http.post('/clients/saveclientbooking/', vm.selectedCleanerJob).success(function (response) {
            	// Add your success stuff here
            	HandleBusySpinner.stop($scope, panelName);
            	$scope.submitted = false;
            	ShowUserMessages.show($scope, response, "Error updating booking details.");

            	if (response.IsValid)
        		{	
            		vm.selectedCleanerJob = response.DataItem;
				}

        	}).error(function (error) {
        		HandleBusySpinner.stop($scope, panelName);
        		$scope.submitted = false;
            	ShowUserMessages.show($scope, error, "Error updating booking details.");

        	});

      	}
	}

})();
