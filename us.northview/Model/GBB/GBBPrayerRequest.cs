using Rock.Model;
using System;

namespace us.northviewchurch.Model.GBB
{
    public class GBBPrayerRequest
    {
        public int PrayerRequestId  { get; set; }
        public int PrayerBatchId    { get; set; }
        public int RequestMappingId { get; set; }
        public int PrayerPartnerId  { get; set; }

        public string ImagePath     { get; set; }
        public string RequestName   { get; set; }
        public string Category      { get; set; }

        public bool      Active     { get; set; }
        public DateTime? CreateDate { get; set; }

        public static GBBPrayerRequest CreateFromRockObjects(GBBPrayerRequestMapping Mapping)
        {
            var gbbRequest = new GBBPrayerRequest()
            {
                PrayerRequestId = Mapping.RockPrayerRequest.Id,
                PrayerBatchId = Mapping.PrayerBatchId,
                RequestMappingId = Mapping.Id,
                PrayerPartnerId = Mapping.PrayerPartnerId ?? -1,
                ImagePath = Mapping.RockPrayerRequest.Text.Substring(Mapping.RockPrayerRequest.Text.IndexOf('[')+1, Mapping.RockPrayerRequest.Text.IndexOf(']')-1),
                RequestName = Mapping.RockPrayerRequest.FirstName,
                Category = Mapping.RockPrayerRequest.Category.Name,
                CreateDate = Mapping.CreatedDateTime,
                Active = Mapping.PrayerBatch.CompletionDate.HasValue == false
            };

            return gbbRequest;
        }
    }
}
