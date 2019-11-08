using System;
using System.Web.UI.HtmlControls;
using System.ComponentModel;
using Rock.Model;
using Rock.Attribute;
using System.Text;
using us.northviewchurch.Model.GBB;

[DisplayName("GBB Prayer Request Upload")]
[Category("northviewchurch > GBB")]
[Description("Adds a Javascript-based widget that will handle the uploading of prayer requests")]
[TextField("Base Upload Path", "The main folder uploaded requests will be stored", true)]
public partial class Plugins_northview_GBB_PrayerCardUploader : Rock.Web.UI.RockBlock
{
    protected void Page_Load(object sender, EventArgs e)
    {
        var basePath = GetAttributeValue("BaseUploadPath");

        //create a string for the inner HTML of the script
        var sb = new StringBuilder();
        sb.Append(String.Format(@"
        var uploadPath = '{0}';
        ", basePath.Replace(@"/",@"//").Replace(@"\", @"\\")));

        //create script control
        var objScript = new HtmlGenericControl("script");
        //add javascript type
        objScript.Attributes.Add("type", "text/javascript");
        //set innerHTML to be our StringBuilder string
        objScript.InnerHtml = sb.ToString();

        //add script to PlaceHolder control
        this.placeHldrPathJS.Controls.Add(objScript);
    }

    protected void btnAssign_Click(object sender, EventArgs e)
    {
        var ctx = new Rock.Data.RockContext();

        var batchSvc = new PrayerBatchService(ctx);

        var prayerPartnerSvc = new PrayerPartnerService(ctx);

        var partners = prayerPartnerSvc.GetPrayerPartners();

        var batch = batchSvc.GetActiveBatch();

        GBBPrayerRequestAssigner.AssignRequests(batch, partners);
    }
}