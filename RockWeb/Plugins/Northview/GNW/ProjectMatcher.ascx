﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ProjectMatcher.ascx.cs" Inherits="Plugins_us_northviewchurch_GNW_ProjectMatcher" %>
<link href="/Scripts/angular-ui-tree/angular-ui-tree.css" rel="stylesheet" />
<script src="/Scripts/angular/angular.js"></script>
<script src="/Scripts/angular-ui/ui-bootstrap-tpls.js"></script>
<script src="/Scripts/angular-ui-tree/angular-ui-tree.js"></script>
<script src="/Scripts/GNW/2019/Matcher/app.js"></script>
<script src="/Scripts/GNW/2019/Matcher/controller.js"></script>
<asp:PlaceHolder ID="placeHldrNodesJS" runat="server" />

<style>
    .angular-ui-tree-node{
        min-height:30px;
    }

    .btn-special{
        background-color: #611BBD;
        border-color: #130269;
    }

    .angular-ui-tree-placeholder {
        background: #f0f9ff;
        border: 2px dashed #bed2db;
        -webkit-box-sizing: border-box;
        -moz-box-sizing: border-box;
        box-sizing: border-box;
    }

</style>

<div class="row">
    <asp:Button runat="server" OnClick="btnMatch_Click" CausesValidation="False" id="btnBatch" Text="AUTO-MATCH" class="btn btn-primary"></asp:Button>
</div>
<div class="row">
    <div class="col-sm-3">
        <div>
            <div class="col-sm-6">
                <label>Volunteer Campus Filter</label>
                <asp:DropDownList runat="server" ID="ddlVolunteerCampuses" ClientIDMode="Static" OnSelectedIndexChanged="ddlCampuses_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>
            </div>
             <div class="col-sm-6">
                 <label>Project Campus Filter</label>
                  <asp:DropDownList runat="server" ID="ddlProjectCampuses" ClientIDMode="Static" OnSelectedIndexChanged="ddlCampuses_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>
            </div>
        </div>
    </div>
    <div class="col-sm-4">
        <h2>Auto Match Results Log</h2>
        <textarea TextMode="MultiLine" Rows="10" id="txtResults" runat="server" style="width:100%;" ></textarea>
    </div>
    <div class="col-sm-3">
        
    </div>
</div>

<div class="row" ng-app="matcherApp" ng-controller="ProjectMatcherCtrl">
    <script type="text/ng-template" id="nodes_renderer1.html">
  <div ui-tree-handle class="tree-node tree-node-content" data-nodetype="{{node.nodeType}}">
    <a class="btn btn-success btn-xs" data-nodrag ng-click="toggle(this)"><span class="glyphicon" ng-class="{'glyphicon-chevron-right': collapsed, 'glyphicon-chevron-down': !collapsed}"></span></a>
    {{node.title}}
    <div class="btn btn-xs" ng-class="{1:'btn-danger', 2:'btn-warning', 3:'btn-success', 4:'btn-special'}[node.ability]" data-nodrag style="margin-left: 6em;"><i class="fa fa-dashboard" style="font-size: 1.5em;"></i></div>
    <div class="btn btn-xs" ng-class="{1:'btn-success', 2:'btn-warning', 3:'btn-danger'}[node.familyFriendly]" data-nodrag  style="margin-right: 8px;"><i class="fa fa-group" style="font-size: 1.5em"></i></div>
  </div>
  <ol ui-tree-nodes="" ng-model="node.nodes" ng-class="{hidden: collapsed}">
    <li ng-repeat="node in node.nodes" ui-tree-node ng-include="'nodes_renderer1.html'">
    </li>
  </ol>
</script>
<script type="text/ng-template" id="nodes_renderer2.html">
  <div ui-tree-handle class="tree-node tree-node-content" data-nodetype="{{node.nodeType}}">
    <a class="btn btn-success btn-xs" data-nodrag ng-click="toggle(this)"><span class="glyphicon" ng-class="{'glyphicon-chevron-right': collapsed, 'glyphicon-chevron-down': !collapsed}"></span></a>
    {{node.title}}
    <div class="btn btn-xs" ng-class="{1:'btn-danger', 2:'btn-warning', 3:'btn-success', 4:'btn-special'}[node.ability]" data-nodrag style="margin-left: 6em;" ><i class="fa fa-dashboard" style="font-size: 1.5em"></i></div>
    <div class="btn btn-xs" ng-class="{1:'btn-success', 2:'btn-warning', 3:'btn-danger'}[node.familyFriendly]" data-nodrag  style="margin-right: 8px;"><i class="fa fa-group" style="font-size: 1.5em"></i></div>
  </div>
  <ol ui-tree-nodes="" ng-model="node.nodes" ng-class="{hidden: collapsed}">
    <li ng-repeat="node in node.nodes" ui-tree-node ng-include="'nodes_renderer1.html'">
    </li>
  </ol>   
</script>

    <div class="">
      <div class="col-sm-12">
        <h3>Manual Assignments</h3>
          <span>Make the desired assignments then click the Assign button</span>
      </div>
        <div>
            <button ng-click="assign();" class="btn btn-primary" type="button">ASSIGN</button>
            <asp:Button runat="server" OnClick="btnAssign_Click" CausesValidation="False" id="btnAssign" Text="ASSIGN" class="btn btn-primary" ClientIDMode="Static" style="display:none;"></asp:Button>
            <asp:HiddenField ID="hdnAssignments" runat="server" ClientIDMode="Static" />
            <asp:HiddenField ID="hdnUnassigned" runat="server" ClientIDMode="Static" />
        </div>
    </div>

    
    <div class="">
      <div class="col-sm-6">
        <h3>Projects</h3>
        <div ui-tree="projectTreeOptions" id="tree1-root">
          <ol ui-tree-nodes="" ng-model="projects" data-nodetype="projectDrop" id="projTree" >
            <li ng-repeat="node in projects" ui-tree-node ng-include="'nodes_renderer1.html'" ></li>
          </ol>
        </div>
      </div>

      <div class="col-sm-6">
        <h3>Volunteers</h3>
        <div ui-tree="teamTreeOptions" id="tree2-root">
          <ol ui-tree-nodes="" ng-model="volunteers" data-nodetype="teamDrop">
            <li ng-repeat="node in volunteers" ui-tree-node ng-include="'nodes_renderer2.html'"></li>
          </ol>
        </div>
      </div>
    </div>
</div>