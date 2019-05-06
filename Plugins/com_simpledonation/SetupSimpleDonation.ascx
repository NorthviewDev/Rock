<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SetupSimpleDonation.ascx.cs" Inherits="Plugins.com_simpledonation.SimpleDonationSetup" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Configure Simple Donation</h1>
            </div>

            <div class="panel-body">
                <Rock:NotificationBox runat="server" Visible="False" ID="nbSimpleDonation"></Rock:NotificationBox>

                <div id="divInstructions" class="alert alert-info" runat="server">You can find your Simple Donation API Key by logging into your Simple Donation admin interface and looking under the settings tab.</div>
                <div id="divUserCreateSuccess" class="alert alert-success" visible="false" runat="server">Simple Donation user created successfully</div>
                <div id="divUserCreateFailure" class="alert alert-danger" visible="false" runat="server">Simple Donation user already exists</div>

                <Rock:RockTextBox ID="tbApiKey" runat="server" AutoPostBack="true" Placeholder="Your API Key"></Rock:RockTextBox>

                <Rock:BootstrapButton CssClass="btn btn-primary margin-v-sm" runat="server" ID="lbConfigure" DataLoadingText="Configuring" OnClick="lbConfigure_Click" Text="Configure" />
                <Rock:BootstrapButton CssClass="btn btn-primary margin-v-sm" runat="server" ID="btnCreateUser" DataLoadingText="Create Simple Donation User" OnClick="btnCreateUser_Click" Text="Create Simple Donation User" />
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
