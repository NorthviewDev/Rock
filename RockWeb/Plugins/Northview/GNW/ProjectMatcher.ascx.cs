using Newtonsoft.Json;
using Rock;
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
[DecimalField("Max Driving Distance", "What's the farthest someone should have to drive (in miles)",true,45.0,key:"MaxDrivingDistance")]
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
            var activeCampuses = new List<KeyValuePair<int, string>>();

            try
            {
                //loadGroupData();

                var campusSvc = new CampusService(new Rock.Data.RockContext());

                activeCampuses = campusSvc.Queryable().Where(x => x.IsActive ?? false).ToList().Select(x => new KeyValuePair<int, string>(x.Id, x.Name)).ToList();
            }
            catch (Exception ex)
            {
                this.AppendError(ex);
            }
            
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

            var distResult = geoLocSvc.GetDrivingDistancesToCampuses(project.ProjectAddress);

            if (distResult.Success)
            {
                string msg = "";

                var success = project.CreateDistancesAttribute(rockCtx, attrValueSvc, attributeSvc, fieldTypeSvc, entityTypeSvc,
                                                               Int32.Parse(GetAttributeValue("TextFieldTypeId")),
                                                               distResult.ResponseObject, out msg);

                if (!success)
                {
                    this.AppendError(msg);
                }
            }

        }

        var maxDistance = GetAttributeValue("MaxDrivingDistance").AsDouble();

        foreach (var team in _volunteerTeams.OrderBy(x=> x.LifeGroupType))
        {
            var potentialProjectsResult = team.FindMatches(_partnerProjects, maxDistance);

            if(potentialProjectsResult.Success)
            {
                var proj = potentialProjectsResult.ResponseObject.First();

                proj.AssignTeam(team);

                var projectId = proj.ID;
                var teamId = team.ID;

                var teamRockGroup = groupSvc.Get(teamId);

                teamRockGroup.ParentGroupId = projectId;

                rockCtx.SaveChanges();

                var capacityStr = new StringBuilder();

                foreach(var key in proj.Shifts.Keys)
                {
                    capacityStr.AppendLine(String.Format("{0}:{1}", key.GetDescription(), proj.Shifts[key]));
                }

                this.txtResults.Value += String.Format("Proj {0} Assigned Team {1}:  {2} Remaining {3}", proj.Name, team.Name, Environment.NewLine, capacityStr.ToString());
            }
            else
            {
                var msg = potentialProjectsResult.Message;
            }
        }

        loadGroupData(selectedVolunteerCampusId, selectedProjectCampusId);
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
                _parentTeamGroupId = Int32.Parse(GetAttributeValue("VolunteerParentGroupID"));

                foreach (var teamNode in unassignedNodes)
                {
                    var projectId = _parentTeamGroupId;
                    var teamId = teamNode.id;

                    if (teamId != projectId)
                    {
                       var teamRockGroup = groupSvc.Get(teamId);

                        var campusGrp = groupSvc.Queryable().Where(x => x.ParentGroupId == _parentTeamGroupId && x.Name == teamRockGroup.Campus.Name).FirstOrDefault();

                        if(campusGrp != null)
                        {
                            teamRockGroup.ParentGroupId = campusGrp.Id;

                            rockCtx.SaveChanges();
                        }
                        else
                        {
                            var msg = String.Format(@"Could not restore team to original group, please contact the administrator with the following info: 
                                                                    Team GroupId: {0}, Team Campus: {1},Team Campus Id: {2}", teamRockGroup.Id, teamRockGroup.Campus.Name, teamRockGroup.CampusId);
                            this.AppendError(msg);
                        }
                        
                    }
                } 
            }

            var selectedVolunteerCampusId = Int32.Parse(this.ddlVolunteerCampuses.SelectedItem.Value);
            var selectedProjectCampusId = Int32.Parse(this.ddlProjectCampuses.SelectedItem.Value);

            loadGroupData(selectedVolunteerCampusId, selectedProjectCampusId);
        }
        
    }

    protected void loadGroupData(int volunteerCampusFilter = -1, int projectCampusFilter = -1)
    {
        _parentProjectGroupId = Int32.Parse(GetAttributeValue("ProjectParentGroupID"));
        _projectGroupTypeId = Int32.Parse(GetAttributeValue("ProjectGroupTypeID"));

        _parentTeamGroupId = Int32.Parse(GetAttributeValue("VolunteerParentGroupID"));
        _teamGroupTypeId = Int32.Parse(GetAttributeValue("VolunteerGroupTypeID"));

        var rockCtx = new Rock.Data.RockContext();
        rockCtx.Database.CommandTimeout = 300;

        var groupSvc = new GroupService(rockCtx);

        var projectGroups = groupSvc.GetAllDescendents(_parentProjectGroupId).Where(x=> x.GroupTypeId == _projectGroupTypeId).ToList();
        var unmatchedVolunteerTeams = groupSvc.GetAllDescendents(_parentTeamGroupId).Where(x => x.GroupTypeId == _teamGroupTypeId).ToList();

        var attrSvc = new AttributeValueService(rockCtx);
        var groupMemberSvc = new GroupMemberService(rockCtx);

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
            var partnerProjResult = PartnerProject.CreateFromRockGroup(projGrp, attrSvc);

            PartnerProject partnerProj = null;

            if (partnerProjResult.Success)
            {
                partnerProj = partnerProjResult.ResponseObject;

                var matchedTeams = groupSvc.GetChildren(partnerProj.ID, 0, false, new List<int> { _teamGroupTypeId }, new List<int> { 0 }, false, false).ToList();

                foreach (var grp in matchedTeams)
                {
                    var result = VolunteerGroup.CreateFromRockGroup(grp, attrSvc, groupMemberSvc);

                    if (result.Success)
                    {
                        partnerProj.AssignTeam(result.ResponseObject);
                    }
                    else
                    {  
                        this.AppendError(result.Message);
                    }
                }

                _partnerProjects.Add(partnerProj);

                var projNode = Node.GetNodesFromProjectGroup(partnerProj);

                projectNodes.Add(projNode); 
            }
            else
            {
                this.AppendError(partnerProjResult.Message);
            }
        }

        foreach (var volTeam in unmatchedVolunteerTeams)
        {
            var volunteerGrpResult = VolunteerGroup.CreateFromRockGroup(volTeam, attrSvc, groupMemberSvc);

            if(volunteerGrpResult.Success)
            {
                _volunteerTeams.Add(volunteerGrpResult.ResponseObject);

                var teamNode = Node.GetNodesFromVolunteerGroup(volunteerGrpResult.ResponseObject);

                teamNodes.Add(teamNode);
            }
            else
            {                
                this.AppendError(volunteerGrpResult.Message);
            }           
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

    protected void AppendError(string message)
    {
        this.pnlErrorMessage.Visible = true;
        this.txtError.InnerHtml += message + Environment.NewLine;
    }

    protected void AppendError(Exception e)
    {
        this.pnlErrorMessage.Visible = true;
        var msg = String.Format("Error! Message: {0} {2} Stack: {1}{2}", e.Message, e.StackTrace, Environment.NewLine);
        this.txtError.InnerHtml += msg;
    }
}