/**=========================================================
 * Refresh panels
 * [panel-refresh] * [data-spinner="standard"]
 =========================================================*/

(function() {
    'use strict';

    angular
        .module('app.panels')
        .directive('panelRefresh', panelRefresh);
       
    function panelRefresh () {
        var directive = {
            controller: Controller,
            restrict: 'A',
            scope: false
        };
        return directive;

    }

    Controller.$inject = ['$scope', '$element'];
    function Controller ($scope, $element) {
      var refreshEvent   = 'panel-refresh',
          whirlClass     = 'whirl',
          defaultSpinner = 'standard';

      // catch clicks to toggle panel refresh
      $element.on('click', function (e) {
          if (e) e.preventDefault();

          var $this = $(this),
              panel = $this.parents('.panel').eq(0),
              spinner = $this.data('spinner');

          triggerPanelRefresh(panel[0], spinner);
      });

      // listen to remove spinner
      $scope.$on('removeSpinner', removeSpinner);

      // method to clear the spinner when done
      function removeSpinner (ev, id) {
        if (!id) return;
        var newid = id.charAt(0) === '#' ? id : ('#'+id);
        angular
          .element(newid)
          .removeClass(whirlClass);
      }

      /**
        Params: 
            panel: the DOM element that correspond to .panel
            spinner: [optional] classname of the spinner to use
      */
      function triggerPanelRefresh(panel, spinner) {
          var $panel = $(panel);

          // start showing the spinner
          $panel.addClass(whirlClass + ' ' + (spinner || defaultSpinner));

          // Emit event when refresh clicked
          $scope.$emit(refreshEvent, $panel.attr('id'));

      }

      $scope.$on('triggerPanelRefresh', function (ev, panel, spinner) {
          triggerPanelRefresh(panel, spinner);
      });      

    }
})();


