(function() {
    'use strict';

    angular
    	.module("magicsearches",[])
    	.controller('MainSearchController', MainSearchController);

    	MainSearchController.$inject = ['$scope', '$http', 'HandleBusySpinner', 'ShowUserMessages','DTOptionsBuilder','editableOptions', 'editableThemes'];
    
    /***********************/
	/***   MAIN SEARCH   ***/
	/***********************/
	function MainSearchController($scope, $http, HandleBusySpinner, ShowUserMessages, DTOptionsBuilder, editableOptions, editableThemes)
	{
		var vm = this;
		var panelName = "panelMainResults";
	
		vm.Search = {};
		vm.SeachResults = {};
		vm.hasSearched = false;

		$scope.userMessages = [];
		$scope.userMessageType = [];

		activate();

		function activate() {
			HandleBusySpinner.start($scope, panelName);

			$scope.dtOptions =  DTOptionsBuilder.newOptions().withOption('order', [5, 'desc']);

			editableOptions.theme = 'bs3';

          	editableThemes.bs3.inputClass = 'input-sm';
          	editableThemes.bs3.buttonsClass = 'btn-sm';
          	editableThemes.bs3.submitTpl = '<button type="submit" class="btn btn-success"><span class="fa fa-check"></span></button>';
            editableThemes.bs3.cancelTpl = '<button type="button" class="btn btn-default" ng-click="$form.$cancel()">'+
                                           '<span class="fa fa-times text-muted"></span>'+
                                         '</button>';

			vm.Search.ServiceLength = 2;
			vm.Search.ServiceDate = new Date();
			vm.Search.ServiceType = "W"
			vm.Search.ServiceDayValue = "1";
			vm.Search.changeServiceType = function() {
				vm.Search.WeeklyJob = (vm.Search.ServiceType=="W") ? true : false;
				vm.Search.FortnightlyJob = (vm.Search.ServiceType=="F") ? true : false;
				vm.Search.OneOffJob = (vm.Search.ServiceType=="O") ? true : false;
			}
			vm.Search.changeServiceDay = function() {
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
			}
			vm.Search.changeServiceType();
			vm.Search.changeServiceDay();

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
		}

		$scope.clearForm = function() {
				vm.Search = {};
				vm.SearchResults = {};
				vm.hasSearched = false;

				activate();
			}

		$scope.searchMatches = function() {
			ShowUserMessages.clear($scope);

			vm.hasSearched = true;
            HandleBusySpinner.start($scope, panelName);

            //console.log("<CLIENT Search> - " + angular.toJson(vm.Search));
			$http.post('/search/matchcleaners', vm.Search).success(function (response) {
				if (!response.IsValid && response.IsValid !== undefined)
        		{	
	   				HandleBusySpinner.stop($scope, panelName);
            		ShowUserMessages.show($scope, response, "Error performing search.");
            		vm.hasSearched = false;
				}
				else
				{
					vm.SearchResults = response.SearchResults;
					//console.log("<MAIN Search Results> - " + angular.toJson(response));
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
			console.log("<MAIN Search validate> - " + angular.toJson(data));
			if (data.length == 0) {
              return colName + ' is mandatory';
            }
      	};
	}

})();