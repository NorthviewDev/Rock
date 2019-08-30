<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GNWDashboard.ascx.cs" Inherits="Plugins_Northview_GNW_GNWDashboard" %>
<script src="/Scripts/d3-cloud/d3.min.js"></script>
<script src="/Scripts/GNW/2019/Dashboard/dashboard.js"></script>
<script src="/Scripts/GNW/2019/Dashboard/stupidtable.js"></script>
<asp:PlaceHolder ID="placeHldrNodesJS" runat="server" />

<style>
.counter
{
    background-color: #eaecf0;
    text-align: center;
}
.projects,.volunteers,.needed-volunteers,.order
{
    margin-top: 70px;
    margin-bottom: 70px;
}
.counter-count
{
    font-size: 18px;
    background-color: #00b3e7;
    border-radius: 50%;
    position: relative;
    color: #ffffff;
    text-align: center;
    line-height: 92px;
    width: 92px;
    height: 92px;
    -webkit-border-radius: 50%;
    -moz-border-radius: 50%;
    -ms-border-radius: 50%;
    -o-border-radius: 50%;
    display: inline-block;
}

.counter-count-wrn
{
    background-color: orange;
}

.counter-count-crt
{
    background-color: red;
}

.projects-p,.volunteers-p,.needed-volunteers-p
{
    font-size: 24px;
    color: #000000;
    line-height: 34px;
}

li.active{
    border-style:dashed;
}

 table {
    border-collapse: collapse;
}

table a:not(.btn), .table a:not(.btn) {
    color: #2a9fd6;
    text-decoration: underline;
}

th, td {
    padding: 5px 10px;
    border: 1px solid #999;
} 

th[data-sort] {
    cursor: pointer;
}

tr.awesome {
    color: red;
}

#msg {
    color: green;
}

</style>

<script>

    var moveBlanks = function (a, b) {
        if (a < b) {
            if (a == "")
                return 1;
            else
                return -1;
        }
        if (a > b) {
            if (b == "")
                return -1;
            else
                return 1;
        }
        return 0;
    };
    var moveBlanksDesc = function (a, b) {
        // Blanks are by definition the smallest value, so we don't have to
        // worry about them here
        if (a < b)
            return 1;
        if (a > b)
            return -1;
        return 0;
    };

    $(document).ready(function () {
        //renderThermometer('MainCampus', 39.6, 800);
        //renderThermometer('Carmel', 19.6, 3800);
        //renderThermometer('Anderson', 89.6, 1800);

        <%foreach(var cmd in this._thermometerRenderStrings)
        {%>
            <%= cmd %>
         <%}%>

        $(".proj-table").stupidtable({            
            "moveBlanks": moveBlanks,
            "moveBlanksDesc": moveBlanksDesc,
        });

        <% if (String.IsNullOrWhiteSpace(this._displayMode) || this._displayMode == "All")
        { %>
            $('.nav-tabs a:first').tab('show');
        <% } else { %>
            $('.nav-tabs a[href="#info-<%= this._displayMode %>"]').tab('show');
        <% }%>

       

        $('.nav-tabs a').on('shown.bs.tab', function(event){
             $($(".tab-pane.active .counter-count")).each(function () {
                $(this).prop('Counter',0).animate({
                    Counter: $(this).text()
                }, {
                    duration: 1500,
                    easing: 'swing',
                    step: function (now) {
                        $(this).text(Math.ceil(now));
                    }
                });
            });
        });
    });
</script>

<h2>Good Neighbor Weekend Dashboard</h2>
 <ul class="nav nav-tabs" id="ulTabContainer" runat="server" >  
     <% foreach (var campus in this._activeCampuses)
      {
         var trimmedName = campus.Name.Replace(" ", "");
    %>
        <li><a data-toggle="tab" data-campusid="<%= campus.ID %>" href="#info-<%= trimmedName %>"><h3><%= campus.Name %></h3><span id="thermo-<%= trimmedName %>"></span></a></li>
     <% } %>

  </ul>

  <div class="tab-content" id="divContentContainer" runat="server">  
     <% foreach (var campus in this._activeCampuses)
         {
             var trimmedName = campus.Name.Replace(" ", "");
             var displayName = String.Format("Data For {0}{1} {2}", campus.ID == -1 ? "" : "The ", campus.Name, campus.ID == -1 ? "" : "Campus" );
    %>
         <div id="info-<%= trimmedName %>" data-campusid="<%= campus.ID %>" class="tab-pane fade">
              <h3><%= displayName %></h3>
              <div>
                  <div class="col-sm-2">
                       <div class="projects">
                            <p class="counter-count"><%= campus.TotalProjects %></p>
                            <p class="projects-p">Projects</p>
                        </div>
                  </div>
                   <div class="col-sm-2">
                       <div class="volunteers">
                            <p class="counter-count"><%= campus.TotalVolunteers %></p>
                            <p class="volunteers-p">Total Volunteers</p>
                        </div>
                  </div>
                   <div class="col-sm-8">
                       <div class="needed-volunteers"> 
                            <%
                                var addtlClass = "";
                                var volRatio = campus.TotalRequiredVolunteers == 0 ? 0.0M : campus.TotalRemainingVolunteerCapacity / campus.TotalRequiredVolunteers;
                                if(volRatio > 0M && volRatio <= .2M)
                                {
                                    addtlClass = "counter-count-wrn";
                                }
                                else if (volRatio >.2M)
                                {
                                     addtlClass = "counter-count-crt";
                                }
                            %>
                            <p class="counter-count <%= addtlClass %>"><%= campus.TotalRemainingVolunteerCapacity %></p>
                            <p class="needed-volunteers-p">Volunteers Needed</p>
                        </div>
                  </div>
              </div>
              <% if(this._displayMode == "All")
              {%>
                 <div>
                    <h3>Project List</h3>
                    <div>
                     <% if(campus.Projects.Count == 0)
                      {%>
                           <h4>No Projects For This Campus</h4>
                        <% }else {%>
                         <table class="proj-table">
                            <thead>
                                <tr>
                                    <th data-sort="string">Name</th>
                                    <th data-sort="int">Max Capacity</th>
                                    <th data-sort="int">Volunteers</th>
                                    <th data-sort="int">Needed</th>
                                </tr>
                            </thead>
                            <tbody>
                                <% foreach (var project in campus.Projects)
                                { %>
                                    <tr>                                    
                                        <td><a href="<%= String.Format("/{0}?GroupId={1}", this._detailsUrl,project.ID.ToString()) %>" ><%= project.Name %></a></td>
                                        <td><%= project.VolunteerCapacity %></td>
                                        <td><%= project.TotalVolunteers %></td>
                                        <td><%= project.RemainingCapacity %></td>
                                     </tr>
                               <% }  %>
                  
                            </tbody>
                        </table>
                        <%} %>
                    </div>
                 </div>
            <% }%>
            </div>
     <% } %>
  </div>