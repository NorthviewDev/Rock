<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GNWDashboard.ascx.cs" Inherits="Plugins_Northview_GNW_GNWDashboard" %>
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