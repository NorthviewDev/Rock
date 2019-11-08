using Rock;
using Rock.Data;
using Rock.Web.UI;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using us.northviewchurch.Model.GBB;

[DisplayName("GBB Object Test")]
[Category("northviewchurch > Tutorials")]
[Description("Test how the GBB Objects are retrieved")]
public partial class Plugins_us_northviewchurch_Tutorial_GBBObjectTest : RockBlock
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            var partnerId = -1;

            var partnerIdStr = PageParameter("partnerId").ToStringSafe();

            Int32.TryParse(partnerIdStr, out partnerId);

            var mappings = new GBBPrayerRequestMappingService(new RockContext()).Queryable("PrayerBatch,RockPrayerRequest").Where(x=>x.PrayerPartnerId == partnerId).ToList();

            var gbbRequests = mappings.Where(x=> x.Active).Select(x=> GBBPrayerRequest.CreateFromRockObjects(x)).ToList();

            var vms = gbbRequests.Select(x => GBBPrayerRequestVM.CreateFromGBBPrayerRequest(x)).ToList();
            
            grdRequests.DataSource = vms;
            grdRequests.DataBind();

            var partners = new PrayerPartnerService(new RockContext()).GetPrayerPartners();

            grdPartners.DataSource = partners;
            grdPartners.DataBind();

        }
    }

    protected void grdRequests_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "Complete")
        {
            btnComplete_Clicked(sender, e);
        }
    }

    protected void btnComplete_Clicked(object sender, GridViewCommandEventArgs e)
    {
        int id = 0;

        var ctx = new RockContext();

        int index = Convert.ToInt32(e.CommandArgument);

        id = Convert.ToInt32(grdRequests.DataKeys[index].Value);

        var mapping = new GBBPrayerRequestMappingService(ctx).Queryable("PrayerBatch,RockPrayerRequest").Where(x=>x.Id == id).FirstOrDefault();

        if(mapping != null)
        {
            mapping.RockPrayerRequest.IsActive = false;

            ctx.SaveChanges();

            var batch = new PrayerBatchService(ctx).Queryable().Where(x => x.Id == mapping.PrayerBatchId).FirstOrDefault();

            if (batch != null)
            {
                if(!batch.PrayerRequestMappings.Any(x=> x.Active))
                {
                    batch.Active = false;

                    ctx.SaveChanges();
                }               
            }
        }
    }

    public class GBBPrayerRequestVM
    {
        public int RequestMappingId { get; set; }
        public string ImageData { get; set; }
        public string RequestName { get; set; }
        public string Category { get; set; }
        public string Created { get; set; }

        public static GBBPrayerRequestVM CreateFromGBBPrayerRequest(GBBPrayerRequest Request)
        {
            return new GBBPrayerRequestVM
            {
                RequestMappingId = Request.RequestMappingId,
                RequestName = Request.RequestName,
                Category = Request.Category,
                Created = Request.CreateDate.HasValue ? Request.CreateDate.Value.ToShortDateString() : "N/A",
                ImageData = Convert.ToBase64String(File.ReadAllBytes(Request.ImagePath))
            };
        }
    }
}