﻿(function() {
    'use strict';

    angular
        .module('magicsettings',[])
        .controller('MasterSettingsController', MasterSettingsController)
		.controller('DefaultSettingsController', DefaultSettingsController)
		.controller('FranchiseController', FranchiseController)
		.controller('FranchiseDetailController', FranchiseDetailController)
		.controller('FranchiseSettingsController', FranchiseSettingsController)
		.controller('PostCodesController', PostCodesController);

	MasterSettingsController.$inject = ['$scope'];
	DefaultSettingsController.$inject = ['$scope','$filter', '$http', 'editableOptions', 'editableThemes','$q', 'HandleBusySpinner','ShowUserMessages'];
    FranchiseController.$inject = ['$scope','$filter', '$http','$q', 'HandleBusySpinner'];
    FranchiseDetailController.$inject = ['$scope','$filter', '$http','$q','ShowUserMessages'];
    FranchiseSettingsController.$inject = ['$scope','$http','ShowUserMessages'];
    PostCodesController.$inject = ['$scope','$filter', '$http', 'editableOptions', 'editableThemes','$q','HandleBusySpinner','ShowUserMessages'];

    function MasterSettingsController($scope)
    {
    	// A parent controller is required where we use tabbing with messaging being sent from the child.
    	$scope.userMessages = [];
		$scope.userMessageType = [];

		$scope.isLoadingResults
    }

    /*****************************/
	/*** SUBURB / ZONE MAPPING ***/
	/*****************************/
    function PostCodesController($scope, $filter, $http, editableOptions, editableThemes, $q, HandleBusySpinner, ShowUserMessages)
	{
		var vm = this;
		var panelName = 'panelPostalDataSettings';
		vm.selectedFranchise = null;

		activate();

		function activate()
		{
			vm.listOfPostcodes = null;

			HandleBusySpinner.start($scope, panelName);

           	//console.log("<POSTCODE FranchiseId> - " + angular.toJson($scope.FranchiseId));
           	vm.selectedFranchise = ($scope.FranchiseId) ? $scope.FranchiseId : null;
			loadPostCodes();

			HandleBusySpinner.stop($scope, panelName);
		};

		function loadPostCodes()
		{
			$http.get('/settings/getpostcodes?FranchiseId=' + vm.selectedFranchise)
                .success(function (data) {
                	vm.listOfPostcodes = data.list;
                	vm.nextNewGuid = data.nextGuid;
                	//console.log("<POSTCODE nextNewGuid> - " + angular.toJson(vm.nextNewGuid));

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
          	HandleBusySpinner.start($scope, panelName);
            vm.inserted = {
              Id: vm.nextNewGuid,
              SuburbName: '',
              PostCode: '',
              Zone: '',
              LinkedZones: '',
              IsNewItem: true
            };

            //console.log("<POSTCODE inserted> - " + angular.toJson(vm.inserted));

            vm.listOfPostcodes.push(vm.inserted);
            HandleBusySpinner.stop($scope, panelName);
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

            //console.log("<SETTING item post> - " + angular.toJson(data));
       		return $http.post('/settings/savepostcodes', data).success(function (response) {
                // Add your success stuff here
                ShowUserMessages.show($scope, response, "Error updating suburb/zone.");
           		loadPostCodes();

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
		activate();

		function activate()
        {
			vm.listOfSettings = null;

			HandleBusySpinner.start($scope, 'panelDataMasterSettings');

            $http.get('/settings/getsettings/?incDisabled=1')
                .success(function (data) {
                    vm.listOfSettings = data.list;

                }).error(function(err) {
                }).finally(function() {
          			HandleBusySpinner.stop($scope, 'panelDataMasterSettings');
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
           		activate();

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
			vm.listOfFranchises = null;
			var _incDisabled = 0;

			HandleBusySpinner.start($scope, 'panelDataMasterSettings');

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

                }).error(function(err) {
                	
                }).finally(function() {
                	HandleBusySpinner.stop($scope, 'panelDataMasterSettings');
                });
		}
	}

	/*************************/
	/*** FRANCHISE DETAILS ***/
	/*************************/
	function FranchiseDetailController($scope, $filter, $http, $q, ShowUserMessages)
	{
		var vm = this;
		var Id = $scope.FranchiseId;
		var _postalType = 1;
		var _physicalType = 0;

		vm.franchise = null;
		vm.addressTypes = null;

		activate();

		function activate()
        {
            $http.get('/settings/getfranchise/?FranchiseId=' + Id)
                .success(function (data) {
                	//console.log("<FRANCHISE> - " + angular.toJson(data.item));
                	vm.franchise = data.item;
                	$scope.FranchiseId = vm.franchise.Id;

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

                });
		}

		vm.submitted = false;
      	vm.validateInput = function(name, type) {
        	var input = vm.franchiseForm[name];
    		return (input.$dirty || vm.submitted) && input.$error[type];
      	};

      	vm.saveData = function(data) {
      		//console.log("<FRANCHISE Data> - " + angular.toJson(data));
            
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
                	
            	return $http.post('/settings/savefranchise', vm.franchise).success(function (response) {
                	// Add your success stuff here
                	$scope.submitted = false;
                	ShowUserMessages.show($scope, response, "Error updating details.");
                	vm.franchise = response.DataItem;

            	}).error(function (error) {
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