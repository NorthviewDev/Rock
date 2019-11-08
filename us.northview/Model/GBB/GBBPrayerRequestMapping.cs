using Rock.Data;
using Rock.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace us.northviewchurch.Model.GBB
{
    [Table("_us_northviewchurch_GBBPrayerRequestMapping")]
    [DataContract]
    public class GBBPrayerRequestMapping : Model<GBBPrayerRequestMapping>, IRockEntity
    {
        [Required]
        [DataMember(IsRequired = true)]
        public int PrayerBatchId { get; set; }

        [Required]
        [DataMember(IsRequired = true)]
        public int RockPrayerRequestId { get; set; }

        [DataMember]
        public int? PrayerPartnerId { get; set; }

        [DataMember]
        public virtual PrayerBatch PrayerBatch { get; set; }

        [DataMember]
        public virtual PrayerRequest RockPrayerRequest { get; set; }

        public virtual bool Active { get { return this.RockPrayerRequest != null ? this.RockPrayerRequest.IsActive ?? false : false; } }
        
    }

    public partial class GBBPrayerRequestMappingConfiguration : EntityTypeConfiguration<GBBPrayerRequestMapping>
    {
        public GBBPrayerRequestMappingConfiguration()
        {            
            this.HasRequired(x => x.PrayerBatch).WithMany(p=>p.PrayerRequestMappings).HasForeignKey(x => x.PrayerBatchId).WillCascadeOnDelete(false);
            this.HasRequired(x => x.RockPrayerRequest).WithMany().HasForeignKey(x => x.RockPrayerRequestId).WillCascadeOnDelete(false);

            this.HasEntitySetName("GBBPrayerRequestMapping");
        }
    }
}
