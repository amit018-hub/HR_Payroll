let table;

$(document).ready(function () {
    initAttendanceTable();

    // Refresh button
    $('#refreshData').on('click', function () {
        if (table) {
            table.ajax.reload(null, false); // keep current page
        }
    });

    // Filter button click
    $('#filterBtn').on('click', function () {
        table.ajax.reload();
    });

    // Export buttons
    $('#exportCsv').on('click', function () {
        table.button('.buttons-csv').trigger();
    });

    $('#exportPdf').on('click', function () {
        table.button('.buttons-pdf').trigger();
    });
});

function initAttendanceTable() {
    table = $('#attn_datatable').DataTable({
        processing: true,
        serverSide: true,
        responsive: true,
        ajax: {
            url: '/MarkAttendance/GetAttendanceReport',
            type: 'POST',
            contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
            data: function (d) {
                return $.param({
                    start: d.start,           // Starting record (0, 10, 20...)
                    length: d.length,         // Page size (10, 25, 50...)
                    fromDate: $('#fromDate').val() || '',
                    toDate: $('#toDate').val() || ''
                });
            },
            dataSrc: function (json) {

                if (!json.status || !json.data) {
                    alert(json.message || "No data available");
                    return [];
                }

                // Set total records for pagination
                json.recordsTotal = json.data.totalCount || 0;
                json.recordsFiltered = json.data.totalCount || 0;

                return json.data.records || [];
            },
            beforeSend: function () {
                $('.loader').removeClass('hide');
            },
            complete: function () {
                $('.loader').addClass('hide');
            },
            error: function (xhr, error, thrown) {
                $('.loader').addClass('hide');
                console.error('DataTable Error:', xhr.responseText);
                alert('Failed to load attendance data. Please try again.');
            }
        },
        columns: [
            {
                data: 'attendanceDate',
                title: 'Action',
                render: function (d, type, row) {
                    if (!d) return "-";

                    const date = new Date(d);
                    let dateStr = date.toLocaleDateString('en-GB');

                    return `
                        <a class="view-attendance-btn text-primary" href="#"
                            data-id="${row.attendanceDate}">
                            ${dateStr}
                        </a>
                    `;
                },
                className: "text-center"
            }
,
            {
                data: 'dayName',
                title: 'Day',
                render: function (d) {
                    return d || '-';
                }
            },
            //{
            //    data: 'isWeekend',
            //    title: 'Weekend',
            //    render: function (d) {
            //        return d === 'Y' || d === true ? 'Yes' : 'No';
            //    },
            //    className: 'text-center'
            //},
            {
                data: 'shiftName',
                title: 'Shift',
                render: function (d) {
                    return d || 'N/A';
                }
            },
            {
                data: 'checkInTime',
                title: 'Check-In',
                render: function (d) {
                    return d || '-';
                },
                className: 'text-center'
            },
            {
                data: 'checkOutTime',
                title: 'Check-Out',
                render: function (d) {
                    return d || '-';
                },
                className: 'text-center'
            },
            {
                data: 'workingHoursFormatted',
                title: 'Hours Worked',
                render: function (d, type, row) {

                    // 1️⃣ Backend-calculated value
                    if (d) {
                        const parts = d.split(':');
                        if (parts.length === 2) {
                            const h = parseInt(parts[0], 10);
                            const m = parseInt(parts[1], 10);
                            return `${h}h ${m}m`;
                        }
                        return d;
                    }

                    // 2️⃣ No check-in → cannot calculate
                    if (!row.checkInTime || !row.attendanceDate) return '-';

                    const baseDate = row.attendanceDate.split('T')[0];
                    const checkIn = new Date(`${baseDate} ${row.checkInTime}`);

                    if (isNaN(checkIn)) return '-';

                    // 3️⃣ Checkout exists → normal calculation
                    if (row.checkOutTime) {
                        const checkOut = new Date(`${baseDate} ${row.checkOutTime}`);

                        // ❗ Prevent cross-midnight calculation
                        if (checkOut < checkIn) return '-';

                        return calculateHours(checkIn, checkOut);
                    }

                    // 4️⃣ No checkout → check if day is still running
                    const endOfDay = new Date(`${baseDate} 23:59:59`);
                    const now = new Date();

                    if (now < endOfDay) {
                        return calculateHours(checkIn, now);
                    }

                    // Day completed → calculate till midnight
                    return calculateHours(checkIn, endOfDay);
                },
                className: 'text-center'
            },
            {
                data: 'status',
                title: 'Status',
                render: function (status, type, row) {

                    // Tooltip information
                    let tooltip = `Shift: ${row.shiftName || 'N/A'}\nRemarks: ${row.remarks || '-'}`;

                    // Get formatted badge using your function
                    let badge = getStatusBadge(status);

                    // Wrap badge with tooltip
                    return `<span title="${tooltip}">${badge}</span>`;
                },
                className: 'text-center'
            }
        ],
        lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
        pageLength: 50,
        order: [[0, 'desc']], // Sort by date descending
        dom: '<"row mb-2"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>rt<"row mt-2"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>B',
        buttons: [
            {
                extend: 'csv',
                text: 'CSV',
                className: 'buttons-csv d-none',
                titleAttr: 'Export CSV',
                exportOptions: {
                    columns: ':visible'
                }
            },
            {
                extend: 'pdf',
                text: 'PDF',
                className: 'buttons-pdf d-none',
                titleAttr: 'Export PDF',
                orientation: 'landscape',
                exportOptions: {
                    columns: ':visible'
                }
            }
        ],
        language: {
            emptyTable: "No attendance records found",
            zeroRecords: "No matching records found",
            search: "Search:",
            paginate: {
                first: "First",
                last: "Last",
                next: "Next",
                previous: "Previous"
            }
        }
    });
}

function calculateHours(start, end) {
    const diffMs = end - start; // milliseconds

    if (diffMs <= 0) return "-";

    const diffHrs = Math.floor(diffMs / (1000 * 60 * 60));
    const diffMins = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));

    return `${diffHrs}h ${diffMins}m`;
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

// When user clicks "View" button in table
$(document).on("click", ".view-attendance-btn", function () {
    let attendanceDate = $(this).data("id");
    loadAttendanceModal(attendanceDate);
});


function loadAttendanceModal(attendanceDate) {

    $("#attendanceModal").modal("show");
    $("#attendanceModalBody").html(`<p>Loading...</p>`);

    $.ajax({
        url: '/MarkAttendance/GetAttendanceHistory?attendanceDate=' + attendanceDate,
        type: 'GET',
        success: function (res) {

            if (!res.data || res.data.length === 0) {
                $("#attendanceModalBody").html("<p class=text-center>No attendance found.</p>");
                return;
            }

            let html = `
                <table class="table table-bordered table-striped">
                    <thead class="table-light">
                        <tr>
                            <th>Action</th>
                            <th>Action Time</th>                            
                            <th>Location</th>
                            <th>Geo Status</th>
                            <th>IP</th>
                            <th>Device</th>
                        </tr>
                    </thead>
                    <tbody>
            `;

            res.data.forEach(h => {

                let actionBadge =
                    h.actionType === "CheckIn"
                        ? `<span class="badge bg-success"><i class="mdi mdi-login"></i> Check-In</span>`
                        : `<span class="badge bg-danger"><i class="mdi mdi-logout"></i> Check-Out</span>`;

                let geoBadge =
                    h.isWithinGeofence === 1
                        ? `<span class="badge bg-success">Inside Office</span>`
                        : `<span class="badge bg-danger">Outside Office</span>`;

                html += `
                    <tr>
                        <td>${actionBadge}</td>
                        <td>${formatDateTimeAMPM(h.actionTime)}</td>
                        <td>${h.address}</td>
                        <td>
                            ${geoBadge}<br>
                            ${h.distanceFromOffice} m
                        </td>
                        <td>${h.ipAddress}</td>
                        <td>${h.deviceInfo}</td>
                    </tr>
                `;
            });

            html += `</tbody></table>`;

            $("#attendanceModalBody").html(html);
        },

        error: function () {
            $("#attendanceModalBody").html(
                "<p class='text-danger'>Failed to load attendance history.</p>"
            );
        }
    });
}

function formatDateTimeAMPM(dateStr) {

    if (!dateStr) return '-';

    // Ensure valid ISO string
    const dt = new Date(dateStr);

    if (isNaN(dt)) return '-';

    return dt.toLocaleString('en-IN', {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        hour12: true
    });
}
