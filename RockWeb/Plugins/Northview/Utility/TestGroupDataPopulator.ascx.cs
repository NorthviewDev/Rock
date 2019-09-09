using Newtonsoft.Json;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

[DisplayName("Test Group Data Populator")]
[Category("Northview > Utilities")]
[Description("Iterates through a hierarchy of groups and ensures that fields of test data are populated")]
[IntegerField("Group EntityType ID", "The ID of the Group EntityTypes", true, 16, key: "GroupEntityTypeId")]
public partial class Plugins_Northview_Utility_TestGroupDataPopulator : Rock.Web.UI.RockBlock
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            loadData();
        } 
    }

    protected void loadData()
    {
        var fieldTypeSelectOptions = new List<String>();

        var rockCtx = new Rock.Data.RockContext();
        
        fieldTypeSelectOptions = FieldTypeCache.All().Select(x=> String.Format("<option value='{0}'>{1}</option>", x.Id, x.Name)).ToList();

        ddlGroupType.DataSource = GroupTypeCache.All().Select(x => new KeyValuePair<int, string>(x.Id, x.Name));
        ddlGroupType.DataValueField = "Key";
        ddlGroupType.DataTextField = "Value";
        ddlGroupType.DataBind();

        var campusSvc = new CampusService(rockCtx);

        var activeCampuses = campusSvc.Queryable().Where(x => x.IsActive ?? false).ToList().Select(x => new KeyValuePair<int, string>(x.Id, x.Name)).ToList();

        this.ddlCampuses.DataValueField = "Key";
        this.ddlCampuses.DataTextField = "Value";
        this.ddlCampuses.DataSource = activeCampuses.OrderBy(x => x.Key);
        this.ddlCampuses.DataBind();

        var sb = new StringBuilder();
        sb.Append(String.Format(@"
        var fieldTypeSelectOptions = ""{0}"";        
        ", String.Join("", fieldTypeSelectOptions)));

        //create script control
        var objScript = new HtmlGenericControl("script");
        //add javascript type
        objScript.Attributes.Add("type", "text/javascript");
        //set innerHTML to be our StringBuilder string
        objScript.InnerHtml = sb.ToString();

        //add script to PlaceHolder control
        this.placeHldrJS.Controls.Add(objScript);
    }

    protected void btnPopulateData_Click(object sender, EventArgs e)
    {
        var rockCtx = new Rock.Data.RockContext();

        var groupSvc = new GroupService(rockCtx);

        var attributeSvc = new AttributeService(rockCtx);
        var fieldTypeSvc = new FieldTypeService(rockCtx);
        var attrValueSvc = new AttributeValueService(rockCtx);
        var entityTypeSvc = new EntityTypeService(rockCtx);

        var serializedData = this.hdnSerializedAttrData.Value;

        var attributeData = JsonConvert.DeserializeObject<List<AttributeData>>(serializedData);

        attributeData.ForEach(x => x.ValueProvider = ValueProvider.CreateForAttribute(x));

        var parentGroupId = grpPicker.GroupId ?? -1;
        var groupType = Int32.Parse(this.ddlGroupType.SelectedValue);

        var campusId = Int32.Parse(this.ddlCampuses.SelectedValue);

        var groups = groupSvc.GetAllDescendents(parentGroupId).Where(x => x.GroupTypeId == groupType).ToList();

        foreach(var grp in groups)
        {
            if(!grp.CampusId.HasValue || grp.CampusId != campusId)
            {
                grp.CampusId = campusId;

                rockCtx.SaveChanges();
            }

            foreach (var attrData in attributeData)
            {
                var attr = attributeSvc.Queryable().FirstOrDefault(t => t.Key == attrData.Key);
                var attrValue = attrValueSvc.Queryable().Where(t => t.EntityId == grp.Id).ToList().FirstOrDefault(x => x.AttributeKey == attrData.Key);

                if (attr == null)
                {
                    var fieldType = fieldTypeSvc.Get(attrData.FieldTypeId);
                    var entityType = entityTypeSvc.Get(Int32.Parse(GetAttributeValue("GroupEntityTypeId")));

                    attr = new Rock.Model.Attribute()
                    {
                        Name = attrData.Key,
                        Key = attrData.Key,
                        FieldType = fieldType,
                        EntityType = entityType                        
                    };

                    attributeSvc.Add(attr);
                    rockCtx.SaveChanges();
                }

                if (attrValue == null)
                {
                    attrValue = new Rock.Model.AttributeValue()
                    {
                        Attribute = attr,
                        Value = attrData.ValueProvider.Next(),
                        EntityId = grp.Id
                    };

                    attrValueSvc.Add(attrValue);
                }
                else if(String.IsNullOrWhiteSpace(attrValue.Value))
                {
                    attrValue.Value = attrData.ValueProvider.Next();
                }

                rockCtx.SaveChanges(); 
            }
        }
    }
}

public enum ValueTypes
{
    None,
    Sequence,    
    Range
}

public static class ValueTypesExtensions
{
    public static ValueTypes ParseValueType(this string values)
    {
        var type = ValueTypes.None;

        if(values.Contains(","))
        {
            type = ValueTypes.Sequence;
        }        
        else if (values.Contains("-"))
        {
            type = ValueTypes.Range;
        }

        return type;
    }
}

public class ValueProvider
{
    public ValueTypes ValueType { get; set; }
    public List<string> Inputs { get; set; }

    public ValueProvider()
    {
        Inputs = new List<string>();
    }

    public string Next()
    {
        var val = "";

        switch (ValueType)
        {            
            case ValueTypes.Sequence:

                var maxIndex = this.Inputs.Count;

                var index = new Random().Next(0, maxIndex);

                val = Inputs[index];

                break;           
            case ValueTypes.Range:

                var min = Int32.Parse(Inputs[0]);
                var max = Int32.Parse(Inputs[1]);

                val = new Random().Next(min, max).ToString();

                break;
            default:
            case ValueTypes.None:
                break;
        }

        return val;
    }

    public static ValueProvider CreateForAttribute(AttributeData attr)
    {        
        var inputs = new List<string>();

        var type = attr.AcceptableValues.ParseValueType();

        if (type == ValueTypes.Sequence)
        {
            inputs.AddRange(attr.AcceptableValues.Split(new string[] { "," },StringSplitOptions.RemoveEmptyEntries));

        }
        else if (type != ValueTypes.None)
        {
            inputs.AddRange(attr.AcceptableValues.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries));

        }

        return new ValueProvider()
        {
            ValueType = type,
            Inputs = inputs
        };
    }
} 

public class AttributeData
{
    public string Key { get; set; }
    public string FieldTypeName { get; set; }
    public int FieldTypeId { get; set; }
    public string AcceptableValues { get; set; }

    public ValueProvider ValueProvider { get; set; }
}