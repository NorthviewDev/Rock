using Rock.Data;
using Rock.Model;
using RockWeb;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Attribute = Rock.Model.Attribute;

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
        ThirteenAndUp,
        [Description("21+")]
        TwentyOneAndOver
    }

    [Serializable]
    public enum ServingShift
    {
        [Description("Saturday Morning")]
        SaturdayAM = 1,
        [Description("Saturday Afternoon")]
        SaturdayPM,
        [Description("Sunday Morning")]
        SundayAM
    }

    public static class ServingShiftExtensions
    {
        public static bool ContainsAny(this Dictionary<ServingShift, decimal> baseList, IEnumerable<ServingShift> list)
        {
            foreach (var val in list)
            {
                if (baseList.Keys.Contains(val))
                {
                    return true;
                }
            }

            return false;
        }
    }

    [Serializable]
    public class PartnerProject
    {
        private string _targetCampus = "";

        public int ID { get; set; }
        public decimal VolunteerCapacity { get; set; }
        public decimal TotalVolunteers { get { return this.AssignedTeams.Sum(x=>x.VolunteerCount); } }
        public string OrgAddress { get; set; }
        public string ProjectAddress { get; set; }
        public string Name { get; set; }
        public string HomeCampus { get; set; }
        public int HomeCampusId { get; set; }
        public Dictionary<string, double> Distances { get; set; }

        public ProjectCategoryTypes ProjectType { get; set; }
        public AbilityLevelTypes AbilityLevel { get; set; }
        public FamilyFriendlyType FamilyFriendliness { get; set; }
        public List<JobCategoryTypes> JobType { get; set; }

        public Dictionary<ServingShift, decimal> Shifts { get; set; }
        public Tuple<decimal, decimal> Coordinates { get; set; }

        public List<VolunteerGroup> AssignedTeams { get; set; }

        public double DistanceToTarget { get { return Distances.ContainsKey(_targetCampus) ? Distances[_targetCampus] : Double.MaxValue; } }

        public int GroupTypeId { get; set; }
        public int GroupEntityTypeId { get; set; }

        public int SiteLeaderId { get; set; }

        public PartnerProject()
        {
            this.Shifts = new Dictionary<ServingShift, decimal>();
            this.Distances = new Dictionary<string, double>();
            this.AssignedTeams = new List<VolunteerGroup>();
        }

        public bool SetTargetCampus(string Campus)
        {
            if (Distances.ContainsKey(Campus))
            {
                _targetCampus = Campus;
                return true;
            }

            return false;
        }

        public bool CreateDistancesAttribute(Rock.Data.DbContext RockCtx, Service<AttributeValue> AttrValueSvc,
                                            Service<Attribute> AttrSvc, Service<FieldType> FieldTypeSvc,
                                            Service<EntityType> EntityTypeSvc, int TextFieldTypeId,
                                            Dictionary<string, double> DistanceMatrix, out string message)
        {
            var success = false;

            message = "Processing Distance Matrix";

            try
            {
                var distResultString = new StringBuilder();

                if (!DistanceMatrix.Any())
                {
                    message = "Distance Matrix did not contain any entries!";
                    return false;
                }

                foreach (var dist in DistanceMatrix)
                {
                    distResultString.AppendFormat("{0}:{1};", dist.Key, dist.Value);
                }

                this.Distances = DistanceMatrix;

                var distAttr = AttrSvc.Queryable().FirstOrDefault(t => t.Key == "Distances");
                var distAttrValue = AttrValueSvc.Queryable().Where(t => t.EntityId == this.ID).ToList().FirstOrDefault(x => x.AttributeKey == "Distances");

                var distMatrixStr = distResultString.ToString();

                if (distAttr == null)
                {
                    var textFieldType = FieldTypeSvc.Get(TextFieldTypeId);
                    var groupEntityType = EntityTypeSvc.Get(this.GroupEntityTypeId);

                    distAttr = new Rock.Model.Attribute()
                    {
                        Name = "Distances",
                        Key = "Distances",
                        FieldType = textFieldType,
                        EntityType = groupEntityType,
                        EntityTypeQualifierColumn = this.GroupTypeId.ToString(),
                        IsActive = true
                    };

                    AttrSvc.Add(distAttr);
                    RockCtx.SaveChanges();
                }

                if (distAttrValue == null)
                {
                    distAttrValue = new Rock.Model.AttributeValue()
                    {
                        Attribute = distAttr,
                        AttributeId = distAttr.Id,
                        Value = distMatrixStr,
                        EntityId = this.ID
                    };

                    AttrValueSvc.Add(distAttrValue);
                }
                else
                {
                    distAttrValue.Value = distMatrixStr;
                }

                RockCtx.SaveChanges();

                success = true;
                message = "Success!";
            }
            catch (Exception e)
            {
                message = String.Format("Error: {0} \r\n Stack: {1}", e.Message, e.StackTrace);
            }

            return success;
        }

        public bool CreateSiteLeaderAttribute(Rock.Data.DbContext RockCtx, Service<AttributeValue> AttrValueSvc,
                                            Service<Attribute> AttrSvc, Service<FieldType> FieldTypeSvc,
                                            Service<EntityType> EntityTypeSvc, int PersonFieldTypeId,
                                            int SiteLeaderId, out string message)
        {
            var success = false;

            message = "Processing Site Leader";

            try
            {
                var ldrAttr = AttrSvc.Queryable().FirstOrDefault(t => t.Key == "SiteLeader");
                var ldrtAttrValue = AttrValueSvc.Queryable().Where(t => t.EntityId == this.ID).ToList().FirstOrDefault(x => x.AttributeKey == "SiteLeader");

                if (ldrAttr == null)
                {
                    var personFieldType = FieldTypeSvc.Get(PersonFieldTypeId);
                    var groupEntityType = EntityTypeSvc.Get(this.GroupEntityTypeId);

                    ldrAttr = new Rock.Model.Attribute()
                    {
                        Name = "Site Leader",
                        Key = "SiteLeader",
                        FieldType = personFieldType,
                        EntityType = groupEntityType,
                        EntityTypeQualifierColumn = this.GroupTypeId.ToString(),
                        IsActive = true
                    };

                    AttrSvc.Add(ldrAttr);
                    RockCtx.SaveChanges();
                }

                if (ldrtAttrValue == null)
                {
                    ldrtAttrValue = new Rock.Model.AttributeValue()
                    {
                        Attribute = ldrAttr,
                        AttributeId = ldrAttr.Id,
                        Value = SiteLeaderId.ToString(),
                        EntityId = this.ID
                    };

                    AttrValueSvc.Add(ldrtAttrValue);
                }
                else
                {
                    ldrtAttrValue.Value = SiteLeaderId.ToString();
                }

                RockCtx.SaveChanges();

                success = true;
                message = "Success!";
            }
            catch (Exception e)
            {
                message = String.Format("Error: {0} \r\n Stack: {1}", e.Message, e.StackTrace);
            }

            return success;
        }

        public static ServiceResult<PartnerProject> CreateFromRockGroup(Group RockGroup, Service<AttributeValue> AttrValueSvc)
        {
            var proj = new PartnerProject();

            var result = new ServiceResult<PartnerProject>();

            try
            {
                var projAttrs = AttrValueSvc.Queryable().Where(t => (t.EntityId == RockGroup.Id || (RockGroup.Id == null && t.EntityId == null))).ToList();

                var orgAddr = projAttrs.FirstOrDefault(x => x.AttributeKey == "OrganizationAddress");
                var projAddr = projAttrs.FirstOrDefault(x => x.AttributeKey == "ProjectAddress");
                var volCap = projAttrs.FirstOrDefault(x => x.AttributeKey == "VolunteerCapacity");
                var abilityLevel = projAttrs.FirstOrDefault(x => x.AttributeKey == "AbilityLevel");
                var famFriendly = projAttrs.FirstOrDefault(x => x.AttributeKey == "FamilyFriendly");
                var campusDistances = projAttrs.FirstOrDefault(x => x.AttributeKey == "Distances");
                var servingShifts = projAttrs.FirstOrDefault(x => x.AttributeKey == "Servingshifts");

                var siteLdr = projAttrs.FirstOrDefault(x => x.AttributeKey == "SiteLeader");

                var volCapDec = 0M;
                var siteLdrId = -1;

                Decimal.TryParse(volCap.Value, out volCapDec);

                var distances = new Dictionary<string, double>();

                if (campusDistances != null)
                {
                    distances = ParseDistanceMatrixString(campusDistances.Value);
                }

                if (siteLdr != null)
                {
                    siteLdrId = siteLdr.ValueAsPersonId ?? -1;
                }

                var shifts = new Dictionary<ServingShift, decimal>();

                if (servingShifts != null)
                {
                    var shiftStrs = servingShifts.Value.Split(new char[] { ',' });

                    foreach (var shiftStr in shiftStrs)
                    {
                        var shiftInt = 0;

                        if (Int32.TryParse(shiftStr, out shiftInt) && shiftInt != (int)ServingShift.SaturdayPM)
                        {
                            shifts.Add((ServingShift)shiftInt, volCapDec);
                        }
                    }
                }

                var campusName = "N/A";

                if (RockGroup.Campus != null && !String.IsNullOrWhiteSpace(RockGroup.Campus.Name))
                {
                    campusName = RockGroup.Campus.Name;
                }

                proj = new PartnerProject
                {
                    ID = RockGroup.Id,
                    Name = RockGroup.Name,
                    HomeCampus = campusName,
                    OrgAddress = orgAddr == null ? "" : orgAddr.ValueFormatted,
                    ProjectAddress = projAddr == null ? "" : projAddr.ValueFormatted,
                    AbilityLevel = (AbilityLevelTypes)Enum.Parse(typeof(AbilityLevelTypes), abilityLevel.Value),
                    FamilyFriendliness = (FamilyFriendlyType)Enum.Parse(typeof(FamilyFriendlyType), famFriendly.Value),
                    VolunteerCapacity = volCapDec,
                    Distances = distances,
                    Shifts = shifts,
                    GroupTypeId = RockGroup.GroupTypeId,
                    GroupEntityTypeId = RockGroup.TypeId,
                    SiteLeaderId = siteLdrId,
                    HomeCampusId = RockGroup.Campus.Id
                };

                result.ResponseObject = proj;

                result.Success = true;

            }
            catch (Exception e)
            {
                var msg = String.Format("Error mapping group {0}: {1}! \r\n Message: {2} \r\n Stack: {3}", RockGroup.Id, RockGroup.Name, e.Message, e.StackTrace);
                result.Message = msg;
            }

            return result;
        }

        public bool HasServingCapacity(VolunteerGroup group)
        {
            foreach (var val in group.Shifts)
            {
                if (this.Shifts.Keys.Contains(val))
                {
                    if (this.Shifts[val] >= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void AssignTeam(VolunteerGroup team)
        {
            var keys = this.Shifts.Keys.ToList();

            foreach (var key in keys)
            {
                if (team.Shifts.Contains(key))
                {
                    this.Shifts[key] -= team.VolunteerCount;
                }
            }

            this.AssignedTeams.Add(team);
        }

        protected static Dictionary<string, double> ParseDistanceMatrixString(string DistanceMatrixStr)
        {
            var distances = new Dictionary<string, double>();

            if (!String.IsNullOrWhiteSpace(DistanceMatrixStr))
            {
                var campuses = DistanceMatrixStr.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var campus in campuses)
                {
                    var distDbl = 0.0;

                    var campusInfo = campus.Split(new char[] { ':' });

                    var name = campusInfo[0];
                    var dist = 0.0;

                    dist = Double.TryParse(campusInfo[1], out distDbl) ? distDbl : dist = Double.MaxValue;

                    distances.Add(name, dist);
                }
            }

            return distances;
        }
    }

    [Serializable]
    public enum LifeGroupTypes
    {
        Unknown,
        LifeGroup,
        Individiual
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
        public int HomeCampusId { get; set; }
        public LifeGroupTypes LifeGroupType { get; set; }

        public List<int> MemberIds { get; set; }

        public int? SiteLeaderId { get; set; }

        public VolunteerGroup()
        {
            this.Shifts = new List<ServingShift>();
            this.MemberIds = new List<int>();
        }

        public static ServiceResult<VolunteerGroup> CreateFromRockGroup(Group RockGroup, Rock.Data.Service<AttributeValue> AttrValueSvc, Rock.Data.Service<GroupMember> GrpMemberSvc)
        {
            var result = new ServiceResult<VolunteerGroup>();

            try
            {
                if (RockGroup == null || RockGroup.Id < 1)
                {
                    result.Message = "RockGroup is null or Id is invalid";
                }
                else
                {
                    var projAttrs = AttrValueSvc.Queryable().Where(t => (t.EntityId == RockGroup.Id || (RockGroup.Id == null && t.EntityId == null))).ToList();

                    var abilityLevel = projAttrs.FirstOrDefault(x => x.AttributeKey == "AbilityLevel");
                    var famFriendly = projAttrs.FirstOrDefault(x => x.AttributeKey == "FamilyFriendly");
                    var servingShifts = projAttrs.FirstOrDefault(x => x.AttributeKey == "ServingShift");
                    var source = projAttrs.FirstOrDefault(x => x.AttributeKey == "Source");

                    var shifts = new List<ServingShift>();

                    if (servingShifts != null)
                    {
                        var shiftStrs = servingShifts.Value.Split(new char[] { ',' });

                        foreach (var shiftStr in shiftStrs)
                        {
                            var shiftInt = 0;

                            if (Int32.TryParse(shiftStr, out shiftInt) && shiftInt != (int)ServingShift.SaturdayPM)
                            {
                                shifts.Add((ServingShift)shiftInt);
                            }
                        }
                    }

                    var abilityLvl = AbilityLevelTypes.High;

                    var familyFrndly = FamilyFriendlyType.ThirteenAndUp;

                    var src = LifeGroupTypes.LifeGroup;

                    var members = GrpMemberSvc.Queryable("Person,GroupRole").AsNoTracking()
                       .Where(m => m.GroupId == RockGroup.Id).ToList();

                    var memberIds = members.Select(x => x.PersonId).ToList();

                    int? siteLeaderId = null;

                    if (memberIds.Any())
                    {
                        siteLeaderId = AttrValueSvc.Queryable().Where(x => x.AttributeId == 33536 && x.EntityId != null && x.EntityId.HasValue
                                                       && memberIds.Contains(x.EntityId.Value) && x.Value != null && x.Value.Length > 1)
                                                       .Select(x => x.EntityId.Value).FirstOrDefault(); 
                    }

                    if (RockGroup.Campus == null)
                    {
                        if (members != null && members.Any())
                        {
                            var member = members.First();

                            var campus = member.Person.GetCampus();

                            if (campus != null)
                            {
                                RockGroup.Campus = campus;
                            }
                            else
                            {
                                result.Message = String.Format("No Campus for {0}! {1}", RockGroup.Id, Environment.NewLine);
                                return result;
                            }
                        }
                        else
                        {
                            result.Message = String.Format("No Campus or Members for {0}! {1}", RockGroup.Id, Environment.NewLine);
                            return result;
                        }
                    }

                    if (abilityLevel != null && !String.IsNullOrWhiteSpace(abilityLevel.Value))
                    {
                        abilityLvl = Enum.TryParse<AbilityLevelTypes>(abilityLevel.Value, out abilityLvl) ? abilityLvl : AbilityLevelTypes.High;
                    }

                    if (famFriendly != null && !String.IsNullOrWhiteSpace(famFriendly.Value))
                    {
                        familyFrndly = Enum.TryParse<FamilyFriendlyType>(famFriendly.Value, out familyFrndly) ? familyFrndly : FamilyFriendlyType.ThirteenAndUp;
                    }

                    if (source != null && !String.IsNullOrWhiteSpace(source.Value))
                    {
                        src = Enum.TryParse<LifeGroupTypes>(source.Value, out src) ? src : LifeGroupTypes.LifeGroup;
                    }

                    var campusName = RockGroup.Campus.Name;

                    var vg = new VolunteerGroup
                    {
                        ID = RockGroup.Id,
                        Name = RockGroup.Name,
                        AbilityLevel = abilityLvl,
                        FamilyFriendliness = familyFrndly,
                        VolunteerCount = members.Count,
                        HomeCampus = campusName,
                        Shifts = shifts,
                        LifeGroupType = src,
                        MemberIds = memberIds,
                        HomeCampusId = RockGroup.Campus.Id,
                        SiteLeaderId = siteLeaderId
                    };

                    result.Success = true;
                    result.ResponseObject = vg;
                }

            }
            catch (Exception e)
            {
                var msg = String.Format("Error mapping group {0}: {1}! \r\n Message: {2} \r\n Stack: {3}", RockGroup.Id, RockGroup.Name, e.Message, e.StackTrace);
                result.Message = msg;
            }

            return result;
        }

        public ServiceResult<List<PartnerProject>> FindMatches(List<PartnerProject> Projects, double MaxDistance = Double.MaxValue)
        {
            var result = new ServiceResult<List<PartnerProject>>();

            var potentials = new List<PartnerProject>();

            try
            {
                PartnerProject groupLeaderProject = Projects.FirstOrDefault(x => this.MemberIds.Contains(x.SiteLeaderId));

                if (groupLeaderProject != null)
                {
                    potentials.Add(groupLeaderProject);
                }
                else
                {
                    var projects = new List<PartnerProject>();

                    if (this.SiteLeaderId.HasValue)
                    {
                        projects = Projects.Where(x => x.HomeCampusId == this.HomeCampusId && x.SetTargetCampus(this.HomeCampus)).ToList();
                    }
                    else
                    {
                        projects = Projects.Where(x => x.SetTargetCampus(this.HomeCampus)).ToList();
                    }

                    if (projects.Any())
                    {
                        projects = projects.Where(x => x.DistanceToTarget < MaxDistance).ToList();

                        if (projects.Any())
                        {
                            projects = projects.Where(x => x.AbilityLevel >= this.AbilityLevel).ToList();

                            if (projects.Any())
                            {
                                projects = projects.Where(x => x.FamilyFriendliness <= this.FamilyFriendliness).ToList();

                                if (projects.Any())
                                {
                                    projects = projects.Where(x => x.HasServingCapacity(this)).ToList();

                                    if (projects.Any())
                                    {
                                        potentials = projects.OrderBy(x=> x.SiteLeaderId)
                                        .ThenBy(x => x.TotalVolunteers)
                                        .ThenByDescending(x => x.FamilyFriendliness)
                                        .ThenBy(x => x.AbilityLevel)
                                        .ThenBy(x => x.DistanceToTarget)
                                        .ThenBy(x => (x.HomeCampusId == this.HomeCampusId ? 0 : 1))
                                        .ToList();

                                        result.Success = true;
                                    }
                                    else
                                    {
                                        result.Message = "No projects after x.HasServingCapacity(this)";
                                    }
                                }
                                else
                                {
                                    result.Message = "No projects after FamilyFriendliness <= this.FamilyFriendliness";
                                }
                            }
                            else
                            {
                                result.Message = "No projects after AbilityLevel >= this.AbilityLevel";
                            }
                        }
                        else
                        {
                            result.Message = "No projects after DistanceToTarget < MaxDistance";
                        }
                    }
                    else
                    {
                        result.Message = "No projects after SetTargetCampus Team Campus: " + this.HomeCampus;
                    }

                    //potentials = Projects.Where(x => x.SetTargetCampus(this.HomeCampus)
                    //            && x.DistanceToTarget < MaxDistance
                    //            && x.AbilityLevel >= this.AbilityLevel
                    //            && x.FamilyFriendliness <= this.FamilyFriendliness
                    //            && x.HasServingCapacity(this))
                    //                .OrderBy(x => x.TotalVolunteers)
                    //                .ThenByDescending(x => x.FamilyFriendliness)
                    //                .ThenBy(x => x.AbilityLevel)
                    //                .ThenBy(x => x.DistanceToTarget)
                    //                .ToList();
                }
            }
            catch (Exception e)
            {
                var msg = String.Format("Error matching group {0}: {1}! \r\n Message: {2} \r\n Stack: {3}", this.ID, this.Name, e.Message, e.StackTrace);
                result.Message = msg;
            }

            result.ResponseObject = potentials;

            return result;
        }
    }

    [Serializable]
    public enum NodeType
    {
        project,
        team,
        projectDrop,
        teamDrop
    }

    [Serializable]
    public class Node
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int familyFriendly { get; set; }
        public string familyFriendlyDesc { get; set; }
        public int ability { get; set; }
        public string abilityDesc { get; set; }
        public string nodeType { get; set; }
        public decimal count { get; set; }
        public NodeType actualType { get; set; }
        public List<Node> nodes { get; set; }
        public bool hasSiteLeader { get; set; }
        public string shift { get; set; }

        public Node()
        {
            nodes = new List<Node>();
        }

        public static Node GetNodesFromProjectGroup(PartnerProject project)
        {
            var shiftStr = "??";

            if(project.Shifts != null && project.Shifts.Keys.Any())
            {
                shiftStr = project.Shifts.Keys.First() == ServingShift.SaturdayAM ? "Sa" : "Su";


            }

            var node = new Node()
            {
                id = project.ID,
                title = project.Name,
                description = project.Name.Length > 18 ? project.Name.Substring(0, 18) : project.Name,
                familyFriendly = (int)project.FamilyFriendliness,
                familyFriendlyDesc = project.FamilyFriendliness.DescriptionAttr(),
                ability = (int)project.AbilityLevel,
                abilityDesc = project.AbilityLevel.DescriptionAttr(),
                nodeType = NodeType.project.ToString(),
                actualType = NodeType.project,
                count = project.Shifts.Values.Sum(),
                nodes = project.AssignedTeams.Select(x => Node.GetNodesFromVolunteerGroup(x)).ToList(),
                hasSiteLeader = project.SiteLeaderId > 0,
                shift = shiftStr
            };

            return node;
        }

        public static Node GetNodesFromVolunteerGroup(VolunteerGroup group)
        {

            var shiftStr = "??";

            if (group.Shifts != null && group.Shifts.Any())
            {
                shiftStr = group.Shifts.First() == ServingShift.SaturdayAM ? "Sa" : "Su";


            }

            var node = new Node()
            {
                id = group.ID,
                title = group.Name,
                description = group.Name.Length > 18 ? group.Name.Substring(0, 18) : group.Name,
                familyFriendly = (int)group.FamilyFriendliness,
                familyFriendlyDesc = group.FamilyFriendliness.DescriptionAttr(),
                ability = (int)group.AbilityLevel,
                abilityDesc = group.AbilityLevel.DescriptionAttr(),
                nodeType = NodeType.team.ToString(),
                count = group.VolunteerCount,
                actualType = NodeType.team,
                hasSiteLeader = group.SiteLeaderId.HasValue,
                shift = shiftStr
            };

            return node;
        }
    }
}
