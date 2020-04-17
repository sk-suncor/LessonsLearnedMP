/// <reference path="http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.7.1-vsdoc.js" />

$(function () {
    //Year Drop down list styling
    $('#Year').selectmenu();

    //Draw the two plots
    UpdateOpenLessonsPlot();
    UpdateClosedLessonsPlot();

    //Update the closed lesson plot when the year dropdown is changed
    //or the lesson type radio is changed
    $('#Year, #IsValidSelected').change(function(){
       UpdateClosedLessonsPlot(); 
    });

    // Bind a listener to the "jqplotDataClick" event.
    // We set the correct search values based on the clicked plot and submit to the search form
    $('#ClosedLessonsChart').bind('jqplotDataClick', function (ev, seriesIndex, pointIndex, data) {
        SearchClosedLessons(series[seriesIndex].lessonTypeId, pointIndex);
    });

    // Bind a listener to the "jqplotDataClick" event.
    // We set the correct search values based on the clicked plot and submit to the search form
    $('#OpenLessonsPie').bind('jqplotDataClick', function (ev, seriesIndex, pointIndex, data) {
        SearchOpenLessons(data[2]);
    });

    //Search the specified lesson status when the legend is clicked
    $('.open-lesson-legend').live('click', function() {
        SearchOpenLessons($(this).attr('data-statusId'));
        return false;
    });

    //Search the specified lesson status when the legend is clicked
    $('.closed-lesson-legend').live('click', function() {
        SearchClosedLessons($(this).attr('data-lessonTypeId'));
        return false;
    });
});

function SearchClosedLessons(lessonType, month) {
    var $lessonTypeValidInput = $('<input id="LessonTypeValidId" name="LessonTypeValidId" value="" type="hidden" />');
    var $lessonTypeInvalidInput = $('<input id="LessonTypeInvalidId" name="LessonTypeInvalidId" value="" type="hidden" />');
    var $typeRadios = $('input:radio[name=IsValidSelected]');

    if ($typeRadios.filter('[value=False]').attr('checked')) {
        $lessonTypeInvalidInput.val(lessonType);
    }
    else {
        $lessonTypeValidInput.val(lessonType);
    }

    var year = $('#Year').val();
    var fromDate = new Date(year, 0, 1);
    var toDate = new Date(parseInt(year) + 1, 0, 0);

    if(typeof month !== "undefined")
    {
        fromDate = new Date(year, month, 1);
        toDate = new Date(year, month + 1, 0);
    }

    var $beginDateInput = $('<input id="ClosedDateBegin" name="ClosedDateBegin" value="" type="hidden" />');
    var $endDateInput = $('<input id="ClosedDateEnd" name="ClosedDateEnd" value="" type="hidden" />');
    var $ownedLessonsInput = $('<input id="ShowOnlyOwnedLessons" name="ShowOnlyOwnedLessons" value="False" type="hidden" />');
    var $statusInput = $('<input id="Status" name="Status" value="6" type="hidden" />');

    $beginDateInput.val(fromDate.format("yyyy-MM-dd"));

    if(year == 0) {
        //Search for everything Pre-2010
        toDate = new Date(2009, 12, 0);
        $beginDateInput.val('');
    }

    $endDateInput.val(toDate.format("yyyy-MM-dd"));
    $ownedLessonsInput.val((admin ? "False" : "True"));

    var $form = $('<form action="/Lesson/MyLessons" method="post"></form>');

    $form.append($beginDateInput);
    $form.append($endDateInput);
    $form.append($ownedLessonsInput);
    $form.append($statusInput);
    $form.append($lessonTypeValidInput);
    $form.append($lessonTypeInvalidInput);

    $('body').append($form);

    $form.submit();
}

function SearchOpenLessons(lessonStatus) {
    var $statusInput = $('<input id="Status" name="Status" value="" type="hidden" />');
    $statusInput.val(lessonStatus);

    var $form = $('<form action="/Lesson/MyLessons" method="post"></form>');

    $form.append($statusInput);
    $('body').append($form);
    $form.submit();
}

//variables to hold the plot objects
var openLessonsPlot;
var closedLessonsPlot;

//Setup and draw the Open Lessons Pie Chart
function UpdateOpenLessonsPlot() {
    if (openLessonsPlot) {
        openLessonsPlot.destroy();
    }

    $.ajax({
        url: siteRoot + "Dashboard/GetOpenLessonsChartData",
        type: "POST",
        data: { noCache: Date() },
        success: function (result) {
            var data = [];
            var dataLabels = [];
            var seriesColors = [];
            var highlightColors = [];
            var legendLabels = [];

            var totalOpenLessons = 0;

            $(result).each(function () {
                var itemArray = [this.Label, this.Percent, this.StatusId];
                data.push(itemArray);

                totalOpenLessons += this.Count;

                //create an element to extract the correct color for this status
                var colorClass = $('<div class="status-' + this.StatusId + '"></div>').hide().appendTo('body');
                var textColor = 'Black';
                var pieSliceColor = colorClass.css('background-color');
                colorClass.remove();

                //create an element to extract the correct highlight color for this status
                var highlightColorClass = $('<div class="status-highlight-' + this.StatusId + '"></div>').hide().appendTo('body');
                var pieSliceHighlightColor = highlightColorClass.css('background-color');
                highlightColorClass.remove();

                var labelHtml = '<div style="color:' + textColor + '; width:75px">' + this.Label + ' ' + this.Percent + '%</div>';

                dataLabels.push(labelHtml);
                seriesColors.push(pieSliceColor);
                highlightColors.push(pieSliceHighlightColor);
                legendLabels.push('<a href="#" data-statusId="' + this.StatusId + '" class="open-lesson-legend">' + this.Label + ' (' + this.Count + ')</a>');
            });

            openLessonsPlot = jQuery.jqplot('OpenLessonsPie', [data],
            {
                sortData: false,
                seriesDefaults: {
                    // Make this a pie chart.
                    renderer: jQuery.jqplot.PieRenderer,
                    rendererOptions: {
                        // Put data labels on the pie slices.
                        // By default, labels show the percentage of the slice.
                        showDataLabels: true,
                        dataLabels: dataLabels,
                        sliceMargin: 4,
                        highlightColors: highlightColors
                    },
                    seriesColors: seriesColors
                },
                legend: { 
                    show: true,
                    labels: legendLabels,
                    location: 'ne',
                    placement: 'outsideGrid'
                },
                noDataIndicator: { show: true, indicator: 'There are no open lessons.' }
            })

            $('.total-open-lessons').html(totalOpenLessons);
        }
    });
}

var series = [];

//Setup and draw the Closed Lesson Chart
function UpdateClosedLessonsPlot() {
    if(closedLessonsPlot) {
        closedLessonsPlot.destroy();
    }

    var isValidSelected = $("input[name='IsValidSelected']:checked").val();
    if(!isValidSelected) {
        isValidSelected = "True";
    }

    var year = $('#Year').val();
    if(!year)
    {
        year = currentYear;
    }

    $.ajax({
        url: siteRoot + "Dashboard/GetClosedLessonsChartData",
        type: "POST",
        data: { year: year, isValidSelected: isValidSelected, noCache: Date() },
        success: function (closedLessonTypeData) {
            var totalClosedLessons = 0;

            series = [];
            var data = [];
            
            var seriesIndex = 0;
            $(closedLessonTypeData).each(function () {
                var pointIndex = 0;

                var seriesData = []
                var lessonType = this;

                var lessonTypeClosedCount = 0;
                $(lessonType.Detail).each(function () {
                    var detail = this;
                    lessonTypeClosedCount += detail.Count;
                    seriesData.push(detail.Count);
                });

                totalClosedLessons += lessonTypeClosedCount;
                var label = '<a href="#" data-lessonTypeId="' + lessonType.LessonTypeId + '" class="closed-lesson-legend">' + lessonType.LessonTypeName + ' (' + lessonTypeClosedCount + ')</a>';
                series.push({ label: label, lessonTypeId: lessonType.LessonTypeId });

                data.push(seriesData);
            });

            var ticks = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

            if(totalClosedLessons == 0)
            {
                data = [];
            }

            var lessonTypeLabel = 'Valid';
            if(isValidSelected == 'False')
            {
                lessonTypeLabel = 'Invalid '
            }
           
            closedLessonsPlot = $.jqplot('ClosedLessonsChart', data, {
                // Tell the plot to stack the bars.
                stackSeries: true,
                sortData: false,
                seriesDefaults: {
                    renderer: $.jqplot.BarRenderer,
                    tickRenderer: $.jqplot.AxisTickRenderer,
                    rendererOptions: {
                        // Put a 30 pixel margin between bars.
                        barMargin: 30
                    },
                },
                series: series,
                axes: {
                    xaxis: {
                        renderer: $.jqplot.CategoryAxisRenderer,
                        ticks: ticks
                    },
                    yaxis: {
                        // Don't pad out the bottom of the data range.  By default,
                        // axes scaled as if data extended 10% above and below the
                        // actual range to prevent data points right on grid boundaries.
                        // Don't want to do that here.
                        padMin: 0,
                        tickInterval: Math.round(totalClosedLessons / 20) + 1
                    }
                },
                highlighter: {
                    show: true,
                    formatString: '<table class="jqplot-highlighter">' +
                        '<tr><td><div class="display-none">%s</div>%s</td></tr>' +
                        '</table>',
                    sizeAdjust: 7.5
                  },
                legend: {
                    show: true,
                    location: 'ne',
                    placement: 'outsideGrid'
                },
                noDataIndicator: { show: true, indicator: 'There are no closed lessons for the selected year.' }
            });

            $('.lesson-type-label').html(lessonTypeLabel);
            $('.year-label').html($('#Year').selectmenu("text"));
            $('.total-closed-lessons').html(totalClosedLessons);
        }
    });
}
