<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GBBObjectTest.ascx.cs" Inherits="Plugins_us_northviewchurch_Tutorial_GBBObjectTest" %>
<script src="/Scripts/bootstrap.min.js"></script>
<script>

    $(document).ready(function () {

        $('.prayer-preview').on('click', function (e) {

            var tgt = e.target;

            var img = document.createElement("img");
            img.src = $(tgt).attr("src");
            img.width = 800;
            img.height = 600;

            $('#divModalBody').append(img);

            $('#fullImg').modal('show');

        });

    });

</script>

<div>
    <asp:UpdatePanel ID="PrayerRequestUpdPnl" runat="server">
        <ContentTemplate>    
            <Rock:Grid ID="grdRequests" runat="server" AllowSorting="true" DataKeyNames="RequestMappingId" OnRowCommand="grdRequests_RowCommand">
                <Columns>
                    <asp:BoundField DataField="RequestName" HeaderText="Request Name" />
                    <asp:BoundField DataField="Category" HeaderText="Category" />                    
                    <asp:TemplateField>
                      <ItemTemplate>
                        <img class="responsive prayer-preview" width="160" height="90" src='<%#String.Format("data:image/jpg;base64, {0}", Eval("ImageData"))%>' />
                      </ItemTemplate>
                    </asp:TemplateField>
                    <asp:ButtonField CausesValidation="false" HeaderText="Action" ButtonType="Button" Text="Complete" CommandName="Complete" />
                </Columns>
            </Rock:Grid>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
<%--<div>
    <asp:UpdatePanel ID="PrayerPartnerUpdPnl" runat="server">
        <ContentTemplate>    
            <Rock:Grid ID="grdPartners" runat="server" AllowSorting="true" DataKeyNames="ID">
                <Columns>
                    <asp:BoundField DataField="ID" HeaderText="ID" />
                    <asp:BoundField DataField="FirstName" HeaderText="First Name" />
                    <asp:BoundField DataField="LastName" HeaderText="Last Name" />
                    <asp:BoundField DataField="MaxRequests" HeaderText="Max Requests" />
                    <asp:BoundField DataField="TotalRequests" HeaderText="Total Requests" />                    
                </Columns>
            </Rock:Grid>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>--%>
<div class="modal fade" id="fullImg" tabindex="-1" role="dialog" aria-hidden="true" style="width:auto;">
  <div class="modal-dialog" role="document">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title" id="modalLabel">Full Size Prayer Request</h5>
        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
          <span aria-hidden="true">&times;</span>
        </button>
      </div>
      <div id="divModalBody" class="modal-body" style="width: min-content;">
        
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>
      </div>
    </div>
  </div>
</div>