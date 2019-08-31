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
        
        var longestDistance = 0.0;
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

        //Grab a Distance Matrix from Bing! if one doesn't already exist
        foreach (var project in _partnerProjects.Where(x=> !x.Distances.Any()))
        {
            var projRockGroup = groupSvc.Get(project.ID);

            var distResult = geoLocSvc.GetDrivingDistancesToCampuses(geoLocSvc.CampusCoordinates, project.OrgAddress);

            if(distResult.Success)
            {
                string msg = "";

                var success = project.CreateDistancesAttribute(rockCtx, attrValueSvc, attributeSvc, fieldTypeSvc, entityTypeSvc, 
                                                               Int32.Parse(GetAttributeValue("TextFieldTypeId")), Int32.Parse(GetAttributeValue("GroupEntityTypeId")), 
                                                               distResult.ResponseObject, out msg);

                if(!success)
                {
                    this.txtLog.Value += msg;
                }
            }

        }

        var distVal = 0.0;

        var maxDistance = Double.TryParse(this.inputMaxDistance.Value, out distVal) ? distVal : Double.MaxValue;

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

                var lastDist = 0.0;

                foreach(var warning in mileageWarnings.Keys)
                {
                    if(distToTravel >= lastDist && distToTravel <= warning)
                    {
                        mileageWarnings[warning]++;
                        break;
                    }

                    lastDist = warning;
                }

                this.txtResults.InnerText += String.Format("Proj {0} Assigned Team {1}: Remaining {2}{3}", proj.Name, team.Name, proj.RemainingCapacity, Environment.NewLine);
            }
            else
            {
                unmatchedTeams.Add(team);
            }
        }

        //Compile some stats!
        unmatchedProjects = this._partnerProjects.Where(x=> x.TotalVolunteers == 0).ToList();
        fullProjects = this._partnerProjects.Where(x => x.RemainingCapacity == 0).ToList();
        averageVolunteerCount = this._partnerProjects.Sum(x => x.TotalVolunteers) / this._partnerProjects.Count;

        var stats = new StringBuilder();

        stats.AppendFormat("Results for Teams from: {0} and Projects for: {1}{2}", this.ddlVolunteerCampuses.SelectedItem.Text, this.ddlProjectCampuses.SelectedItem.Text, Environment.NewLine);
        stats.AppendFormat("Average Volunteers per Project: {0} {1}", averageVolunteerCount, Environment.NewLine);
        stats.AppendFormat("Longest Driving Distance: {0} {1}", longestDistance, Environment.NewLine);
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

        this.txtResults.Value = stats.ToString();

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
            var partnerProj = PartnerProject.CreateFromRockGroup(projGrp, attrSvc);

            var matchedTeams = groupSvc.GetChildren(partnerProj.ID, 0, false, new List<int> { _teamGroupTypeId }, new List<int> { 0 }, false, false).ToList();

            var groups = matchedTeams.Select(x => VolunteerGroup.CreateFromRockGroup(x, attrSvc)).ToList();

            partnerProj.AssignedTeams = groups;

            _partnerProjects.Add(partnerProj);
        }

        foreach (var volTeam in unmatchedVolunteerTeams)
        {
            var volunteerGrp = VolunteerGroup.CreateFromRockGroup(volTeam, attrSvc);

            _volunteerTeams.Add(volunteerGrp);
        }
    }

    protected void ddlCampuses_SelectedIndexChanged(object sender, EventArgs e)
    {
        var selectedVolunteerCampusId = Int32.Parse(this.ddlVolunteerCampuses.SelectedItem.Value);
        var selectedProjectCampusId = Int32.Parse(this.ddlProjectCampuses.SelectedItem.Value);

        loadGroupData(selectedVolunteerCampusId, selectedProjectCampusId);
    }
}