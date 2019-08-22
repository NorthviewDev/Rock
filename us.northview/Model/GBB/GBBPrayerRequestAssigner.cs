using System;
using System.Collections.Generic;
using System.Linq;

namespace us.northviewchurch.Model.GBB
{
    public static class GBBPrayerRequestAssigner
    {
        public static void AssignRequests(PrayerBatch CurrentBatch, List<GBBPrayerPartner> PrayerPartners )
        {
            var prayerRequests = CurrentBatch.PrayerRequestMappings.ToList();

            //Order it by LastActive so the people who haven't had any in a while will get some first
            PrayerPartners = PrayerPartners.OrderByDescending(x => x.LastActive).ToList();

            //Assign one at a time until there are either no more unassigned requests or no more partner capacity
            while (prayerRequests.Any(x=> x.PrayerPartnerId.HasValue == false) && PrayerPartners.Any(x=> x.TotalRequests <= x.MaxRequests))
            {
                //Grab the unassigned requests as a Stack for ease of managing the list
                var unassignedRequests = new Stack<GBBPrayerRequestMapping>(prayerRequests.Where(x => x.PrayerPartnerId.HasValue == false));

                var availablePartners = PrayerPartners.Where(x => x.TotalRequests <= x.MaxRequests).ToList();

                //Go through the list of available partners and assign one at a time. This will ensure the most even distribution
                foreach(var partner in availablePartners)
                {
                    GBBPrayerRequestMapping request = null;
                    if (unassignedRequests.Any())
                    {
                        request = unassignedRequests.Pop();

                        AssignRequest(request, partner);
                    }
                }
            }

            //Done assigning, let's see how we did and adjust if necessary

            if(prayerRequests.Any(x => x.PrayerPartnerId.HasValue == false))
            {
                //We have more requests than the current list of Partners are willing to take.
                //What to do? Send a notification to someone?
            }
            else if(PrayerPartners.Any(x => x.TotalRequests == 0))
            {
                //We didn't have enough requests to assign to everyone
                //In order to keep engagement high, we'll try to take some from people who have the most
                //and redistribute if possible. 
                //This scenario is only likely if partners have incomplete prior batches and there weren't enough requests in the current batch to go to everyone

                var moreThanOneCount = new Stack<GBBPrayerPartner>(PrayerPartners.Where(x => x.TotalRequests > 1).OrderByDescending(x=> x.TotalRequests));

                if(moreThanOneCount.Any())
                {
                    var noRequests = new Stack<GBBPrayerPartner>(PrayerPartners.Where(x => x.TotalRequests == 0));

                    while(moreThanOneCount.Any() && noRequests.Any())
                    {

                        var sharingPartner = moreThanOneCount.Pop();
                        var takingPartner = noRequests.Pop();

                        var assignedRequest = prayerRequests.Where(x => x.PrayerPartnerId == sharingPartner.ID).FirstOrDefault();

                        AssignRequest(assignedRequest, takingPartner);
                        sharingPartner.TotalRequests--;
                    }

                    if(noRequests.Any())
                    {
                        //At the end of this sharing reassignment, there were still some that didn't get any
                        //What to do, if anything?
                    }
                }
                else
                {
                    //Shoot, everyone already has just one. Nothing else to do in this case.
                    //Do we want to send some sort of notification?
                }
            }
        }

        public static void AssignRequest(GBBPrayerRequestMapping RequestMapping, GBBPrayerPartner Partner)
        {
            //The try/catch is in here so that a null Partner or Request won't crash the entire assignment job
            try
            {
                RequestMapping.PrayerPartnerId = Partner.ID;
                Partner.TotalRequests++;
            }
            catch (Exception e)
            {
                //What to do here?
            }
        }
    }
}
