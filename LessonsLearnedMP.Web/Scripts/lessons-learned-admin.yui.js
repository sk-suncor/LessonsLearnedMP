/// <reference path="http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.7.1-vsdoc.js" />

var referenceValueHasChanges = false;
var disciplineUsersHasChanges = false;
var previousReferenceType;
var previousDiscipline;
var systemValues;

$(function () {

    //Hook up events on the Reference Value Lists
    $("#ReferenceValueDisabled").livequery(function () {

        $(this).bind('listschanged', function () {
            referenceValueHasChanges = true;

            //Make sure system values are not disabled
            $('#ReferenceValueDisabled option').each(function () {
                if ($.inArray($(this).val(), systemValues) > -1) {
                    //Migration
                    if ($(this).val() != 'MIGRATION') {
                        $(this).remove();
                        if ($('#ReferenceValueEnabled option').length > 0) {
                            $('#ReferenceValueEnabled option').eq(0).before('<option>' + $(this).val() + '</option>');
                        }
                        else {
                            $('#ReferenceValueEnabled').append('<option>' + $(this).val() + '</option>');
                        }

                        alert($(this).val() + ' option cannot be disabled.');
                    }
                }
            });

            //Make sure Migration Value is not disabled
            $('#ReferenceValueEnabled option').each(function () {
                //Migration
                if ($(this).val() == 'MIGRATION') {
                    $(this).remove();
                    if ($('#ReferenceValueDisabled option').length > 0) {
                        $('#ReferenceValueDisabled option').eq(0).before('<option>' + $(this).val() + '</option>');
                    }
                    else {
                        $('#ReferenceValueDisabled').append('<option>' + $(this).val() + '</option>');
                    }

                    alert($(this).val() + ' option cannot be enabled.');
                    return false;
                }
            });
        });
    });

    $(".lesson-action").click(function () {
        //Ensure all of the list items are sent to the controller
        $('#ReferenceValueEnabled, #ReferenceValueDisabled, #DisciplineUsers').each(function () {
            if ($(this).attr("mvcType") != "true") {
                $('option', this).attr("selected", "selected");
            }
        });

        $("#AdminForm").submit();
    });

    $('.multi-add').live("click", function () {
        //get the ID of the left-Right List to add value to.
        //get ID of text box to get content from
        // pass into AddElementToLeftList()
        //done!

        var txtboxId = $(this).attr('data-inputId');
        var lrlistId = $(this).attr('data-listId');
        var disabledListId = $(this).attr('data-disabledListId');

        AddElementToLeftList(txtboxId, lrlistId, disabledListId);
        return false;
    });

    //Move the discipline user to the select list.  Warn the user if they are addign the same value twice
    $('#AddDisciplineUser').live("click", function () {
        disciplineUsersHasChanges = true;

        var inputValue = $('#BpoUsers').selectmenu("value");
        var inputLabel = $('#BpoUsers').selectmenu("text");

        if (inputValue != '') {
            var exists = false;

            $('#DisciplineUsers option').each(function () {
                if (this.value == inputValue) {
                    exists = true;
                    return false;
                }
            });

            if (!exists) {
                $('#DisciplineUsers').append('<option value="' + inputValue + '">' + inputLabel + '</option>');
            }
            else {
                $("#AddDisciplineUserWarning").show('pulsate', null, 100, function () {
                    setTimeout(function () {
                        $(".value-exists:visible").removeAttr("style").fadeOut(1000);
                    }, 1000);
                });
            }
        }
        return false;
    });

    //Remove the selecvted option(s) when clicking button
    $('#RemoveDisciplineUser').live("click", function () {
        $("#DisciplineUsers option:selected").remove();
    });

    //Set the primary discipline user if an option is selected
    $('#PrimaryDisciplineUser').live("click", function () {
        if ($("#DisciplineUsers option:selected").length == 1) {
            $('#PrimaryBpoName').val($("#DisciplineUsers option:selected").html());
            $('#PrimaryDisciplineUser').val($("#DisciplineUsers option:selected").val());
        }
    });

    //Add the reference value to the list upon pressing enter
    $('.reference-list-entry').livequery(function () {
        $(this).keydown(function (e) {
            if (e.which == 13) {
                e.preventDefault();
                AddElementToLeftList($(this).attr('id'), $(this).attr('data-listId'), $(this).attr('disabledListId'));
            }
        });
    });

    //Select Menu Styling
    $('.use-select-menu').selectmenu();

    //Store the current reference type and discipline selections so we can revert back
    previousReferenceType = $('#EditReferenceType').selectmenu("value");
    previousDiscipline = $('#EditingDisciplineUsersReferenceValue').selectmenu("value");

    //Warn the user if they are changing to a new list without saving the current one
    //Otherwise update the Reference area with the new values
    $('#EditReferenceType').change(function () {
        if (referenceValueHasChanges) {
            $('<div>You must save your changes before Administrating another list.  Click Cancel if you wish to save your changes.</div>').dialog({
                modal: true,
                width: 300,
                title: 'Unsaved Changes!',
                draggable: false,
                resizable: false,
                buttons: {
                    "Discard Changes": function () {
                        $(this).dialog("close");
                        UpdateReferenceList();
                    },
                    Cancel: function () {
                        $('#EditReferenceType').selectmenu("value", previousReferenceType);
                        $(this).dialog("close");
                    }
                }
            }).dialog('open')
        }
        else {
            UpdateReferenceList();
        }
    });

    //Warn the user if they are changing to a new list without saving the current one
    //Otherwise update the Discipline area with the new values
    $('#EditingDisciplineUsersReferenceValue').change(function () {
        if (disciplineUsersHasChanges) {
            $('<div>You must save your changes before Administrating another list.  Click Cancel if you wish to save your changes.</div>').dialog({
                modal: true,
                width: 300,
                title: 'Unsaved Changes!',
                draggable: false,
                resizable: false,
                buttons: {
                    "Discard Changes": function () {
                        $(this).dialog("close");
                        UpdateDisciplineUserList();
                    },
                    Cancel: function () {
                        $('#EditingDisciplineUsersReferenceValue').selectmenu("value", previousDiscipline);
                        $(this).dialog("close");
                    }
                }
            }).dialog('open')
        }
        else {
            UpdateDisciplineUserList();
        }
    });

    //Add the selected primary admin user
    $('#AssignPrimaryAdministrator').live("click", function () {
        if ($("#AdminUsers option:selected").length == 1) {
            $('#PrimaryAdminName').val($("#AdminUsers option:selected").html());
            $('#PrimaryAdminUser').val($("#AdminUsers option:selected").val());
        }
    });

    //Prompt before sending emails
    $('#SendMail').click(function () {
        $('<div>Are you sure you wish to send these notifications?</div>').dialog({
            modal: true,
            width: 300,
            title: 'Send Notifications?',
            draggable: false,
            resizable: false,
            buttons: {
                "Send Notifications": function () {
                    $(this).dialog("close");
                    $('#AdminSendNotificationsForm').submit();
                },
                Cancel: function () {
                    $(this).dialog("close");
                }
            }
        }).dialog('open')
    });
});

//Request the desired list information from the server
function UpdateReferenceList() {
    $.ajax({
        url: siteRoot + "Admin/EditReferenceValues",
        type: "GET",
        dataType: "html",
        data: { referenceType: $('#EditReferenceType').selectmenu("value"), noCache: Date() },
        success: function (data) {
            $('#EditReferenceValueTarget').html(data);
            referenceValueHasChanges = false;
            previousReferenceType = $('#EditReferenceType').selectmenu("value");
        }
    });
}

//Request the desired list information from the server
function UpdateDisciplineUserList() {
    $.ajax({
        url: siteRoot + "Admin/EditBpoUsers",
        type: "GET",
        dataType: "html",
        data: { disciplineId: $('#EditingDisciplineUsersReferenceValue').selectmenu("value"), noCache: Date() },
        success: function (data) {
            $('#EditBpoUsersTarget').html(data);
            disciplineUsersHasChanges = false;
            previousDiscipline = $('#EditingDisciplineUsersReferenceValue').selectmenu("value");
        }
    });
}

//Adds an element to the list checking for duplicates
function AddElementToLeftList(inputId, lrlistId, disabledListId) {
    var inputValue = $('#' + inputId).val();
    if (inputValue != '') {
        var exists = false;
        $('#' + lrlistId + ' option').each(function () {
            if (this.text == inputValue) {
                exists = true;
                return false;
            }
        });
        $('#' + disabledListId + ' option').each(function () {
            if (this.text == inputValue) {
                exists = true;
                return false;
            }
        });

        if (!exists) {
            $('#' + lrlistId).append('<option>' + inputValue + '</option>');
            $('#' + inputId).val('');
        }
        else {
            $(".value-exists[data-parentInputId='" + inputId + "']").show('pulsate', null, 100, function () {
                setTimeout(function () {
                    $(".value-exists:visible").removeAttr("style").fadeOut(1000);
                }, 1000);
            });
        }
    }
}

//Hook up the button click methods on the left/right list control
function InitializeLeftRightList(leftselector, rightselector) {
    $("#" + leftselector + " option").removeAttr("selected");

    $("#" + leftselector + "__CopyRight").click(function () {
        $('#' + leftselector + " option:selected").appendTo("#" + rightselector);
        $("#" + rightselector).trigger('listschanged');
    });

    $("#" + leftselector + "__CopyLeft").click(function () {
        $("#" + rightselector + " option:selected").appendTo("#" + leftselector);
        $("#" + rightselector).trigger('listschanged');
    });

    $("#" + leftselector + "__CopyAllRight").click(function () {
        $("#" + leftselector + " option").appendTo("#" + rightselector);
        $("#" + rightselector).trigger('listschanged');
    });

    $("#" + leftselector + "__CopyAllLeft").click(function () {
        $("#" + rightselector + " option").appendTo("#" + leftselector);
        $("#" + rightselector).trigger('listschanged');
    });

    $("#" + leftselector + "__MoveUpLeft").click(function () {
        $('#' + leftselector + " option:selected").each(function () {
            $(this).insertBefore($(this).prev());
        });
        $("#" + rightselector).trigger('listschanged');
    });

    $("#" + leftselector + "__MoveDownLeft").click(function () {
        $('#' + leftselector + " option:selected").each(function () {
            $(this).insertAfter($(this).next());
        });
        $("#" + rightselector).trigger('listschanged');
    });

    $("#" + leftselector + "__MoveUpRight").click(function () {
        $('#' + rightselector + " option:selected").each(function () {
            $(this).insertBefore($(this).prev());
        });
        $("#" + rightselector).trigger('listschanged');
    });

    $("#" + leftselector + "__MoveDownRight").click(function () {
        $('#' + rightselector + " option:selected").each(function () {
            $(this).insertAfter($(this).next());
        });
        $("#" + rightselector).trigger('listschanged');
    });
}
