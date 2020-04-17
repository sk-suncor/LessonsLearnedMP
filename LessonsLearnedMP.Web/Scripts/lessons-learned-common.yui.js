/// <reference path="http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.7.1-vsdoc.js" />
$(function () {
    //Make autocomplete highlight the matched text
    monkeyPatchAutocomplete();

    //Default the enter button to click the action when typing in an input (Issue 37)
    $("input").livequery(function () {
        $(this).bind("keydown", function (event) {
            // track enter key
            var keycode = (event.keyCode ? event.keyCode : (event.which ? event.which : event.charCode));
            if (keycode == 13) { // keycode for enter key
                // force the 'Enter Key' to implicitly click the Action button

                $('.lesson-action-main:first').click();

                //$('.lesson-action-main').click();
                return false;
            }
        });
    });

    //Change user's role (Debug function only)
    $('#RoleChange').change(function () {
        var $form = $("<form action='/Shared/ChangeRole' id='ChangeRoleForm' method='post' name='ChangeRoleForm'></form>");
        $form.append('<input type="hidden" id="ReturnToUrl" name="ReturnToUrl" value="' + location.href + '" />');
        $form.append('<input type="hidden" id="RoleId" name="RoleId" value="' + $(this).val() + '" />');
        $('body').append($form);
        $form.submit();
    });

    //Button theming
    $(".button").livequery(function () {
        $(this).button().end().removeAttr('title');
    });

    //Simulate tabs
    $("#Navigation .button").livequery(function () {
        $(this).removeClass("ui-corner-all").addClass("ui-corner-top");
    });

    //Remove button hover states
    $("#Navigation .button.selected-tab").livequery(function () {
        $(this).off('mouseenter.button');
        $(this).off('mouseleave.button');
    });

    //Trigger the parent control
    $('.clickable').livequery(function () {
        $(this).click(function () {
            var controlId = $(this).attr('data-controlId');
            if (controlId) {
                $('#' + controlId).focus().click();
            }

            var hideId = $(this).attr('data-hideId');
            if (hideId) {
                $('#' + hideId).hide(500);
            }
        });
    });

    //Action Button Styling
    $(".lesson-action-ddl, .grid-button").livequery(function () {
        $(this).button().show()
			.next()
				.button({
				    text: false,
				    icons: {
				        primary: "ui-icon-triangle-1-s"
				    }
				}).show()
				.parent().buttonset();
    });

    //Make sure radio buttons act correctly
    $("input[type=radio]").livequery(function () {
        $(this).fix_radios();
    });

    $(".delete-record").live('click', function () {
        var that = $(this);

        $('<div>Are you sure you wish to delete the Lesson "' + that.attr('data-title') + '"?</div>').dialog({
            modal: true,
            width: 300,
            title: 'Delete the Lesson?',
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

    $(".un-delete-record").live('click', function () {
        var formTag = $('<form action="' + $(this).attr('data-url') + '" method="post"></form>');
        $("body").append(formTag);
        formTag.submit();
        return false;
    });

    /* Bug fix that moves the dialog directly into the body tag since some jQueryUI dialogs are
    sometimes not placed above the overlay div due to their parent divs */
    $("div.ui-dialog").livequery(function () {
        $(this).each(function () {
            $(this).appendTo($("html body")[0]);
        });
    });

    //Add the "More..." link to the error alert if there's several items in the list
    if ($('.validation-summary-errors ul li').length > 4) {
        $('.validation-summary-errors ul li:nth-child(3)').after('<li><a href="#" class="error-more closed">More....</a></li>');
    }

    //Expand the error alert when the link is clicked
    $('.error-more').live('click', function () {
        if ($(this).toggleClass('closed').hasClass('closed')) {
            $(this).html('More....');
        }
        else {
            $(this).html('Less....');
        }
        $('#_FAULT .ui-state-error').toggleClass('opened');
        return false;
    });

    //Download the log file if requested
    if (csvDownloadRequired) {
        window.location = siteRoot + "Lesson/DownloadFile";
    }

    //Show a tooltip of the help screen when the user hovers over the icon
    $(".helptip").livequery(function () {
        $(this).tooltip({
            showURL: false,
            bodyHandler: function () {
                var helpTopic = $(this).attr('data-helpTopic');
                var bodyHtml = '';

                if (helpTopic) {
                    $.ajax({
                        url: siteRoot + "Shared/HelpTopic",
                        type: "GET",
                        dataType: "html",
                        async: false,
                        data: { helpTopic: helpTopic },
                        success: function (data) {
                            bodyHtml = data;
                        }
                    });
                }

                return bodyHtml;
            }
        });
    });
});

//http://stackoverflow.com/questions/2435964/jqueryui-how-can-i-custom-format-the-autocomplete-plug-in-results
//http://en.wikipedia.org/wiki/Monkey_patch
function monkeyPatchAutocomplete() {

    // don't really need this, but in case I did, I could store it and chain
    var oldFn = $.ui.autocomplete.prototype._renderItem;

    $.ui.autocomplete.prototype._renderItem = function (ul, item) {
        return $("<li></li>")
              .data("item.autocomplete", item)
              .append("<a>" + item.label + "</a>")
              .appendTo(ul);
    };
}

function toggleValidation(element, valid) {
    if (valid) {
        $(element).addClass('field-validation-valid').removeClass('field-validation-error');
    }
    else {
        $(element).addClass('field-validation-error').removeClass('field-validation-valid');
    }
}

$.fn.fix_radios = function () {
    function focus() {
        if (!this.checked) return;
        if (!this.was_checked) {
            $(this).change();
        }
    }

    function change(e) {
        if (this.was_checked) {
            e.stopImmediatePropagation();
            return;
        }
        $("input[name='" + this.name + "']").each(function () {
            this.was_checked = this.checked;
        });
    }
    return this.focus(focus).change(change);
}