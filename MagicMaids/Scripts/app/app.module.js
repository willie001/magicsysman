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
          	'angular.filter',
			'cleave.js'
        ])

		.filter('trustAsHtml',['$sce', function($sce) {
			  return function(text) {
			    return $sce.trustAsHtml(text);
			  };
			}])

		.factory('moment', ['$window',  function($window) {
      		return $window.moment;
    	}])

    	.factory('manageTimeZoneCookie', ['$cookies', 'moment', function($cookies, moment) {
    		var factory = {};
	    		
    		factory.set = function($cookies, moment)
			{
				var timezone_cookie = "timezoneoffset";
	      		var timezoneName_cookie = "timezonename";
	      		if (!$cookies.get(timezone_cookie) || !$cookies.get(timezoneName_cookie)) { // if the timezone cookie not exists create one.
	            
	                // check if the browser supports cookie
	                var test_cookie = 'test cookie';
	                $cookies.put(test_cookie, 'oatmeal');

	                if ($cookies.get(test_cookie)) { // browser supports cookie

	                    // delete the test cookie.
	                    $cookies.remove(test_cookie);

	                    // create a new cookie 
	                    var currentName = moment.tz.guess();
	                    $cookies.put(timezone_cookie, new Date().getTimezoneOffset());
	                    $cookies.put(timezoneName_cookie, currentName);
	                    location.reload(); // re-load the page
	                }
	            }
	            else { 
	            	// if the current timezone and the one stored in cookie are different then
	                // store the new timezone in the cookie and refresh the page.
	                var storedOffset = parseInt($cookies.get(timezone_cookie));
	                var currentOffset = new Date().getTimezoneOffset();
	                if (storedOffset !== currentOffset) { // user may have changed the timezone
	                    $cookies.put(timezone_cookie, new Date().getTimezoneOffset());
	                }
	                var storedName = $cookies.get(timezoneName_cookie);
	                var currentName = moment.tz.guess();
	                if (storedName && storedName !== currentName) { // user may have changed the timezone
	                    $cookies.put(timezoneName_cookie, currentName);
	                    location.reload();
	                }
	            }
            }

            return factory;
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

		.directive('datetimepickerNeutralTimezone', function() {
		    return {
		      restrict: 'A',
		      priority: 1,
		      require: 'ngModel',
		      link: function (scope, element, attrs, ctrl) {
				    ctrl.$formatters.push(function (value) {
						if (value == undefined)
						{
							return;
						}
						var date = new Date(Date.parse(value));
	          			if (date.getFullYear() <= 1970)
						{
							return;
						}
						date = new Date(date.getTime() + (60000 * date.getTimezoneOffset()));
	          			return date;
		        });

		        ctrl.$parsers.push(function (value) {
						var date = new Date(value.getTime() - (60000 * value.getTimezoneOffset()));
		          		return date;
		        });
		      }
		  }
		})
			
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

					if (msgs != undefined && msgs.MsgCssClass)
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

