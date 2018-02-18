(function() {
    'use strict';

    angular
        .module('magicsettings',[])
        .controller('MasterSettingsController', MasterSettingsController)
		.controller('DefaultSettingsController', DefaultSettingsController)
		.controller('FranchiseController', FranchiseController)
		.controller('FranchiseDetailController', FranchiseDetailController)
		.controller('FranchiseSettingsController', FranchiseSettingsController)
		.controller('PostCodesController', PostCodesController)
		.controller('RatesController', RatesController);

	MasterSettingsController.$inject = ['$scope'];
	DefaultSettingsController.$inject = ['$scope','$filter', '$http', 'editableOptions', 'editableThemes','$q', 'HandleBusySpinner','ShowUserMessages'];
    FranchiseController.$inject = ['$scope','$filter', '$http','$q', 'HandleBusySpinner'];
    FranchiseDetailController.$inject = ['$scope','$filter', '$http','$q','$location','$rootScope', 'HandleBusySpinner', 'ShowUserMessages'];
    FranchiseSettingsController.$inject = ['$scope','$http','ShowUserMessages'];
    PostCodesController.$inject = ['$scope','$filter', '$http', 'editableOptions', 'editableThemes','$q','ShowUserMessages'];
    RatesController.$inject = ['$scope', '$http', 'ngDialog', 'ShowUserMessages'];

    function MasterSettingsController($scope)
    {
   		var vm = this;

    	// A parent controller is required where we use tabbing with messaging being sent from the child.
    	$scope.userMessages = [];
		$scope.userMessageType = [];

		$scope.isLoadingResults;
		$scope.DataRecordStatus = { IsNewDataRecord: false};

		$scope.loadPostcodeData = function() {
			$scope.$broadcast('loadPostcode');
		}

		$scope.loadRatesData = function() {
			$scope.$broadcast('loadRates');
		}
    }

    /**************/
	/*** RATES ***/
	/*************/
    function RatesController($scope, $http, ngDialog, ShowUserMessages)
	{
		var vm = this;
		vm.selectedFranchise = null;
		vm.enableAdd = false;
		vm.isSaving = false;
		vm.isCallback = false;

		if (!$scope.rateData){
			$scope.rateData = {};
			$scope.rateData.AvailableRateApplications = [];	
			$scope.rateData.SelectedRates = {};	
			$scope.rateData.IsNewItem = true;
		}
		$scope.$on('loadRates', loadRates);

		activate();
		$scope.rateData.FranchiseId = vm.selectedFranchise;
        
		function activate()
		{
			$scope.rateData.disabled = false;
			vm.listOfRates = null;
           	vm.selectedFranchise = ($scope.FranchiseId) ? $scope.FranchiseId : null;
		};

		function loadRates()
		{
			//console.log("<RATES property> - " + angular.toJson(someProp));
			$http.get('/settings/getrates?FranchiseId=' + vm.selectedFranchise)
                .success(function (data) {
                	vm.listOfRates = data.list;
                	vm.nextNewGuid = data.nextGuid;
                	vm.contextSelections = data.contextSelections;

                }).error(function(err) {

                }).finally(function() {
                	//console.log("<RATES CONTEXT SELECTION> - " + angular.toJson(vm.contextSelections));
					$http.get('/settings/getratetypesjson?ContextSelection=' + vm.contextSelections)
		                .success(function (data) {
		                	//console.log('<RATE TYPES> ' + angular.toJson(data.item));
		                	$scope.rateData.AvailableRateApplications = data.item;

	                		if ($scope.rateData.AvailableRateApplications.length == 0 && vm.isCallback == false)
		                	{
		                		if (vm.isSaving == true)
		                		{
		                			ShowUserMessages.show($scope,"Rates saved successfully and all the rates are now configured. To add more rates the used categories must be changed.", "Error updating rate.");
		                		}
		                		else
		                		{
		                			ShowUserMessages.show($scope,"All the rates are already configured. To add more rates the used categories must be changed.", "Error updating rate.");
		                		}
		                		vm.enableAdd = true;
		                		vm.isSaving = false;
		                	}		

		                }).error(function(err) {
		                	
		                }).finally(function() {
		                	vm.isCallback == true;
		                });
		               
                });
            
		}

		$scope.openRatesPopupForm = function (item) {
			if (item)
			{
				$scope.rateData.IsNewItem = false;
				$scope.rateData.SelectedRates = {};
				$scope.rateData.SelectedRates.selected = item.SelectedRates; 
				angular.extend($scope.rateData, {
						FranchiseId: vm.selectedFranchise,
						Id: item.Id,
						RateCode: item.RateCode,
						RateAmount: item.RateAmount,
						IsActive: item.IsActive
					});
			} else {
				$scope.rateData.IsNewItem = true;
				$scope.rateData.SelectedRates = {};	
				angular.extend($scope.rateData, {
						FranchiseId: vm.selectedFranchise,
						Id: vm.nextNewGuid,
						RateCode: null,
						RateAmount: null,
						IsActive: true
					});
			}
			//console.log("<RATES EDIT2> - " + angular.toJson($scope.rateData.SelectedRates));
				
			ngDialog.open({
              template: 'static/RatesEditor.html',
              className: 'ngdialog-theme-default',
              width: '40%',
              scope: $scope,
              cache: false,
              preCloseCallback: function(value) {
              	vm.isCallback = true;
              	if (value=='confirm')
                {
		       		if (confirm('Are you sure you want to cancel?')) {
		       			loadRates();
		            	return true;
		        	}
		        	else
		        	{
		        		return false;
		        	}
        		}
		        	loadRates();
	            	return true;
		    	},
            });
          };

		$scope.saveRatesPopupForm = function () {
			//console.log('<RATE SAVE> ' + angular.toJson($scope.rateData.SelectedRates.selected));
			$scope.rateData.SelectedRatesJson = angular.toJson($scope.rateData.SelectedRates.selected);
			return $http.post('/settings/saverate', $scope.rateData).success(function (response) {
	            // Add your success stuff here
	        	//console.log("<RATE SAVE response post> - " + angular.toJson(response));
	   			ShowUserMessages.show($scope, response, "Error updating rate.");
	   			if (response.IsValid)
        		{	
	   				vm.isSaving = true;
	       			loadRates();
       			}
       			ngDialog.close(); 

	        }).error(function (error) {

	        	//console.log("<RATE SAVE error post> - " + angular.toJson(error));
	            ShowUserMessages.show($scope, error, "Error updating rate.");

	        });
		};

		$scope.toggleRateState = function(item) {
          	//console.log('<RATE TOGGLE> ' + angular.toJson(item));
			if (item)
			{
				$scope.rateData.IsNewItem = false;
				$scope.rateData.SelectedRates = {};
				$scope.rateData.SelectedRates.selected = item.SelectedRates; 
				angular.extend($scope.rateData, {
						FranchiseId: vm.selectedFranchise,
						Id: item.Id,
						RateCode: item.RateCode,
						RateAmount: item.RateAmount,
						IsActive: item.IsActive
					});
			}
			//console.log('<RATE TOGGLE2> ' + angular.toJson($scope.rateData));
			$scope.saveRatesPopupForm();
        };

        $scope.removeRate = function(index) {
          	alert('Not ready yet!'); 
          	return false;
          	vm.listOfRates.splice(index, 1);
        };

	}

    /*****************************/
	/*** SUBURB / ZONE MAPPING ***/
	/*****************************/
    function PostCodesController($scope, $filter, $http, editableOptions, editableThemes, $q, ShowUserMessages)
	{
		var vm = this;
		vm.selectedFranchise = null;

		$scope.$on('loadPostcode', loadPostCodes);

		activate();

		function activate()
		{
			vm.listOfPostcodes = null;

           	//console.log("<POSTCODE FranchiseId> - " + angular.toJson($scope.FranchiseId));
           	vm.selectedFranchise = ($scope.FranchiseId) ? $scope.FranchiseId : null;
		};

		function loadPostCodes()
		{
			$http.get('/settings/getpostcodes?FranchiseId=' + vm.selectedFranchise)
                .success(function (data) {
                	vm.listOfPostcodes = data.list;
                	vm.nextNewGuid = data.nextGuid;
                	//console.log("<POSTCODES loaded> - " + angular.toJson(vm.listOfPostcodes));

                }).error(function(err) {

                }).finally(function() {

                });
		};

		vm.validateData = function(data, colName) {
            if (data.length == 0) {
              return colName + ' is mandatory';
            }
          };

	      vm.addData = function() {
	        vm.inserted = {
	          Id: vm.nextNewGuid,
	          SuburbName: '',
	          PostCode: '',
	          Zone: '',
	          LinkedZones: '',
	          IsNewItem: true
	        };

            //console.log("<POSTCODE inserted> - " + angular.toJson(vm.inserted));

            if (vm != null && vm.listOfPostcodes != null)
            {
            	vm.listOfPostcodes.push(vm.inserted);
            }
          };

          vm.removeSuburb = function(index) {
          	alert('Not ready yet!'); 
          	return false;
          	vm.listOfPostcodes.splice(index, 1);
          };

          vm.saveData = function(data, id, isNew) {
			angular.extend(data, {
					FranchiseId: vm.selectedFranchise,
					Id: id,
					IsNewItem: isNew
				});

            //console.log("<POSTCODE data post> - " + angular.toJson(data));
       		return $http.post('/settings/savepostcodes', data).success(function (response) {
                // Add your success stuff here
            	//console.log("<POSTCODE response post> - " + angular.toJson(response));
       			ShowUserMessages.show($scope, response, "Error updating suburb/zone.");
       			if (response.IsValid)
        		{	
           			loadPostCodes();
				}

            }).error(function (error) {

                ShowUserMessages.show($scope, error, "Error updating suburb/zone.");

            });
        }
	}

	/****************/
	/*** SETTINGS ***/
	/****************/
    function DefaultSettingsController($scope, $filter, $http, editableOptions, editableThemes, $q, HandleBusySpinner, ShowUserMessages)
	{
		var vm = this;
		var panelName = 'panelDataMasterSettings';
		activate();

		function activate()
        {
			vm.listOfSettings = null;

			HandleBusySpinner.start($scope, panelName);

            $http.get('/settings/getsettings/?incDisabled=1')
                .success(function (data) {
                    vm.listOfSettings = data.list;

                }).error(function(err) {
                }).finally(function() {
          			HandleBusySpinner.stop($scope, panelName);
                });

		}

		vm.validateSettingValue = function(data) {
		    if (data.length <= 0 ) {
	          return 'Setting value is required';
	        }

	        if (data.length > 50)
	        {
	        	return 'Setting value must be no more than 50 characters';
	        }
      	};

		vm.saveData = function(data, item) {
            //console.log("<SETTING Data> - " + angular.toJson(data));
			//console.log("<SETTING item> - " + angular.toJson(item));
               	
			angular.extend(item, {
				SettingValue: data.SettingValue
			});
			//console.log("<SETTING item post> - " + angular.toJson(item));
            
            return $http.post('/settings/savesettings', item).success(function (response) {
                // Add your success stuff here
                //console.log("<SETTING item post> - " + angular.toJson(item));
           		ShowUserMessages.show($scope, response, "Error updating default settings.");
           		if (response.IsValid)
        		{	
           			activate();
				}

            }).error(function (error) {

                ShowUserMessages.show($scope, error, "Error updating default settings.");

            });
      	}
	}

	/*************************/
	/*** FRANCHISE SUMMARY ***/
	/*************************/
	function FranchiseController($scope, $filter, $http, $q, HandleBusySpinner)
	{
		var vm = this;
		var panelName = 'panelDataMasterSettings';
		vm.listOfFranchises = null;
		var _incDisabled = 0;
			
		$scope.activeResult = " active ";
		$scope.toggleActiveSearch = function() {
			var checkBox = document.getElementById('checkActiveSearch');
			if (checkBox)
			{
				checkBox.checked = !checkBox.checked;
				if (checkBox.checked == 'on' || checkBox.checked == true)
				{
					$scope.activeResult = " ";
				}
				else
				{
					$scope.activeResult = " active ";
				}
				activate();
				
			}
		};

		activate();

		function activate()
        {
			HandleBusySpinner.start($scope, panelName);

			var checkBox = document.getElementById('checkActiveSearch');
			if (checkBox )
			{
				if (checkBox.checked == true)
				{
					_incDisabled = 1;
				}
				else
				{
					_incDisabled = 0;
				}
			}

            $http.get('/settings/getfranchises/?incDisabled=' + _incDisabled)
                .success(function (data) {
					vm.listOfFranchises = data.list;
					//console.log("<FRANCHISE LIST> - " + angular.toJson(data.list));
                	
                }).error(function(err) {
                	
                }).finally(function() {
                	HandleBusySpinner.stop($scope, panelName);
                });
		}
	}

	/*************************/
	/*** FRANCHISE DETAILS ***/
	/*************************/
	function FranchiseDetailController($scope, $filter, $http, $q, $location, $rootScope, HandleBusySpinner, ShowUserMessages)
	{
		var vm = this;
		var _postalType = 1;
		var _physicalType = 0;
		var panelName = "panelDataMasterSettings";
		var Id = $scope.FranchiseId;
		
		if ($rootScope.childMessage)
		{
			ShowUserMessages.show($scope, $rootScope.childMessage, "Error updating details.");
            $rootScope.childMessage = null; 	
		}

		vm.franchise = null;
		vm.addressTypes = null;

		activate();

		function activate()
        {
        	HandleBusySpinner.start($scope, panelName);

        	 $http.get('/settings/getfranchise/?FranchiseId=' + Id)
                .success(function (data) {
                	//console.log("<FRANCHISE> - " + angular.toJson(data.item));
                	vm.franchise = data.item;
                	$scope.FranchiseId = vm.franchise.Id;
                	$scope.DataRecordStatus.IsNewDataRecord = data.item.IsNewItem;
                	$scope.CopyToPostal = $scope.DataRecordStatus.IsNewDataRecord;
           
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

		vm.submitted = false;
      	vm.validateInput = function(name, type) {
        	var input = vm.franchiseForm[name];
    		return (input.$dirty || vm.submitted) && input.$error[type];
      	};

      	vm.saveData = function(data) {
      		//console.log("<FRANCHISE Data> - " + angular.toJson(vm.franchise));
		 	$scope.submitted = true;

		 	var chkCopy = document.getElementById('CopyToPostal').checked;
		 	if (chkCopy == true)
		 	{
		 		var guid = vm.franchise.PostalAddress.Id;
		 		vm.franchise.PostalAddress = angular.copy(vm.franchise.PhysicalAddress);
		 		vm.franchise.PostalAddress.AddressType = _postalType;
		 		vm.franchise.PostalAddress.Id = guid;
		 	}

			if (vm.franchiseForm.$valid) {

                HandleBusySpinner.start($scope, panelName);
	
            	return $http.post('/settings/savefranchise', vm.franchise).success(function (response) {
                	// Add your success stuff here
                	HandleBusySpinner.stop($scope, panelName);
                	$scope.submitted = false;
                	ShowUserMessages.show($scope, response, "Error updating details.");
                	if (response.IsValid)
            		{	
                		vm.franchise = response.DataItem;
                		$scope.FranchiseId = vm.franchise.Id;

	        			if ($scope.DataRecordStatus.IsNewDataRecord)
	                	{
	            			$scope.DataRecordStatus.IsNewDataRecord = false;
	            			$rootScope.childMessage = response;
	                		$location.path('/franchiseregister/'+$scope.FranchiseId);
	                	}
					}

            	}).error(function (error) {
            		HandleBusySpinner.stop($scope, panelName);
            		$scope.submitted = false;
                	ShowUserMessages.show($scope, error, "Error updating details.");

            	});


        	}
        	else
        	{
        		//console.log("<XX> - " + angular.toJson(vm.franchiseForm.$error));
            	$scope.submitted = false;
            	ShowUserMessages.show($scope, "Error updating franchise details - please review validation errors", "Error updating details.");
        		return false;
        	}
      	}
	}

	/***************************/
	/*** FRANCHISE SETTINGS ***/
	/**************************/
	function FranchiseSettingsController($scope,$http, ShowUserMessages)
	{
		var vm = this;
		var Id = $scope.FranchiseId;

		vm.franchiseSettings = null;

		activate();

		function activate()
        {
        	if (!Id)
        		return;

            $http.get('/settings/getfranchisesettings/?FranchiseId=' + Id)
                .success(function (data) {
                	//console.log("<FRANCHISE> - " + angular.toJson(data.item));
                	vm.franchiseSettings = data.item;
                	$scope.FranchiseId = vm.franchiseSettings.Id;

                }).error(function(err) {
                	
                }).finally(function() {

                });
		}

		vm.submitted = false;

      	vm.saveData = function() {
		 	$scope.submitted = true;

			//console.log("<FRANCHISE SETTINGS Data> - " + angular.toJson(vm.franchiseSettingForm));		
        	if (vm.franchiseSettingForm.$valid) {

                return $http.post('/settings/savefranchisesettings', vm.franchiseSettings).success(function (response) {
                	// Add your success stuff here
                	$scope.submitted = false;
                	ShowUserMessages.show($scope, response, "Error updating franchise settings.");
                	vm.franchiseSettings = response.DataItem;
                	//console.log("<XX Franchise Settings> - " + angular.toJson(vm.franchiseSettings));
                	activate();
            	
            	}).error(function (error) {
            		$scope.submitted = false;
                	ShowUserMessages.show($scope, error, "Error updating franchise settings.");

            	});
        	}
        	else
        	{
        		//console.log("<XX> - " + angular.toJson(vm.franchiseSettingForm.$error));
            	$scope.submitted = false;
            	ShowUserMessages.show($scope, "Error updating franchise settings - please review validation errors", "Error updating franchise settings.");
        		return false;
        	}
      	}
	}
})();