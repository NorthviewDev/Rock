using Rock.Attribute;
using Rock.Model;
using RockWeb;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using us.northviewchurch.Model.GNW;

[DisplayName("GNW Project Volunteer Matcher Analyzer")]
[Category("Northview > GNW")]
[Description("Provides analytics on the Project Matcher without actually making the assignments")]
[IntegerField("Volunteer GroupType ID", "The ID of the GroupType that contains volunteer groups", true, 0, key: "VolunteerGroupTypeID")]
[IntegerField("Default Project GroupType ID", "The ID of the GroupType assigned project groups", true, 0, key: "ProjectGroupTypeID")]
[IntegerField("Text FieldType ID", "The ID of the Text FieldType", true, 1, key: "TextFieldTypeId")]
[IntegerField("Group EntityType ID", "The ID of the Group EntityTypes", true, 16, key: "GroupEntityTypeId")]
public partial class Plugins_us_northviewchurch_GNW_ProjectMatcherAnalyzer : Rock.Web.UI.RockBlock
{    
    private int _projectGroupTypeId = 0;
    private List<PartnerProject> _partnerProjects = new List<PartnerProject>();
   
    private int _teamGroupTypeId = 0;
    private List<VolunteerGroup> _volunteerTeams = new List<VolunteerGroup>();

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
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
        
        var longestDistance = 0.0;
        var longestSiteLeaderDistance = 0.0;

        var longestDriveData = "";
        var longestLeaderDriveData = "";

        var unmatchedTeams = new List<VolunteerGroup>();
        var unmatchedProjects = new List<PartnerProject>();
        var fullProjects = new List<PartnerProject>();
        var averageVolunteerCount = 0M;

        var mileageWarnings = new Dictionary<double, int>();

        if (!String.IsNullOrWhiteSpace(this.hdnMileageWarnings.Value))
        {
            var warnings = this.hdnMileageWarnings.Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var warning in warnings)
            {
                var warnVal = 0.0;

                if (Double.TryParse(warning, out warnVal))
                {
                    mileageWarnings.Add(warnVal, 0);
                }
            } 
        }

        var selectedVolunteerCampusId = Int32.Parse(this.ddlVolunteerCampuses.SelectedItem.Value);
        var selectedProjectCampusId = Int32.Parse(this.ddlProjectCampuses.SelectedItem.Value);

        loadGroupData(selectedVolunteerCampusId, selectedProjectCampusId);

        var distancelessProjs = _partnerProjects.Where(x => !x.Distances.Any() || x.Distances.Values.Contains(-1)).ToList();

        //Grab a Distance Matrix from Bing! if one doesn't already exist
        foreach (var project in distancelessProjs)
        {
            var projRockGroup = groupSvc.Get(project.ID);

            var distResult = geoLocSvc.GetDrivingDistancesToCampuses(project.ProjectAddress);

            if(distResult.Success)
            {
                string msg = "";

                var success = project.CreateDistancesAttribute(rockCtx, attrValueSvc, attributeSvc, fieldTypeSvc, entityTypeSvc, 
                                                               Int32.Parse(GetAttributeValue("TextFieldTypeId")), 
                                                               distResult.ResponseObject, out msg);

                if(!success)
                {
                    this.txtLog.InnerHtml += msg;
                }
            }

        }

        var distVal = 0.0;

        var maxDistance = Double.TryParse(this.inputMaxDistance.Value, out distVal) ? distVal : Double.MaxValue;

        foreach (var team in _volunteerTeams.OrderBy(x => x.LifeGroupType))
        {
            var potentialProjectsResult = team.FindMatches(_partnerProjects, maxDistance);

            if (potentialProjectsResult.Success)
            {
                var proj = potentialProjectsResult.ResponseObject.First();

                proj.AssignTeam(team);

                var projectId = proj.ID;
                var teamId = team.ID;

                var distToTravel = proj.DistanceToTarget;

                if(team.MemberIds.Contains(proj.SiteLeaderId))
                {
                    if (distToTravel != Double.MaxValue && distToTravel > longestSiteLeaderDistance)
                    {
                        longestSiteLeaderDistance = distToTravel;

                        longestLeaderDriveData = String.Format("Group {0}({1}) -> Project {2}({3})", team.Name, team.ID, proj.Name, proj.ID);

                    }
                }
                else
                {
                    if (distToTravel != Double.MaxValue && distToTravel > longestDistance)
                    {
                        longestDistance = distToTravel;

                        longestDriveData = String.Format("Group {0}({1}) -> Project {2}({3})", team.Name, team.ID, proj.Name, proj.ID);
                    }

                    var lastDist = 0.0;

                    foreach (var warning in mileageWarnings.Keys)
                    {
                        if (distToTravel >= lastDist && distToTravel <= warning)
                        {
                            mileageWarnings[warning]++;
                            break;
                        }

                        lastDist = warning;
                    }
                }

                var capacityStr = new StringBuilder();

                foreach (var key in proj.Shifts.Keys)
                {
                    capacityStr.AppendLine(String.Format("{0}:{1}", key.DescriptionAttr(), proj.Shifts[key]));
                }

                this.txtLog.InnerHtml += String.Format("Proj {0} Assigned Team {1}:  {2} Remaining {3}", proj.Name, team.Name, Environment.NewLine, capacityStr.ToString());
            }
            else
            {
                this.txtLog.InnerHtml += String.Format("Unable to match team {0}:{1}! Reason: {2}", team.ID, team.Name, potentialProjectsResult.Message);
                unmatchedTeams.Add(team);
            }
        }

        var unmatchedDict = unmatchedTeams.ToLookup(x => x.HomeCampus);

        //Compile some stats!
        unmatchedProjects = this._partnerProjects.Where(x=> x.TotalVolunteers == 0).ToList();
        fullProjects = this._partnerProjects.Where(x => x.Shifts.Values.Sum() == 0).ToList();
        averageVolunteerCount = this._partnerProjects.Sum(x => x.TotalVolunteers) / this._partnerProjects.Count;

        var stats = new StringBuilder();

        stats.AppendFormat("Results for Teams from: {0} and Projects for: {1}{2}", this.ddlVolunteerCampuses.SelectedItem.Text, this.ddlProjectCampuses.SelectedItem.Text, Environment.NewLine);
        stats.AppendFormat("Average Volunteers per Project: {0} {1}", averageVolunteerCount.ToString("0.##"), Environment.NewLine);
        stats.AppendFormat("Longest Driving Distance: {0} {1}", longestDistance.ToString("0.##"), Environment.NewLine);
        stats.AppendFormat("Longest Drive Info: {0} {1}", longestDriveData, Environment.NewLine);
        stats.AppendFormat("Longest Site Leader Driving Distance: {0} {1}", longestSiteLeaderDistance.ToString("0.##"), Environment.NewLine);
        stats.AppendFormat("Longest Site Leader Drive Info: {0} {1}", longestLeaderDriveData, Environment.NewLine);
        stats.AppendFormat("Number of Unmatched Teams: {0} {1}", unmatchedTeams.Count, Environment.NewLine);
        stats.AppendFormat("Number of Unmatched Projects: {0} {1}", unmatchedProjects.Count, Environment.NewLine);
        stats.AppendFormat("Number of Full Projects: {0} {1}", fullProjects.Count, Environment.NewLine);

        if(mileageWarnings.Any())
        {
            stats.AppendLine("Mileage Alerts:");

            var lastDist = 0.0;

            foreach (var warning in mileageWarnings)
            {
                stats.AppendFormat("Number driving >= {0} and <= {1} miles: {2}{3}", lastDist, warning.Key, warning.Value, Environment.NewLine);
                lastDist = warning.Key;
            }
        }

        this.txtResults.InnerText = stats.ToString();

    }

    protected void loadGroupData(int volunteerCampusFilter= -1, int projectCampusFilter = -1)
    {        
        _projectGroupTypeId = Int32.Parse(GetAttributeValue("ProjectGroupTypeID"));
        
        _teamGroupTypeId = Int32.Parse(GetAttributeValue("VolunteerGroupTypeID"));

        var rockCtx = new Rock.Data.RockContext();

        var groupSvc = new GroupService(rockCtx);

        var projectGroups = groupSvc.Queryable().Where(x=> x.GroupTypeId == _projectGroupTypeId).ToList();
        var unmatchedVolunteerTeams = groupSvc.Queryable().Where(x => x.GroupTypeId == _teamGroupTypeId && x.ParentGroup.GroupTypeId != _projectGroupTypeId).ToList();

        var attrSvc = new AttributeValueService(rockCtx);
        var groupMemberSvc = new GroupMemberService(rockCtx);

        var projectNodes = new List<Node>();
        var teamNodes = new List<Node>();

        if(volunteerCampusFilter > -1)
        {
            unmatchedVolunteerTeams = unmatchedVolunteerTeams.Where(x => x.CampusId.HasValue && x.CampusId.Value == volunteerCampusFilter).ToList();
        }

        if (projectCampusFilter > -1)
        {
            projectGroups = projectGroups.Where(x => x.CampusId.HasValue && x.CampusId.Value == projectCampusFilter).ToList();
        }

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
                        this.txtLog.InnerHtml += result.Message;
                    }
                }

                _partnerProjects.Add(partnerProj); 
            }
            else
            {
                this.txtLog.InnerHtml += partnerProjResult.Message;
            }
        }

        foreach (var volTeam in unmatchedVolunteerTeams)
        {
            var volunteerGrpResult = VolunteerGroup.CreateFromRockGroup(volTeam, attrSvc, groupMemberSvc);

            if (volunteerGrpResult.Success)
            {
                _volunteerTeams.Add(volunteerGrpResult.ResponseObject);

                var teamNode = Node.GetNodesFromVolunteerGroup(volunteerGrpResult.ResponseObject);

                teamNodes.Add(teamNode);
            }
            else
            {
                this.txtLog.InnerHtml += volunteerGrpResult.Message;
            }
        }
    }

    protected void ddlCampuses_SelectedIndexChanged(object sender, EventArgs e)
    {
        var selectedVolunteerCampusId = Int32.Parse(this.ddlVolunteerCampuses.SelectedItem.Value);
        var selectedProjectCampusId = Int32.Parse(this.ddlProjectCampuses.SelectedItem.Value);       
    }
}