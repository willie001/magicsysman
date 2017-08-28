/*!
 * 
 * Angle - Bootstrap Admin App + AngularJS
 * 
 * Version: 3.5.4
 * Author: @themicon_co
 * Website: http://themicon.co
 * License: https://wrapbootstrap.com/help/licenses
 * 
 */

// APP START
// ----------------------------------- 

(function() {
    'use strict';

    angular
        .module('magicmaids', [
            'app.core',
            'app.routes',
            'app.sidebar',
            'app.navsearch',
            'app.preloader',
            'app.loadingbar',
            'app.translate',
            'app.settings',
            'app.dashboard',
            'app.icons',
            'app.flatdoc',
            'app.notify',
            'app.bootstrapui',
            'app.elements',
            'app.panels',
            'app.charts',
            'app.forms',
            'app.locale',
            'app.maps',
            'app.pages',
            'app.tables',
            'app.extras',
            'app.mailbox',
            'app.utils',
            'magicsettings',
            'magicclients',
            'magiclogs',
            'toggle-switch',
          	'angular.filter'
        ])
        .factory('HandleBusySpinner', ['$timeout', function ($timeout) {

    		var factory = {};
    		var spinTime = 300;

    		factory.start = function ($scope, panelName) {
		    	$scope.isLoadingResults = true;
   				return $timeout(function() {
        			var _panel = document.getElementById(panelName);
        			if (_panel)
        			{
    					$scope.$broadcast('triggerPanelRefresh', _panel, 'traditional');
   					}
				},spinTime);  
    		}

		    //method that returns an object from given array
		    factory.stop = function ($scope, panelName) {
		        $scope.isLoadingResults = false;
	            var _panel = document.getElementById(panelName);
    			if (_panel)
    			{
    				return $timeout(function () {
	              		$scope.$broadcast('removeSpinner', panelName);
	          		}, spinTime);
    			}
		    }

		    return factory;
		}])
		.factory('ShowUserMessages', [function () {
			var factory = {};

			factory.show = function($scope, msgs, titleMsg) {

				//console.log("<X2 scope> - " + angular.toJson(msgs));
            	if (msgs.MsgCssClass)
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
	                	$scope.userMessages.push(titleMsg);
	                }
                };
                //console.log("<X3> - " + angular.toJson($scope.userMessages));
				window.scrollTo(0,0);
			}

			return factory;
		}]);
})();

