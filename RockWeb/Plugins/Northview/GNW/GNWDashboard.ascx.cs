using Newtonsoft.Json;
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
[TextField("Group Detail URL",required: true, key: "GroupDetailUrl")]
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

        if(String.IsNullOrWhiteSpace(_displayMode))
        {
            _displayMode = "All";
        }

        var rockCtx = new Rock.Data.RockContext();

        var groupSvc = new GroupService(rockCtx);

        var campusSvc = new CampusService(rockCtx);
        var personSvc = new PersonService(rockCtx);
        var attrSvc = new AttributeValueService(rockCtx);

        if(String.IsNullOrWhiteSpace(this._displayMode) || this._displayMode == "All")
        {
            _activeCampuses = campusSvc.Queryable().Where(x => x.IsActive ?? false).ToList().Select(x => new CampusInfo(x.Id, x.Name)).ToList();
        }
        else
        {
            _activeCampuses = campusSvc.Queryable().Where(x => x.Name == this._displayMode).ToList().Select(x => new CampusInfo(x.Id, x.Name)).ToList();
        }

        var campusDataStrings = new List<string>();

        var adults = personSvc.Queryable().ToList().Where(x => x.Age.HasValue && x.Age >= 18).ToList();

        var allNorthview = new CampusInfo(-1, "All Northview");

        foreach (var campus in _activeCampuses)
        {
            var memberCount = adults.Where(x=> x.GetCampus() != null && x.GetCampus().Id == campus.ID).Count();

            var campusVolunteers = groupSvc.Queryable().Where(x => x.GroupTypeId == _teamGroupTypeId && x.CampusId == campus.ID).ToList().Select(x => VolunteerGroup.CreateFromRockGroup(x, attrSvc)).ToList();

            var campusProjects = groupSvc.Queryable().Where(x => x.GroupTypeId == _projectGroupTypeId && x.CampusId == campus.ID).ToList().Select(x => PartnerProject.CreateFromRockGroup(x, attrSvc)).ToList();

            var volunteerCount = campusVolunteers.Sum(x => x.VolunteerCount);
            var projectCapacity = campusProjects.Sum(x => x.RemainingCapacity);

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

            //_thermometerRenderStrings.Add(String.Format("renderThermometer('{0}', {1}, {2});{3}", campus.Name.Replace(" ", ""), volunteerCount/memberCount, memberCount, Environment.NewLine));

            _thermometerRenderStrings.Add(String.Format("renderThermometer('{0}', {1}, {2});{3}",campus.Name.Replace(" ",""),new Random().Next(100,800)*.11,new Random().Next(750,2000), Environment.NewLine));
            
        }
        //TESTING

        var fishers = new CampusInfo()
        {
            ID = -2,
            Name = "Fishers",
            AdultMembers = 1000,
            TotalProjects = 20,
            TotalVolunteers = 400,
            TotalRemainingVolunteerCapacity = 100,            
        };

        _thermometerRenderStrings.Add(String.Format("renderThermometer('{0}', {1}, {2});{3}", fishers.Name.Replace(" ", ""), (decimal)fishers.TotalVolunteers / fishers.AdultMembers, fishers.AdultMembers, Environment.NewLine));

        _activeCampuses.Add(fishers);

        allNorthview.AdultMembers = 7673;
        allNorthview.TotalProjects = 120;
        allNorthview.TotalVolunteers = 600;
        allNorthview.TotalRemainingVolunteerCapacity = 1000;

        //END TESTING

        _activeCampuses.Add(allNorthview);
        _thermometerRenderStrings.Add(String.Format("renderThermometer('{0}', {1}, {2});{3}", "AllNorthview", (decimal)allNorthview.TotalVolunteers / allNorthview.AdultMembers, allNorthview.AdultMembers, Environment.NewLine));

        _activeCampuses = _activeCampuses.OrderBy(x => x.Name).ToList();

        //create a string for the inner HTML of the script

        var sb = new StringBuilder();
        sb.Append(String.Format(@"
        var campusData = [{0}];
        ", String.Join(",", campusDataStrings)));

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