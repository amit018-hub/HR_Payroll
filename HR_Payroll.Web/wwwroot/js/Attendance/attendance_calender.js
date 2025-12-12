document.addEventListener("DOMContentLoaded", function () {

    var calendarEl = document.getElementById("calendar");

    var calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: "dayGridMonth",

        headerToolbar: {
            right: "prev,next today",
            center: "title",
            left: ""
        },

        editable: false,
        selectable: false,
        navLinks: false,

        events: function (info, successCallback, failureCallback) {

            const from = info.startStr;
            const to = info.endStr;

            let today = new Date().toISOString().split("T")[0];

            var formData = new FormData();
            formData.append("fromDate", from);
            formData.append("toDate", to);

            $.ajax({
                url: "/MarkAttendance/GetAttendanceCalendar",
                method: "POST",
                data: formData,
                contentType: false,
                processData: false,
                success: function (json) {

                    if (!json.status) {
                        successCallback([]);
                        return;
                    }

                    // 🔥 AUTO-BIND LAST 5 RECORDS WHEN PAGE LOADS
                    bindLastFive(json.data);

                    let events = [];

                    json.data.forEach(x => {
                        let eventDate = x.attendanceDate.split("T")[0];

                        if (eventDate > today) return; // Skip future dates

                        let bg = "#17a2b8", txt = "#fff";                        
                        switch (x.status) {
                            case "Present": bg = "#28a745"; break;
                            case "Absent": bg = "#dc3545"; break;
                            case "Weekend": bg = "#6c757d"; break;
                            default:
                                if (x.status?.startsWith("Leave"))
                                    bg = "#ff9f43";
                        }

                        let iconHtml = "";
                        switch (x.status) {
                            case "Present": iconHtml = '<i class="mdi mdi-check-circle text-white"></i>'; break;
                            case "Absent": iconHtml = '<i class="mdi mdi-close-circle text-white"></i>'; break;
                            case "Weekend": iconHtml = '<i class="mdi mdi-calendar text-white"></i>'; break;
                            default:
                                if (x.status?.toLowerCase().includes("leave"))
                                    iconHtml = '<i class="mdi mdi-briefcase-remove-outline text-white"></i>';  // 🌴 NEW LEAVE ICON
                        }

                        events.push({
                            title: x.status.replace(" ", "\n"),
                            start: x.attendanceDate,
                            allDay: true,
                            display: "block",
                            backgroundColor: bg,
                            textColor: txt,
                            extendedProps: {
                                iconHtml: iconHtml, 
                                remarks: x.leaveRemarks || x.remarks,
                                badgeHtml: getStatusBadge(x.status)
                            }
                        });
                    });

                    successCallback(events);
                },

                error: function (err) {
                    console.error("Calendar load error:", err);
                    failureCallback(err);
                }
            });
        },
        eventContent: function (arg) {
            return {
                html: arg.event.extendedProps.iconHtml + " " + arg.event.title
            };
        },

        eventDidMount: function (info) {
            if (info.event.extendedProps.remarks) {
                info.el.setAttribute("title", info.event.extendedProps.remarks);
            }
        },

        // 🔥 USER CLICKS ANY EVENT → UPDATE LEFT PANEL
        eventClick: function (info) {

            //let status = info.event.title.replace("\n", " ");
            let status = info.event.extendedProps.badgeHtml;
            let date = info.event.startStr;
            let remarks = info.event.extendedProps.remarks || "No remarks found";

            $("#attendanceDetailsBox").html(`
                <li class="list-group-item align-items-center d-flex">
                    <div class="media">
                        <img src="/assets/images/small/calendar.svg"
                            class="me-3 thumb-sm align-self-center rounded-circle" alt="">
                        <div class="media-body align-self-center">
                            <h6 class="mt-0 mb-1">${status}</h6>
                            <p class="text-muted mb-0"><b>Date:</b> ${date}</p>
                            <p class="text-muted mb-0"><b>Remarks:</b> ${remarks}</p>
                        </div>
                    </div>
                </li>
            `);
        }
    });

    calendar.render();
});


// ------------------------------------------------------
// 🔥 FUNCTION: BIND LAST 5 RECORDS TO LEFT PANEL
// ------------------------------------------------------
function bindLastFive(data) {

    // Sort by newest date first
    let sorted = data.sort((a, b) => new Date(b.attendanceDate) - new Date(a.attendanceDate));

    // Take last 5 recent
    let lastFive = sorted.slice(0, 5);

    let html = "";

    lastFive.forEach(x => {
        let date = x.attendanceDate.split("T")[0];
        let remarks = x.leaveRemarks || x.remarks || "No remarks available";
        let badge = getStatusBadge(x.status);
        html += `
            <li class="list-group-item align-items-center d-flex">
                <div class="media">
                    <img src="/assets/images/small/calendar.svg"
                        class="me-3 thumb-sm align-self-center rounded-circle" alt="">
                    <div class="media-body align-self-center">
                        <h6 class="mt-0 mb-1">${badge}</h6>
                        <p class="text-muted mb-0"><b>Date:</b> ${date}</p>
                        <p class="text-muted mb-0"><b>Remarks:</b> ${remarks}</p>
                    </div>
                </div>
            </li>
        `;
    });

    $("#attendanceDetailsBox").html(html);
}

function getStatusBadge(status) {
    if (!status) return '<span class="badge bg-secondary">N/A</span>';

    const statusLower = status.toLowerCase().trim();
    let badgeClass = 'bg-secondary';
    let icon = '<i class="mdi mdi-calendar-remove me-1"></i>';

    if (statusLower === 'present') {
        badgeClass = 'bg-success';
        icon = '<i class="mdi mdi-check-circle me-1"></i>';
    } else if (statusLower === 'absent') {
        badgeClass = 'bg-danger';
        icon = '<i class="mdi mdi-close-circle me-1"></i>';
    } else if (statusLower === 'wfh' || statusLower === 'work from home') {
        badgeClass = 'bg-info';
        icon = '<i class="mdi mdi-home me-1"></i>';
    } else if (statusLower === 'half day' || statusLower === 'halfday') {
        badgeClass = 'bg-warning';
        icon = '<i class="mdi mdi-clock-outline me-1"></i>';
    } else if (statusLower.includes("leave")) {
        badgeClass = 'bg-warning';
        icon = '<i class="mdi mdi-briefcase-remove-outline me-1"></i>';
    }

    return `<span class="badge ${badgeClass}">${icon}${status}</span>`;
}
