<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GBBObjectTest.ascx.cs" Inherits="Plugins_us_northviewchurch_Tutorial_GBBObjectTest" %>

<div>
    <asp:UpdatePanel ID="PrayerRequestUpdPnl" runat="server">
        <ContentTemplate>    
            <Rock:Grid ID="grdRequests" runat="server" AllowSorting="true" DataKeyNames="RequestMappingId">
                <Columns>
                    <asp:BoundField DataField="RequestName" HeaderText="Request Name" />
                    <asp:BoundField DataField="Category" HeaderText="Category" />
                    <asp:TemplateField>
                      <ItemTemplate>
                        <img class="responsive" width="160" height="90" src='<%#String.Format("data:image/jpg;base64, {0}", Eval("ImageData"))%>' />
                      </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </Rock:Grid>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
<div>
    <asp:UpdatePanel ID="PrayerPartnerUpdPnl" runat="server">
        <ContentTemplate>    
            <Rock:Grid ID="grdPartners" runat="server" AllowSorting="true" DataKeyNames="ID">
                <Columns>
                    <asp:BoundField DataField="FirstName" HeaderText="First Name" />
                    <asp:BoundField DataField="LastName" HeaderText="Last Name" />
                    <asp:BoundField DataField="MaxRequests" HeaderText="Max Requests" />
                    <asp:BoundField DataField="TotalRequests" HeaderText="Total Requests" />                    
                </Columns>
            </Rock:Grid>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>