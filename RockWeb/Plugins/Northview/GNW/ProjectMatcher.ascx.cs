using Newtonsoft.Json;
using Rock.Attribute;
using Rock.Model;
using RockWeb;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using us.northviewchurch.Model.GNW;

[DisplayName("GNW Project Volunteer Matcher")]
[Category("Northview > GNW")]
[Description("Automatically matches volunteers to GNW projects")]
[IntegerField("Volunteer Parent Group ID","The ID of the Parent Group that contains volunteer groups", true, 0)]
[IntegerField("Volunteer GroupType ID", "The ID of the GroupType that contains volunteer groups", true, 0)]
[IntegerField("Project Parent Group ID", "The ID of the Parent Group that contains project groups", true, 0)]
[IntegerField("Project GroupType ID", "The ID of the GroupType assigned project groups", true, 0)]
[IntegerField("Text FieldType ID", "The ID of the Text FieldType", true, 1, key: "TextFieldTypeId")]
[IntegerField("Group EntityType ID", "The ID of the Group EntityTypes", true, 16, key: "GroupEntityTypeId")]
public partial class Plugins_us_northviewchurch_GNW_ProjectMatcher : Rock.Web.UI.RockBlock
{
    private int _parentProjectGroupId = 0;
    private int _projectGroupTypeId = 0;
    private List<PartnerProject> _partnerProjects = new List<PartnerProject>();

    private int _parentTeamGroupId = 0;
    private int _teamGroupTypeId = 0;
    private List<VolunteerGroup> _volunteerTeams = new List<VolunteerGroup>();

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            loadGroupData();

            var campusSvc = new CampusService(new Rock.Data.RockContext());

            var activeCampuses = campusSvc.Queryable().Where(x => x.IsActive ?? false).ToList().Select(x => new KeyValuePair<int, string>(x.Id, x.Name)).ToList();
            activeCampuses.Add(new KeyValuePair<int, string>(-1, "All"));

            this.ddlProjectCampuses.DataValueField = "Key";
            this.ddlProjectCampuses.DataTextField = "Value";
            this.ddlProjectCampuses.DataSource = activeCampuses.OrderBy(x => x.Key);
            this.ddlProjectCampuses.DataBind();

            this.ddlVolunteerCampuses.DataValueField = "Key";
            this.ddlVolunteerCampuses.DataTextField = "Value";
            this.ddlVolunteerCampuses.DataSource = activeCampuses.OrderBy(x => x.Key);
            this.ddlVolunteerCampuses.DataBind();
        }
    }

    protected void btnMatch_Click(object sender, EventArgs e)
    {
        var rockCtx = new Rock.Data.RockContext();

        var groupSvc = new GroupService(rockCtx);

        var attributeSvc = new AttributeService(rockCtx);
        var fieldTypeSvc = new FieldTypeService(rockCtx);
        var attrValueSvc = new AttributeValueService(rockCtx);
        var entityTypeSvc = new EntityTypeService(rockCtx);
        var personAliasSvc = new PersonAliasService(rockCtx);

        var geoLocSvc = new GeoLocationService(new WebCaller());

        var selectedVolunteerCampusId = Int32.Parse(this.ddlVolunteerCampuses.SelectedItem.Value);
        var selectedProjectCampusId = Int32.Parse(this.ddlProjectCampuses.SelectedItem.Value);

        loadGroupData(selectedVolunteerCampusId, selectedProjectCampusId);

        //Grab a Distance Matrix from Bing! if one doesn't already exist
        foreach (var project in _partnerProjects.Where(x => !x.Distances.Any()))
        {
            var projRockGroup = groupSvc.Get(project.ID);

            var distResult = geoLocSvc.GetDrivingDistancesToCampuses(geoLocSvc.CampusCoordinates, project.OrgAddress);

            if (distResult.Success)
            {
                string msg = "";

                var success = project.CreateDistancesAttribute(rockCtx, attrValueSvc, attributeSvc, fieldTypeSvc, entityTypeSvc,
                                                               Int32.Parse(GetAttributeValue("TextFieldTypeId")), Int32.Parse(GetAttributeValue("GroupEntityTypeId")),
                                                               distResult.ResponseObject, out msg);

                if (!success)
                {
                    this.txtResults.Value += msg;
                }
            }

        }

        foreach (var team in _volunteerTeams)
        {
            var potentialProjects = team.FindMatches(_partnerProjects);

            if(potentialProjects.Any())
            {
                var proj = potentialProjects.First();

                proj.TotalVolunteers += team.VolunteerCount;

                var projectId = proj.ID;
                var teamId = team.ID;

                var teamRockGroup = groupSvc.Get(teamId);

                teamRockGroup.ParentGroupId = projectId;

                rockCtx.SaveChanges();

                this.txtResults.Value += String.Format("Proj {0} Assigned Team {1}: Remaining {2}{3}", proj.Name, team.Name, proj.RemainingCapacity, Environment.NewLine);
            }
        }

        loadGroupData();
    }

    protected void btnAssign_Click(object sender, EventArgs e)
    {
        if(Page.IsPostBack)
        {
            var assignmentJson = this.hdnAssignments.Value;
            var unassignedJson = this.hdnUnassigned.Value;

            var assignmentNodes = JsonConvert.DeserializeObject<List<Node>>(assignmentJson);

            var unassignedNodes = JsonConvert.DeserializeObject<List<Node>>(unassignedJson);

            var rockCtx = new Rock.Data.RockContext();

            var groupSvc = new GroupService(rockCtx);

            if (assignmentNodes != null)
            {
                foreach (var projectNode in assignmentNodes)
                {
                    if (projectNode.actualType == NodeType.project)
                    {
                        if (projectNode.nodes != null && projectNode.nodes.Any())
                        {
                            var teamNodes = projectNode.nodes.Where(x => x.actualType == NodeType.team).ToList();

                            foreach (var teamNode in teamNodes)
                            {
                                var projectId = projectNode.id;
                                var teamId = teamNode.id;

                                if (teamId != projectId)
                                {
                                    var teamRockGroup = groupSvc.Get(teamId);

                                    teamRockGroup.ParentGroupId = projectId;

                                    rockCtx.SaveChanges(); 
                                }
                            }
                        }
                    }
                } 
            }

            if (unassignedNodes != null)
            {
                foreach (var teamNode in unassignedNodes)
                {
                    var projectId = _parentTeamGroupId;
                    var teamId = teamNode.id;

                    if (teamId != projectId)
                    {
                        var teamRockGroup = groupSvc.Get(teamId);

                        teamRockGroup.ParentGroupId = projectId;

                        rockCtx.SaveChanges();
                    }
                } 
            }

            loadGroupData();
        }
        
    }

    protected void loadGroupData(int volunteerCampusFilter = -1, int projectCampusFilter = -1)
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

        if (volunteerCampusFilter > -1)
        {
            unmatchedVolunteerTeams = unmatchedVolunteerTeams.Where(x => x.CampusId.HasValue && x.CampusId.Value == volunteerCampusFilter).ToList();
        }

        if (projectCampusFilter > -1)
        {
            projectGroups = projectGroups.Where(x => x.CampusId.HasValue && x.CampusId.Value == projectCampusFilter).ToList();
        }

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

    protected void ddlCampuses_SelectedIndexChanged(object sender, EventArgs e)
    {
        var selectedVolunteerCampusId = Int32.Parse(this.ddlVolunteerCampuses.SelectedItem.Value);
        var selectedProjectCampusId = Int32.Parse(this.ddlProjectCampuses.SelectedItem.Value);

        loadGroupData(selectedVolunteerCampusId, selectedProjectCampusId);
    }
}