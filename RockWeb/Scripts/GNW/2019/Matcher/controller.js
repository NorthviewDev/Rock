'use strict';

angular.module('matcherApp')
    .controller('ProjectMatcherCtrl', ['$scope', function ($scope) {
        $scope.remove = function (scope) {
        scope.remove();
        };

        $scope.toggle = function (scope) {
        scope.toggle();
        };

        $scope.assign = function () {

            $("#hdnAssignments").val(JSON.stringify($scope.projects));
            $("#hdnUnassigned").val(JSON.stringify($scope.volunteers));

            $("#btnAssign").click();

        };

        $scope.teamTreeOptions = {
            dropped: function (event) {

                var src = $scope.volunteers[event.source];
                var $dest = $(".angular-ui-tree-placeholder").parent().parent().children('div.tree-node');
            },
            beforeDrop: function (event) {

                var srcType = event.source.nodeScope.$element.children().first().data('nodetype');
                var destType = event.dest.nodesScope.$element.parent().children().first().data('nodetype');

                return destType === 'project';
            },
            beforeDrag: function (sourceNodeScope) {

                return true;
            }
        };

        $scope.projectTreeOptions = {
            dropped: function (event) {

                var src = $scope.volunteers[event.source];
                var $dest = $(".angular-ui-tree-placeholder").parent().parent().children('div.tree-node');
            },
            beforeDrop: function (event) {

                var srcType = event.source.nodeScope.$element.children().first().data('nodetype');
                var destType = event.dest.nodesScope.$element.data('nodetype');

                if (destType === 'teamDrop') { //This could be a top-level <ol>, meaning the user is potentially moving the team back to its start
                    return true;
                }
                else {
                    destType = event.dest.nodesScope.$element.parent().children().first().data('nodetype'); //otherwise it is a <li>
                }

                return destType === 'project';
            },
            beforeDrag: function (sourceNodeScope) {

                var type = sourceNodeScope.$element.children().first().data('nodetype');;

                return type === 'team';
            }
        };

        $scope.projects = projectNodes;
        $scope.volunteers = volunteerNodes; 
    }]
);