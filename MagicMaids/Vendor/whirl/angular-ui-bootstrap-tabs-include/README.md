# Angular UI Bootstrap Tab Include

This directive allows you to lazily load HTML fragments into Bootstrap tabs.

## Requirements

[Angular UI Bootstrap](https://github.com/angular-ui/bootstrap)

## Installation

Install using `bower`

    bower install --save angular-ui-bootstrap-tabs-include

Load the script files into your application.

```html
<script src="bower_components/angular/angular.js"></script>
<script src="bower_components/angular-ui-bootstrap-bower/ui-bootstrap-tpls.js"></script>
<script src="bower_components/angular-ui-bootstrap-tabs-include/lib/angular-ui-bootstrap-tabs-include.js"></script>
```

Add this module as a dependency in your AngularJS app.

```javascript
angular.module('myApp', [ 'ui.bootstrap', 'ui.bootstrap.tabs.include' ]);
```

## Usage

The `tabInclude` directive is a simple wrapper around `ngInclude` that is aware of the tab state.

For each of the tabs in your tabset, you can now include a `tab-include` attribute that points to a partial which will be loaded into the tab pane when selected.

```html
<tabset>
    <tab heading="Foo" tab-include="'partials/foo.html'"></tab>
    <tab heading="Bar" tab-include="'partials/bar.html'"></tab>
</tabset>
```

 Other options supported by `ngInclude` are supported by `tabInclude` as well:

- `tab-onload`
- `tab-autoscroll`

You can read about the `ngInclude` directive and its options from [here](https://code.angularjs.org/1.2.26/docs/api/ng/directive/ngInclude).

## See Also
[http://blog.xebia.com/2013/08/28/bootstraps-tabs-and-lazy-data-loading-in-angularjs](http://blog.xebia.com/2013/08/28/bootstraps-tabs-and-lazy-data-loading-in-angularjs)