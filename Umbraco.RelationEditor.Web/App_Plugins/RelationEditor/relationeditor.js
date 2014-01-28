(function () {
    function EditRelationsController($scope, dialogService, relationsResources, assetsService, navigationService, eventsService) {
        var dialog;

        function relationPicked(index, data) {
            var set = $scope.resourceSets[index],
                relations = set.Relations,
                existingIndex = -1,
                existing = $.grep(relations, function(e, i) {
                    var isIt = e.ChildId === data.id;
                    if (isIt) {
                        existingIndex = i;
                    }
                    return isIt;
                });

            if (existing.length > 0) {
                if (existing[0].State === "Deleted") {
                    existing[0].State = existing[0].OldState;
                    relations.splice(existingIndex, 1);
                    relations.push(existing[0]);
                }
                return;
            }

            $scope.resourceSets[index].Relations.push({
                ChildId: data.id,
                ChildName: data.name,
                State: "New"
            });
        }

        $scope.ready = false;
        $scope.data = {};

        $scope.pickRelation = function (index) {
            var selected = eventsService.on("dialogs.treePickerController.select", function (evt, args) {
                relationPicked(index, args.node);
                args.node.filtered = true;
                dialogService.close(dialog);
            });

            dialogService.close(dialog);
            dialog = dialogService.treePicker({
                treeAlias: $scope.resourceSets[index].ChildType.TreeType,
                section: $scope.resourceSets[index].ChildType.Section,
                closeCallback: function () {
                    selected();
                }
            });
        };

        $scope.remove = function (relation) {
            relation.OldState = relation.State;
            relation.State = "Deleted";
        };

        $scope.isActive = function(relation) {
            return relation.State !== "Deleted";
        };

        $scope.save = function () {
            $scope.ready = false;
            relationsResources.save($scope.data)
                .then(function() {
                    navigationService.hideDialog();
                });
        };

        assetsService.loadCss(Umbraco.Sys.ServerVariables.umbracoSettings.appPluginsPath + "/RelationEditor/relationeditor.css");

        var promise = relationsResources.getById($scope.currentNode.section, $scope.currentNode.nodeType, $scope.currentNode.id);
        promise.then(function (data) {
            $scope.data = data;
            $scope.resourceSets = data.Sets;
            $scope.ready = true;
        });
    }

    function RelationsResources($q, $http, umbDataFormatter, umbRequestHelper) {
        return {
            getById: function(section, treeType, id) {
                return umbRequestHelper.resourcePromise(
                    $http.get(
                        Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + "/relationseditor/relations/getrelations", {
                            params: {
                                section: section,
                                treeType: treeType || "",
                                parentId: id
                            }
                        }),
                    'Failed to retreive relations for content id ' + id);
            },
            save: function(set) {
                return umbRequestHelper.resourcePromise(
                    $http.post(
                        Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + "/relationseditor/relations/saverelations", set),
                    "Failed to save relations for content id " + set.ParentId
                );
            }
        };
    }

    angular.module("umbraco")
        .factory("RelationEditor.RelationResources", ["$q", "$http", "umbDataFormatter", "umbRequestHelper", RelationsResources])
        .controller("RelationEditor.EditRelationsController", [
            "$scope",
            "dialogService",
            "RelationEditor.RelationResources",
            "assetsService",
            "navigationService",
            "eventsService",
            EditRelationsController]);

    /*
     jQuery UI Sortable plugin wrapper
    
    
     @param [ui-sortable] {object} Options to pass to $.fn.sortable() merged onto ui.config
    */
    angular.module('umbraco')
      .value('releditSortableConfig', {})
      .directive('releditSortable', ['releditSortableConfig', '$log',
            function (releditSortableConfig, log) {
                return {
                    require: '?ngModel',
                    link: function (scope, element, attrs, ngModel) {


                        function combineCallbacks(first, second) {
                            if (second && (typeof second === 'function')) {
                                return function (e, ui) {
                                    first(e, ui);
                                    second(e, ui);
                                };
                            }
                            return first;
                        }


                        var opts = {}; //scope.$eval(element.attr('reledit-sortable')) || {};}


                        var callbacks = {
                            receive: null,
                            remove: null,
                            start: null,
                            stop: null,
                            update: null
                        };


                        var apply = function (e, ui) {
                            if (ui.item.sortable.resort || ui.item.sortable.relocate) {
                                scope.$apply();
                            }
                        };


                        angular.extend(opts, releditSortableConfig);


                        if (ngModel) {


                            ngModel.$render = function () {
                                element.sortable('refresh');
                            };


                            callbacks.start = function (e, ui) {
                                // Save position of dragged item
                                ui.item.sortable = { index: ui.item.index() };
                            };


                            callbacks.update = function (e, ui) {
                                // For some reason the reference to ngModel in stop() is wrong
                                ui.item.sortable.resort = ngModel;
                            };


                            callbacks.receive = function (e, ui) {
                                ui.item.sortable.relocate = true;
                                // if the item still exists (it has not been cancelled)
                                if ('moved' in ui.item.sortable) {
                                    // added item to array into correct position and set up flag
                                    ngModel.$modelValue.splice(ui.item.index(), 0, ui.item.sortable.moved);
                                }
                            };


                            callbacks.remove = function (e, ui) {
                                // copy data into item
                                if (ngModel.$modelValue.length === 1) {
                                    ui.item.sortable.moved = ngModel.$modelValue.splice(0, 1)[0];
                                } else {
                                    ui.item.sortable.moved = ngModel.$modelValue.splice(ui.item.sortable.index, 1)[0];
                                }
                            };


                            callbacks.stop = function (e, ui) {
                                // digest all prepared changes
                                if (ui.item.sortable.resort && !ui.item.sortable.relocate) {


                                    // Fetch saved and current position of dropped element
                                    var end, start;
                                    start = ui.item.sortable.index;
                                    end = ui.item.index();


                                    // Reorder array and apply change to scope
                                    ui.item.sortable.resort.$modelValue.splice(end, 0, ui.item.sortable.resort.$modelValue.splice(start, 1)[0]);


                                }
                            };


                            scope.$watch(attrs.releditSortable, function (newVal) {
                                angular.forEach(newVal, function (value, key) {


                                    if (callbacks[key]) {
                                        // wrap the callback
                                        value = combineCallbacks(callbacks[key], value);


                                        if (key === 'stop') {
                                            // call apply after stop
                                            value = combineCallbacks(value, apply);
                                        }
                                    }


                                    element.sortable('option', key, value);
                                });
                            }, true);


                            angular.forEach(callbacks, function (value, key) {


                                opts[key] = combineCallbacks(value, opts[key]);
                            });


                            // call apply after stop
                            opts.stop = combineCallbacks(opts.stop, apply);


                        } else {
                            log.info('ui.sortable: ngModel not provided!', element);
                        }


                        // Create sortable
                        element.sortable(opts);
                    }
                };
            }
      ]);


}());