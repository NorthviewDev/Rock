using Rock.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace us.northviewchurch.Model.GBB
{
    [Table("_us_northviewchurch_PrayerBatch")]
    [DataContract]
    public class PrayerBatch: Model<PrayerBatch>, IRockEntity
    {
        [DataMember]
        public bool Active { get; set; }
        [DataMember]
        public DateTime? CompletionDate { get; set; }
        
        [DataMember]
        public virtual ICollection<GBBPrayerRequestMapping> PrayerRequestMappings
        {
            get { return _requests ?? (_requests = new Collection<GBBPrayerRequestMapping>()); }
            set { _requests = value; }
        }

        private ICollection<GBBPrayerRequestMapping> _requests;
    }

    public partial class PrayerBatchConfiguration : EntityTypeConfiguration<PrayerBatch>
    {
        public PrayerBatchConfiguration()
        {
            this.HasEntitySetName("PrayerBatch");
        }
    }
}
