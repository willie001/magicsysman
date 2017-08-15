(function() {
    'use strict';

    angular
        .module('magicsettings',[])
        .controller('DefaultSettingsController', DefaultSettingsController)
		.controller('FranchiseController', FranchiseController)
		.controller('FranchiseDetailController', FranchiseDetailController);

	//PostCodesController.$inject = ['$filter', '$http', 'editableOptions', 'editableThemes','$q'];
    DefaultSettingsController.$inject = ['$scope','$filter', '$http', 'editableOptions', 'editableThemes','$q', '$timeout'];
    FranchiseController.$inject = ['$scope','$filter', '$http','$q', '$timeout'];
    FranchiseDetailController.$inject = ['$scope','$filter', '$http','$q'];

    function PostCodesController($filter, $http, editableOptions, editableThemes, $q)
	{
		var vm = this;
		activate();

		function activate()
		{
			vm.listOfPostcodes = null;
			$http.get('/settings/getpostcodes')
				.success(function (data) {
					vm.listOfPostcodes = data.list;
				});
			
		}

		vm.validateData = function(data, id) {
            if (id.length == 0 ) {
              return 'Primary key is not set';
            }

            if (data.SuburbName.length == 0) {
              return 'Suburb name is mandatory';
            }

            if (data.PostCode.length == 0) {
              return 'Post code is mandatory';
            }
            
            if (data.ZoneID.length == 0) {
              return 'Zone Id is mandatory';
            }
          };

          vm.addSuburb = function() {
	          vm.inserted = {
	              SuburbName: '',
	              PostCode: '',
	              ZoneID: 0
	            };
	            vm.listOfPostcodes.push(vm.inserted);
          }

          vm.saveSuburb = function(data, id) {
          }

          vm.removeSuburb = function(index) {
            //vm.users.splice(index, 1);
          };
	}

	/****************/
	/*** SETTINGS ***/
	/****************/
    function DefaultSettingsController($scope, $filter, $http, editableOptions, editableThemes, $q, $timeout)
	{
		var vm = this;
		activate();

		function activate()
        {
			vm.listOfSettings = null;

    		$timeout(function(){
    			$scope.isLoadingResults = true;
   				$scope.$parent.$broadcast('triggerPanelRefresh', document.getElementById('panelDataMasterSettings'),'traditional');
			},150);

            $http.get('/settings/getsettings/?incDisabled=1')
                .success(function (data) {
                    vm.listOfSettings = data.list;
                    $scope.isLoadingResults = false;

                }).error(function(err) {
                	
                }).finally(function() {
                	$timeout(function () {
                  		$scope.$broadcast('removeSpinner', 'panelDataMasterSettings');
              		}, 100);
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
            
			$scope.userMessages = [];
			$scope.userMessageType = [];

            return $http.post('/settings/savesettings', item).success(function (response) {
                // Add your success stuff here
                _showUserMessages($scope, response);

            }).error(function (error) {

                _showUserMessages($scope, error);

            });

            function _showUserMessages($scope, msgs) {
            	$scope.userMessageType.push(msgs.MsgCssClass);

            	var _bHasErrors = false;
            	if (msgs.Errors && angular.isObject(msgs.Errors)) {
                	for (var i = 0; i < msgs.Errors.length; i++) {
                		var message = msgs.Errors[i].Message;
                		_bHasErrors = true;	
            			$scope.userMessages.push(message) ;
        			}
                };

                if (!_bHasErrors)
                {
	                if (msgs.Message != null) {
	                    $scope.userMessages.push(msgs.Message);
	                } else {
	                	$scope.userMessages.push("Error updating default settings.");
	                }
                };
            }
      	}
	}

	/*************************/
	/*** FRANCHISE SUMMARY ***/
	/*************************/
	function FranchiseController($scope, $filter, $http, $q, $timeout)
	{
		var vm = this;

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
			$scope.isLoadingResults = false;
			var _incDisabled = 0;

    		$timeout(function(){
    			$scope.isLoadingResults = true;
   				$scope.$parent.$broadcast('triggerPanelRefresh', document.getElementById('panelDataMasterSettings'),'traditional');
			},500);

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
                	$scope.isLoadingResults = false;
					vm.listOfFranchises = data.list;

                }).error(function(err) {
                	
                }).finally(function() {
                	$scope.isLoadingResults = false;
                	$timeout(function () {
                  		$scope.$broadcast('removeSpinner', 'panelDataMasterSettings');
              		}, 500);

                });
		}
	}

	/*************************/
	/*** FRANCHISE DETAILS ***/
	/*************************/
	function FranchiseDetailController($scope, $filter, $http, $q)
	{
		var vm = this;
		var Id = $scope.Id;
		var _postalType = 1;
		var _physicalType = 0;

		vm.franchise = null;
		vm.addressTypes = null;

		activate();

		function activate()
        {
            $http.get('/settings/getfranchise/?Id=' + Id)
                .success(function (data) {
                	console.log("<FRANCHISE> - " + angular.toJson(data.item));
                	vm.franchise = data.item;
                	$scope.Id = vm.franchise.Id;

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
      		//alert('saving data');
		 	$scope.submitted = true;

		 	var chkCopy = document.getElementById('CopyToPostal').checked;
		 	if (chkCopy == true)
		 	{
		 		var guid = vm.franchise.PostalAddress.Id;
		 		vm.franchise.PostalAddress = angular.copy(vm.franchise.PhysicalAddress);
		 		vm.franchise.PostalAddress.AddressType = _postalType;
		 		vm.franchise.PostalAddress.Id = guid;
		 	}

			$scope.userMessages = [];
			$scope.userMessageType = [];

			if (vm.franchiseForm.$valid) {
                	
            	return $http.post('/settings/savefranchise', vm.franchise).success(function (response) {
                	// Add your success stuff here
                	$scope.submitted = false;
                	_showUserMessages($scope, response);
                	vm.franchise = response.DataItem;

            	}).error(function (error) {
            		$scope.submitted = false;
                	_showUserMessages($scope, error);

            	});
        	}
        	else
        	{
        		//console.log("<XX> - " + angular.toJson(vm.franchiseForm.$error));
            	$scope.submitted = false;
            	_showUserMessages($scope, "Error updating franchise details - please review validation errors");
        		return false;
        	}


            function _showUserMessages($scope, msgs) {
            	//console.log("<X2> - " + angular.toJson(msgs));
            	if (msgs.MsgCssClass != null)
            		$scope.userMessageType.push(msgs.MsgCssClass);
				else
					$scope.userMessageType.push('alert bg-warning-light');

            	var _bHasErrors = false;
            	var prevMsg = "";
            	if (msgs.Errors && angular.isObject(msgs.Errors)) {
                	for (var i = 0; i < msgs.Errors.length; i++) {
                		var message = msgs.Errors[i].Message;
                		_bHasErrors = true;	
                		if (prevMsg != message)
                		{
            				$scope.userMessages.push(message) ;
            			}
            			prevMsg = message;
        			}
                };

                if (!_bHasErrors)
                {
	                if (msgs.Message != null) {
	                    $scope.userMessages.push(msgs.Message);
					} else if (msgs != null) {
						$scope.userMessages.push(msgs);
	                } else {
	                	$scope.userMessages.push("Error updating details");
	                }
                };
                //console.log("<X3> - " + angular.toJson($scope.userMessages));
				window.scrollTo(0,0);
            };


      	}
	}
})();