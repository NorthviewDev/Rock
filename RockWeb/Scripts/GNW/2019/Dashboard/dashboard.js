function renderThermometer(appendId, curVolunteers, totalPopulation) {
    var width = 80,
        height = 180,
        maxTemp = 100.0,
        minTemp = 0,
        currentTemp = curVolunteers;

    var bottomY = height - 5,
        topY = 5,
        bulbRadius = 20,
        tubeWidth = 21.5,
        tubeBorderWidth = 1,
        mercuryColor = "rgb(230,0,0)",
        innerBulbColor = "rgb(230, 200, 200)"
    tubeBorderColor = "#999999";

    var bulb_cy = bottomY - bulbRadius,
        bulb_cx = width / 2,
        top_cy = topY + tubeWidth / 2;

    var margin = { top: 20, right: 20, bottom: 50, left: 70 };

    var svg = d3.select("#thermo-" + appendId)
        .append("svg")
        .attr("width", width)
        .attr("height", height);


    var defs = svg.append("defs");

    // Define the radial gradient for the bulb fill colour
    var bulbGradient = defs.append("radialGradient")
        .attr("id", "bulbGradient-" + appendId)
        .attr("cx", "50%")
        .attr("cy", "50%")
        .attr("r", "50%")
        .attr("fx", "50%")
        .attr("fy", "50%");

    bulbGradient.append("stop")
        .attr("offset", "0%")
        .style("stop-color", innerBulbColor);

    bulbGradient.append("stop")
        .attr("offset", "90%")
        .style("stop-color", mercuryColor);

    // Circle element for rounded tube top
    svg.append("circle")
        .attr("r", tubeWidth / 2)
        .attr("cx", width / 2)
        .attr("cy", top_cy)
        .style("fill", "#FFFFFF")
        .style("stroke", tubeBorderColor)
        .style("stroke-width", tubeBorderWidth + "px");


    // Rect element for tube
    svg.append("rect")
        .attr("x", width / 2 - tubeWidth / 2)
        .attr("y", top_cy)
        .attr("height", bulb_cy - top_cy)
        .attr("width", tubeWidth)
        .style("shape-rendering", "crispEdges")
        .style("fill", "#FFFFFF")
        .style("stroke", tubeBorderColor)
        .style("stroke-width", tubeBorderWidth + "px");


    // White fill for rounded tube top circle element
    // to hide the border at the top of the tube rect element
    svg.append("circle")
        .attr("r", tubeWidth / 2 - tubeBorderWidth / 2)
        .attr("cx", width / 2)
        .attr("cy", top_cy)
        .style("fill", "#FFFFFF")
        .style("stroke", "none")


    // Main bulb of thermometer (empty), white fill
    svg.append("circle")
        .attr("r", bulbRadius)
        .attr("cx", bulb_cx)
        .attr("cy", bulb_cy)
        .style("fill", "#FFFFFF")
        .style("stroke", tubeBorderColor)
        .style("stroke-width", tubeBorderWidth + "px");


    // Rect element for tube fill colour
    svg.append("rect")
        .attr("x", width / 2 - (tubeWidth - tubeBorderWidth) / 2)
        .attr("y", top_cy)
        .attr("height", bulb_cy - top_cy)
        .attr("width", tubeWidth - tubeBorderWidth)
        .style("shape-rendering", "crispEdges")
        .style("fill", "#FFFFFF")
        .style("stroke", "none");


    // Scale step size
    var step = 5;

    // Determine a suitable range of the temperature scale
    var domain = [
        step * Math.floor(minTemp / step),
        step * Math.ceil(maxTemp / step)
    ];

    if (minTemp - domain[0] < 0.66 * step)
        domain[0] -= step;

    if (domain[1] - maxTemp < 0.66 * step)
        domain[1] += step;


    // D3 scale object
    var scale = d3.scaleLinear()
        .range([bulb_cy - bulbRadius / 2 - 8.5, top_cy])
        .domain(domain);


    // Max and min temperature lines
    [minTemp, maxTemp].forEach(function (t) {

        var isMax = (t == maxTemp),
            label = (isMax ? totalPopulation : minTemp),
            textCol = (isMax ? "rgb(230, 0, 0)" : "rgb(0, 0, 230)"),
            textOffset = (isMax ? -4 : 4);

        svg.append("line")
            .attr("id", label + "Line-" + appendId)
            .attr("x1", width / 2 - tubeWidth / 2)
            .attr("x2", width / 2 + tubeWidth / 2 + 22)
            .attr("y1", scale(t))
            .attr("y2", scale(t))
            .style("stroke", tubeBorderColor)
            .style("stroke-width", "1px")
            .style("shape-rendering", "crispEdges");

        svg.append("text")
            .attr("x", width / 2 + tubeWidth / 2 + 2)
            .attr("y", scale(t) + textOffset)
            .attr("dy", isMax ? null : "0.75em")
            .text(label)
            .style("fill", textCol)
            .style("font-size", "11px")

    });


    var tubeFill_bottom = bulb_cy,
        tubeFill_top = scale(currentTemp);

    // Rect element for the red mercury column
    svg.append("rect")
        .attr("x", width / 2 - (tubeWidth - 10) / 2)
        .attr("y", tubeFill_top)
        .attr("width", tubeWidth - 10)
        .attr("height", tubeFill_bottom - tubeFill_top)
        .style("shape-rendering", "crispEdges")
        .style("fill", mercuryColor)


    // Main thermometer bulb fill
    svg.append("circle")
        .attr("r", bulbRadius - 6)
        .attr("cx", bulb_cx)
        .attr("cy", bulb_cy)
        .style("fill", "url(#bulbGradient-" + appendId+")")
        .style("stroke", mercuryColor)
        .style("stroke-width", "2px");
};	

function renderProgressBar(appendId, curVolunteers, neededVolunteers) {

    //set up svg using margin conventions - we'll need plenty of room on the left for labels
    var margin = {
        top: 15,
        right: 25,
        bottom: 15,
        left: 15
    };

    var width = 960 - margin.left - margin.right,
        height = 500 - margin.top - margin.bottom;

    var svg = d3.select("#graphic").append("svg")
        .attr("width", width + margin.left + margin.right)
        .attr("height", height + margin.top + margin.bottom)
        .append("g")
        .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

    var x = d3.scale.linear()
        .range([0, width])
        .domain([0, d3.max(data, function (d) {
            return d.value;
        })]);

    var y = d3.scale.ordinal()
        .rangeRoundBands([height, 0], .1)
        .domain(data.map(function (d) {
            return d.name;
        }));

    //make y axis to show bar names
    var yAxis = d3.svg.axis()
        .scale(y)
        //no tick marks
        .tickSize(0)
        .orient("left");

    var gy = svg.append("g")
        .attr("class", "y axis")
        .call(yAxis)

    var bars = svg.selectAll(".bar")
        .data(data)
        .enter()
        .append("g")

    //append rects
    bars.append("rect")
        .attr("class", "bar")
        .attr("y", function (d) {
            return y(d.name);
        })
        .attr("height", y.rangeBand())
        .attr("x", 0)
        .attr("width", function (d) {
            return x(d.value);
        });

    //add a value label to the right of each bar
    bars.append("text")
        .attr("class", "label")
        //y position of the label is halfway down the bar
        .attr("y", function (d) {
            return y(d.name) + y.rangeBand() / 2 + 4;
        })
        //x position is 3 pixels to the right of the bar
        .attr("x", function (d) {
            return x(d.value) + 3;
        })
        .text(function (d) {
            return d.value;
        });

};