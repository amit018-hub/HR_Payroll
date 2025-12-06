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
                                    bg = "#007bff";
                        }

                        events.push({
                            title: x.status.replace(" ", "\n"),
                            start: x.attendanceDate,
                            allDay: true,
                            display: "block",
                            backgroundColor: bg,
                            textColor: txt,
                            extendedProps: {
                                remarks: x.leaveRemarks || x.attendanceRemarks
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

        eventDidMount: function (info) {
            if (info.event.extendedProps.remarks) {
                info.el.setAttribute("title", info.event.extendedProps.remarks);
            }
        },

        // 🔥 USER CLICKS ANY EVENT → UPDATE LEFT PANEL
        eventClick: function (info) {

            let status = info.event.title.replace("\n", " ");
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
        let remarks = x.leaveRemarks || x.attendanceRemarks || "No remarks available";

        html += `
            <li class="list-group-item align-items-center d-flex">
                <div class="media">
                    <img src="/assets/images/small/calendar.svg"
                        class="me-3 thumb-sm align-self-center rounded-circle" alt="">
                    <div class="media-body align-self-center">
                        <h6 class="mt-0 mb-1">${x.status}</h6>
                        <p class="text-muted mb-0"><b>Date:</b> ${date}</p>
                        <p class="text-muted mb-0"><b>Remarks:</b> ${remarks}</p>
                    </div>
                </div>
            </li>
        `;
    });

    $("#attendanceDetailsBox").html(html);
}
