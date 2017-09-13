(function() {

    'use strict';

    var deps;
    var module;

    deps = [ 'ui.bootstrap' ];
    module = angular.module('ui.bootstrap.tabs.include', deps);

    module.directive('tabInclude', function() {

        function postLink(scope, iElement, iAttrs) {
            // We don't need the attribute watcher because even if the tabUrl 
            // has changed, the tab still has to be selected for 
            // ng-include to be activated.
            
            // http://stackoverflow.com/a/26863821/273493
            iElement.isolateScope().$watch('active',
                function(newValue) {
                    if (newValue) {
                        scope.$include = iAttrs.tabInclude;
                    }
                }
            );
        }

        return {
            require: [ '^tab', '^tabset' ],
            restrict: 'A',
            priority: 1,
            compile: function(tElement, tAttrs, transclude) {
                
                // During compilation of each tab, add the markup for the 
                // ng-include element. The source of the element will be 
                // filled during the linking phase.
                var elem = angular.element('<div></div>');
                elem.attr('ng-include', '$include');
                if (tAttrs.tabOnload) {
                    elem.attr('onload', tAttrs.tabOnload);
                }
                if (tAttrs.tabAutoscroll) {
                    elem.attr('autoscroll', tAttrs.autoscroll);
                }
                tElement.append(elem);
                
                return postLink;
            }
        };
    });

})();