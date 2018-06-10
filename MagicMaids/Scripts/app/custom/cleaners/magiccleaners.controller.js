(function() {
    'use strict';

    angular
    	.module("magiccleaners",[])
    	.controller('MasterCleanerController', MasterCleanerController)
    	.controller('MasterCleanerHeaderController', MasterCleanerHeaderController)
    	.controller('CleanerSearchController', CleanerSearchController)
    	.controller('CleanerDetailsController', CleanerDetailsController)
    	.controller('CleanerAddressController', CleanerAddressController)
		.controller('CleanerAvailabilityController', CleanerAvailabilityController)
		.controller('CleanerLeaveController', CleanerLeaveController)
		.factory('cleanerTeamFactory', function() {
			var data = {
				Primary: {},
				TeamList: [],
				TeamSize: 0,
				TeamSelectionList: [],
				IsTeamSelectionListSet: false
			};

			return {
					getTeamSelectionList: function() {
						
						if (data.IsTeamSelectionListSet == true)
						{
							return data.TeamSelectionList;
						}

						data.TeamSelectionList = [];

						var teamMember = {
			    			Id: data.Primary.Id,
			    			FirstName: data.Primary.FirstName,
			    			LastName: data.Primary.LastName,
							DisplayName: data.Primary.FirstName + ' ' + data.Primary.LastName,
							IsPrimary: true
			    		};

		    			data.TeamSelectionList.push(teamMember);

			            // Cleaner team count includes primary member
						for(var i=0; i<=data.TeamSize-2; i++)
			    		{
			    			var teamMember = {
			    				Id: data.TeamList[i].Id,
			    				FirstName: data.TeamList[i].FirstName,
			    				LastName: data.TeamList[i].LastName,
			    				DisplayName:  data.TeamList[i].FirstName + ' ' +  data.TeamList[i].LastName,
			    				IsPrimary: false
			    			};
			    			data.TeamSelectionList.push(teamMember);
			    		}

			    		data.IsTeamSelectionListSet = true;
			    		return data.TeamSelectionList;
            	
				},
				getIsTeamSelectionListSet: function() {
		    		return data.IsTeamSelectionListSet ;
				},
				getPrimary: function() {
					return data.Primary;
				},
				getTeamSize: function() {
					return data.TeamSize;
				},
				getTeam: function() {
					return data.TeamList;
				},
				setPrimary: function(cleaner) {
					data.IsTeamSelectionListSet = false;
					data.Primary = cleaner;
				},
				setTeamSize: function(value) {
					data.TeamSize = value;
				},
				setTeam: function(team) {
					data.IsTeamSelectionListSet = false;
					data.TeamList = team;
				}
			};

		});

	MasterCleanerController.$inject = ['$scope'];
    MasterCleanerHeaderController.$inject = ['$scope','$location','$route','$state'];
    CleanerSearchController.$inject = ['$scope', '$http', 'HandleBusySpinner', 'ShowUserMessages','DTOptionsBuilder', '$cookies', 'moment', 'manageTimeZoneCookie'];
    CleanerDetailsController.$inject = ['$scope','$filter', '$http','$q','$location','$rootScope', '$state', 'HandleBusySpinner', 'ShowUserMessages', 'ngDialog', 'cleanerTeamFactory'];
    CleanerAddressController.$inject = [];
	CleanerAvailabilityController.$inject = ['$scope','$http','HandleBusySpinner', 'ShowUserMessages', 'cleanerTeamFactory','moment'];
	CleanerLeaveController.$inject = ['$scope','$filter','$http', 'HandleBusySpinner', 'ShowUserMessages','editableOptions', 'editableThemes'];

	function MasterCleanerController($scope)
    {
   		var vm = this;

    	// A parent controller is required where we use tabbing with messaging being sent from the child.
    	$scope.userMessages = [];
		$scope.userMessageType = [];

		$scope.isLoadingResults;
		$scope.DataRecordStatus = { IsNewDataRecord: true};
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
	function CleanerSearchController($scope, $http, HandleBusySpinner, ShowUserMessages, DTOptionsBuilder, $cookies, moment, manageTimeZoneCookie)
	{
		var vm = this;
		var panelName = "panelCleanerResults";

		vm.Search = {};
		vm.SeachResults = {};
		vm.Search.SelectedFranchise = {};
		vm.hasSearched = false;

		activate();

		function activate() {
			manageTimeZoneCookie.set($cookies, moment, location);
			HandleBusySpinner.start($scope, panelName);
			$scope.searchCriteria = false;
			$scope.dtOptions =  DTOptionsBuilder.newOptions().withOption('order', [5, 'desc']);

			$http.get('/settings/getactivefranchises')
                .success(function (data) {
                	//console.log('<ACTIVE FRANCHISES> ' + angular.toJson(data.list));
                	vm.availableFranchises = data.list;

                }).error(function(err) {
                	//console.log('<ACTIVE FRANCHISES ERROR> ' + angular.toJson(err));
                }).finally(function() {
                	HandleBusySpinner.stop($scope, panelName);
                });
		}

	 $scope.clearForm = function() {
			vm.Search = {};
			vm.SearchResults = {};
			vm.hasSearched = false;
			$scope.searchCriteria = false;
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

				$scope.searchCriteria = true;
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
	function CleanerDetailsController($scope, $filter, $http, $q, $location, $rootScope, $state, HandleBusySpinner, ShowUserMessages, ngDialog, cleanerTeamFactory)
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
		$scope.AvailableZones = [];

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
                	if (data.selectedFranchise != null)
                	{
                		$scope.FranchiseId = data.selectedFranchise.Id;
                	}
                	$scope.DataRecordStatus.IsNewDataRecord = data.item.IsNewItem;

                	if (data.selectedFranchise)
                	{
                		vm.cleaner.SelectedFranchise = data.selectedFranchise;
                	}

                	cleanerTeamFactory.setPrimary(vm.cleaner);
                	

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
                	loadFranchiseZones();
                	$scope.CopyToPostal = $scope.DataRecordStatus.IsNewDataRecord;

                	HandleBusySpinner.stop($scope, panelName);
                });
		}

		function loadFranchiseZones()
		{
			$http.get('/cleaners/getfranchisezonesjson/?FranchiseId=' + $scope.FranchiseId)
                .success(function (data) {
            		//console.log('<FRANCHISE ZONES> ' + angular.toJson(data.item));
                	$scope.AvailableZones = data.item;

                }).error(function(err) {
                	
                }).finally(function() {

                });
		}

		function loadTeamMembers()
		{
			$http.get('/cleaners/getcleanerteam/?CleanerId=' + vm.cleaner.Id)
                .success(function (data) {
                	vm.cleanerTeam = data.list;
                	//console.log('<TEAM> ' + angular.toJson(vm.cleanerTeam));
                	
                	cleanerTeamFactory.setTeam(vm.cleanerTeam);
                	cleanerTeamFactory.setTeamSize(data.teamSize);

                }).error(function(err) {
                	
                }).finally(function() {
            		// force reload of team
                	var reload = cleanerTeamFactory.getTeamSelectionList();

                });
		}

		$scope.updateZones = function() {
			$scope.FranchiseId = vm.cleaner.SelectedFranchise.Id;
			loadFranchiseZones();
		}

		$scope.openTeamPopupForm = function (item) {
			//console.log("<CLEANER VM> - " + angular.toJson($scope.teamMember));
			//console.log("<CLEANER VM> - " + angular.toJson(item));

			if (!item)
			{
				$scope.teamMember = null;
			}

			if (!$scope.teamMember)
			{
				$scope.teamMember = {};
				$scope.teamMember.IsNewItem = true;
				$scope.teamMember.PhysicalAddress = {};
				$scope.teamMember.PostalAddress = {};
			}

			//console.log("<CLEANER scope> - " + angular.toJson($scope.teamMember));

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
						$scope.teamMember.PostalAddress = angular.copy(vm.cleaner.PostalAddress);
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
              template: 'static/CleanerTeam.html',
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
	   			if (response.IsValid)
        		{	
	   				vm.isSaving = true;

	   				ngDialog.close(); 
				}

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
						loadTeamMembers();
						location.reload();
	            	});
			}
		}


		vm.submitted = false;
      	vm.validateInput = function(name, type) {
        	var input = vm.cleanerForm[name];
    		return (input.$dirty || vm.submitted) && input.$error[type];
      	};

      	vm.saveData = function() {
      		//console.log("<CLEANER SAVE> - " + angular.toJson(vm.cleaner));
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
            		//console.log("<CLEANER SAVE> - " + angular.toJson(response));

                	// Add your success stuff here
                	HandleBusySpinner.stop($scope, panelName);
                	$scope.submitted = false;
                	ShowUserMessages.show($scope, response, "Error updating details.");

                	if (response.IsValid)
            		{	
                		vm.cleaner = response.DataItem;
                		$scope.CleanerId = vm.cleaner.Id;
                		$scope.FranchiseId = vm.cleaner.MasterFranchiseRefId;

    					if ($scope.DataRecordStatus.IsNewDataRecord)
                		{
            				$scope.DataRecordStatus.IsNewDataRecord = false;
                		}

                		activate();
                		$rootScope.childMessage = response;
                		$state.go("app.cleaner_details", { "CleanerId": $scope.CleanerId});
					}

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
        		//return false;
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
	function CleanerAvailabilityController($scope, $http, HandleBusySpinner, ShowUserMessages, cleanerTeamFactory, moment)
	{
		var vm = this;
		var panelName = "panelCleanerDetails";
		var Id = $scope.CleanerId;

		vm.cleanerRoster = [];
		vm.maxTeamSize = 1;

		activate();

		function activate()
		{
        	vm.isMeridian = true;
        	vm.hrStep = 1;
        	vm.minStep = 5;

        	var days = 7;

        	for(var i=1; i<=days; i++)
        	{
        		var roster = {
        			Weekday : "",
        			IsActive : false,
        			StartTime : undefined,
        			EndTime : undefined,
        			TeamCount : null,
        			TeamMembers: []
        		};

        		// not zero based to number days from 1
        		switch(i){
        			case 1: 
        				roster.Weekday = "Monday";
        				break;
					case 2: 
        				roster.Weekday = "Tuesday";
        				break;
					case 3: 
        				roster.Weekday = "Wednesday";
        				break;
					case 4: 
        				roster.Weekday = "Thursday";
        				break;
					case 5: 
        				roster.Weekday = "Friday";
        				break;
					case 6: 
        				roster.Weekday = "Saturday";
        				break;
					case 7: 
        				roster.Weekday = "Sunday";
        				break;
        		}

        		vm.cleanerRoster.push(roster);
        	}

        	if (Id)
        		loadCleanerRoster();
		}

		function loadCleanerRoster()
		{
			$http.get('/cleaners/getcleanerroster/?CleanerId=' + Id)
                .success(function (data) {
                	//console.log("<cleanerRoster DATA> - " + angular.toJson(data));
                	angular.forEach(vm.cleanerRoster, function(value, key) {
                		var _day = value.Weekday;
                		angular.forEach(data.list, function(value2, key2) {
                			var _innerDay = value2.Weekday;

                			if (_day == _innerDay)
                			{
                				vm.cleanerRoster[key].StartTime = value2.StartTime;
                				vm.cleanerRoster[key].EndTime = value2.EndTime;
                				vm.cleanerRoster[key].TeamCount = value2.TeamCount;
                				vm.cleanerRoster[key].IsActive = value2.IsActive;
                				vm.cleanerRoster[key].TeamMembers = angular.copy(value2.TeamMembers);
                			}
            			});
                	});

                }).error(function(err) {
                	
                }).finally(function() {
                	console.log("<cleanerRoster DATA2> - " + angular.toJson(vm.cleanerRoster));
                });

		}

		$scope.cleanerRosterChanged = function(val, dateVal) {
			if (!val.IsActive)
    		{
				val.StartTime =  undefined;
				val.EndTime = undefined;
				val.TeamCount = null;
				val.TeamMembers = [];
    		} else {
    			console.log("<CLEANER ROSTER Change> - " + angular.toJson(dateVal));
    		
    			if (val.TeamMembers)
    			{
    				val.TeamCount = val.TeamMembers.length;
    			}
    			else
    			{
    				val.TeamCount = 0;
    			}

				if (dateVal != null && dateVal != undefined)
				{
					var _newDate = new Date(dateVal.getTime() + (60000 * dateVal.getTimezoneOffset()));
					console.log("<CLEANER ROSTER Change> - " + angular.toJson(dateVal.getTimezoneOffset()));
    				console.log("<CLEANER ROSTER Change> - " + angular.toJson(_newDate));
    				return _newDate;
				}
    		}
    	}

    	$scope.Range = function() {
    		var result = [];
    		for(var i=1; i<=cleanerTeamFactory.getTeamSize(); i++)
    		{
    			result.push(i);
    		}
    		return result;
    	}

    	$scope.getTeamList = function() {
    		if (cleanerTeamFactory.getIsTeamSelectionListSet() == true)
    		{
    			return cleanerTeamFactory.getTeamSelectionList();
    		}
    		else
    		{
    			return null;
    		}
    	}

    	vm.validateInput = function(name, type) {
        	var input = vm.cleanerRosterForm[name];
    		return (input.$dirty || vm.submitted) && input.$error[type];
      	};

		vm.saveData = function(data) {
      		console.log("<CLEANER ROSTER Data> - " + angular.toJson(vm.cleanerRoster));
		 	$scope.submitted = true;

			if (vm.cleanerRosterForm.$valid) {

                HandleBusySpinner.start($scope, panelName);
	
            	return $http.post('/cleaners/saveCleanerRoster/?CleanerId='+Id, vm.cleanerRoster).success(function (response) {
                	// Add your success stuff here
                	HandleBusySpinner.stop($scope, panelName);
                	$scope.submitted = false;
                	ShowUserMessages.show($scope, response, "Error updating roster details.");

                	if (response.IsValid)
            		{	
                		vm.cleanerRoster = response.DataItem;
					}

            	}).error(function (error) {
            		HandleBusySpinner.stop($scope, panelName);
            		$scope.submitted = false;
                	ShowUserMessages.show($scope, error, "Error updating roster details.");

            	});


        	}
        	else
        	{
        		//console.log("<XX> - " + angular.toJson(vm.cleanerRosterForm.$error));
            	$scope.submitted = false;
            	ShowUserMessages.show($scope, "Error updating roster details - please review validation errors", "Error updating roster details.");
        		return false;
        	}
      	}

		
	}

	/**************************/
	/***   CLEANER LEAVE    ***/
	/**************************/
	function CleanerLeaveController($scope, $filter, $http, HandleBusySpinner, ShowUserMessages, editableOptions, editableThemes)
	{
		var vm = this;
		var panelName = "panelCleanerDetails";
		var CleanerId = $scope.CleanerId;

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
			if (!CleanerId)
				return;

			HandleBusySpinner.start($scope, panelName);
			
			$http.get('/cleaners/getleavedates?CleanerId='+CleanerId)
                .success(function (data) {
                	vm.listOfLeave = data.list;
                	vm.nextNewGuid = data.nextGuid;

                }).error(function(err) {

                }).finally(function() {

					angular.forEach(vm.listOfLeave, function(value, key) {
						value.StartDate = new Date(value.StartDate);
						value.EndDate = new Date(value.EndDate);
					});
                	//console.log("<LEAVE loaded> - " + angular.toJson(vm.listOfLeave));
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
	       			return $http.post('/cleaners/DeleteLeaveDates/?id=' + id).success(function (response) {
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
					PrimaryCleanerRefId: CleanerId,
					Id: id,
					IsNewItem: isNew
				});

            //console.log("<LEAVE data post> - " + angular.toJson(data));
       		return $http.post('/cleaners/saveleavedates', data).success(function (response) {
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

		$scope.searchCustomers = function(leaveItem) {
			alert('affected customer search coming soon');
		}
	}

})();

