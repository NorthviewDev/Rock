using Newtonsoft.Json;
using Rock.Attribute;
using Rock.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using us.northviewchurch.Model.GNW;

[DisplayName("GNW Dashboard")]
[Category("Northview > GNW")]
[Description("Dashboard for monitoring GNW projects")]
[IntegerField("Volunteer Parent Group ID", "The ID of the Parent Group that contains volunteer groups", true, 0)]
[IntegerField("Volunteer GroupType ID", "The ID of the GroupType that contains volunteer groups", true, 0)]
[IntegerField("Project Parent Group ID", "The ID of the Parent Group that contains project groups", true, 0)]
[IntegerField("Project GroupType ID", "The ID of the GroupType assigned project groups", true, 0)]
public partial class Plugins_Northview_GNW_GNWDashboard : Rock.Web.UI.RockBlock
{
    private int _parentProjectGroupId = 0;
    private int _projectGroupTypeId = 0;
    private List<PartnerProject> _partnerProjects = new List<PartnerProject>();

    private int _parentTeamGroupId = 0;
    private int _teamGroupTypeId = 0;
    private List<VolunteerGroup> _volunteerTeams = new List<VolunteerGroup>();

    protected void Page_Load(object sender, EventArgs e)
    {
        loadGroupData();
    }

    protected void loadGroupData()
    {
        _parentProjectGroupId = Int32.Parse(GetAttributeValue("ProjectParentGroupID"));
        _projectGroupTypeId = Int32.Parse(GetAttributeValue("ProjectGroupTypeID"));

        _parentTeamGroupId = Int32.Parse(GetAttributeValue("VolunteerParentGroupID"));
        _teamGroupTypeId = Int32.Parse(GetAttributeValue("VolunteerGroupTypeID"));

        var rockCtx = new Rock.Data.RockContext();

        var groupSvc = new GroupService(rockCtx);

        var projectGroups = groupSvc.GetChildren(_parentProjectGroupId, 0, false, new List<int> { _projectGroupTypeId }, new List<int> { 0 }, false, false).ToList();
        var unmatchedVolunteerTeams = groupSvc.GetChildren(_parentTeamGroupId, 0, false, new List<int> { _teamGroupTypeId }, new List<int> { 0 }, false, false).ToList();

        var attrSvc = new AttributeValueService(rockCtx);

        var projectNodes = new List<Node>();
        var teamNodes = new List<Node>();

        foreach (var projGrp in projectGroups)
        {
            var partnerProj = PartnerProject.CreateFromRockGroup(projGrp, attrSvc);

            var matchedTeams = groupSvc.GetChildren(partnerProj.ID, 0, false, new List<int> { _teamGroupTypeId }, new List<int> { 0 }, false, false).ToList();

            var groups = matchedTeams.Select(x => VolunteerGroup.CreateFromRockGroup(x, attrSvc)).ToList();

            partnerProj.AssignedTeams = groups;

            _partnerProjects.Add(partnerProj);

            var projNode = Node.GetNodesFromProjectGroup(partnerProj);

            projectNodes.Add(projNode);
        }

        foreach (var volTeam in unmatchedVolunteerTeams)
        {
            var volunteerGrp = VolunteerGroup.CreateFromRockGroup(volTeam, attrSvc);

            _volunteerTeams.Add(volunteerGrp);

            var teamNode = Node.GetNodesFromVolunteerGroup(volunteerGrp);

            teamNodes.Add(teamNode);
        }

        //create a string for the inner HTML of the script

        var projNodesStrings = JsonConvert.SerializeObject(projectNodes);

        var volNodesStrings = JsonConvert.SerializeObject(teamNodes);

        var sb = new StringBuilder();
        sb.Append(String.Format(@"
        var projectNodes = {0};
        var volunteerNodes = {1};
        ", String.Join(",", projNodesStrings), String.Join(",", volNodesStrings)));

        //create script control
        var objScript = new HtmlGenericControl("script");
        //add javascript type
        objScript.Attributes.Add("type", "text/javascript");
        //set innerHTML to be our StringBuilder string
        objScript.InnerHtml = sb.ToString();

        //add script to PlaceHolder control
        this.placeHldrNodesJS.Controls.Add(objScript);
    }
}