﻿// Write your Javascript code.

$(function () { 
    $('.datepicker').datepicker();

    $('.datepicker').on('changeDate', function () {
        $(this).datepicker('hide');
    });

    $("#categoriesDropdown").change(function () {
        $('#Tags').val($('#Tags').val() + $('#categoriesDropdown :selected').text() + ',');
    });
});
