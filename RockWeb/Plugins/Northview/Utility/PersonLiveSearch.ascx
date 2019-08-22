<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonLiveSearch.ascx.cs" Inherits="Plugins_us_northviewchurch_Tutorial_PersonLiveSearch" %>
<script type="text/javascript">
    function RefreshLiveSearchUpdatePanel() {
        __doPostBack("<%= LiveSearchTxtBox.ClientID %>", "");
    };
</script>
<asp:TextBox 

    ID="LiveSearchTxtBox" 

    runat="server" 

    OnKeyUp="RefreshLiveSearchUpdatePanel();"

    OnTextChanged="LiveSearchTxtBox_TextChanged" 

    Columns="50"></asp:TextBox>        
<br />
<asp:UpdatePanel ID="LiveSearchUpdPnl" runat="server">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="LiveSearchTxtBox" />
    </Triggers>
    <ContentTemplate>    
    <Rock:Grid ID="gPeople" runat="server" AllowSorting="true"
    OnRowSelected="gPeople_RowSelected" DataKeyNames="Id">
        <Columns>
            <asp:BoundField DataField="FirstName" HeaderText="First Name" />
            <asp:BoundField DataField="LastName" HeaderText="Last Name" />
        </Columns>
    </Rock:Grid>

</ContentTemplate>
</asp:UpdatePanel>