<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TestGroupDataPopulator.ascx.cs" Inherits="Plugins_Northview_Utility_TestGroupDataPopulator" %>
<asp:PlaceHolder ID="placeHldrJS" runat="server" />

<script>

    const jsonTemplate = ({ key, fieldTypeName, fieldTypeId, values }) => { return { Key: key, FieldTypeName: fieldTypeName, FieldTypeId: fieldTypeId, AcceptableValues: values }; };

    function addAttribute() {
        var rows = $('.attr-table tbody').children('tr').length;

        var txtAttrKey = "<input id='txtAttrKey" + rows + "' class='attr-key'  type='text'/>";
        var ddlFieldType = "<select id='ddlFieldType" + rows + "' class='attr-type' >" + fieldTypeSelectOptions + "</select>";
        var txtValues = "<input id='txtValues" + rows + "' type='text' class='attr-values' />";

        var newRow = "<tr><td>" + txtAttrKey + "</td><td>" + ddlFieldType + "</td><td>" + txtValues + "</td></tr>";

        $('.attr-table tbody').append(newRow);
    };

    function serialize() {

        var info = '';

        var infoArr = [];

        var $rows = $('.attr-table tbody').children('tr');        

        $rows.each(function (row) {

            var attrKey = $($('.attr-key')[row]).val();
            var fieldTypeId = $($('.attr-type')[row]).val();
            var fieldTypeName = $($('.attr-type option:selected')[row]).text();
            var values = $($('.attr-values')[row]).val();
            var json = objectify(jsonTemplate, { key:attrKey, fieldTypeName:fieldTypeName, fieldTypeId:fieldTypeId, values:values });

            infoArr.push(json);

        });

        info = JSON.stringify(infoArr);

        $("#hdnSerializedAttrData").val(info);

        return true;
    };

    function objectify(template, data) {
         return template(data);
    }

    $(document).ready(function () {
        
    });
    

</script>
<div style="clear:both;">
    <div class="col-sm-6">   
    
        <div class="form-group">
            <h2>General Group Attributes</h2>

            <div class="form-group">
                <label>Parent Group</label>
                <Rock:GroupPicker ID="grpPicker" runat="server" />
            </div>
            <div class="form-group">
                <label>Home Campus</label>
                <asp:DropDownList runat="server" ID="ddlCampuses" ClientIDMode="Static" ></asp:DropDownList>
            </div>
            <div class="form-group">
                <label>Group Type</label>
                <asp:DropDownList ID="ddlGroupType" runat="server"></asp:DropDownList>
            </div>
            <div class="form-group">
                <label>Max Number of Groups</label>
                <input type="number" id="txtMaxGroups" runat="server"/>
            </div>
        </div>

        <a class="btn btn-primary" href="javascript: addAttribute();" >Add Attribute</a>
        <table class="attr-table">
            <thead>
                <tr>
                    <th >Attribute Key</th>
                    <th >Field Type</th>
                    <th >Acceptable Values</th>                        
                </tr>
            </thead>
            <tbody>           
            </tbody>
        </table>
         <asp:HiddenField id="hdnSerializedAttrData" ClientIDMode="Static" runat="server" />
    </div>
    <div class="col-sm-6">
        <div class ="row">
        <div class="col-sm-4">

        </div>
        <div class="col-sm-4">
            <asp:Button ID="btnPopulateData" runat="server" Text="Populate Data" OnClick="btnPopulateData_Click" OnClientClick="serialize();" />
        </div>
        <div class="col-sm-4">

        </div>
    </div>
        <div class="row">
         <div class="col-sm-2">

        </div>
        <div class="col-sm-8">
            <textarea TextMode="MultiLine" Rows="10" id="txtLog" runat="server" style="width:100%;" ></textarea>
        </div>
        <div class="col-sm-2">

        </div>
    </div>
</div>
</div>
