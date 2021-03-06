﻿$(function () {
    //$('.datepicker').datepicker();

    //$('.datepicker').on('changeDate', function () {
    //    $(this).datepicker('hide');
    //});

    $('#projectStatusFilter')
        .change(function () {
            $('#projectListResults').load('Home/GetProjectList?projectStatusFilter=' +
                $('#projectStatusFilter').val() + '&projectCategoryFilter=' +
                $('#projectCategoryFilter').val());
        });

    $('#projectCategoryFilter')
        .change(function () {
            $('#projectListResults').load('Home/GetProjectList?projectStatusFilter=' +
                $('#projectStatusFilter :selected').val() + '&projectCategoryFilter=' +
                $('#projectCategoryFilter :selected').val());
        });

    $('#voteForButton')
        .click(function () {
            var $this = $(this);

            if ($this.text().trim() === '') {
                $this.append('Yes');
            }

            $('#projectVoteResults').load('/ProjectDetails/VoteFor?projectId=' + $('#ProjectId').val());
        });

    $('#voteAgainstButton')
        .click(function () {
            var $this = $(this);

            if ($this.text().trim() === '') {
                $this.append('No');
            }

            $('#projectVoteResults').load('/ProjectDetails/VoteAgainst?projectId=' + $('#ProjectId').val());
        });

    function barWidth() {
        var barWidth = $('.progress-bar').width();
        $('.progress-fill-text').css('width', barWidth);
    }

    barWidth();

    window.onresize = function () {
        barWidth();
    }

    $('#voteTestButton')
        .click(function (e) {
            e.preventDefault();
            $('#projectDetailsTabs a[href="#Results"]').tab('show');
        });

    $('.replyToCommentButton')
       .click(function () {
           var id = '#' + this.id;
           $(id).load('/ProjectDetails/GetCommentReplyForm?commentId=' + this.id + '&projectId=' + $('#projectId').val());
           $('.' + this.id).hide();
       });

    $('#file').change(function () {
        $("#fileInputHelperText").empty();
        $("#fileInputHelperText").append(this.value.split('\\').pop());
    });

    tinymce.init({ selector: 'textarea.richEditor' });
});
