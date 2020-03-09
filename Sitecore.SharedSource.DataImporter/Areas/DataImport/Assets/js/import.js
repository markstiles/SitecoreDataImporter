jQuery.noConflict();

//import
jQuery(document).ready(function () {

    var importForm = ".import-form";
    
    jQuery('h2').dblclick(function () {
        $(this).next(".Section").toggle();
    });

    setInterval(GetJobs, 1000);

    //get jobs
    function GetJobs()
    {
        jQuery(".result-failure").hide();
        
        jQuery.post(jQuery(importForm).attr("job-action"), {})
        .done(function (r) 
        {
            if (r.Failed)
            {
                jQuery(".result-failure").show();
                return;
            } 

            var jobList = "";
            jobList += "<thead>";
            jobList += "<td>Job</td>";
            jobList += "<td>Category</td>";
            jobList += "<td>Status</td>";
            jobList += "<td>Processed</td>";
            jobList += "<td>Priority</td>";
            jobList += "<td>Start Time</td>";
            jobList += "</thead>";

            for (var i = 0; i < r.Jobs.length; i++) {
                var job = r.Jobs[i];

                jobList += "<tr>";
                jobList += "<td>" + job.Name + "</td>";
                jobList += "<td>" + job.Category + "</td>";
                jobList += "<td>" + job.State + "</td>";
                jobList += "<td>" + job.Processed + " / " + job.Total + "</td>";
                jobList += "<td>" + job.Priority + "</td>";
                jobList += "<td>" + job.QueueTime + "</td>";
                jobList += "</tr>";

                jQuery(".jobListing").html(jobList);
            }
        });
    }

    //run import
    var timer;
    jQuery(importForm + " .import-submit")
        .click(function (event) {
            event.preventDefault();

            var idValue = jQuery(importForm + " #id").val();
            var dbValue = jQuery(importForm + " #db").val();
            var importTypeValue = jQuery(this).attr("rel");

            jQuery(".importError").hide();
            jQuery(".importStatus").hide();
            jQuery(".form-buttons").hide();
            jQuery(".progress-indicator").show();

            jQuery.post(jQuery(importForm).attr("import-action"),
                {
                    id: idValue,
                    db: dbValue,
                    importType: importTypeValue 
                })
                .done(function (r) {
                    if (r.Failed) {
                        jQuery(".importError").show();
                        jQuery(".importError").text(r.Error);
                        jQuery(".importStatus").hide();
                        jQuery(".form-buttons").show();
                        jQuery(".progress-indicator").hide();

                        return;
                    }

                    var timer = setInterval(function () {
                        jQuery.post(jQuery(importForm).attr("status-action"),
                            {
                                handleName: r.HandleName
                            })
                            .done(function (jobResult) {
                                if (jobResult.Total < 0)
                                    return;
                                
                                jQuery(".progress-indicator").hide();
                                jQuery(".importStatus").show();
                                jQuery(".status-number").text(numberWithCommas(jobResult.Current) + " of " + numberWithCommas(jobResult.Total));
                                var percent = jobResult.Current / jobResult.Total * 100;
                                jQuery(".status-bar-color").attr("style", "width:" + percent + "%;");

                                if (jobResult.Completed)
                                {
                                    jQuery(".form-buttons").show();
                                    clearInterval(timer);
                                }
                            });
                    }, 500);
                });
        });

    function numberWithCommas(x) {
        return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    }
});
