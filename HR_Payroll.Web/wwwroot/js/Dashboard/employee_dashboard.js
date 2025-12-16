$(function () {
    loadEmployeeDashboard();
});

/* =========================
   LOAD DASHBOARD DATA
   ========================= */

$(document).on("click", "#refreshDashboard", function () {
    loadEmployeeDashboard();
});

function loadEmployeeDashboard() {

    $.ajax({
        url: "/Dashboard/GetEmployeeDashboard",
        type: "GET",
        beforeSend: function () {
            $("#refreshDashboard i").addClass("mdi-spin");
        },
        success: function (res) {

            /* =========================
               KPI CARDS
               ========================= */

            const status = res.data.todayStatus && res.data.todayStatus.trim() !== ""
                ? res.data.todayStatus
                : "Absent";

            let statusText = "Not Checked In";
            let statusColor = "bg-danger";
            let statusTextClass = "text-danger";
            let statusIcon = "mdi-account-off";

            if (status === "Present") {
                statusText = "Present";
                statusColor = "bg-success";
                statusTextClass = "text-success";
                statusIcon = "mdi-check-circle-outline";
            }
            else if (status === "WFH") {
                statusText = "Work From Home";
                statusColor = "bg-primary";
                statusTextClass = "text-info";
                statusIcon = "mdi-home-account";
            }
            else if (status === "Leave") {
                statusText = "On Leave";
                statusColor = "bg-warning";
                statusTextClass = "text-warning";
                statusIcon = "mdi-beach";
            }

            $("#todayStatus")
                .text(statusText)
                .removeClass("text-success text-danger text-info text-warning")
                .addClass(statusTextClass);

            $("#todayStatusIcon")
                .removeClass("bg-success bg-danger bg-primary bg-warning")
                .addClass(statusColor);

            $("#todayStatusIconI")
                .removeClass()
                .addClass(`mdi ${statusIcon}`);

            $("#todayHours").text(
                status === "Absent" ? "0h 00m" : (res.data.todayWorkingHours || "0h 00m")
            );

            $("#monthlyHours").text(res.data.monthlyWorkingHours || "0h");
            $("#leaveBalance").text(res.data.leaveTaken ?? 0);

            /* =========================
               CHART
               ========================= */

            let dates = [];
            let presentData = [];

            if (Array.isArray(res.data.attendanceChart)) {
                res.data.attendanceChart.forEach(x => {
                    dates.push(x.attDate);
                    presentData.push(x.isPresent);
                });
            }

            if (window.employeeAttendanceChartObj) {
                window.employeeAttendanceChartObj.destroy();
            }

            window.employeeAttendanceChartObj = new ApexCharts(
                document.querySelector("#employeeAttendanceChart"),
                {
                    chart: { type: "area", height: 300, toolbar: { show: false } },
                    series: [{ name: "Present", data: presentData }],
                    xaxis: { categories: dates },
                    colors: ["#0d6efd"],
                    stroke: { curve: "smooth", width: 2 },
                    dataLabels: { enabled: false },
                    grid: { strokeDashArray: 4 }
                }
            );

            window.employeeAttendanceChartObj.render();

            /* =========================
               RECENT ATTENDANCE
               ========================= */

            let rows = "";

            if (Array.isArray(res.data.recentAttendance) && res.data.recentAttendance.length > 0) {

                res.data.recentAttendance.forEach(x => {

                    let badgeClass = "secondary";
                    if (x.status === "Present") badgeClass = "success";
                    else if (x.status === "Absent") badgeClass = "danger";
                    else if (x.status === "WFH") badgeClass = "info";

                    rows += `
                        <tr>
                            <td>${x.date}</td>
                            <td>${x.checkIn}</td>
                            <td>${x.checkOut}</td>
                            <td>
                                <span class="badge bg-${badgeClass}">
                                    ${x.status}
                                </span>
                            </td>
                        </tr>`;
                });

            } else {
                rows = `
                    <tr>
                        <td colspan="4" class="text-center text-muted">
                            No attendance records found
                        </td>
                    </tr>`;
            }

            $("#recentAttendanceBody").html(rows);
        },
        complete: function () {
            $("#refreshDashboard i").removeClass("mdi-spin");
        },
        error: function () {
            console.error("Failed to load employee dashboard data.");
            $("#recentAttendanceBody").html(`
                <tr>
                    <td colspan="4" class="text-center text-danger">
                        Unable to load data
                    </td>
                </tr>
            `);
        }
    });
}

// Toggle dropdown on button click
document.addEventListener("DOMContentLoaded", function () {

    initDropdown();

    const checkInBtn = document.getElementById("checkInToggle");

    if (checkInBtn) {
        checkInBtn.addEventListener("click", function (e) {
            e.preventDefault();
            const isShown =
                document.querySelector(".dropdown-menu.show") !== null;

            if (isShown) {
                hideDropdown();
            } else {
                showDropdown();                
            }
        });
    }
});
