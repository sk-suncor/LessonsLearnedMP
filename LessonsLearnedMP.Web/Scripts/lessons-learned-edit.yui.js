/// <reference path="http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.7.1-vsdoc.js" />
$(function () {

    $('select:not(.nostyle)').livequery(function () {
        $(this).selectmenu();
    });

    //Disable all the options in the Theme drop down
    //So the user can still see what's selected, but can't change anything
    if (readOnly) {
        $("#ThemeIds option").each(function () {
            $(this).attr('disabled', 'disabled');
        });
    }

    //Theme Multi-seledct control
    $("#ThemeIds").multiselect({
        noneSelectedText: readOnly ? 'None' : 'Select Theme(s)',
        classes: 'theme' + (readOnly ? ' ui-state-disabled' : ''),
        header: !readOnly
    });

    //Show the Theme Description and prompt the user when "Other" is selected
    $("#ThemeIds").bind("multiselectcheckall", function () {
        if (!readOnly) {
            $('#ThemeDescriptionTarget').show();
            alert('Enter a Theme Description');
        }
    });

    //Hide the Theme Description and clear it's value when "Other" is unchecked
    $("#ThemeIds").bind("multiselectuncheckall", function () {
        if (!readOnly) {
            $('#ThemeDescription').val('');
            $('#ThemeDescriptionTarget').hide();
        }
    });

    //Show or hide the Theme Description and prompt the user when "Other" is selected
    $("#ThemeIds").bind("multiselectclick", function (event, ui) {
        if (ui.value == themeOtherValue) {
            if (ui.checked) {
                $('#ThemeDescriptionTarget').show();
                alert('Enter a Theme Description');
            }
            else {
                $('#ThemeDescription').val('');
                $('#ThemeDescriptionTarget').hide();
            }
        }
    });

    //Show a tooltip of the selected options when the user hovers over the Theme List
    $(".ui-multiselect.theme").tooltip({
        bodyHandler: function () {
            var checkedValues = $.map($('#ThemeIds').multiselect("getChecked"), function (input) {
                return input.title;
            });

            var items = [];

            $.each(checkedValues, function (i, item) {
                items.push('<li>' + item + '</li>');
            });

            var html = $('<ul></ul>').append(items.join(''));

            return $(html).html();
        }
    });

    //Style Risk Ranking colors
    $('#RiskRankingId').selectmenu({
        format: function (text) {
            return '<span class="ui-widget ui-corner-all risk-ranking-' + text + '">&nbsp;&nbsp;' + text + '</span>';
        }
    });

    //Text box and textarea styling
    $('.section-item input:text, .section-item input:password, .section-item textarea').livequery(function () {
        $(this).hover(
            function () { $(this).addClass('ui-state-active'); },
            function () { $(this).removeClass('ui-state-active'); }
        );
    });

    //Text box and textarea styling
    $('.section-item input:text, .section-item input:password, .section-item textarea').livequery(function () {
        $(this).focusin(function () {
            $(this).addClass('ui-state-focus');
        });
    });

    //Text box and textarea styling
    $('.section-item input:text, .section-item input:password, .section-item textarea').livequery(function () {
        $(this).focusout(function () {
            $(this).removeClass('ui-state-focus');
        });
    });

    //Date picker control
    $('.datepicker').livequery(function () {
        $(this).datepicker({
            showAnim: "slideDown",
            dateFormat: "yy-mm-dd",
            minDate: -365,
            beforeShow: function () {
                $("#ui-datepicker-div").wrap("<div class='inverse-theme' />");
            }
        });
    });

    //Submit the form when the action button is clicked, populating the SaveAction
    //Pop-up validation window if required
    $("#HeaderContent .lesson-action").click(function () {
        var saveAction = $(this).attr('data-saveAction');

        if (saveAction == saveActionAdminToBPO
            || saveAction == saveActionBPOToBPO) {
            var content = $('#SectionTransferToBPO').html();
            $('#SectionTransferToBPO').html('');

            $(content).dialog({
                modal: true,
                width: 600,
                title: 'Transfer to BPO',
                draggable: false,
                resizable: false,
                buttons: {
                    "Transfer to BPO": function () {
                        var valid = true;

                        if (!$('#TransferBpoComment', this).val()) {
                            valid = false;
                            $('#TransferBpoComment_validationMessage').addClass('field-validation-error').removeClass('field-validation-valid').addClass('field-validation-error');
                        }

                        if (!$('#TransferBpoDisciplineId', this).val()) {
                            valid = false;
                            $('#TransferBpoDisciplineId_validationMessage').addClass('field-validation-error').removeClass('field-validation-valid').addClass('field-validation-error');
                        }

                        if (valid) {
                            $('#LessonForm').append($('#SectionTransferToBPOWrapper').html());
                            $('#LessonForm').append('<input type="hidden" id="SaveAction" name="SaveAction" value="' + saveAction + '" />');
                            $('#LessonForm').submit();
                        }
                    },
                    Cancel: function () {
                        $(this).dialog("close");
                    }
                },
                beforeClose: function () {
                    $('#SectionTransferToBPO').html(content);
                },
                open: function () {
                    setTimeout(function () {
                        $('#TransferBpoComment').focus();
                    }, 200);
                }
            }).dialog('open')
            .find('select').selectmenu().end() //style select menu
            .find('textarea').addClass('ui-widget ui-state-default ui-corner-all').end()  //style text box
            .find('#SectionTransferToBPOWrapper').show();
        }
        else if (saveAction == saveActionAdminToClarification
            || saveAction == saveActionBPOToClarification
            || saveAction == saveActionClarificationToBPO
            || saveAction == saveActionClarificationToAdmin) {
            var content = $('#SectionRequiresClarification').html();
            $('#SectionRequiresClarification').html('');

            var title = "Request Clarification";
            var buttonTitle = "Request Clarification";

            if (saveAction == saveActionClarificationToBPO) {
                title = "Clarify and Transfer To BPO";
                buttonTitle = "Transfer Back to BPO";
            }

            if (saveAction == saveActionClarificationToAdmin) {
                title = "Clarify and Transfer To Administrator";
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

                                    if (!$('#ClarificationComment', this).val()) {
                                        valid = false;
                                        $('#ClarificationComment_validationMessage').addClass('field-validation-error').removeClass('field-validation-valid').addClass('field-validation-error');
                                    }

                                    if (valid) {
                                        $('#LessonForm').append($('#SectionRequiresClarificationWrapper').html());
                                        $('#LessonForm').append('<input type="hidden" id="SaveAction" name="SaveAction" value="' + saveAction + '" />');
                                        $('#LessonForm').submit();
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
                    $('#SectionRequiresClarification').html(content);
                },
                open: function () {
                    setTimeout(function () {
                        $('#ClarificationComment').focus();
                    }, 200);
                }
            }).dialog('open')
            .find('select').selectmenu().end() //style select menu
            .find('textarea').addClass('ui-widget ui-state-default ui-corner-all').end()  //style text box
            .find('#SectionRequiresClarificationWrapper').show();
        }
        else if (saveAction == saveActionBPOToClose) {

            //Copy the resolution & Lesson Type values to the closed window to keep changes made this edit
            $('#CloseResolution').val($('#Resolution').val());
            var $closeRadios = $('input:radio[name=CloseIsLessonTypeValidSelected]');
            var $openRadios = $('input:radio[name=IsLessonTypeValidSelected]');

            var hideLessonTypeSelector = 'CloseLessonTypeInvalidId-button';
            var showLessonTypeSelector = 'CloseLessonTypeValidId-button';

            if ($openRadios.filter('[value=True]').attr('checked')) {
                $closeRadios.filter('[value=True]').attr('checked', true);
            }
            else {
                $closeRadios.filter('[value=True]').removeAttr('checked');
            }

            if ($openRadios.filter('[value=False]').attr('checked')) {
                $closeRadios.filter('[value=False]').attr('checked', true);
                hideLessonTypeSelector = 'CloseLessonTypeValidId-button';
                showLessonTypeSelector = 'CloseLessonTypeInvalidId-button';
            }
            else {
                $closeRadios.filter('[value=False]').removeAttr('checked');
            }

            $('#CloseLessonTypeValidId').val($('#LessonTypeValidId').val());
            $('#CloseLessonTypeInvalidId').val($('#LessonTypeInvalidId').val());

            var content = $('#SectionClosedPopup').html();
            $('#SectionClosedPopup').html('');

            $(content).dialog({
                modal: true,
                width: 600,
                title: 'Validate and Close',
                draggable: false,
                resizable: false,
                buttons: {
                    "Close": function () {
                        var that = this;
                        var valid = true;

                        toggleValidation($('#CloseLessonTypeId_validationMessage'), true);
                        toggleValidation($('#CloseResolution_validationMessage'), true);

                        if (!$('#CloseLessonTypeValidId', this).val()
                            && !$('#CloseLessonTypeInvalidId', this).val()) {
                            valid = false;
                            toggleValidation($('#CloseLessonTypeId_validationMessage'), false);
                        }

                        if ($.trim($('#CloseResolution', that).val()) == '') {
                            valid = false;
                            toggleValidation($('#CloseResolution_validationMessage'), false);
                        }

                        if (valid) {
                            $('#LessonForm').append($('#SectionClosedPopupWrapper').html());
                            $('#LessonForm').append('<input type="hidden" id="SaveAction" name="SaveAction" value="' + saveAction + '" />');

                            //copy closed values to real values
                            $('#LessonTypeValidId').val($('#CloseLessonTypeValidId').val());
                            $('#LessonTypeInvalidId').val($('#CloseLessonTypeInvalidId').val());
                            $('#Resolution').val($('#CloseResolution').val());

                            $('#LessonForm').submit();
                        }
                    },
                    Cancel: function () {
                        $(this).dialog("close");
                    }
                },
                beforeClose: function () {
                    $('#SectionClosedPopup').html(content);
                },
                open: function () {
                    $('#CloseLessonTypeInvalidId-button').hide();

                    setTimeout(function () {
                        $('#CloseComment').focus();
                    }, 200);
                }
            }).dialog('open')
            .find('select').selectmenu().end() //style select menu
            .find('textarea').addClass('ui-widget ui-state-default ui-corner-all').end()  //style text box
            .find('#' + hideLessonTypeSelector).hide().end()
            .find('#' + showLessonTypeSelector).show().end()
            .find('#SectionClosedPopupWrapper').show();
        }
        else if (saveAction == saveActionDelete) {
            $('<div>Are you sure you wish to delete the Lesson "' + $('#Title').val() + '"?</div>').dialog({
                modal: true,
                width: 300,
                title: 'Delete the Lesson?',
                draggable: false,
                resizable: false,
                buttons: {
                    "Delete": function () {
                        $('#LessonForm').append('<input type="hidden" id="SaveAction" name="SaveAction" value="' + saveAction + '" />');
                        $('#LessonForm').submit();
                    },
                    Cancel: function () {
                        $(this).dialog("close");
                    }
                }
            }).dialog('open');
        }
        else if (saveAction == saveActionAssignToUser) {

            var content = $('#SectionAssignToUser').html();
            $('#SectionAssignToUser').html('');
            //$('#SectionAssignToUser option').clone().appendTo('#TransferBpoDisciplineId');

            var title = "Assign to user";
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

                            if (!$('#AssignToUserId', this).val()) {
                                valid = false;
                                $('#AssignToUserId_validationMessage').addClass('field-validation-error').removeClass('field-validation-valid').addClass('field-validation-error');
                            }

                            if (valid) {
                                $('#LessonForm').append($('#SectionAssignToUserWrapper').html());
                                $('#LessonForm').append('<input type="hidden" id="SaveAction" name="SaveAction" value="' + saveAction + '" />');
                                $('#LessonForm').submit();
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
                    $('#SectionAssignToUser').html(content);
                },
                open: function () {
                    setTimeout(function () {
                        $('#AssignUserId').focus();
                    }, 200);
                }
            }).dialog('open')
            //.find('select').selectmenu().end() //style select menu
            .find('textarea').addClass('ui-widget ui-state-default ui-corner-all').end()  //style text box
            .find('#SectionAssignToUserWrapper').show();
        }
        else {
            $('#LessonForm').append('<input type="hidden" id="SaveAction" name="SaveAction" value="' + saveAction + '" />');
            $('#LessonForm').submit();
        }

        return false;
    });

    //Column definition for the comment list view
    var commentColumns = [
                    { "sName": "Enabled", "bVisible": false },
                    { "sName": "Id", "bVisible": false },
                    { "sName": "Date", "sWidth": "130px", "bSearchable": false, "bSortable": false,
                        "fnRender": function (obj) {
                            return "<div class='dateField'>" + obj.aData[obj.iDataColumn] + "</div>";
                        }
                    },
                    { "sName": "User", "sWidth": "100px", "bSearchable": false, "bSortable": false,
                        "fnRender": function (obj) {
                            return "<div class='userField'>" + obj.aData[obj.iDataColumn] + "</div>";
                        }
                    },
                    { "sName": "Type", "sWidth": "130px", "bSearchable": false, "bSortable": false,
                        "fnRender": function (obj) {
                            return "<div class='typeField'>" + obj.aData[obj.iDataColumn] + "</div>";
                        }
                    },
                    { "sName": "Comment", "bSearchable": false, "bSortable": false,
                        "fnRender": function (obj) {
                            return "<div class='commentField'>" + obj.aData[obj.iDataColumn] + "</div>";
                        }
                    },
                    { "sName": "Actions", "sWidth": "100px", "bSearchable": false, "bSortable": false }
                    ];

    //Remove the "Actions" column if the user is not an admin user
    if (!admin) {
        commentColumns.pop();
        commentColumns.push({ "sName": "Actions", "bVisible": false });
    }

    var filterval = "";
    var infoMessage = "";

    //Define the list table
    var oCommentTable = $('#CommentList').dataTable({
        "aLengthMenu": [[10, 25, 50, 100], [10, 25, 50, 100]],
        "bJQueryUI": true,
        "sPaginationType": "full_numbers",
        "bProcessing": true,
        "bServerSide": true,
        "bFilter": true,
        "bLengthChange": true,
        "bStateSave": false,
        "bAutoWidth": false,
        "iDisplayLength": pageSize,
        "bScrollCollapse": true,
        "sScrollY": 150,
        "sDom": '<"H"ir>t<"F">;',
        "sAjaxSource": siteRoot + "Lesson/GetLessonCommentList/" + $('#CommentList').attr('data-lessonId'),
        "aoColumns": commentColumns,
        "iDeferLoading": 0,
        "fnRowCallback": function (nRow, aData, iDisplayIndex) {
            if (aData[0] == "False") {
                $(nRow).addClass('deleted');
            }

            return nRow;
        },
        "fnInfoCallback": function (oSettings, iStart, iEnd, iMax, iTotal, sPre) {
            //Set the message on the grid
            return '<span class="comment-header">Comments</span>' + sPre + " " + infoMessage;
        },
        "fnDrawCallback": function (oSettings) {
            InitializeSelectAllCheckbox('select-all', 'lesson-checkbox');
        }
    });

    if (oCommentTable.length > 0) {
        //Render the table on page load
        oCommentTable.fnDraw();
    }

    //Hover states for rows
    $('#CommentList tr').livequery(function () {
        $(this).hover(
            function () { $(this).addClass('ui-state-hover'); },
            function () { $(this).removeClass('ui-state-hover'); }
        );

        var that = this;
        var selector = 'td';

        if (admin) {
            selector = 'td:not(:last-child)';
        }

        //Show the full comment in a window
        $(selector, this).click(function (e) {
            var comment = $(that).find('.commentField').html();
            var date = $(that).find('.dateField').html();
            var user = $(that).find('.userField').html();
            var type = $(that).find('.typeField').html();

            var tag = $("<div></div>");
            tag.html(comment).dialog({
                modal: true,
                width: 500,
                title: type + ' - ' + user + ' (' + date + ')'
            }).dialog('open');
        });
    });

    //Show/Hide More details
    $('.more').click(function () {
        $(this).toggleClass('expanded');

        if ($(this).hasClass('expanded')) {
            $('.more-label-button').removeClass('sprite-plus-button').addClass('sprite-minus-button');
            $('.more-label-text').html('Less...');
            $('.more-details').show(500);
        }
        else {
            $('.more-label-button').removeClass('sprite-minus-button').addClass('sprite-plus-button');
            $('.more-label-text').html('More...');
            $('.more-details').hide(500);
        }
    });

    //Get the autocomplete list from the server after the 3rd character
    $("#ContactName").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: siteRoot + "Shared/AutoCompleteContactList",
                type: "POST",
                dataType: "json",
                data: { term: $("#ContactName").val() },
                success: function (data) {
                    response($.map(data, function (item) {
                        return { label: item.Label, dataName: item.Name, dataEmail: item.Email, dataPhone: item.Phone }
                    }))
                }
            })
        },
        minLength: 3,
        focus: function (event, ui) {
            $("#ContactName").val(ui.item.dataName);
            return false;
        },
        select: function (event, ui) {
            $("#ContactName").val(ui.item.dataName);
            $("#ContactPhone").val(ui.item.dataPhone);
            $("#ContactEmail").val(ui.item.dataEmail);

            return false;
        }
    });

    //Set the Valid/Invalid radio buttons
    toggleLessonType();
    toggleCloseLessonType();

    $("input[name='IsLessonTypeValidSelected']").livequery(function () {
        $(this).change(function () {
            toggleLessonType();
        });
    });

    $("input[name='CloseIsLessonTypeValidSelected']").livequery(function () {
        $(this).change(function () {
            toggleCloseLessonType();
        });
    });

    //Prompt the user before deleting the comment
    $(".delete-comment").live('click', function () {
        var that = $(this);

        $('<div>Are you sure you wish to delete the "' + that.attr('data-commentType') + '" comment?</div>').dialog({
            modal: true,
            width: 300,
            title: 'Delete the Comment?',
            draggable: false,
            resizable: false,
            buttons: {
                "Delete": function () {
                    var formTag = $('<form action="' + that.attr('data-url') + '" method="post"></form>');
                    $("body").append(formTag);
                    formTag.submit();
                },
                Cancel: function () {
                    $(this).dialog("close");
                }
            }
        }).dialog('open');

        return false;
    });

    //Submit the form to undelete the requested comment
    $(".Un-delete-comment").live('click', function () {
        var formTag = $('<form action="' + $(this).attr('data-url') + '" method="post"></form>');
        $("body").append(formTag);
        formTag.submit();
        return false;
    });
});

//Show's the correct lesson type list
function toggleLessonType() {
    if ($("input[name='IsLessonTypeValidSelected']:checked").val() == 'True') {
        $('#LessonTypeInvalidId-button').hide();
        $('#LessonTypeValidId-button').show();
    }
    else {
        $('#LessonTypeValidId-button').hide();
        $('#LessonTypeInvalidId-button').show();
    }
}

//Show's the correct lesson type list
function toggleCloseLessonType() {
    if ($("input[name='CloseIsLessonTypeValidSelected']:checked").val() == 'True') {
        $('#CloseLessonTypeInvalidId-button').hide();
        $('#CloseLessonTypeValidId-button').show();
    }
    else {
        $('#CloseLessonTypeValidId-button').hide();
        $('#CloseLessonTypeInvalidId-button').show();
    }
}