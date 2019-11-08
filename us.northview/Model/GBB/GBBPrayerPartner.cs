using Rock.Model;
using System;
using System.Linq;

namespace us.northviewchurch.Model.GBB
{
    public class GBBPrayerPartner
    {
        public int    ID        { get; set; }
        public string FirstName { get; set; }
        public string LastName  { get; set; }
        public string Email     { get; set; }
        public bool   Active    { get; set; }        
        public int      MaxRequests   { get; set; }
        public int      TotalRequests { get; set; }
        public DateTime LastActive    { get; set; }

        public static GBBPrayerPartner CreateFromRockGroupMember(GroupMember RockMember, Rock.Data.Service<AttributeValue> AttrValueSvc, int MaxRequestsAttrVal, int TotalRequestsAttrVal )
        {
            var projAttrs = AttrValueSvc.Queryable().Where(t => t.EntityId == RockMember.Id && (t.AttributeId == MaxRequestsAttrVal || t.AttributeId == TotalRequestsAttrVal)).ToList();

            var maxReqs = projAttrs.FirstOrDefault(x => x.AttributeId == MaxRequestsAttrVal);
            var totReqs = projAttrs.FirstOrDefault(x => x.AttributeId == TotalRequestsAttrVal);

            var maxReqsInt = 0;

            if(maxReqs != null && maxReqs.IsValid)
            {
                Int32.TryParse(maxReqs.Value, out maxReqsInt);
            }

            var totReqsInt = 0;

            if (totReqs != null && totReqs.IsValid)
            {
                Int32.TryParse(totReqs.Value, out totReqsInt);
            }
            

            var partner = new GBBPrayerPartner
            {
                ID = RockMember.Id,
                FirstName = RockMember.Person.FirstName,
                LastName = RockMember.Person.LastName,
                Email = RockMember.Person.Email,
                Active = RockMember.GroupMemberStatus == GroupMemberStatus.Active,
                MaxRequests = maxReqsInt,
                TotalRequests = totReqsInt
            };

            return partner;
        }
    }
}
