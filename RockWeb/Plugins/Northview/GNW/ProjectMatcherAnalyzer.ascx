<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ProjectMatcherAnalyzer.ascx.cs" Inherits="Plugins_us_northviewchurch_GNW_ProjectMatcherAnalyzer" %>

<script>

    function addMileageWarning() {

        var $container = $("#divMileageWarnings");
		
		var curWarnings = $container.children('.mileage').length || 0;
		
		$container.append('<div class="mileage form-row"><label  class="form-control" for="warning' + curWarnings + '">Mileage Alert</label><input  class="count" name="warning' + curWarnings + '" type="number" min="0" value=""><span style="margin-left: 5px; display: inline-block;" class="btn btn-primary" onclick="removeWarning(' + curWarnings + ');">Delete</span></p></div>');
    }

    function collateMileageWarnings() {

        var info = '';

        var $warnings = $("#divMileageWarnings input");

        $warnings.each(function (el) {
            info += $($warnings[el]).val() + ';';
        });

        $("#hdnMileageWarnings").val(info);

        return true;
    }

    function removeWarning(index) {
        var $container = jQuery("#divMileageWarnings");
	
	    $container.children('.mileage')[index].remove();
    }

</script>

<div class="row">
    <div class="col-sm-3">
        <label>Maximum Drive Distance (Mi)</label>
        <input id="inputMaxDistance" class="form-control" type="number" min="0" runat="server" />

        <a class="btn btn-primary" href="javascript: addMileageWarning();">Add Mileage Warning</a>
        <div id="divMileageWarnings">

        </div>

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

        <asp:Button runat="server" OnClientClick="javascript:collateMileageWarnings()" OnClick="btnMatch_Click" CausesValidation="False" id="btnBatch" Text="AUTO-MATCH" class="btn btn-primary"></asp:Button>
    </div>
    <div class="col-sm-4">
        <h2>Auto Match Results Data</h2>
        <textarea TextMode="MultiLine" Rows="10" id="txtResults" runat="server" style="width:100%;" ></textarea>
    </div>
    <div class="col-sm-3">
        <h2>Log</h2>
        <textarea TextMode="MultiLine" Rows="10" id="txtLog" runat="server" style="width:100%;" ></textarea>
    </div>
    
    <asp:HiddenField id="hdnMileageWarnings" ClientIDMode="Static" runat="server" />
</div>