/// <reference path="http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.7.1-vsdoc.js" />
$(function () {

    //Combobox styling
    $('select:not(.nostyle)').livequery(function() {
        $("#LessonList_length select").selectmenu(); 
       //$(this).selectmenu(); 
       //$("#LessonList.datatables").selectmenu(); 
    });

    //Allow the longform view to be resizable
    $("#LongFormViewContent:not(.no-resize)").livequery(function() {
        $(this).resizable({
            minHeight: 200,
            handles: "s",
            alsoResize: "#LongFormView",
            resize: function(event, ui) { 
                $('#LongFormView').css('max-height', ui.size.height);
            },
            stop: function(event, ui) {
                //Save the height in a cookie
                $.ajax({
                    url: siteRoot + 'Shared/SetLongFormViewHeight/',
                    type: "POST",
                    dataType: "json",
                    data: { height: ui.size.height }
                });
            }
        });
    });

    //Allow the lesson grid to be resizable
    $("#LessonGrid .dataTables_scrollBody").livequery(function() {
        $(this).resizable({
            minHeight: 50,
            handles: "s",
            stop: function(event, ui) {
                //Save the height in a cookie
                $.ajax({
                    url: siteRoot + 'Shared/SetLessonListHeight/',
                    type: "POST",
                    dataType: "json",
                    data: { height: ui.size.height }
                });
            }
        });
    });

    //Hover states for row buttons
    $('.row-combo-button, .row-menu-button, .ui-menu-item').livequery(function () {
        $(this).hover(
            function () { $(this).addClass('ui-state-hover'); },
            function () { $(this).removeClass('ui-state-hover'); }
        );
    });

    //Show the menu when the menu button is clicked
    $(".row-menu-button").livequery(function () {
        $(this).click(function () {
            var that = this;
            var menu = $(this).parent().parent().find('.ui-menu').show().position({
                my: "left top",
                at: "left bottom",
                of: that
            });

            $(document).one("click", function () {
                menu.hide();
            });
            return false;
        });
    });

     $("#LessonList_length").change(function () {
            alert('Handler for .change() called.');
        });
    //Save the list length selection to a cookie
    $('#LessonList_length').live("change", function () {
        $.ajax({
            url: siteRoot + 'Shared/SetSearchListPageSize/',
            type: "POST",
            dataType: "json",
            data: { size: $('option:selected', this).val() }
        });
    });

    //Column definition for the list view
    var columns = [
                    { "sName": "Enabled", "sWidth": "0px", "bVisible": false },
                    {
                        "sName": "Selected", "sWidth": "30px", "bSearchable": false, "bSortable": false,
                        "sClass": "center first",
                        "fnRender": function (obj) {
                            return "<div class='checkbox-field'><input class='lesson-checkbox' type='checkbox' /></div>";
                        }
                    },
                    {
                        "sName": "Id", "sWidth": "75px", "bSearchable": false, "bSortable": true,
                        "fnRender": function (obj) {
                            var sReturn = obj.aData[obj.iDataColumn];
                            sReturn = "<div class='id-field'><a href='" + siteRoot + "Lesson/" + (obj.aData[10] == 1 ? "Submit" : "Edit") + "/" + sReturn + "'>" + sReturn + "</a></div>";
                            return sReturn;
                        }
                    },
                    { "sName": "Status", "sWidth": "100px", "bSearchable": false, "bSortable": true,
                            "fnRender": function (obj) {
                            return "<div class='status-field'>" + obj.aData[obj.iDataColumn] + "</div>";
                        }
                    },
                    { "sName": "Title", "bSearchable": false, "bSortable": true,
                        "fnRender": function (obj) {
                            return "<div class='title-field'>" + obj.aData[obj.iDataColumn] + "</div>";
                        }
                    },
                    { "sName": "Discipline", "sWidth": "100px", "bSearchable": false, "bSortable": true,
                            "fnRender": function (obj) {
                            return "<div class='discipline-field'>" + obj.aData[obj.iDataColumn] + "</div>";
                        }
                    },
                    { "sName": "SubmitDate", "sWidth": "70px", "bSearchable": false, "bSortable": true,
                            "fnRender": function (obj) {
                            return "<div class='session-field'>" + obj.aData[obj.iDataColumn] + "</div>";
                        }
                    },
                    { "sName": "Contact", "sWidth": "140px", "bSearchable": false, "bSortable": true,
                            "fnRender": function (obj) {
                            return "<div class='contact-field'>" + obj.aData[obj.iDataColumn] + "</div>";
                        }
                    },
                    { "sName": "Actions", "sClass": "last", "sWidth": "100px", "bSearchable": false, "bSortable": false,
                            "fnRender": function (obj) {
                            return "<div class='actions-field'>" + obj.aData[obj.iDataColumn] + "</div>";
                        }
                    },
                    { "sName": "LessonId", "sWidth": "0px", "bVisible": false },
                    { "sName": "StatusId", "sWidth": "0px", "bVisible": false }
                    ];

    var filterval = "";
    var infoMessage = "";

    //Define the list table
    var oTable = $('#LessonList').dataTable(
    {
        "aLengthMenu": [[10, 25, 50, 100], [10, 25, 50, 100]],
        "bJQueryUI": true,
        "sPaginationType": "full_numbers",
        "bProcessing": true,
        "bServerSide": true,
        "bFilter": true,
        "bSortMulti": false,
        "bLengthChange": true,
        "bStateSave": false,
        "bAutoWidth": false,
        "iDisplayLength": pageSize,
        //"sScrollY": calcDataTableHeight(),
        "sScrollY": lessonListHeight,
        "sDom": '<"H"lir<"#LessonHelp">>t<"F"<"#BulkActions">p>;',
        "sAjaxSource": siteRoot + "Lesson/GetLessonList",
        "aoColumns": columns,
        "bDeferRender": true,
        "iDeferLoading": 0,
          "fnServerData": function ( sSource, aoData, fnCallback ) 
          {
          $.ajax(
          {
            "dataType": 'json', 
            "type": "POST", 
            "url": sSource, 
            "data": aoData, 
            "success": function(data) 
            {
                infoMessage = data.gridMessage;
                fnCallback(data);
            }
            
          } );
          },
        "fnRowCallback": function (nRow, aData, iDisplayIndex) 
        {
            if(aData[0] == "False")
            {
                $(nRow).addClass('deleted');
            }

            $(nRow).attr('data-enabled', aData[0]);
            $(nRow).attr('data-lessonId', aData[9]);

            //Set a class on each status cell for highlighting
            $('td:eq(2)', nRow).addClass('status-' + aData[10]);

            //Highlight the row we are editing
            if(typeof selectedLessonId !== "undefined" && selectedLessonId == aData[9])
            {
                $(nRow).addClass('editing-row');
            }

            return nRow;
        },
        "fnInfoCallback": function (oSettings, iStart, iEnd, iMax, iTotal, sPre) 
        {
            //Set the message on the grid
            return infoMessage + ", " + sPre;
        },
        "fnDrawCallback": function (oSettings) 
        {
            InitializeSelectAllCheckbox('select-all', 'lesson-checkbox');
            $('#LessonHelp').html('<span class="web-sprite sprite-help-browser float-right helptip" data-helpTopic="1" style="margin-right: 5px"></span>');
            $('.deferred').show();
        },
        "fnServerParams": function (aoData) 
        {
            aoData.push({ "name": "SearchModelJson", "value": searchModelString });
            aoData.push({ "name": "NavigationPage", "value": navigationPage });
        },
    });

    if(oTable.length)
    {
        //Render the table on page load
        //Initial render and default sort to Number column
        oTable.fnSort([[2, 'desc']]);
    }

    //Bulk Lesson Button handlers
    //Submit the form when the action button is clicked, populating the SaveAction
    //Pop-up validation window if required
    $("#BulkActions .lesson-action").live('click', function () {
        $(".row-menu-button").parent().parent().find('.ui-menu').hide();

        var saveAction = $(this).attr('data-saveAction');

        if(saveAction == saveActionNone) {
            return;
        }

        var childCheckboxes = $('.lesson-checkbox:checked');
        var selectedLessonIds = [];

        if(childCheckboxes.length > 0) {
            childCheckboxes.each(function() {
                selectedLessonIds.push(parseInt($(this).closest('tr').attr('data-lessonId')));
            });
        }

        if (saveAction == saveActionAdminToBPO
            || saveAction == saveActionBPOToBPO) {//ISSUE 1 METHOD 1
            //Populate the discipline list with the list generated by the ActionButton html
            $('#BulkActionTransferBpoDisciplineId option').remove();
            $('#BulkActionTransferBpoDisciplineId option').clone().appendTo('#BulkLesson_TransferBpoDisciplineId');
 

            var content = $('#SectionTransferToBPOBulk').html();
            $('#SectionTransferToBPOBulk').html('');

            $(content).dialog({
                modal: true,
                width: 600,
                title: 'Transfer Selected to BPO',
                draggable: false,
                resizable: false,
                buttons: {
                    "Transfer Selected": function () {
                        var valid = true;

                        if (!$('#BulkLesson_TransferBpoComment', this).val()) {
                            valid = false;
                            $('#BulkLesson_TransferBpoComment_validationMessage').addClass('field-validation-error').removeClass('field-validation-valid').addClass('field-validation-error');
                        }

                        if (!$('#BulkLesson_TransferBpoDisciplineId', this).val()) {
                            valid = false;
                            $('#BulkLesson_TransferBpoDisciplineId_validationMessage').addClass('field-validation-error').removeClass('field-validation-valid').addClass('field-validation-error');
                        }

                        if (valid) {
                            $('#BulkLessonForm').append($('#SectionTransferToBPOWrapperBulk').html());
                            $('#BulkLessonForm').append('<input type="hidden" id="BulkSelectedLessons" name="BulkSelectedLessons" value="' + selectedLessonIds.join(",") + '" />');
                            $('#BulkLessonForm').append('<input type="hidden" id="BulkLesson_SaveAction" name="BulkLesson.SaveAction" value="' + saveAction + '" />');
                            $('#BulkLessonForm').append('<input type="hidden" id="BulkLesson_ReturnToAction" name="BulkLesson.ReturnToAction" value="' + pageAction + '" />');
                            $('#BulkLessonForm').submit();
                        }
                    },
                    Cancel: function () {
                        $(this).dialog("close");
                    }
                },
                beforeClose: function () {
                    $('#SectionTransferToBPOBulk').html(content);
                },
                open: function () {
                    setTimeout(function () {
                        $('#BulkLesson_TransferBpoComment').focus();
                    }, 200);
                }
            }).dialog('open')
            //.find('select').selectmenu().end() //style select menu
            .find('textarea').addClass('ui-widget ui-state-default ui-corner-all').end()  //style text box
            .find('#SectionTransferToBPOWrapperBulk').show();
        }
        else if (saveAction == saveActionAdminToClarification
            || saveAction == saveActionBPOToClarification
            || saveAction == saveActionClarificationToBPO
            || saveAction == saveActionClarificationToAdmin) {

            var content = $('#SectionRequiresClarificationBulk').html();
            $('#SectionRequiresClarificationBulk').html('');

            var title = "Request Clarification For Selected Lessons";
            var buttonTitle = "Request Clarification";

            if (saveAction == saveActionClarificationToBPO) {
                title = "Clarify and Transfer Selected Lessons To BPO";
                buttonTitle = "Transfer Back to BPO";
            }

            if (saveAction == saveActionClarificationToAdmin) {
                title = "Clarify and Transfer Selected Lessons To Administrator";
                buttonTitle = "Transfer Back to Administrator";
            }

            $(content).dialog({
                modal: true,
                width: 600,
                title: title,
                draggable: false,
                resizable: false,
                buttons: [
                    {
                        text: buttonTitle,
                        click: function () {
                            var valid = true;

                            if (!$('#BulkLesson_ClarificationComment', this).val()) {
                                valid = false;
                            }

                            toggleValidation($('#BulkLesson_ClarificationComment_validationMessage'), valid);

                            if (valid) {
                                $('#BulkLessonForm').append($('#SectionRequiresClarificationWrapperBulk').html());
                                $('#BulkLessonForm').append('<input type="hidden" id="BulkSelectedLessons" name="BulkSelectedLessons" value="' + selectedLessonIds.join(",") + '" />');
                                $('#BulkLessonForm').append('<input type="hidden" id="BulkLesson_SaveAction" name="BulkLesson.SaveAction" value="' + saveAction + '" />');
                                $('#BulkLessonForm').append('<input type="hidden" id="BulkLesson_ReturnToAction" name="BulkLesson.ReturnToAction" value="' + pageAction + '" />');
                                $('#BulkLessonForm').submit();
                            }
                        }
                    },
                    {
                        text: "Cancel",
                        click: function () {
                            $(this).dialog("close");
                        }
                    }
                ],
                beforeClose: function () {
                    $('#SectionRequiresClarificationBulk').html(content);
                },
                open: function () {
                    setTimeout(function () {
                        $('#BulkLesson_ClarificationComment').focus();
                    }, 200);
                }
            }).dialog('open')
            //.find('select').selectmenu().end() //style select menu
            .find('textarea').addClass('ui-widget ui-state-default ui-corner-all').end()  //style text box
            .find('#SectionRequiresClarificationWrapperBulk').show();
        }
        else if (saveAction == saveActionBPOToClose) {
            var content = $('#SectionClosedPopupBulk').html();
            $('#SectionClosedPopupBulk').html('');

            $(content).dialog({
                modal: true,
                width: 600,
                title: 'Validate and Close',
                draggable: false,
                resizable: false,
                buttons: {
                    "Process Selected": function () {
                        var that = this;
                        var valid = true;

                        toggleValidation($('#BulkLesson_LessonTypeId_validationMessage'), true);
                        toggleValidation($('#BulkLesson_Resolution_validationMessage'), true);

                        if (!$('#BulkLesson_LessonTypeValidId', this).val()
                            && !$('#BulkLesson_LessonTypeInvalidId', this).val()) {
                            valid = false;
                            toggleValidation($('#BulkLesson_LessonTypeId_validationMessage'), false);
                        }

                        if ($.trim($('#BulkLesson_Resolution', that).val()) == '') {
                            valid = false;
                            toggleValidation($('#BulkLesson_Resolution_validationMessage'), false);
                        }

                        if (valid) {
                            $('#BulkLessonForm').append($('#SectionClosedPopupWrapperBulk').html());
                            $('#BulkLessonForm').append('<input type="hidden" id="BulkSelectedLessons" name="BulkSelectedLessons" value="' + selectedLessonIds.join(",") + '" />');
                            $('#BulkLessonForm').append('<input type="hidden" id="BulkLesson_SaveAction" name="BulkLesson.SaveAction" value="' + saveAction + '" />');
                            $('#BulkLessonForm').append('<input type="hidden" id="BulkLesson_ReturnToAction" name="BulkLesson.ReturnToAction" value="' + pageAction + '" />');
                            $('#BulkLessonForm').submit();
                        }
                    },
                    Cancel: function () {
                        $(this).dialog("close");
                    }
                },
                beforeClose: function () {
                    $('#SectionClosedPopupBulk').html(content);
                },
                open: function () {
                    $('#BulkLesson_LessonTypeInvalidId-button').hide();
                    setTimeout(function () {
                        $('#BulkLesson_CloseComment').focus();
                    }, 200);
                }
            }).dialog('open')
            //.find('select').selectmenu().end() //style select menu
            .find('textarea').addClass('ui-widget ui-state-default ui-corner-all').end()  //style text box
            //.find('#BulkLesson_LessonTypeInvalidId-button').hide().end()
            .find('#CloseSectionClosedPopupWrapperBulk').show();
        }
        else if (saveAction == saveActionDelete) {
            $('<div>Are you sure you wish to delete all of the selected lessons?</div>').dialog({
                modal: true,
                width: 300,
                title: 'Delete all selected Lessons?',
                draggable: false,
                resizable: false,
                buttons: {
                    "Delete Selected": function () {
                        $('#BulkLessonForm').append('<input type="hidden" id="BulkLesson_SaveAction" name="BulkLesson.SaveAction" value="' + saveAction + '" />');
                        $('#BulkLessonForm').append('<input type="hidden" id="BulkSelectedLessons" name="BulkSelectedLessons" value="' + selectedLessonIds.join(",") + '" />');
                        $('#BulkLessonForm').append('<input type="hidden" id="BulkLesson_ReturnToAction" name="BulkLesson.ReturnToAction" value="' + pageAction + '" />');
                        $('#BulkLessonForm').submit();
                    },
                    Cancel: function () {
                        $(this).dialog("close");
                    }
                }
            }).dialog('open');
        }
        else if (saveAction == saveActionAssignToUser) {

            var content = $('#SectionAssignToUserBulk').html();
            $('#SectionAssignToUserBulk').html('');
            $('#SectionAssignToUserBulk option').clone().appendTo('#BulkLesson_TransferBpoDisciplineId');

            var title = "Assign selected to user";
            var buttonTitle = "Assign";

            $(content).dialog({
                modal: true,
                width: 600,
                title: title,
                draggable: false,
                resizable: false,
                buttons: [
                    {
                        text: buttonTitle,
                        click: function () {
                            var valid = true;

                            if (!$('#BulkLesson_AssignToUserId', this).val()) {
                                valid = false;
                                $('#BulkLesson_AssignToUserId_validationMessage').addClass('field-validation-error').removeClass('field-validation-valid').addClass('field-validation-error');
                            }

                            if (valid) {
                                $('#BulkLessonForm').append($('#SectionAssignToUserWrapperBulk').html());
                                $('#BulkLessonForm').append('<input type="hidden" id="BulkSelectedLessons" name="BulkSelectedLessons" value="' + selectedLessonIds.join(",") + '" />');
                                $('#BulkLessonForm').append('<input type="hidden" id="BulkLesson_SaveAction" name="BulkLesson.SaveAction" value="' + saveAction + '" />');
                                $('#BulkLessonForm').append('<input type="hidden" id="BulkLesson_ReturnToAction" name="BulkLesson.ReturnToAction" value="' + pageAction + '" />');
                                $('#BulkLessonForm').submit();
                            }
                        }
                    },
                    {
                        text: "Cancel",
                        click: function () {
                            $(this).dialog("close");
                        }
                    }
                ],
                beforeClose: function () {
                    $('#SectionAssignToUserBulk').html(content);
                },
                open: function () {
                    setTimeout(function () {
                        $('#BulkLesson_AssignUserId').focus();
                    }, 200);
                }
            }).dialog('open')
            //.find('select').selectmenu().end() //style select menu
            .find('textarea').addClass('ui-widget ui-state-default ui-corner-all').end()  //style text box
            .find('#SectionAssignToUserWrapperBulk').show();
        }
        else {
            $('#BulkLessonForm').append('<input type="hidden" id="BulkSelectedLessons" name="BulkSelectedLessons" value="' + selectedLessonIds.join(",") + '" />');
            $('#BulkLessonForm').append('<input type="hidden" id="BulkLesson_SaveAction" name="BulkLesson.SaveAction" value="' + saveAction + '" />');
            $('#BulkLessonForm').append('<input type="hidden" id="BulkLesson_ReturnToAction" name="BulkLesson.ReturnToAction" value="' + pageAction + '" />');
            $('#BulkLessonForm').submit();
        }

        return false;
    });

    toggleBulkLessonType();

    $("input[name='BulkLesson.IsLessonTypeValidSelected']").livequery(function () {
        $(this).change(function () {
            toggleBulkLessonType();
        });
    });
});

var calcDataTableHeight = function() {
    return Math.round($('#LessonGridResize').height());
};

function toggleBulkLessonType() {
    if ($("input[name='BulkLesson.IsLessonTypeValidSelected']:checked").val() == 'True') {
//        $('#BulkLesson_LessonTypeInvalidId-button').hide();
//        $('#BulkLesson_LessonTypeValidId-button').show();

        $('#BulkLesson_LessonTypeInvalidId').hide();
        $('#BulkLesson_LessonTypeValidId').show();
    }
    else {
//        $('#BulkLesson_LessonTypeValidId-button').hide();
//        $('#BulkLesson_LessonTypeInvalidId-button').show();

        $('#BulkLesson_LessonTypeValidId').hide();
        $('#BulkLesson_LessonTypeInvalidId').show();
    }
}

//Updates the master checkbox's state based on it's children
function UpdateSelectAllCheckbox(masterId, childId) {
    var masterCheckbox = $('.' + masterId);
    var childCheckbox = $('.' + childId).not('.not-linked');

    var totalCheckBoxes = childCheckbox.length;
    var checkedBoxes = childCheckbox.filter(':checked').length;

    //2 = on
    if (totalCheckBoxes == checkedBoxes) {
        masterCheckbox.removeClass('state-unchecked state-intermediate').addClass('state-checked');
    }
    //1 = partial
    else if (checkedBoxes > 0) {
        masterCheckbox.removeClass('state-checked state-unchecked').addClass('state-intermediate');
    }
    //0 = off
    else {
        masterCheckbox.removeClass('state-checked state-intermediate').addClass('state-unchecked');
    }

    UpdateBulkActionButton();
};

function UpdateBulkActionButton()
{
    $('#BulkActions').html('');

    var childCheckboxes = $('.lesson-checkbox:checked');
    var selectedLessonIds = [];

    if(childCheckboxes.length > 0) {

        childCheckboxes.each(function() {
            selectedLessonIds.push(parseInt($(this).closest('tr').attr('data-lessonId')));
        });
    }

    $.ajax({
        url: siteRoot + "Shared/ActionButtonBulk",
        type: "POST",
        dataType: "html",
        contentType: 'application/json',
        data: JSON.stringify({ selectedLessonIds: selectedLessonIds }),
        success: function (data) {
            $('#BulkActions').html(data);
        }
    });
}

$.extend({
    distinct : function(anArray) {
       var result = [];
       $.each(anArray, function(i,v){
           if ($.inArray(v, result) == -1) result.push(v);
       });
       return result;
    }
});

//Initialized the Master and Child checkboxes
function InitializeSelectAllCheckbox(masterId, childId) {
    var masterCheckbox = $('.' + masterId);
    var childCheckbox = $('.' + childId).not('.not-linked');
    
    masterCheckbox.unbind('click');

    masterCheckbox.click(function () {
        if ($(this).hasClass('state-checked')) {
            //currently checked.  Uncheck.
            childCheckbox.attr('checked', false).change();
        }
        else {
            //Check all
            childCheckbox.attr('checked', true).change();
        }
        
        UpdateSelectAllCheckbox(masterId, childId);
    });

    childCheckbox.unbind('click');

    childCheckbox.click(function () {
        UpdateSelectAllCheckbox(masterId, childId);
    });

    masterCheckbox.unbind('hover');
    //hover states on select all checkboxes
    masterCheckbox.hover(
        function () { $(this).addClass('hover'); },
        function () { $(this).removeClass('hover'); }
	);

    //Set the master checkmox state on load
    UpdateSelectAllCheckbox(masterId, childId);
}