<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GNWDashboard.ascx.cs" Inherits="Plugins_Northview_GNW_GNWDashboard" %>
<%@ Import Namespace="us.northviewchurch.Model.GNW" %> 
<%@ Import Namespace="RockWeb" %> 

<script src="/Scripts/d3-cloud/d3.min.js"></script>
<script src="/Scripts/GNW/2019/Dashboard/dashboard.js"></script>
<script src="/Scripts/stupidtable.js"></script>
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

.onoffswitch {
    position: relative; width: 90px;
    -webkit-user-select:none; -moz-user-select:none; -ms-user-select: none;
}
.onoffswitch-checkbox {
    display: none;
}
.onoffswitch-label {
    display: block; overflow: hidden; cursor: pointer;
    border: 2px solid #999999; border-radius: 20px;
}
.onoffswitch-inner {
    display: block; width: 200%; margin-left: -100%;
    transition: margin 0.3s ease-in 0s;
}
.onoffswitch-inner:before, .onoffswitch-inner:after {
    display: block; float: left; width: 50%; height: 30px; padding: 0; line-height: 30px;
    font-size: 14px; color: white; font-family: Trebuchet, Arial, sans-serif; font-weight: bold;
    box-sizing: border-box;
}
.onoffswitch-inner:before {
    content: "ON";
    padding-left: 10px;
    background-color: #34A7C1; color: #FFFFFF;
}
.onoffswitch-inner:after {
    content: "OFF";
    padding-right: 10px;
    background-color: #EEEEEE; color: #999999;
    text-align: right;
}
.onoffswitch-switch {
    display: block; width: 18px; margin: 6px;
    background: #FFFFFF;
    position: absolute; top: 0; bottom: 0;
    right: 56px;
    border: 2px solid #999999; border-radius: 20px;
    transition: all 0.3s ease-in 0s; 
}
.onoffswitch-checkbox:checked + .onoffswitch-label .onoffswitch-inner {
    margin-left: 0;
}
.onoffswitch-checkbox:checked + .onoffswitch-label .onoffswitch-switch {
    right: 0px; 
}

</style>

<script>

    var refreshPage = true;

    function cache_clear() {

        if (refreshPage) {
             window.location.reload(true);
            // window.location.reload(); use this if you do not remove cache
        }       
    };

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
            $('.nav-tabs a[href="#info-<% Response.Write(this._displayMode); %>"]').tab('show');
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

        $('.onoffswitch').on('click', function (event) {
            refreshPage = !refreshPage;
        });

         setInterval(function() {
            cache_clear()
          }, pageRefreshRate);
    });
</script>

<h2>Good Neighbor Weekend Dashboard</h2>

<div>
    <label>Auto Page Refresh</label>
    <div class="onoffswitch">
    <input type="checkbox" name="onoffswitch" class="onoffswitch-checkbox" id="switchRefresh" checked>
    <label class="onoffswitch-label" for="switchRefresh">
        <span class="onoffswitch-inner"></span>
        <span class="onoffswitch-switch"></span>
    </label>
</div>
</div>

<div style="overflow-x:scroll;">
    <% var ulWidth =   this._activeCampuses.Where(x=> x.Included).Count() * 200;   %>
 <ul class="nav nav-tabs" id="ulTabContainer" style="width: <% Response.Write(ulWidth); %>px;"  >
     <% foreach (var campus in this._activeCampuses.Where(x=> x.Included))
      {
         var trimmedName = campus.Name.Replace(" ", "");
    %>
        <li><a data-toggle="tab" data-campusid="<% Response.Write(campus.ID); %>" href="#info-<% Response.Write(trimmedName); %>"><h3><% Response.Write(campus.Name); %></h3><span id="thermo-<% Response.Write(trimmedName); %>"></span></a></li>
     <% } %>

  </ul>
</div>
<div style="display:none;">
    <textarea TextMode="MultiLine" Rows="10" id="txtDebugLog" runat="server" style="width:100%;" ></textarea>
</div>

  <div class="tab-content" id="divContentContainer" runat="server">  
     <% foreach (var campus in this._activeCampuses.Where(x=> x.Included))
         {
             var trimmedName = campus.Name.Replace(" ", "");
             var displayName = String.Format("Data For {0}{1} {2}", campus.ID == -1 ? "" : "The ", campus.Name, campus.ID == -1 ? "" : "Campus" );
    %>
         <div id="info-<% Response.Write(trimmedName); %>" data-campusid="<% Response.Write(campus.ID); %>" class="tab-pane fade">
              <h3><% Response.Write(displayName); %></h3>
              <div>
                  <div class="col-sm-2">
                       <div class="projects">
                            <p class="counter-count"><% Response.Write(campus.TotalProjects); %></p>
                            <p class="projects-p">Projects</p>
                        </div>
                  </div>
                   <div class="col-sm-2">
                       <div class="volunteers">
                            <p class="counter-count"><% Response.Write(campus.TotalVolunteers); %></p>
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
                            <p class="counter-count <% Response.Write(addtlClass); %>"><% Response.Write(campus.TotalRemainingVolunteerCapacity); %></p>
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
                                    <% foreach(var shift in Enum.GetValues(typeof(ServingShift))) 
                                        { %>
                                            <th data-sort="int">Needed <% Response.Write(((ServingShift)shift).DescriptionAttr());%></th>
                                        <%}
                                        %>
                                    
                                </tr>
                            </thead>
                            <tbody>
                                <% foreach (var project in campus.Projects)
                                { %>
                                    <tr>                                    
                                        <td><a href="<% Response.Write(String.Format("/{0}?GroupId={1}", this._detailsUrl, project.ID.ToString())); %>" ><% Response.Write(project.Name); %></a></td>
                                        <td><%= project.VolunteerCapacity %></td>
                                        <td><%= project.TotalVolunteers %></td>
                                         <% foreach(var shiftVal in Enum.GetValues(typeof(ServingShift)))
                                             {
                                                 var shift = (ServingShift)shiftVal;
                                                 if (project.Shifts.ContainsKey(shift))
                                                 {
                                                     var lineWidth = ((project.VolunteerCapacity - project.Shifts[shift]) / project.VolunteerCapacity) * 100;
                                                     if(lineWidth > 90.0M)
                                                     {
                                                         lineWidth = 90.0M;
                                                     }

                                                     %>
                                                     <td><span style="display:inline-block; background-color:#00b3e7; margin-right: 5px; width:<% Response.Write(lineWidth);  %>%;" >&nbsp;</span><% Response.Write(project.Shifts[shift]);  %></td>
                                                <% }
                                                else
                                                { %>
                                                    <td><span style="display:inline-block; background-color:#00b3e7; margin-right: 5px; width:90%;" >&nbsp;</span> 0</td>
                                              <%  }
                                        %>
                                           
                                        <%}
                                        %>
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