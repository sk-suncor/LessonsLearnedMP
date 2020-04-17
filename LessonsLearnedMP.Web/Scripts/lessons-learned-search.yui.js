/// <reference path="http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.7.1-vsdoc.js" />
$(function () {

    $('#HeaderContent .lesson-action-main,#HeaderContent .lesson-action-ddl').click(function () {
        $('#LessonForm').submit();
    });

    //Assumption - Only option is "Clear Search"
    $("#HeaderContent .lesson-action-option").click(function () {
        $('#LessonForm').append('<input type="hidden" id="Clear" name="Clear" value="True" />')
        $('#LessonForm').submit();

        return false;
    });

    //Combobox styling
    //$('select:not(.nostyle)').selectmenu(); COMMENTED OUT

    //Date picker control
    $('.datepicker').datepicker({
        showAnim: "slideDown",
        dateFormat: "yy-mm-dd",
        minDate: -365,
        beforeShow: function () { $("#ui-datepicker-div").wrap("<div class='inverse-theme' />"); }
    });

    $('.more').click(function () {
        toggleExpanded();
    });

    if ($('#AdvancedSearch').val() == 'True') {
        toggleExpanded();
    }

    toggleLessonType();

    $("input[name='IsLessonTypeValidSelected']").change(function () {
        toggleLessonType();
    });

    $("#ThemeId").change(function () {
        if ($(this).val() == themeOtherValue) {
            $('#ThemeDescriptionTarget').show();
        }
        else {
            $('#ThemeDescription').val('');
            $('#ThemeDescriptionTarget').hide();
        }
    });
});

function toggleExpanded() {
    $('.more').toggleClass('expanded');

    if ($('.more').hasClass('expanded')) {
        $('.more-label-button').removeClass('sprite-plus-button').addClass('sprite-minus-button');
        $('.more-label-text').html('Simple...');
        $('.more-details').show(500);
        $('#AdvancedSearch').val('True')
    }
    else {

        $('.more-label-button').removeClass('sprite-minus-button').addClass('sprite-plus-button');
        $('.more-label-text').html('Advanced...');
        $('.more-details').hide(500);
        $('.more-details input').each(function () {
            $(this).val('');
        });
            
        //$('.more-details select').selectmenu("index", 0); COMMENTED OUT
        $('#AdvancedSearch').val('False')
    }
}

function toggleLessonType() {
//    if ($("input[@name='IsLessonTypeValidSelected']:checked").val() == 'True') {
//        $('#LessonTypeInvalidId-button').hide();
//        $('#LessonTypeValidId-button').show();
//    }
//    else {
//        $('#LessonTypeValidId-button').hide();
//        $('#LessonTypeInvalidId-button').show();
//    }
    if ($("input[@name='IsLessonTypeValidSelected']:checked").val() == 'True') {
        $('#SelectedLessonTypeInvalidIdDisplay').hide();
        $('#SelectedLessonTypeValidIdDisplay').show();

        //$("#SelectedLessonTypeInvalidId option:selected").removeAttr("selected");

        $("#SelectedLessonTypeInvalidId").multiselect("uncheckAll");
    }
    else {
        $('#SelectedLessonTypeValidIdDisplay').hide();
        $('#SelectedLessonTypeInvalidIdDisplay').show();

        $("#SelectedLessonTypeValidId").multiselect("uncheckAll");
    }

}
