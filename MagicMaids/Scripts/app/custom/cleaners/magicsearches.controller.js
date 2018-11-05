
(function() {
    'use strict';
	angular
    	.module("magicsearches",[])
    	.controller('MainSearchController', MainSearchController);

    	MainSearchController.$inject = ['$scope', '$http', '$state','HandleBusySpinner', 'ShowUserMessages','DTOptionsBuilder','editableOptions', 'editableThemes', '$cookies', 'moment','manageTimeZoneCookie','savedJobBookingFactory'];
    
    /***********************/
	/***   MAIN SEARCH   ***/
	/***********************/
	function MainSearchController($scope, $http, $state, HandleBusySpinner, ShowUserMessages, DTOptionsBuilder, editableOptions, editableThemes, $cookies, moment, manageTimeZoneCookie, savedJobBookingFactory)
	{
		var vm = this;
		var panelName = "panelMainResults";

		$scope.changeServiceType = function() {
			vm.Search.WeeklyJob = (vm.Search.ServiceType=="W") ? true : false;
			vm.Search.FortnightlyJob = (vm.Search.ServiceType=="F") ? true : false;
			vm.Search.OneOffJob = (vm.Search.ServiceType=="O") ? true : false;
			vm.Search.VacateClean = (vm.Search.ServiceType=="V") ? true : false;
		}

		$scope.changeZoneFilter = function() {
			if (vm.Search.FilterZonesNone == true)
			{
				// ensure only vacate or once off selected for ALL zone filter - need specific date
				if (vm.Search.ServiceType == "W" || vm.Search.ServiceType == "F")
				{
					vm.Search.ServiceType = "O";
					vm.Search.WeeklyJob = false;
					vm.Search.FortnightlyJon = false;
					vm.Search.OneOffJob = true;
				}
			}
		}

		$scope.changeServiceDay = function() {
			if (vm.Search.ServiceDayValue == 1)
				vm.Search.ServiceDay = "Monday";

			if (vm.Search.ServiceDayValue == 2)
				vm.Search.ServiceDay = "Tuesday";

			if (vm.Search.ServiceDayValue == 3)
				vm.Search.ServiceDay = "Wednesday";

			if (vm.Search.ServiceDayValue == 4)
				vm.Search.ServiceDay = "Thursday";

			if (vm.Search.ServiceDayValue == 5)
				vm.Search.ServiceDay = "Friday";

			if (vm.Search.ServiceDayValue == 6)
				vm.Search.ServiceDay = "Saturday";

			if (vm.Search.ServiceDayValue == 7)
				vm.Search.ServiceDay = "Sunday";

			vm.Search.ServiceDayValue = vm.Search.ServiceDayValue.toString();	// does not like it if the value is integer
		}

		$scope.clearForm = function() {
			var cookieName = "SearchCriteria_cleanerMatch";
			savedJobBookingFactory.set(null, null);
			if ($cookies.get(cookieName)) { // browser supports cookie
				$cookies.remove(cookieName);
				activate();
			}	
		}

		$scope.getLocation = function(val) {
    		return $http.get('/search/getsearchsuburbs', {
	          	params: {
	            	address: val,
	            	sensor: false
	          	}
    		}).then(function(res){
	      		var addresses = [];
	      		angular.forEach(res.data.item, function(value){
	    			/*jshint -W106*/
	        		addresses.push(value);
	      		});
	      		vm.SuburbList = addresses;
				//console.log("<SEARCH Suburb List> - " + angular.toJson(vm.SuburbList));
	      		
    		});
		}

		$scope.checkCustomerState = function(selectedCleaner, selectedJob) {
			//console.log("<JOB PICKED -  cleaner> - " + angular.toJson(selectedCleaner));
			//console.log("<JOB PICKED -  job> - " + angular.toJson(selectedJob));

			savedJobBookingFactory.set(selectedCleaner, selectedJob);

			if (vm.Search.RepeatCustomer == "N")
			{
				$state.go("app.client_details");
			}
			else
			{
				$state.go("app.clients");
			}
		}

		activate();
			
		function activate() {
			vm.Search = {};
			vm.SeachResults = {};
			vm.Search.FilterZone = {};
			vm.hasSearched = false;
			vm.SuburbList = [];

			$scope.getLocation();

			$scope.userMessages = [];
			$scope.userMessageType = [];

			HandleBusySpinner.start($scope, panelName);
			manageTimeZoneCookie.set($cookies, moment, location);

			$scope.dtOptions =  DTOptionsBuilder.newOptions().withOption('order', [1, 'desc']);

			editableOptions.theme = 'bs3';

          	editableThemes.bs3.inputClass = 'input-sm';
          	editableThemes.bs3.buttonsClass = 'btn-sm';
          	editableThemes.bs3.submitTpl = '<button type="submit" class="btn btn-success"><span class="fa fa-check"></span></button>';
            editableThemes.bs3.cancelTpl = '<button type="button" class="btn btn-default" ng-click="$form.$cancel()">'+
                                           '<span class="fa fa-times text-muted"></span>'+
                                         '</button>';
                                         
			vm.Search.Ironing = false;
			var _dfltGap = 2;
			vm.Search.ServiceLengthMins = _dfltGap*60;
			vm.Search.ServiceLengthForControl = new Date(2001, 1, 1, _dfltGap, 0);
			vm.Search.ServiceDate = new Date();
			vm.Search.ServiceType = "W"
			vm.Search.ServiceDayValue = "1";
			vm.Search.FilterZonesPrimary = true;
			vm.Search.FilterZonesSecondary = true;
			vm.Search.FilterZonesApproved = false;
			vm.Search.FilterZonesOther = false;
			vm.Search.RepeatCustomer = "Y";

			vm.isMeridian = true;
    		vm.hrStep = 1;
    		vm.minStep = 5;

			$scope.changeServiceType();
			$scope.changeServiceDay();

			vm.date = [];
			vm.date.clear = function () {
	            vm.Search.ServiceDate = null;
          	};	

          	// Disable weekend selection
	       	vm.date.disabled = function(date, mode) {
	       		//return ( mode === 'day' && ( date.getDay() === 0 || date.getDay() === 6 ) );
	       		return ( mode === 'day' && ( date.getDay() === 0 ) );
	        };

          	vm.date.toggleDateRange = function() {
	            vm.date.minDate = new Date();
          		vm.date.maxDate = vm.date.minDate.setMonth(vm.date.minDate.getMonth() + 6);
          	};
          	vm.date.toggleDateRange();

          	vm.date.open = function($event) {
            	$event.preventDefault();
            	$event.stopPropagation();

            	vm.date.opened = true;
          	};

          	vm.date.dateOptions = {
            	formatYear: 'yyyy',
	            startingDay: 1		
          	};

      		vm.date.initDate = new Date();
          	vm.date.formats = ['dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
          	vm.date.format = vm.date.formats[0];

			var criteria = {};
			$http.get('/search/GetSearchCriteria/')
                .success(function (data) {
                    criteria = data.item;
					if (criteria.HasCriteria == true)
					{
						vm.Search = criteria;
						$scope.changeServiceType();
						$scope.changeServiceDay();
						vm.Search.FilterRating = vm.Search.FilterRating.toString();		// does not like it if the value is integer
						vm.Search.ServiceDate = new Date(vm.Search.ServiceDate);		// uib-datepicker is blank when repopulated even if date is set

						$scope.searchMatches();
					}
					//console.log("<SEARCH Criteria> - " + angular.toJson(data.item));
			
                }).error(function(err) {
                }).finally(function() {
				});

			$scope.searchCriteria = false; // expand search panel on first load
		}

		$scope.searchMatches = function() {
			ShowUserMessages.clear($scope);
			vm.hasSearched = true;
            HandleBusySpinner.start($scope, panelName);

            //console.log("<CLIENT Search> - " + angular.toJson(vm.Search));
			$http.post('/search/matchcleaners', vm.Search).success(function (response) {
				//console.log("<MAIN Search Results> - " + angular.toJson(response));
				if (!response.IsValid && response.IsValid !== undefined)
        		{	
	   				HandleBusySpinner.stop($scope, panelName);
            		ShowUserMessages.show($scope, response, "Error performing search.");
            		vm.hasSearched = false;
				}
				else
				{
					$scope.searchCriteria = true;
					vm.SearchResults = response.SearchResults;
					HandleBusySpinner.stop($scope, panelName);
				}

			}).error(function (error) {
        		//console.log("<MAIN Search Errors> - " + angular.toJson(error));
        		HandleBusySpinner.stop($scope, panelName);
            	ShowUserMessages.show($scope, error, "Error performing search.");
            	vm.hasSearched = false;
        	});   	
		}

		$scope.validateData = function(data, colName) {
			//console.log("<MAIN Search validate> - " + angular.toJson(data));
			if (data.length == 0) {
              return colName + ' is mandatory';
            }
      	};
	}

})();