(function() {
    'use strict';

    angular
        .module('magiclogs',[])
        .controller('LogEntriesController', LogEntriesController)
		.controller('LogEntryController', LogEntryController);

    LogEntriesController.$inject = ['$scope','$filter', '$http','$q', '$timeout'];
    LogEntryController.$inject = ['$scope','$filter', '$http','$q'];

    /***************************/
	/*** LOG ENTRIES SUMMARY ***/
	/***************************/
	function LogEntriesController($scope, $filter, $http, $q, $timeout)
	{
		var vm = this;
		activate();

		function activate()
		{
			vm.listOfLogs = null;
			$scope.isLoadingResults = false;
			var _incDisabled = 0;

    		$timeout(function(){
    			$scope.isLoadingResults = true;
   				$scope.$parent.$broadcast('triggerPanelRefresh', document.getElementById('panelApplicationLogs'),'traditional');
			},500);

			$http.get('/logentries/getlogentries/')
                .success(function (data) {
                	vm.listOfLogs = data.list;
                	$scope.isLoadingResults = false;

                }).error(function(err) {
                	
                }).finally(function() {
                	$timeout(function () {
                  		$scope.$broadcast('removeSpinner', 'panelApplicationLogs');
              		}, 500);
                });
		}
	}

	/*************************/
	/*** LOG ENTRY DETAILS ***/
	/*************************/
	function LogEntryController($scope, $filter, $http, $q)
	{
		    		
		var vm = this;
		var Id = $scope.Id;
		console.log("<LOGENTRY> - " + angular.toJson(vm));
        
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