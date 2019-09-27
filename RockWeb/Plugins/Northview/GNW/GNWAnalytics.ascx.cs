using Rock.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using us.northviewchurch.Model.GNW;

[DisplayName("GNW Analytics")]
[Category("Northview > GNW")]
[Description("Runs general utility tasks")]
public partial class Plugins_Northview_Utility_GNWAnalytics : Rock.Web.UI.RockBlock
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void btnRun_Click(object sender, EventArgs e)
    {
        try
        {
            var rockCtx = new Rock.Data.RockContext();

            var groupSvc = new GroupService(rockCtx);

            var attributeSvc = new AttributeService(rockCtx);
            var fieldTypeSvc = new FieldTypeService(rockCtx);
            var attrValueSvc = new AttributeValueService(rockCtx);
            var entityTypeSvc = new EntityTypeService(rockCtx);
            var groupMemberSvc = new GroupMemberService(rockCtx);

            var orgGroupType = 216;
            var volGroupType = 217;

            var assignedVolunteers = groupSvc.Queryable().Where(x => x.GroupTypeId == volGroupType && x.ParentGroup.GroupTypeId == orgGroupType).ToList();
            var unassignedVolunteers = groupSvc.Queryable().Where(x => x.GroupTypeId == volGroupType && x.ParentGroup.GroupTypeId != orgGroupType).ToList();

            var projectGroups = groupSvc.Queryable().Where(x => x.GroupTypeId == orgGroupType && x.CampusId == 4).ToList();

            var groupSignupGroupId = 254554;
            var individualSignupGroupId = 254555;

            var assignedNoMembers = assignedVolunteers.Where(x => x.Members.Count == 0).Count();
            var unassignedNoMembers = unassignedVolunteers.Where(x => x.Members.Count == 0).Count();

            var noMembersCount = assignedNoMembers + unassignedNoMembers;

            var matchedVolunteers = new List<VolunteerGroup>();
            var unmatchedVolunteers = new List<VolunteerGroup>();

            var projects = new List<PartnerProject>();

            foreach (var volTeam in assignedVolunteers)
            {
                var volunteerGrpResult = VolunteerGroup.CreateFromRockGroup(volTeam, attrValueSvc, groupMemberSvc);

                if (volunteerGrpResult.Success)
                {
                    matchedVolunteers.Add(volunteerGrpResult.ResponseObject);
                }
            }

            foreach (var volTeam in unassignedVolunteers)
            {
                var volunteerGrpResult = VolunteerGroup.CreateFromRockGroup(volTeam, attrValueSvc, groupMemberSvc);

                if (volunteerGrpResult.Success)
                {
                    unmatchedVolunteers.Add(volunteerGrpResult.ResponseObject);
                }
            }

            var assignedVolLookup = matchedVolunteers.ToLookup(x => x.HomeCampus);
            var unassignedVolLookup = unmatchedVolunteers.ToLookup(x => x.HomeCampus);

            this.txtResults.InnerHtml += String.Format("Total Groups With No Members: {0}{1}", noMembersCount.ToString(), Environment.NewLine);
            this.txtResults.InnerHtml += String.Format("Matched Groups With No Members: {0}{1}", assignedNoMembers.ToString(), Environment.NewLine);
            this.txtResults.InnerHtml += String.Format("Unmatched Groups With No Members: {0}{1}", unassignedNoMembers.ToString(), Environment.NewLine);

            this.txtResults.InnerHtml += "****Campus Group Assignment Breakdown****" + Environment.NewLine;

            foreach (var group in assignedVolLookup)
            {
                var unassignedCount = 0;
                var emptyGroups = 0;
                if (unassignedVolLookup.Contains(group.Key))
                {
                    unassignedCount = unassignedVolLookup[group.Key].Count();

                    emptyGroups += unassignedVolLookup[group.Key].Where(x => x.VolunteerCount == 0).Count();
                }

                emptyGroups += group.Where(x => x.VolunteerCount == 0).Count();

                this.txtResults.InnerHtml += String.Format("Campus {0} : {1} Groups Assigned {2} Groups Unassigned {3} Groups Empty {4}", group.Key, group.AsEnumerable().Count(), unassignedCount, emptyGroups, Environment.NewLine);
            }

            this.txtResults.InnerHtml += "****Campus Project Assignment Breakdown****" + Environment.NewLine;

            foreach (var projGrp in projectGroups)
            {

                //var projAttrs = attrValueSvc.Queryable().Where(t => (t.EntityId == projGrp.Id)).ToList();

                //var campusDistances = projAttrs.FirstOrDefault(x => x.AttributeKey == "Distances");

                //this.txtResults.InnerHtml += String.Format("Project: {0}:{1} Distances: {2} {3}", projGrp.Id, projGrp.Name, campusDistances.Value, Environment.NewLine);

                var partnerProjResult = PartnerProject.CreateFromRockGroup(projGrp, attrValueSvc);

                PartnerProject partnerProj = null;

                if (partnerProjResult.Success)
                {
                    partnerProj = partnerProjResult.ResponseObject;

                    var matchedTeams = groupSvc.GetChildren(partnerProj.ID, 0, false, new List<int> { volGroupType }, new List<int> { 0 }, false, false).ToList();

                    foreach (var grp in matchedTeams)
                    {
                        var result = VolunteerGroup.CreateFromRockGroup(grp, attrValueSvc, groupMemberSvc);

                        if (result.Success)
                        {
                            partnerProj.AssignTeam(result.ResponseObject);
                        }
                    }

                    projects.Add(partnerProj);
                }
            }

            var projLookup = projects.ToLookup(x => x.HomeCampus);

            foreach (var group in projLookup)
            {
                var totalProjects = 0;
                var fullProjects = 0;
                var emptyProjects = 0;

                var projs = projLookup[group.Key].ToList();

                totalProjects = projs.Count;
                fullProjects = projs.Where(x => x.Shifts.Values.Sum() <= 0).Count();
                emptyProjects = projs.Where(x => x.TotalVolunteers <= 0).Count();

                this.txtResults.InnerHtml += String.Format("Campus {0} : {1} Total Projects - {2} Full, {3} Empty {4}", group.Key, totalProjects, fullProjects, emptyProjects, Environment.NewLine);
            }

        }
        catch (Exception ex)
        {
            this.txtResults.InnerHtml += String.Format("Error! Message: {0} {2} Stack: {1}{2}", ex.Message, ex.StackTrace, Environment.NewLine);
        }
    }
}