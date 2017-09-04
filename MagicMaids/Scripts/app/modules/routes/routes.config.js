﻿/**=========================================================
 * Module: config.js
 * App routes and resources configuration
 =========================================================*/


(function() {
    'use strict';

    angular
        .module('app.routes')
        .config(routesConfig);

    routesConfig.$inject = ['$stateProvider', '$locationProvider', '$urlRouterProvider', 'RouteHelpersProvider'];
    function routesConfig($stateProvider, $locationProvider, $urlRouterProvider, helper){

        // Set the following to true to enable the HTML5 Mode
        // You may have to set <base> tag in index and a routing configuration in your server
        //$locationProvider.html5Mode(true);

        // defaults to dashboard
        $urlRouterProvider.otherwise('/Dashboard');

        //
        // Application Routes
        // -----------------------------------
        $stateProvider
          .state('app', {
              //url: '/app',
              abstract: true,
              //templateUrl: helper.basepath('App/Index'),
              resolve: helper.resolveFor('fastclick', 'modernizr', 'icons', 'screenfull', 'animo', 'sparklines', 'slimscroll', 'classyloader', 'toaster', 'whirl'),
              views: {
                  'content': {
                      template: '<div data-ui-view="" autoscroll="false" ng-class="app.viewAnimation" class="content-wrapper"></div>',
                      controller: ['$rootScope', function ($rootScope) {
                          // Uncomment this if you are using horizontal layout
                          // $rootScope.app.layout.horizontal = true;

                          // Due to load times on local server sometimes the offsidebar is displayed before go offscreen
                          // so it's hidden by default and after 1sec we show it offscreen
                          // [If removed, also the hide class must be removed from .offsidebar]
                          setTimeout(function () {
                              angular.element('.offsidebar').removeClass('hide');
                          }, 3000);

                      }]
                  }
              }

          })

          // url maps to controller method name, not the view.
          .state('app.dashboard', {
              url: '/Dashboard',
              title: 'Dashboard',
              templateUrl: helper.basepath('Dashboard/DashboardV1'),
              resolve: helper.resolveFor('flot-chart', 'flot-chart-plugins', 'weather-icons')
          })
          .state('app.servervars', {
              url: '/servervars',
              title: 'Server Variables',
              templateUrl: helper.basepath('Dashboard/ServerVars')
          })
          .state('app.clients', {
              url: '/clients',
              title: 'Search Customers',
              templateUrl: helper.basepath('Clients/Clients')
          })
          .state('app.client_details', {
              url: '/clientdetails',
              title: 'Customer Details',
              templateUrl: helper.basepath('Clients/ClientDetails')
          })
          .state('app.cleaners', {
              url: '/cleaners',
              title: 'Cleaners',
              templateUrl: helper.basepath('Cleaners/Cleaners')
          })
          .state('app.cleaner_details', {
              url: '/cleanerdetails',
              title: 'Cleaner Details',
              templateUrl: helper.basepath('Cleaners/CleanerDetails'),
              resolve: helper.resolveFor('angularFileUpload', 'filestyle')
          })

          .state('app.settings_user_accounts', {
              url: '/useraccounts',
              title: 'Settings - User Accounts',
              templateUrl: helper.basepath('Settings/UserAccounts')
          })
          .state('app.settings_user_account_details', {
              url: '/useraccountdetails',
              title: 'Settings - User Account Details',
              templateUrl: helper.basepath('Settings/UserAccountDetails')
          })

          .state('app.settings_master_franchises', {
              url: '/settings',
              title: 'Settings - Master Franchises',
              templateUrl: helper.basepath('Settings/Franchises')
          })

          .state('app.settings_master_franchise_register', {
              url: '/franchiseregister/:FranchiseId',
              controller: function($scope, $stateParams) {
            		$scope.FranchiseId = $stateParams.FranchiseId;
            	},
              title: 'Settings - Master Franchises - Register', 
              templateUrl: helper.basepath('Settings/FranchiseRegister'),
              resolve: helper.resolveFor('xeditable')
          })

          .state('app.settings_templates', {
              url: '/templates',
              title: 'Settings - Templates',
              templateUrl: helper.basepath('Settings/Templates'),
          })
          .state('app.settings_template_manage', {
              url: '/templatemanage',
              title: 'Settings - Manage Template',
              templateUrl: helper.basepath('Settings/TemplateManage'),
          })
          .state('app.settings_master_settings', {
              url: '/mastersettings',
              title: 'Global Settings',
              templateUrl: helper.basepath('Settings/MasterSettings'),
              resolve: helper.resolveFor('xeditable')
          })
          .state('app.settings_app_logsentries', {
              url: '/logentries',
              title: 'Application Logs',
              templateUrl: helper.basepath('LogEntries/LogEntries')
          })
          .state('app.settings_app_logsentry', {
              url: '/logentry/:Id',
              controller: function($scope, $stateParams) {
            		$scope.Id = $stateParams.Id;
            	},title: 'Application Log Details',
              templateUrl: helper.basepath('LogEntries/LogEntry')
          })

          //
          // Single Page Routes
          // -----------------------------------
          //.state('page', {
          //    url: '/page',
          //    abstract: true,
          //    views: {
          //        'main': {
          //            templateUrl: helper.basepath('Pages/Page'),
          //            controller: ['$rootScope', function ($rootScope) {
          //                $rootScope.app.layout.isBoxed = false;
          //            }]
          //        }
          //    },
          //    resolve: helper.resolveFor('modernizr', 'icons')

          //})
          //.state('page.404', {
          //    url: '/404',
          //    title: 'Not Found',
          //    templateUrl: helper.basepath('Pages/Error404')
          //})
          //.state('page.500', {
          //    url: '/500',
          //    title: 'Server error',
          //    templateUrl: helper.basepath('Pages/Error500')
          //})
          //
          // CUSTOM RESOLVES
          //   Add your own resolves properties
          //   following this object extend
          //   method
          // -----------------------------------
          // .state('app.someroute', {
          //   url: '/some_url',
          //   templateUrl: 'path_to_template.html',
          //   controller: 'someController',
          //   resolve: angular.extend(
          //     helper.resolveFor(), {
          //     // YOUR RESOLVES GO HERE
          //     }
          //   )
          // })
          ;

    } // routesConfig

})();

