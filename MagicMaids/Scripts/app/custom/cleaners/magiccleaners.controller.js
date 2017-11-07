(function() {
    'use strict';

    angular
    	.module("magiccleaners",[])
    	.controller('MasterCleanerController', MasterCleanerController)
    	.controller('MasterCleanerHeaderController', MasterCleanerHeaderController)
    	.controller('CleanerSearchController', CleanerSearchController)
    	.controller('CleanerDetailsController', CleanerDetailsController)
    	.controller('CleanerAddressController', CleanerAddressController)
		.controller('CleanerAvailabilityController', CleanerAvailabilityController);

	MasterCleanerController.$inject = ['$scope'];
    MasterCleanerHeaderController.$inject = ['$scope','$location','$route','$state'];
    CleanerSearchController.$inject = ['$scope', '$http', 'HandleBusySpinner', 'ShowUserMessages','DTOptionsBuilder'];
    CleanerDetailsController.$inject = ['$scope','$filter', '$http','$q','$location','$rootScope', '$state', 'HandleBusySpinner', 'ShowUserMessages', 'ngDialog'];
    CleanerAddressController.$inject = [];
	CleanerAvailabilityController.$inject = [];

	function MasterCleanerController($scope)
    {
   		var vm = this;

    	// A parent controller is required where we use tabbing with messaging being sent from the child.
    	$scope.userMessages = [];
		$scope.userMessageType = [];

		$scope.isLoadingResults;
		$scope.DataRecordStatus = { IsNewDataRecord: true};

		/*$scope.loadTeam = function() {
			$scope.$broadcast('loadTeam');
		}*/
    }

    function MasterCleanerHeaderController($scope, $location, $route, $state)
    {
		$scope.addNewCleaner = function() {
			$scope.userMessages = [];
			$scope.userMessageType = [];

			$scope.DataRecordStatus = { IsNewDataRecord: true};

			$state.go($state.current, {CleanerId: null}, {reload: true});
		};
    }

    /***********************/
	/*** CLEANER SEARCH  ***/
	/***********************/
	function CleanerSearchController($scope, $http, HandleBusySpinner, ShowUserMessages, DTOptionsBuilder)
	{
		var vm = this;
		var panelName = "panelCleanerResults";
	
		vm.Search = {};
		vm.SeachResults = {};
		vm.Search.SelectedFranchise = {};
		vm.hasSearched = false;

		activate();

		function activate() {
			HandleBusySpinner.start($scope, panelName);

			$http.get('/settings/getactivefranchises')
                .success(function (data) {
                	//console.log('<ACTIVE FRANCHISES> ' + angular.toJson(data.list));
                	vm.availableFranchises = data.list;

                }).error(function(err) {
                	
                }).finally(function() {
                	HandleBusySpinner.stop($scope, panelName);
                });
		}

	 $scope.clearForm = function() {
			vm.Search = {};
			vm.SearchResults = {};
			vm.hasSearched = false;
		}

	$scope.searchCleaners = function() {
			//console.log("<CLEANER Search> - " + angular.toJson(vm.Search));
			vm.hasSearched = true;

			if (vm.Search.SelectedFranchise)
			{
				vm.Search.SelectedFranchiseId = vm.Search.SelectedFranchise.Id;
            }

            HandleBusySpinner.start($scope, panelName);

			$http.post('/cleaners/searchcleaner/', vm.Search).success(function (response) {
				//console.log("<CLEANER Search Results> - " + angular.toJson(response.SearchResults));
				vm.SearchResults = response.SearchResults;
				HandleBusySpinner.stop($scope, panelName);
                
			}).error(function (error) {
        		HandleBusySpinner.stop($scope, panelName);
            	ShowUserMessages.show($scope, error, "Error performing cleaner search.");
        	});   	
		}
	}

    /***********************/
	/*** CLEANER DETAILS ***/
	/***********************/
	function CleanerDetailsController($scope, $filter, $http, $q, $location, $rootScope, $state, HandleBusySpinner, ShowUserMessages, ngDialog)
	{
		var vm = this;
		var _postalType = 1;
		var _physicalType = 0;
		var panelName = "panelCleanerDetails";
		var Id = $scope.CleanerId;

		if ($rootScope.childMessage)
		{
			ShowUserMessages.show($scope, $rootScope.childMessage, "Error updating details.");
            $rootScope.childMessage = null; 	
		}

		vm.cleaner = {};
		vm.cleanerTeam = {};
		vm.cleaner.SelectedFranchise = {};
		vm.addressTypes = null;

		activate();

		function activate()
		{
        	HandleBusySpinner.start($scope, panelName);
        	$http.get('/cleaners/getcleaner/?CleanerId=' + Id)
                .success(function (data) {
                	//console.log("<CLEANER> - " + angular.toJson(data.item));
                	//console.log("<FRANCHISE> - " + angular.toJson(data.selectedFranchise));
                	vm.cleaner = data.item;
                	$scope.CleanerId = vm.cleaner.Id;
                	$scope.DataRecordStatus.IsNewDataRecord = data.item.IsNewItem;

                	if (data.selectedFranchise)
                	{
                		vm.cleaner.SelectedFranchise = data.selectedFranchise;
                	}

                }).error(function(err) {
                	
                }).finally(function() {
                	if ($scope.DataRecordStatus.IsNewDataRecord == true)
					{
						$http.get('/cleaners/getnextcleanercode/')
			                .success(function (data) {
			                	//console.log('<CLEANER CODE> ' + angular.toJson(data));
			                	vm.cleaner.CleanerCode = data.item;

			                }).error(function(err) {
			                	
			                }).finally(function() {
			                	
			                });
					};

					loadTeamMembers();
                });

			$http.get('/settings/getactivefranchises')
                .success(function (data) {
                	//console.log('<ACTIVE FRANCHISES> ' + angular.toJson(data.list));
                	vm.availableFranchises = data.list;

                }).error(function(err) {
                	
                }).finally(function() {

                });
               
			$http.get('/addresses/getaddresstypesjson')
                .success(function (data) {
                	//console.log('<ADDRESS TYPES> ' + angular.toJson(data.item));
                	vm.addressTypes = data.item;

                	var result = $filter('filter')(vm.addressTypes, {name:'Postal'})[0];
                	_postalType = result.id;

                	result = $filter('filter')(vm.addressTypes, {name:'Physical'})[0];
                	_physicalType = result.id;

                }).error(function(err) {
                	
                }).finally(function() {
                	HandleBusySpinner.stop($scope, panelName);
                });
		}

		function loadTeamMembers()
		{
			$http.get('/cleaners/getcleanerteam/?CleanerId=' + vm.cleaner.Id)
                .success(function (data) {
                	//console.log('<TEAM> ' + angular.toJson(data.list));
                	vm.cleanerTeam = data.list;

                }).error(function(err) {
                	
                }).finally(function() {
                	

                });
		}

		$scope.openTeamPopupForm = function (item) {
			//console.log("<CLEANER VM> - " + angular.toJson(item));

			$scope.teamMember = {};
			$scope.teamMember.IsNewItem = true;
			$scope.teamMember.PhysicalAddress = {};
			$scope.teamMember.PostalAddress = {};

			console.log("<CLEANER scope> - " + angular.toJson($scope.teamMember));

			if (item)
			{
				angular.extend($scope.teamMember, item);
				$scope.teamMember.IsNewItem = false;
			} else {
				angular.extend($scope.teamMember, {
						PrimaryCleanerRefId: vm.cleaner.Id,
						IsActive: true
					});

					if ($scope.teamMember.PostalAddress.State == null && $scope.teamMember.PhysicalAddress.State == null)
					{
						$scope.teamMember.PostalAddress = angular.copy(vm.cleaner.PhysicalAddress);
						$scope.teamMember.PhysicalAddress = angular.copy(vm.cleaner.PhysicalAddress);

						// new addresses need new GUID. Currently set to parent address ID
						// so it will have to be reset in save
						$scope.teamMember.PostalAddress.IsNewItem =  true;
						$scope.teamMember.PhysicalAddress.IsNewItem = true;
					}
			}
			//console.log("<CLEANER VM> - " + angular.toJson(vm.cleaner));
			//console.log("<TEAM EDIT> - " + angular.toJson($scope.teamMember));
				
			ngDialog.open({
              template: '/views/Cleaners/CleanerTeam.html',
              className: 'ngdialog-theme-default custom-width-900',
              height: '700px',
              scope: $scope,
              cache: false,
              preCloseCallback: function(value) {
	              	if (value=='confirm')
	                {
			       		if (confirm('Are you sure you want to cancel?')) {
			       			HandleBusySpinner.start($scope, panelName);
			       			loadTeamMembers();
			       			HandleBusySpinner.stop($scope, panelName);
	        	
			            	return true;
			        	}
			        	else
			        	{
			        		return false;
			        	}
	        		}

	        		HandleBusySpinner.start($scope, panelName);
	       			loadTeamMembers();

	       			HandleBusySpinner.stop($scope, panelName);
        			return true;
		    	},
            });
          };

		$scope.saveTeamPopupForm = function () {
			//console.log('<TEAM SAVE> ' + angular.toJson($scope.teamMember));
			return $http.post('/cleaners/saveteamMember', $scope.teamMember).success(function (response) {
	            // Add your success stuff here
	        	//console.log("<TEAM SAVE response post> - " + angular.toJson(response));
	   			ShowUserMessages.show($scope, response, "Error updating team member.");
	   			vm.isSaving = true;

	   			ngDialog.close(); 

	        }).error(function (error) {

	        	//console.log("<TEAM SAVE error post> - " + angular.toJson(error));
	            ShowUserMessages.show($scope, error, "Error updating team member.");

	        });
		};

		$scope.deleteTeamMember = function(id, ix, name) {
			if (confirm('Are you sure you want to delete the record for ' + name + '?')) {
				HandleBusySpinner.start($scope, panelName);
			
				return $http.post('/cleaners/deleteteamMember/?CleanerId=' + id).success(function (response) {
	                	// Add your success stuff here
	                	//console.log("<TEAM Delete Success> - " + angular.toJson(response));

	                	ShowUserMessages.show($scope, response, "Error deleting team member.");
	                	
	            	}).error(function (error) {
	            		//console.log("<TEAM Delete Error> - " + angular.toJson(error));
	        			ShowUserMessages.show($scope, error, "Error deleting team member.");

				 	}).finally(function() {
				 		//console.log("<TEAM Delete Finally>");
	        			
				 		HandleBusySpinner.stop($scope, panelName);
				 		if (ix)
						{
							vm.cleanerTeam.splice(ix, 1);
						}
						else
						{
							activate();
						}
	            	});
			}
		}


		vm.submitted = false;
      	vm.validateInput = function(name, type) {
        	var input = vm.cleanerForm[name];
    		return (input.$dirty || vm.submitted) && input.$error[type];
      	};

      	vm.saveData = function() {
      		//console.log("<CLEANER SelectedFranchise> - " + angular.toJson(vm.cleaner.SelectedFranchise));
      		$scope.submitted = true;

      		if (vm.cleaner.SelectedFranchise && vm.cleaner.SelectedFranchise.Id)
      		{
		 		angular.extend(vm.cleaner, {
						MasterFranchiseRefId: vm.cleaner.SelectedFranchise.Id
					});
			};

			var chkCopy = document.getElementById('CopyToPostal').checked;
		 	if (chkCopy == true)
		 	{
		 		var guid = vm.cleaner.PostalAddress.Id;
		 		vm.cleaner.PostalAddress = angular.copy(vm.cleaner.PhysicalAddress);
		 		vm.cleaner.PostalAddress.AddressType = _postalType;
		 		vm.cleaner.PostalAddress.Id = guid;
		 	}

			//console.log("<CLEANER Data> - " + angular.toJson(vm.cleaner));
			if (vm.cleanerForm.$valid) {

                HandleBusySpinner.start($scope, panelName);
	
            	return $http.post('/cleaners/savecleanerdetails', vm.cleaner).success(function (response) {
                	// Add your success stuff here
                	HandleBusySpinner.stop($scope, panelName);
                	$scope.submitted = false;
                	ShowUserMessages.show($scope, response, "Error updating details.");
                	vm.cleaner = response.DataItem;
                	$scope.CleanerId = vm.cleaner.Id;

        			if ($scope.DataRecordStatus.IsNewDataRecord)
                	{
            			$scope.DataRecordStatus.IsNewDataRecord = false;
                	}

                	activate();
                	$rootScope.childMessage = response;
                	$state.go("app.cleaner_details", { "CleanerId": $scope.CleanerId});

            	}).error(function (error) {
            		HandleBusySpinner.stop($scope, panelName);
            		$scope.submitted = false;
                	ShowUserMessages.show($scope, error, "Error updating details.");

            	});
        	}
        	else
        	{
        		//console.log("<XX> - " + angular.toJson(vm.cleanerForm.$error));
            	$scope.submitted = false;
            	ShowUserMessages.show($scope, "Error updating cleaner details - please review validation errors", "Error updating details.");
        		return false;
        	}
      	}
	}

	/***********************/
	/*** CLEANER ADDRESS ***/
	/***********************/
	function CleanerAddressController()
	{
		var vm = this;
		activate();

		function activate()
		{
        	
		}
	}

	/****************************/
	/*** CLEANER AVAILABILITY ***/
	/****************************/
	function CleanerAvailabilityController()
	{
		var vm = this;
		activate();

		function activate()
		{
        	/*vm.today = function() {
            	vm.dt = new Date();
          	};
          	vm.today();*/

          	vm.dt1 = {
				opened: false
			};

			vm.dt2 = {
			    opened: false
			};

          	vm.clear = function () {
            	vm.dt = null;
          	};	

          	vm.disabled = function(date, mode) {
          		return false;
        		// Disable weekend selection
          		//return ( mode === 'day' && ( date.getDay() === 0 || date.getDay() === 6 ) );
          	};

          	vm.toggleMin = function() {
            	vm.minDate = vm.minDate ? null : new Date();
          	};
          	vm.toggleMin();

          	vm.open = function($event,opened) {
			    $event.preventDefault();
			    $event.stopPropagation();

			    vm[opened] = true;
			  };

          	vm.dateOptions = {
            	formatYear: 'yyyy',
            	startingDay: 1,
            	yearColumns: 3,
            	yearRows: 4,
            	showWeeks: false,
            	minDate: new Date()
          	};

          	vm.initDate = new Date();
          	vm.formats = ['dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
          	vm.format = vm.formats[0];
		}
	}

})();

