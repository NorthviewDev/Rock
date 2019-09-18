using Rock;
using Rock.Attribute;
using Rock.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using us.northviewchurch.Model.GNW;

[DisplayName("GNW Dashboard")]
[Category("Northview > GNW")]
[Description("Dashboard for monitoring GNW projects")]
[IntegerField("Volunteer GroupType ID", "The ID of the GroupType that contains volunteer groups", true, 0)]
[IntegerField("Project GroupType ID", "The ID of the GroupType assigned project groups", true, 0)]
[TextField("Group Detail URL", required: true, key: "GroupDetailUrl")]
public partial class Plugins_Northview_GNW_GNWDashboard : Rock.Web.UI.RockBlock
{
    private int _projectGroupTypeId = 0;
    private List<PartnerProject> _partnerProjects = new List<PartnerProject>();

    private int _teamGroupTypeId = 0;
    private List<VolunteerGroup> _volunteerTeams = new List<VolunteerGroup>();

    protected List<CampusInfo> _activeCampuses = new List<CampusInfo>();
    protected List<string> _thermometerRenderStrings = new List<string>();
    protected string _detailsUrl = "";

    protected string _displayMode = "All";

    protected Dictionary<string, int> CampusGoals = new Dictionary<string, int>()
    {
        {"Anderson",601},
        {"Binford",288},
        {"Carmel",3854},
        {"Fishers",824},
        {"Greater Lafayette",587},
        {"Kokomo",246},
        {"North Put",50},
        {"Peru",70},
        {"Flora",70}
    };

    protected void Page_Load(object sender, EventArgs e)
    {
        loadGroupData();
    }

    protected void loadGroupData()
    {
        _projectGroupTypeId = Int32.Parse(GetAttributeValue("ProjectGroupTypeID"));
        _teamGroupTypeId = Int32.Parse(GetAttributeValue("VolunteerGroupTypeID"));
        _detailsUrl = GetAttributeValue("GroupDetailUrl");
        _displayMode = PageParameter("displayMode").ToStringSafe();

        if (String.IsNullOrWhiteSpace(_displayMode))
        {
            _displayMode = "All";
        }

        var rockCtx = new Rock.Data.RockContext();

        var groupSvc = new GroupService(rockCtx);

        var campusSvc = new CampusService(rockCtx);
        var personSvc = new PersonService(rockCtx);
        var attrSvc = new AttributeValueService(rockCtx);

        var refreshRate = 120 * 1000;

        _activeCampuses = campusSvc.Queryable().Where(x => x.IsActive ?? false).ToList().Select(x => new CampusInfo(x.Id, x.Name)).ToList();

        if (this._displayMode != "All")
        {
            refreshRate = 30 * 1000;
        }

        var campusDataStrings = new List<string>();

        var allNorthview = new CampusInfo(-1, "All Northview");

        foreach (var campus in _activeCampuses)
        {
            if (CampusGoals.ContainsKey(campus.Name))
            {

                if (this._displayMode == "All" || this._displayMode == campus.Name)
                {
                    campus.Included = true;
                }

                var memberCount = CampusGoals[campus.Name];

                var campusVolunteerGroups = groupSvc.Queryable().Where(x => x.GroupTypeId == _teamGroupTypeId && x.CampusId == campus.ID).ToList();

                var campusVolunteers = new List<VolunteerGroup>();

                foreach (var cvg in campusVolunteerGroups)
                {
                    var volResult = VolunteerGroup.CreateFromRockGroup(cvg, attrSvc);

                    if (volResult.Success)
                    {
                        campusVolunteers.Add(volResult.ResponseObject);
                    }
                    else
                    {
                        txtDebugLog.InnerText += volResult.Message;
                    }
                }

                var campusProjectsGroups = groupSvc.Queryable().Where(x => x.GroupTypeId == _projectGroupTypeId && x.CampusId == campus.ID).ToList();

                var campusProjects = new List<PartnerProject>();

                foreach (var cpg in campusProjectsGroups)
                {
                    var projResult = PartnerProject.CreateFromRockGroup(cpg, attrSvc);

                    if (projResult.Success)
                    {

                        var partnerProj = projResult.ResponseObject;

                        var matchedTeams = groupSvc.GetChildren(partnerProj.ID, 0, false, new List<int> { _teamGroupTypeId }, new List<int> { 0 }, false, false).ToList();

                        foreach (var grp in matchedTeams)
                        {
                            var result = VolunteerGroup.CreateFromRockGroup(grp, attrSvc);

                            if (result.Success)
                            {
                                partnerProj.AssignTeam(result.ResponseObject);
                            }
                            else
                            {
                                txtDebugLog.InnerText += result.Message;
                            }
                        }

                        campusProjects.Add(partnerProj);
                    }
                    else
                    {
                        txtDebugLog.InnerText += projResult.Message;
                    }
                }

                var volunteerCount = campusVolunteers.Sum(x => x.VolunteerCount);
                var projectCapacity = campusProjects.Sum(x => x.Shifts.Values.Sum());

                campus.TotalProjects = campusProjects.Count;
                campus.Projects = campusProjects;

                campus.TotalRemainingVolunteerCapacity = projectCapacity;
                campus.TotalVolunteers = volunteerCount;
                campus.AdultMembers = memberCount;

                allNorthview.TotalProjects += campusProjects.Count;
                allNorthview.Projects.AddRange(campusProjects);

                allNorthview.TotalRemainingVolunteerCapacity += projectCapacity;
                allNorthview.TotalVolunteers += volunteerCount;
                allNorthview.AdultMembers += memberCount;

                var attendeeRatio = memberCount == 0 ? 0M : (((decimal)volunteerCount) / memberCount) * 100;


                _thermometerRenderStrings.Add(String.Format("renderThermometer('{0}', {1}, {2});{3}", campus.Name.Replace(" ", ""), attendeeRatio, memberCount, Environment.NewLine));
            }

        }

        allNorthview.Included = true;
        _activeCampuses.Add(allNorthview);
        _thermometerRenderStrings.Add(String.Format("renderThermometer('{0}', {1}, {2});{3}", "AllNorthview", ((decimal)allNorthview.TotalVolunteers / allNorthview.AdultMembers) * 100, allNorthview.AdultMembers, Environment.NewLine));

        _activeCampuses = _activeCampuses.OrderBy(x => x.Name).ToList();

        //create a string for the inner HTML of the script

        var sb = new StringBuilder();
        sb.Append(String.Format(@"
        var campusData = [{0}];
        var pageRefreshRate = {1};
        ", String.Join(",", campusDataStrings), refreshRate));

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

public class CampusInfo
{
    public int ID { get; set; }
    public string Name { get; set; }
    public int TotalProjects { get; set; }
    public int TotalVolunteers { get; set; }
    public decimal TotalRemainingVolunteerCapacity { get; set; }
    public decimal TotalRequiredVolunteers { get { return this.TotalVolunteers + this.TotalRemainingVolunteerCapacity; } }
    public int AdultMembers { get; set; }
    public List<PartnerProject> Projects { get; set; }
    public bool Included { get; set; }

    public CampusInfo()
    {
        Projects = new List<PartnerProject>();
    }

    public CampusInfo(int id, string name)
    {
        ID = id;
        Name = name;
        Projects = new List<PartnerProject>();
    }
}