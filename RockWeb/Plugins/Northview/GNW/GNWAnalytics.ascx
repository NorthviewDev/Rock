<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GNWAnalytics.ascx.cs" Inherits="Plugins_Northview_Utility_GNWAnalytics" %>
<div class="col-sm-12">
    <asp:Button ID="btnRun" Text="Run" runat="server" OnClick="btnRun_Click" />
</div>
<div class="col-sm-12">
    <asp:Button ID="btnReport" Text="Run Report" runat="server" OnClick="btnReport_Click" />
</div>
<div class="col-sm-12">
     <textarea TextMode="MultiLine" Rows="10" id="txtResults" runat="server" style="width:100%;" ></textarea>1
</div>
<asp:Panel ID="pnlResultsGrid" runat="server" >
    <div class="panel-body" style="display:none;">
        <div class="grid grid-panel">
            <Rock:Grid ID="gReport" runat="server" AllowSorting="true" EmptyDataText="No Results" />
        </div>
    </div>
</asp:Panel>

