using Rock.Data;
using Rock.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace us.northviewchurch.Model.GBB
{
    public class PrayerPartnerService
    {
        public virtual DbContext Context { get; set; }

        protected virtual int MaxRequestsAttributeId    { get; set; }
        protected virtual int TotalRequestsAttributeId  { get; set; }
        protected virtual int PrayerPartnerGroupId      { get; set; }

        public PrayerPartnerService(RockContext context)
        {
            Context = context;

            try
            {
                var maxRequestsAttrVal  = -1;
                var totalRequestsAttrVal = -1;
                var prayerPartnerGroupVal = -1;

                Int32.TryParse(ConfigurationManager.AppSettings["GBBMaxRequestsAttributeValue"], out maxRequestsAttrVal);
                Int32.TryParse(ConfigurationManager.AppSettings["GBBTotalRequestsAttributeValue"], out totalRequestsAttrVal);
                Int32.TryParse(ConfigurationManager.AppSettings["GBBPrayerPartnerGroupValue"], out prayerPartnerGroupVal);

                MaxRequestsAttributeId = maxRequestsAttrVal;
                TotalRequestsAttributeId = totalRequestsAttrVal;
                PrayerPartnerGroupId = prayerPartnerGroupVal;
            }
            catch(Exception e)
            {
                //Log?
                Console.Error.Write(e);
            }
        }

        public virtual List<GBBPrayerPartner> GetPrayerPartners()
        {
            var partners = new List<GBBPrayerPartner>();

            var members = GetMembersForPartnerGroup(new GroupMemberService((RockContext)this.Context));

            foreach(var member in members)
            {
                var gbbPartner = GBBPrayerPartner.CreateFromRockGroupMember(member,new AttributeValueService((RockContext)this.Context), MaxRequestsAttributeId, TotalRequestsAttributeId);

                partners.Add(gbbPartner);
            }

            return partners;
        }

        public virtual GBBPrayerPartner GetPrayerPartnerByGroupMemberId(int GroupMemberId)
        {
            GBBPrayerPartner partner = null;

            var member = GetMembersForPartnerGroup(new GroupMemberService((RockContext)this.Context), GroupMemberId: GroupMemberId).FirstOrDefault();

            if(member != null)
            {
                partner = GBBPrayerPartner.CreateFromRockGroupMember(member, new AttributeValueService((RockContext)this.Context), MaxRequestsAttributeId, TotalRequestsAttributeId);
            }

            return partner;
        }

        public virtual GBBPrayerPartner GetPrayerPartnerByPersonId(int PersonId)
        {
            GBBPrayerPartner partner = null;

            var member = GetMembersForPartnerGroup(new GroupMemberService((RockContext)this.Context), PersonId: PersonId).FirstOrDefault();

            if (member != null)
            {
                partner = GBBPrayerPartner.CreateFromRockGroupMember(member, new AttributeValueService((RockContext)this.Context), MaxRequestsAttributeId, TotalRequestsAttributeId);
            }

            return partner;
        }

        protected virtual List<GroupMember> GetMembersForPartnerGroup(Service<GroupMember> GroupService, int? GroupMemberId = null, int? PersonId = null)
        {
            var members = new List<GroupMember>();

            var memberQry = GroupService.Queryable("Person")
                .Where(t => t.GroupId == PrayerPartnerGroupId);

            if(GroupMemberId.HasValue)
            {
                memberQry = memberQry.Where(t=> t.GroupId == GroupMemberId.Value);
            }
            else if (PersonId.HasValue)
            {
                memberQry = memberQry.Where(t=> t.PersonId == PersonId.Value);
            }

            members = memberQry.ToList();

            return members;
        }

    }
}
