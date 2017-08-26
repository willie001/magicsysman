(function() {
    'use strict';

    angular
    	.module("magiclogs",[])
    	.controller('LogEntriesController', LogEntriesController)
    	.controller('LogEntryController', LogEntryController);

    LogEntriesController.$inject = ['$scope','$filter','$http','$q','HandleBusySpinner'];
    LogEntryController.$inject = ['$scope','$filter','$http','$q'];

    /***************************/
	/*** LOG ENTRIES SUMMARY ***/
	/***************************/
	function LogEntriesController($scope, $filter, $http, $q, HandleBusySpinner)
	{
		var vm = this;
		activate();

		function activate()
		{
        	vm.listOfLogs = null;
			var _incDisabled = 0;

			HandleBusySpinner.start($scope, 'panelApplicationLogs');
        
			$http.get('/logentries/getlogentries/')
                .success(function (data) {
                	vm.listOfLogs = data.list;

                }).error(function(err) {
                	//no action
                }).finally(function() {
                	HandleBusySpinner.stop($scope, 'panelApplicationLogs');
                });
		}
	}

    /*************************/
	/*** LOG ENTRY DETAILS ***/
	/*************************/
	function LogEntryController($scope, $filter, $http, $q)
	{
		//alert('3');
		var vm = this;
		var Id = $scope.Id;
		//console.log("<LOGENTRY> - " + angular.toJson(vm));
        
		vm.logEntry = null;

		activate();

		function activate()
        {
            $http.get('/logentries/getLogEntry/?Id=' + Id)
                .success(function (data) {
                	//console.log("<LOGENTRY> - " + angular.toJson(data.item));
            		vm.logEntry = data.item;
                	$scope.Id = data.item.Id;

                }).error(function(err) {
                	
                }).finally(function() {

                });
		}

	}

})();

