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
            'app.icons',
            'app.flatdoc',
            'app.notify',
            'app.bootstrapui',
            'app.elements',
            'app.panels',
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
            'magicsearches',
            'magiclogs',
            'magiccleaners',
            'toggle-switch',
          	'angular.filter'
        ])

		.filter('trustAsHtml',['$sce', function($sce) {
			  return function(text) {
			    return $sce.trustAsHtml(text);
			  };
			}])

		.factory('HandleBusySpinner', ['$timeout', function ($timeout) {

    		var factory = {};
    		var spinTime = 150;

    		factory.start = function ($scope, panelName) {
		    	$scope.isLoadingResults = true;
   				return $timeout(function() {
        			var _panel = document.getElementById(panelName);
        			if (_panel)
        			{
    					$scope.$emit('triggerPanelRefresh', _panel, 'traditional');
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
	              		$scope.$emit('removeSpinner', panelName);
	          		}, spinTime);
    			}
		    }

		    return factory;
		}])


		.factory('ShowUserMessages', [function () {
			var factory = {};

			factory.clear = function($scope){
				$scope.userMessages.splice(0, $scope.userMessages.length); // clear message array
				$scope.userMessageType.splice(0, $scope.userMessageType.length);
			}

			factory.show = function($scope, msgs, titleMsg) {
				/*
				console.log("<X1 usermsg> - " + angular.toJson($scope.userMessages));
				console.log("<X2 msg type> - " + angular.toJson($scope.userMessageType));
				console.log("<X3 msgs> - " + angular.toJson(msgs));
				*/
				if ($scope.userMessages && $scope.userMessageType)
				{
					$scope.userMessages.splice(0, $scope.userMessages.length); // clear message array

					if (msgs.MsgCssClass)
	            		$scope.userMessageType.push(msgs.MsgCssClass);
					else
						$scope.userMessageType.push('alert bg-warning-light');
				}
				
            	var _bHasErrors = false;
            	var prevMsg = "";
            	//console.log("<X4 errors> - " + angular.toJson(msgs.Errors));
            	if (msgs.Errors && angular.isObject(msgs.Errors)) {
                	for (var i = 0; i < msgs.Errors.length; i++) {
                		var message = msgs.Errors[i].Message;
                		var key = msgs.Errors[i].Key;
                		_bHasErrors = true;	
                		if (prevMsg != message && $scope.userMessages)
                		{
                			if (message == "A value is required." && key.length > 0)
                			{
                				$scope.userMessages.push('A value is required for ' + key) ;
                			}
                			else
                			{
                				$scope.userMessages.push(message) ;
                			}
            				
            			}
            			prevMsg = message;
        			}
                };

                if (!_bHasErrors && $scope.userMessages)
                {
	                if (msgs.Message != null) {
	                    $scope.userMessages.push(msgs.Message);
					} else if (msgs != null) {
						$scope.userMessages.push(msgs);
	                } else {
	                	$scope.userMessages.push(titleMsg);
	                }
                };
                //console.log("<X5> - " + angular.toJson($scope.userMessages));
				
                window.scrollTo(0,0);
			}

			return factory;
		}]);

})();

