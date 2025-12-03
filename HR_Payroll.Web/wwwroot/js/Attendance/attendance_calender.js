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

                    let events = [];

                    json.data.forEach(x => {

                        let eventDate = x.attendanceDate.split("T")[0];

                        // ❌ Skip FUTURE STATUS (except today)
                        if (eventDate > today) return;

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

                    // 🔥 Ensure TODAY ALWAYS SHOWS STATUS (even if backend missing)
                    let todayRecord = json.data.find(x => x.attendanceDate === today);

                    if (todayRecord) {
                        let bg =
                            todayRecord.status === "Present" ? "#28a745" :
                                todayRecord.status === "Absent" ? "#dc3545" :
                                    todayRecord.status === "Weekend" ? "#6c757d" :
                                        "#17a2b8";

                        events.push({
                            title: todayRecord.status.replace(" ", "\n"),
                            start: todayRecord.attendanceDate,
                            allDay: true,
                            display: "block",
                            backgroundColor: bg,
                            textColor: "#fff",
                            extendedProps: {
                                remarks: todayRecord.leaveRemarks || todayRecord.attendanceRemarks
                            }
                        });
                    }

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
        }
    });

    calendar.render();
});
