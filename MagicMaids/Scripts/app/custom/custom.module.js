(function() {
    'use strict';

    angular
        .module('custom', [
            // request the the entire framework
            'magicmaids',
            // or just modules
            'app.core',
            'app.sidebar'
            /*...*/
        ]);
})();
