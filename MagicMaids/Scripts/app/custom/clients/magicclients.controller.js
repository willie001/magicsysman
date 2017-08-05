(function() {
    'use strict';

    angular
        .module('magicclients',[])
        .controller('ClientsController', ClientsController);
	
	ClientsController.$inject = ['$filter', '$http', 'editableOptions', 'editableThemes','$q'];
	function ClientsController($filter, $http, editableOptions, editableThemes, $q)
	{
		var vm = this;
		activate();

		function activate()
		{
			vm.listOfClients = null;
			$http.get('/clients/getclients')
				.success(function (data) {

					vm.listOfClients = data.list;

				});
			
		}

		vm.validateData = function(data, id) {
            if (id.length == 0 ) {
              return 'Primary key is not set';
            }

            if (data.GivenName.length == 0) {
              return 'Given name is mandatory';
            }

            if (data.Surname.length == 0) {
              return 'Surname is mandatory';
            }

            if (data.Title.length == 0) {
              return 'Tiele is mandatory';
            }
          };

          vm.addClient = function() {
	          vm.inserted = {
	              TItle: '',
	              GivenName: '',
	              Surname: ''
	            };
	            vm.listOfClients.push(vm.inserted);
          }

          vm.saveClient = function(data, id) {
          }

          vm.removeClient = function(index) {
            //vm.users.splice(index, 1);
          };
	}
})();