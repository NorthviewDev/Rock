using Rock.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace us.northviewchurch.Model.GNW
{
    [Serializable]
    public enum ProjectCategoryTypes
    {
        None = -1,
        [Description("Neighborhood Cleanup/Light Construction/Painting")]
        NeighborhoodCleanup_LightConstruction_Painting,
        [Description("Sorting/Assembly")]
        Sorting_Assembly,
        [Description("Cleaning and Organizing")]
        Cleaning_Organizing
    }

    [Serializable]
    public enum JobCategoryTypes
    {
        None = -1,
        Painting,
        [Description("Landscaping/Yard work")]
        Landscaping_YardWork,
        [Description("Cleaning/clean-up (light)")]
        Cleaning_Light,
        [Description("Cleaning/clean-up (heavy)")]
        Cleaning_Heavy,
        [Description("Stocking/sorting/packing/organizing")]
        Stocking_Sorting_Packing_Organizing,
        Moving,
        [Description("Crafting (i.e. sewing, writing, art painting, making)")]
        Crafting,
        [Description("Serving food/packing food")]
        Serving_PackingFood,
        [Description("Building/assembling (heavy)")]
        Building_Assembling_Heavy,
        [Description("Repair/light construction (unskilled)")]
        Repair_Construction_Unskilled,
        [Description("Repair/light construction (skilled)")]
        Repair_Construction_Skilled
    }

    [Serializable]
    public enum AbilityLevelTypes
    {
        None = -1,
        [Description("Heavy Lifting required")]
        High = 1,
        [Description("Moderate manual work, light lifting")]
        Medium,
        [Description("Simple tasks, sitting, etc.")]
        Low,
        [Description("Special accomodations")]
        Special
    }

    [Serializable]
    public enum SupplyPurachaserType
    {
        None = -1,
        Northview,
        Organization
    }

    [Serializable]
    public enum FamilyFriendlyType
    {
        None = -1,
        [Description("Children 5 years and younger")]
        ChildrenFiveUnder = 1,
        [Description("Children 6-12 years")]
        ChildrenSixToTwelve,
        [Description("13 Years and up")]
        ThirteenAndUp
    }

    [Serializable]
    public class ServingShift
    {
        public int ID { get; set; }
        public DayOfWeek Day { get; set; }
        public TimeSpan Time { get; set; }
    }

    [Serializable]
    public class PartnerProject
    {
        public int ID { get; set; }
        public decimal VolunteerCapacity { get; set; }
        public decimal TotalVolunteers { get; set; }
        public string OrgAddress { get; set; }
        public string Name { get; set; }
        public Dictionary<string,double> Distances { get; set; }

        public ProjectCategoryTypes ProjectType { get; set; }
        public AbilityLevelTypes AbilityLevel { get; set; }
        public FamilyFriendlyType FamilyFriendliness { get; set; }
        public List<JobCategoryTypes> JobType { get; set; }

        public List<ServingShift> Shifts { get; set; }
        public Tuple<decimal, decimal> Coordinates { get; set; }

        public List<VolunteerGroup> AssignedTeams { get; set; }

        public decimal RemainingCapacity { get { return VolunteerCapacity - TotalVolunteers; } }

        public double DistanceToHome { get { return Distances.ContainsKey(_homeCampus) ? Distances[_homeCampus] : Double.MaxValue; } }

        private string _homeCampus;

        public PartnerProject()
        {
            this.Shifts = new List<ServingShift>();
            this.Distances = new Dictionary<string, double>();
        }

        public bool SetHomeCampus(string Campus)
        {
            if(Distances.ContainsKey(Campus))
            {
                _homeCampus = Campus;
                return true;
            }

            return false;
        }

        public static PartnerProject CreateFromRockGroup(Group RockGroup, Rock.Data.Service<AttributeValue> AttrValueSvc)
        {
            var projAttrs = AttrValueSvc.Queryable().Where(t => (t.EntityId == RockGroup.Id || (RockGroup.Id == null && t.EntityId == null))).ToList();

            var orgAddr = projAttrs.FirstOrDefault(x => x.AttributeKey == "OrganizationAddress");
            var volCap = projAttrs.FirstOrDefault(x => x.AttributeKey == "VolunteerCapacity");
            var abilityLevel = projAttrs.FirstOrDefault(x => x.AttributeKey == "AbilityLevel");
            var famFriendly = projAttrs.FirstOrDefault(x => x.AttributeKey == "FamilyFriendly");
            var campusDistances = projAttrs.FirstOrDefault(x => x.AttributeKey == "CampusDistances");

            var volCapDec = 0M;

            Decimal.TryParse(volCap.Value, out volCapDec);

            var distances = new Dictionary<string, double>();

            if (campusDistances != null && !String.IsNullOrWhiteSpace(campusDistances.Value))
            {
                var campuses = campusDistances.Value.Split(new char[] {';'});

                foreach(var campus in campuses)
                {
                    var distDbl = 0.0;

                    var campusInfo = campus.Split(new char[] { ':' });

                    var name = campusInfo[0];
                    var dist = 0.0;

                    dist = Double.TryParse(campusInfo[1], out distDbl) ? distDbl : dist = Double.MaxValue ;

                    distances.Add(name, dist);
                }


            }

            var proj = new PartnerProject
            {
                ID = RockGroup.Id,
                Name = RockGroup.Name,
                OrgAddress = orgAddr == null ? "" : orgAddr.Value,
                AbilityLevel = (AbilityLevelTypes)Enum.Parse(typeof(AbilityLevelTypes), abilityLevel.Value),
                FamilyFriendliness = (FamilyFriendlyType)Enum.Parse(typeof(FamilyFriendlyType), famFriendly.Value),
                VolunteerCapacity = volCapDec,
                TotalVolunteers = RockGroup.Members.Count,
                Distances = distances
            };

            return proj;
        }
    }

    [Serializable]
    public class VolunteerGroup
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int VolunteerCount { get; set; }
        public AbilityLevelTypes AbilityLevel { get; set; }
        public FamilyFriendlyType FamilyFriendliness { get; set; }
        public List<ServingShift> Shifts { get; set; }
        public Tuple<decimal, decimal> HomeChurchCoordinates { get; set; }
        public string HomeCampus { get; set; }

        public VolunteerGroup()
        {
            this.Shifts = new List<ServingShift>();
        }

        public static VolunteerGroup CreateFromRockGroup(Group RockGroup, Rock.Data.Service<AttributeValue> AttrValueSvc)
        {
            var projAttrs = AttrValueSvc.Queryable().Where(t => (t.EntityId == RockGroup.Id || (RockGroup.Id == null && t.EntityId == null))).ToList();

            var abilityLevel = projAttrs.FirstOrDefault(x => x.AttributeKey == "AbilityLevel");
            var famFriendly = projAttrs.FirstOrDefault(x => x.AttributeKey == "FamilyFriendly");

            var vg = new VolunteerGroup
            {
                ID = RockGroup.Id,
                Name = RockGroup.Name,
                AbilityLevel = (AbilityLevelTypes)Enum.Parse(typeof(AbilityLevelTypes), abilityLevel.Value),
                FamilyFriendliness = (FamilyFriendlyType)Enum.Parse(typeof(FamilyFriendlyType), famFriendly.Value),
                VolunteerCount = RockGroup.Members.Count,
                HomeCampus = RockGroup.ParentGroup.Name
            };

            return vg;
        }

        public List<PartnerProject> FindMatches(List<PartnerProject> Projects, double MaxDistance = Double.MaxValue)
        {
            var potentials = Projects.Where(x => x.SetHomeCampus(this.HomeCampus) &&  x.DistanceToHome < MaxDistance && x.AbilityLevel >= this.AbilityLevel 
                                            && x.FamilyFriendliness <= this.FamilyFriendliness && (x.RemainingCapacity >= this.VolunteerCount))
                                        .OrderByDescending(x => x.FamilyFriendliness)
                                        .ThenBy(x => x.AbilityLevel)
                                        .ThenBy(x => x.DistanceToHome)
                                        .ToList();

            return potentials;
        }
    }
}
