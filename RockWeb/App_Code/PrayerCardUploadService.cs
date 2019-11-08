using Newtonsoft.Json;
using Rock.Data;
using Rock.Model;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web.Script.Services;
using System.Web.Services;
using us.northviewchurch.Model.GBB;

/// <summary>
/// Summary description for PrayerCardUploadService
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class PrayerCardUploadService : System.Web.Services.WebService
{

    public PrayerCardUploadService()
    {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod, ScriptMethod(ResponseFormat = ResponseFormat.Json, UseHttpGet = false)]
    public string GetBatchGuid()
    {
        return Guid.NewGuid().ToString();
    }

    [WebMethod, ScriptMethod(ResponseFormat = ResponseFormat.Json, UseHttpGet = false)]
    public string UploadRequests(string batch, string basePath)
    {
        var valid = false;
        var msg = String.Empty;

        try
        {
            int userId = UserLoginService.GetCurrentUser().Id;

            var ctx = new RockContext();
            var batchSvc = new PrayerBatchService(ctx);
            var personAliasSvc = new PersonAliasService(ctx);

            var personAlias = personAliasSvc.Queryable().Where(x=>x.PersonId == userId).FirstOrDefault();

            int? batchId = null;

            batchId = batchSvc.GetActiveBatchId() ?? batchSvc.CreateNewBatch();

            if(!batchId.HasValue )
            {
                msg = "Cannot find or create an active batch!";
            }
            else
            {
                if (Directory.Exists(basePath))
                {
                    var batchDirPath = Path.Combine(basePath, batchId.ToString());

                    var batchDir = Directory.CreateDirectory(batchDirPath);

                    if (batchDir != null && batchDir.Exists)
                    {
                        var path = "";

                        using (var stream = new MemoryStream(Convert.FromBase64String(batch)))
                        {
                            var requestGuid = Guid.NewGuid();

                            var rawBmp = Bitmap.FromStream(stream);

                            var resizedBmp = new Bitmap(rawBmp, new Size(1280, 768));

                            using (var ms = new MemoryStream())
                            {
                                resizedBmp.Save(ms, ImageFormat.Jpeg);
                                var bmpBytes = ms.ToArray();

                                path = String.Format(@"{0}/{1}.JPG", batchDir.FullName, requestGuid);

                                File.WriteAllBytes(path, bmpBytes);
                            }
                        }

                        var prayerSvc = new PrayerRequestService(ctx);

                        var catSvc = new CategoryService(ctx);

                        var gbbCat = catSvc.Queryable().FirstOrDefault(x=> x.Name == "GBB Auto Uploader");

                        var newRequest = new PrayerRequest()
                        {
                            Text = String.Format("[{0}] ////This prayer request was uploaded by the GBB Auto Uploader. The URI links to the scan of the original request.", path),
                            Category = gbbCat,
                            FirstName = "GBB Prayer Request",
                            EnteredDateTime = DateTime.Now,
                            IsApproved = true,
                            IsActive = true,
                            ApprovedOnDateTime = DateTime.Now,
                            ApprovedByPersonAliasId = personAlias.Id
                        };

                        prayerSvc.Add(newRequest);
                        ctx.SaveChanges();

                        var mapSvc = new GBBPrayerRequestMappingService(ctx);

                        var mapping = new GBBPrayerRequestMapping()
                        {
                            CreatedDateTime = DateTime.Now,
                            PrayerBatchId = batchId.Value,
                            RockPrayerRequestId = newRequest.Id
                        };

                        mapSvc.Add(mapping);
                        ctx.SaveChanges();

                        valid = true;
                    }
                }
                else
                {
                    msg = String.Format("The path {0} does not exist or cannot be reached!", basePath);
                }
            }            
        }
        catch (Exception e)
        {
            msg = String.Format("Error uploading requests to path {0}: {1} \r\n Stack Trace: {2}",basePath, e.Message, e.StackTrace);
        }

        return JsonConvert.SerializeObject(new { result = valid, message = msg });
    }

}
