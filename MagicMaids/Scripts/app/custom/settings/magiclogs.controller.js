(function() {
    'use strict';

    angular
    	.module("magiclogs",[])
    	.controller('LogEntriesController', LogEntriesController)
    	.controller('LogEntryController', LogEntryController)
    	.factory('DeleteLogEntries', [function () {
			var LogEntriesFactory = {};

			LogEntriesFactory.deleteLogEntry = function ($scope,$http, ShowUserMessages, id) {

            	return $http.post('/logentries/deletelogentry/?id=' + id).success(function (response) {
                	// Add your success stuff here
        			ShowUserMessages.show($scope, response, "Error deleting log entry.");
                	
            	}).error(function (error) {
        			ShowUserMessages.show($scope, error, "Error deleting log entry.");

            	});
            }

            LogEntriesFactory.clearLogEntries = function($scope,$http, ShowUserMessages) {
            	if (confirm('Are you sure you want to delete ALL log entries?')) {
	       			return $http.post('/logentries/deletealllogentries/').success(function (response) {
                		// Add your success stuff here
        				ShowUserMessages.show($scope, response, "Error clearing log entries.");
                	
        			}).error(function (error) {
        				ShowUserMessages.show($scope, error, "Error clearing log entries.");

        			});
	        	}
            }

            return LogEntriesFactory;
		}]);

    LogEntriesController.$inject = ['$scope','$filter','$http','$q','HandleBusySpinner','ShowUserMessages','DeleteLogEntries','DTOptionsBuilder'];
    LogEntryController.$inject = ['$scope','$filter','$http','$q','$location','HandleBusySpinner','ShowUserMessages','DeleteLogEntries'];

    /***************************/
	/*** LOG ENTRIES SUMMARY ***/
	/***************************/
	function LogEntriesController($scope, $filter, $http, $q, HandleBusySpinner, ShowUserMessages, DeleteLogEntries,DTOptionsBuilder)
	{
		var vm = this;
		activate();

		//alert($scope.pageCount);
		/*$scope.dtOptions = DTOptionsBuilder.newOptions()
      	.withOption('serverSide', true)
      	.withOption('processing', true)
      	.withPaginationType('full_numbers');*/

		function activate()
		{
        	vm.listOfLogs = null;
			vm.disableButtons = false;

			HandleBusySpinner.start($scope, 'panelApplicationLogs');
        
			$http.get('/logentries/getlogentries/')
                .success(function (data) {
                	vm.listOfLogs = data.list;
                	vm.disableButtons = (vm.listOfLogs.length == 0);

                }).error(function(err) {
                	//no action
                }).finally(function() {
                	HandleBusySpinner.stop($scope, 'panelApplicationLogs');
                });
		}

		$scope.deleteEntry = function(id, ix) {
			HandleBusySpinner.start($scope, 'panelApplicationLogs');
			DeleteLogEntries.deleteLogEntry($scope, $http, ShowUserMessages, id);
			HandleBusySpinner.stop($scope, 'panelApplicationLogs');

			if (ix)
			{
				vm.listOfLogs.splice(ix, 1);
			}
			else
			{
				activate();
			}
		}

		$scope.clearLogEntries = function() {
			HandleBusySpinner.start($scope, 'panelApplicationLogs');
			DeleteLogEntries.clearLogEntries($scope, $http, ShowUserMessages);
			HandleBusySpinner.stop($scope, 'panelApplicationLogs');

			activate();
		}
	}

    /*************************/
	/*** LOG ENTRY DETAILS ***/
	/*************************/
	function LogEntryController($scope, $filter, $http, $q, $location, HandleBusySpinner, ShowUserMessages, DeleteLogEntries)
	{
		var vm = this;
		var Id = $scope.Id;
		//console.log("<LOGENTRY> - " + angular.toJson(vm));
        
		vm.logEntry = null;

		activate();

		function activate()
        {
        	HandleBusySpinner.start($scope, 'panelApplicationLogs');
        
            $http.get('/logentries/getLogEntry/?Id=' + Id)
                .success(function (data) {
                	//console.log("<LOGENTRY> - " + angular.toJson(data));
            		vm.logEntry = data.item;
                	if (data.item.Id != null)
                	{
                		$scope.Id = data.item.Id;
					}
                }).error(function(err) {
                	
                }).finally(function() {
                	HandleBusySpinner.stop($scope, 'panelApplicationLogs');
                });
		}

		$scope.deleteEntry = function() {
			HandleBusySpinner.start($scope, 'panelApplicationLogs');
			DeleteLogEntries.deleteLogEntry($scope, $http, ShowUserMessages, $scope.Id);
			HandleBusySpinner.stop($scope, 'panelApplicationLogs');

			$location.path('/logentries');
		}
			
	}

})();

