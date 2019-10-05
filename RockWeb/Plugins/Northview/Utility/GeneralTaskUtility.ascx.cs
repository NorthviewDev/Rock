using Rock.Data;
using Rock.Model;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;

[DisplayName("General Task Utility")]
[Category("Northview > Utilities")]
[Description("Runs general utility tasks")]
public partial class Plugins_Northview_Utility_GeneralTaskUtility : Rock.Web.UI.RockBlock
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void btnRun_Click(object sender, EventArgs e)
    {
        try
        {
            var rockCtx = new RockContext();

            var groupSvc = new GroupService(rockCtx);

            var attributeSvc = new AttributeService(rockCtx);
            var fieldTypeSvc = new FieldTypeService(rockCtx);
            var attrValueSvc = new AttributeValueService(rockCtx);
            var entityTypeSvc = new EntityTypeService(rockCtx);

            var orgGroupType = 216;
            var volGroupType = 217;

            var assignedVolunteers = groupSvc.Queryable().Where(x => x.GroupTypeId == volGroupType && x.ParentGroup.GroupTypeId == orgGroupType).ToList();

            var groupSignupGroupId = 254554;
            var individualSignupGroupId = 254555;

            var campusSvc = new CampusService(rockCtx);

            foreach (var volGroup in assignedVolunteers)
            {
                var projAttrs = attrValueSvc.Queryable().Where(t => (t.EntityId == volGroup.Id )).ToList();

                if (projAttrs != null)
                {
                    var source = projAttrs.FirstOrDefault(x => x.AttributeKey == "Source");

                    if (source == null || String.IsNullOrWhiteSpace(source.Value))
                    {
                        this.txtResults.InnerHtml += String.Format("Group {0} has no Source attribute! {1}", volGroup.Id, Environment.NewLine);
                    }
                    else
                    {

                        var campusId = 0;

                        if (volGroup.Campus == null)
                        {
                            
                            if(volGroup.Members != null && volGroup.Members.Any())
                            {
                                var member = volGroup.Members.First();

                                var campus = member.Person.GetCampus();

                                if(campus != null)
                                {
                                    campusId = campus.Id;
                                }
                                else
                                {
                                    this.txtResults.InnerHtml += String.Format("No Campus for {0}! {1}", volGroup.Id, Environment.NewLine);
                                    continue;
                                }
                            }
                            else
                            {
                                this.txtResults.InnerHtml += String.Format("No Campus or Members for {0}! {1}", volGroup.Id, Environment.NewLine);
                                volGroup.ParentGroupId = 281103;
                                rockCtx.SaveChanges();
                                continue;
                            }
                        }
                        else
                        {
                            campusId = volGroup.Campus.Id;
                        }

                        if (source.Value == "1")
                        {
                            var groupParentGroup = groupSvc.Queryable().Where(x => x.ParentGroupId == groupSignupGroupId && x.CampusId == campusId).FirstOrDefault();

                            if (groupParentGroup != null)
                            {
                                volGroup.ParentGroupId = groupParentGroup.Id;

                                rockCtx.SaveChanges();
                            }
                            else
                            {
                                this.txtResults.InnerHtml += String.Format("No Parent Group found for {0}, campus: {1}! {2}", volGroup.Id, volGroup.Campus.Name, Environment.NewLine);
                            }
                        }
                        else
                        {
                            var indParentGroup = groupSvc.Queryable().Where(x => x.ParentGroupId == individualSignupGroupId && x.CampusId == campusId).FirstOrDefault();

                            if (indParentGroup != null)
                            {
                                volGroup.ParentGroupId = indParentGroup.Id;

                                rockCtx.SaveChanges();
                            }
                            else
                            {
                                this.txtResults.InnerHtml += String.Format("No Parent Group found for {0}, campus: {1}! {2}", volGroup.Id, volGroup.Campus.Name, Environment.NewLine);
                            }
                        }


                    } 
                }
                else
                {
                    this.txtResults.InnerHtml += String.Format("No Attributes Foundd for Group {0}, campus: {1}! {2}", volGroup.Id, volGroup.Campus.Name, Environment.NewLine);
                }
            }
        }
        catch (Exception ex)
        {
            this.txtResults.InnerHtml += String.Format("Error! Message: {0} {2} Stack: {1}{2}", ex.Message, ex.StackTrace, Environment.NewLine);
        }
    }    
}