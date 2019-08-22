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

[DisplayName("GNW Project Volunteer Matcher Analyzer")]
[Category("Northview > GNW")]
[Description("Provides analytics on the Project Matcher without actually making the assignments")]
[IntegerField("Volunteer Parent Group ID","The ID of the Parent Group that contains volunteer groups", true, 0)]
[IntegerField("Volunteer GroupType ID", "The ID of the GroupType that contains volunteer groups", true, 0)]
[IntegerField("Default Project Parent Group ID", "The ID of the Parent Group that contains project groups", true, 0)]
[IntegerField("Default Project GroupType ID", "The ID of the GroupType assigned project groups", true, 0)]
public partial class Plugins_us_northviewchurch_GNW_ProjectMatcherAnalyzer : Rock.Web.UI.RockBlock
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

    protected void btnMatch_Click(object sender, EventArgs e)
    {
        var rockCtx = new Rock.Data.RockContext();

        var groupSvc = new GroupService(rockCtx);
        
        var geoLocSvc = new GeoLocationService(new TestWebCaller());

        var longestDistance = 0.0;
        var unmatchedTeams = new List<VolunteerGroup>();
        var unmatchedProjects = new List<PartnerProject>();
        var fullProjects = new List<PartnerProject>();
        var averageVolunteerCount = 0;

        foreach (var project in _partnerProjects.Where(x=> !x.Distances.Any()))
        {
            var projRockGroup = groupSvc.Get(project.ID);

            var distResult = geoLocSvc.GetDrivingDistancesToCampuses(geoLocSvc.CampusCoordinates, project.OrgAddress);

            if(distResult.Success)
            {
                var distResultString = new StringBuilder();

                foreach(var dist in distResult.ResponseObject)
                {
                    distResultString.AppendFormat("{0}:{1};",dist.Key, dist.Value);
                }

                var distances = new Dictionary<string, double>();

                var campuses = distResultString.ToString().Split(new char[] { ';' });

                foreach (var campus in campuses)
                {
                    var distDbl = 0.0;

                    var campusInfo = campus.Split(new char[] { ':' });

                    var name = campusInfo[0];
                    var dist = 0.0;

                    dist = Double.TryParse(campusInfo[1], out distDbl) ? distDbl : dist = Double.MaxValue;

                    distances.Add(name, dist);
                }

                project.Distances = distances;

                //projRockGroup.AttributeValues["Distances"].Value = distResultString.ToString();

                //rockCtx.SaveChanges();
            }

        }

        var maxDistance = Double.Parse(GetAttributeValue("ProjectParentGroupID"));

        foreach (var team in _volunteerTeams)
        {
            var potentialProjects = team.FindMatches(_partnerProjects, maxDistance);

            if(potentialProjects.Any())
            {
                var proj = potentialProjects.First();

                proj.TotalVolunteers += team.VolunteerCount;

                var projectId = proj.ID;
                var teamId = team.ID;

                var teamRockGroup = groupSvc.Get(teamId);

                teamRockGroup.ParentGroupId = projectId;

                var distToTravel = proj.DistanceToHome;

                if(distToTravel != Double.MaxValue && distToTravel > longestDistance)
                {
                    longestDistance = distToTravel;
                }

                this.txtResults.InnerText += String.Format("Proj {0} Assigned Team {1}: Remaining {2}{3}", proj.Name, team.Name, proj.RemainingCapacity, Environment.NewLine);
            }
            else
            {
                unmatchedTeams.Add(team);
            }
        }

        //loadGroupData();
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

                                    //rockCtx.SaveChanges(); 
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

                        //rockCtx.SaveChanges();
                    }
                } 
            }

            loadGroupData();
        }
        
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

[Serializable]
public enum NodeType
{
    project,
    team,
    projectDrop,
    teamDrop
}

[Serializable]
public class Node
{
    public int id { get; set; }
    public string title { get; set; }
    public int familyFriendly { get; set; }
    public int ability { get; set; }
    public string nodeType { get; set; }
    public NodeType actualType { get; set; }
    public List<Node> nodes { get; set; }

    public Node()
    {
        nodes = new List<Node>();
    }

    public static Node GetNodesFromProjectGroup(PartnerProject project)
    {
        var node = new Node()
        {
            id = project.ID,
            title = project.Name,
            familyFriendly = (int)project.FamilyFriendliness,
            ability = (int)project.AbilityLevel,
            nodeType = NodeType.project.ToString(),
            actualType = NodeType.project,
            nodes = project.AssignedTeams.Select(x=> Node.GetNodesFromVolunteerGroup(x)).ToList()
        };
                
        return node;
    }

    public static Node GetNodesFromVolunteerGroup(VolunteerGroup group)
    {
        var node = new Node()
        {
            id = group.ID,
            title = group.Name,
            familyFriendly = (int)group.FamilyFriendliness,
            ability = (int)group.AbilityLevel,
            nodeType = NodeType.team.ToString(),
            actualType = NodeType.team,
        };

        return node;
    }
}