using Rock.Data;
using Rock.Web.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
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

            var mappings = new GBBPrayerRequestMappingService(new RockContext()).Queryable("PrayerBatch,RockPrayerRequest").ToList();

            var gbbRequests = mappings.Select(x=> GBBPrayerRequest.CreateFromRockObjects(x)).ToList();

            var vms = gbbRequests.Select(x => GBBPrayerRequestVM.CreateFromGBBPrayerRequest(x)).ToList();
            
            grdRequests.DataSource = vms;
            grdRequests.DataBind();

            var partners = new PrayerPartnerService(new RockContext()).GetPrayerPartners();

            grdPartners.DataSource = partners;
            grdPartners.DataBind();

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