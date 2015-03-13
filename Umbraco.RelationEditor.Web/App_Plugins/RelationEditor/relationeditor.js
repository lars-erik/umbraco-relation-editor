(function () {
    var ObjectTypes = {
        Document: "C66BA18E-EAF3-4CFF-8A22-41B16D66A972",
        Media: "B796F64C-1F99-4FFB-B886-4BF4BC011A9C"
    };

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
            var selected;

            function nodePicked(evt, args) {
                args.node.filtered = true;
                if (args.node.metaData.relationDisallowed) {
                    return;
                }
                relationPicked(index, args.node);
                dialogService.close(dialog);
            }

            function searchSelected(entity) {
                relationsResources.isAllowedEntity(
                    $scope.data.ParentType,
                    $scope.data.ParentAlias,
                    $scope.resourceSets[index].Alias,
                    entity.metaData.treeAlias,
                    entity.id
                ).then(function(result) {
                    if (result.IsAllowed) {
                        relationPicked(index, entity);
                        dialogService.close(dialog);
                    }
                });
            }

            selected = eventsService.on("dialogs.treePickerController.select", nodePicked);
            dialogService.close(dialog);
            dialog = dialogService.treePicker({
                customTreeParams: $.param({
                    relationEditor: true,
                    parentType: $scope.data.ParentType,
                    parentTypeAlias: $scope.data.ParentAlias,
                    relationAlias: $scope.resourceSets[index].Alias
                }),
                treeAlias: $scope.resourceSets[index].ChildType.TreeType,
                section: $scope.resourceSets[index].ChildType.Section,
                callback: searchSelected,
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
            $(data).each(function (i, type) {
                $(type.Sets).each(function (i2, set) {
                    $(set.Relations).each(function(i3, relation) {
                        relation.RemoveTitle = relation.Readonly ? "Must be removed from " + relation.ChildName : "Remove relation";
                    });
                });
            });
            $scope.data = data;
            $scope.resourceSets = data.Sets;
            $scope.ready = true;
        });
    }

    function EnableRelationsController(scope, relationsResources, assetsService, navigationService) {
        var type = scope.currentNode.nodeType,
            id = scope.currentNode.id,
            promise = null;

        function find(array, predicate) {
            var result = $.grep(array, predicate);
            return result.length === 1 ? result[0] : null;
        }

        function findChildTypes(relationType) {
            if (relationType.ChildObjectType.toUpperCase() === ObjectTypes.Document) {
                return scope.data.contentTypes;
            }
            if (relationType.ChildObjectType.toUpperCase() === ObjectTypes.Media) {
                return scope.data.mediaTypes;
            }
            return [];
        }

        function hierarchize(types, configuredType, parentId) {
            return $.map(
                $.grep(types, function(t) { return t.ParentId == parentId; }),
                function (t) {
                    return {
                        id: t.Id,
                        name: t.Name,
                        checked: find(configuredType.EnabledChildTypes, function(ect) {
                            return ect.Alias === t.Alias;
                        }) != null,
                        items: hierarchize(types, configuredType, t.Id)
                    }
                });
        }

        function enabledChangedHandler(relationType) {
            return function (newValue) {
                var configuredType = find(scope.data.configuration.EnabledRelations, function (er) { return er.Alias === relationType.Alias; }),
                    index;
                if (configuredType === null && newValue) {
                    configuredType = {
                        Alias: relationType.Alias,
                        EnabledChildTypes: []
                    };
                    relationType.childTypes = {
                        items: hierarchize(
                            findChildTypes(relationType),
                            configuredType,
                            -1
                        )
                    };
                    scope.data.configuration.EnabledRelations.push(configuredType);
                } else if (configuredType !== null && newValue === false) {
                    index = scope.data.configuration.EnabledRelations.indexOf(configuredType);
                    scope.data.configuration.EnabledRelations.splice(index, 1);
                    relationType.childTypes.items = [];
                }
            }
        }

        function updateJson() {
            scope.json = JSON.stringify(scope.data.configuration, null, "\t");
        }

        scope.isEnabled = function(relationType, index) {
            return relationType.Enabled;
        }

        scope.childrenChanged = function (relationType, result) {
            var configuredType = find(scope.data.configuration.EnabledRelations, function(er) { return er.Alias === relationType.Alias; }),
                childTypes = findChildTypes(relationType);

            configuredType.EnabledChildTypes = $.map(
                $.grep(childTypes, function(ct) {
                    return result[ct.Id];
                }),
                function(ct) {
                    return {
                        Alias: ct.Alias
                    };
                }
            );

            updateJson();
        }

        scope.save = function () {
            scope.ready = false;
            relationsResources.saveConfiguration(type, id, scope.data.configuration)
                .then(function() {
                    navigationService.hideDialog();
                });
        }

        scope.data = {configuration: {}};
        scope.type = {};
        scope.ready = false;
        scope.json = "";

        assetsService.loadCss(Umbraco.Sys.ServerVariables.umbracoSettings.appPluginsPath + "/RelationEditor/relationeditor.css");

        promise = relationsResources.configuration(type, id);
        promise.then(function(data) {
            scope.data = data;

            angular.forEach(data.relationTypes, function(rt) {

                var configuredType = find(data.configuration.EnabledRelations, function (er) { return er.Alias == rt.Alias; }),
                    childTypes = findChildTypes(rt);
                rt.isContent = childTypes.length > 0;
                rt.Enabled = false;

                if (configuredType != null) {
                    rt.Enabled = true;
                    configuredType.EnabledChildTypes = configuredType.EnabledChildTypes || [];
                } else {
                    configuredType = { EnabledChildTypes: [] };
                };

                rt.childTypes = {
                    items: hierarchize(
                        childTypes,
                        configuredType,
                        -1
                    )
                };

                scope.$watch(function() { return rt.Enabled; }, enabledChangedHandler(rt));
            });

            scope.$watchCollection("data.configuration.EnabledRelations", updateJson);

            scope.ready = true;
        });
    }

    function RelationsResources($q, $http, umbDataFormatter, umbRequestHelper) {
        var root = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + "/backoffice/relationseditor/relations/",
            enableRoot = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + "/backoffice/relationseditor/settings/";
        return {
            getById: function(section, treeType, id) {
                return umbRequestHelper.resourcePromise(
                    $http.get(
                        root + "getrelations", {
                            params: {
                                section: section || "",
                                treeType: treeType || "",
                                parentId: id
                            }
                        }),
                    'Failed to retreive relations for content id ' + id);
            },
            save: function(set) {
                return umbRequestHelper.resourcePromise(
                    $http.post(
                        root + "saverelations", set),
                    "Failed to save relations for content id " + set.ParentId
                );
            },
            isAllowedEntity: function(parentType, parentAlias, relationAlias, treeAlias, id) {
                return umbRequestHelper.resourcePromise(
                    $http.get(root + "isallowedentity", {
                        params: {
                            parentTypeName: parentType,
                            parentAlias: parentAlias,
                            relationAlias: relationAlias,
                            treeAlias: treeAlias,
                            id: id
                        }
                    }),
                    "Failed to validate entity"
                );
            },
            configuration: function(type, id) {
                return umbRequestHelper.resourcePromise(
                    $http.get(enableRoot + "getconfiguration", {
                        params: {
                            type: type,
                            id: id
                        }
                    }),
                    "Failed to retrieve configuration"
                );
            },
            saveConfiguration: function(type, id, configuration) {
                return umbRequestHelper.resourcePromise(
                    $http.post(
                        enableRoot + "saveconfiguration",
                        {id:id,type:type,configuration:configuration}
                    ),
                    "Failed to save configuration"
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
            EditRelationsController])
        .controller("RelationEditor.EnableRelationsController", [
            "$scope",
            "RelationEditor.RelationResources",
            "assetsService",
            "navigationService",
            EnableRelationsController
        ]);

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
      ])

     /**
     * Bastardized version of
     * https://github.com/pbosin/ng_tree_btn
     *
     * @ngdoc directive
     * @name ngTreeBtn
     *
     * @description
     * The 'ngTreeBtn' directive instantiates a template with the multi-select drop-down.
     * Drop-down is marked up as a Bootstrap button with a caret.
     * Checked state of options is kept in local object checkMarks
     *
     * @element - any
     * @param ng-tree-btn - data object with dropdown options tree;
     *     each leaf of the tree is selectable and has to have an "id" property.
     * @param label - the text on the button
     * @param btnCls - additional style class(es) for the button
     * @param callback - function which takes selection results as object with
     *     property names as tree leaves' ids and values as boolean "checked" status
     *     example of result argument passed to callback: {"123":true,"763":false,"2":true}
     * @param enabled - function used to determine enabled/disabled state
     *
     * @author pbosin
     */
    .directive('ngTreeBtn', function () {
        return {
            scope: {
                options: '=ngTreeBtn',
                handleRes: '&callback',
                label: '@',
                isEnabled: '&'
            },
            controller: function ($scope, $attrs) {


                $scope.btnCls = $attrs.btncls;


                $scope.hasChildren = function (item) {
                    return (typeof (item.items) !== "undefined" && item.items.length > 0);
                };


                $scope.toggleDrop = function () {
                    if ($scope.isEnabled()) {
                        $scope.opened = !$scope.opened;
                    }
                };


                function isChecked(obj) {
                    var i;
                    if (!obj || typeof (obj) === "undefined") {
                        return false;
                    }
                    if (typeof (obj.items) === "undefined" || obj.items.length == 0) {
                        //tree leaf
                        return typeof (obj.id) !== "undefined" && checkMarks[obj.id];
                    } else {
                        ////traverse children
                        //for (i in obj.items) {
                        //    if (!isChecked(obj.items[i])) {
                        //        return false;
                        //    }
                        //}
                        //return true;
                        return checkMarks[obj.id];
                    }
                }

                $scope.checked = function (item) {
                    return isChecked(item);
                };

                $scope.uncheckAll = function(parent) {
                    var key;
                    for (key in parent.items) {
                        setItemCheck(parent.items[key], false);
                        $scope.uncheckAll(parent.items[key]);
                    }
                }

                $scope.allChecked = function(parent) {
                    var key,
                        result = true;
                    for (key in parent.items) {
                        if (isChecked(parent.items[key])) {
                            result = false;
                        }
                        if (result) {
                            result = $scope.allChecked(parent.items[key]);
                        }
                    }
                    return result;
                }

                function setItemCheck(obj, newState) {
                    var i;
                    //if ($scope.hasChildren(obj)) {
                    //    for (i in obj.items) {
                    //        setItemCheck(obj.items[i], newState);
                    //    }
                    //} else {
                        checkMarks[obj.id] = newState;
                    //}
                }

                $scope.selectItem = function (obj) {
                    var newState = !isChecked(obj);
                    setItemCheck(obj, newState);
                    return false;
                };


                function saveResult(obj, result) {
                    var i;
                    if ($scope.hasChildren(obj)) {
                        for (i in obj.items) {
                            saveResult(obj.items[i], result);
                        }
                    }
                    if (obj.id) {
                        result["" + obj.id] = checkMarks[obj.id];
                    }
                }

                $scope.confirmMulti = function () {
                    var result = {};
                    saveResult($scope.options, result);
                    $scope.toggleDrop();
                    $scope.handleRes({ res: result });
                };


                function addChk(chks, item) {
                    var i;
                    if (typeof (item) !== "undefined" && $scope.hasChildren(item)) {
                        for (i in item.items) {
                            addChk(chks, item.items[i]);
                        }
                    }
                    chks[item.id] = (typeof (item.checked) !== "undefined" && item.checked);
                }
                function initChks(items) {
                    var chks = {}, i;
                    if (typeof (items) !== "undefined") {
                        for (i in items) {
                            addChk(chks, items[i]);
                        }
                    }
                    return chks;
                }

                var checkMarks = [];

                $scope.$watchCollection("options.items", function() { checkMarks = initChks($scope.options.items); });

            },
            templateUrl: Umbraco.Sys.ServerVariables.umbracoSettings.appPluginsPath + "/RelationEditor/ng-tree-btn.html"
        }
    });


}());