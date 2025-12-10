// admin_dashboard.js
$(document).ready(function () {
    console.log('Initializing dashboard...');
    loadDashboardData();

    // Refresh data every 5 minutes (300000 ms)
    setInterval(loadDashboardData, 300000);
});

// Global chart variables
let attendanceChart = null;
let leaveChart = null;

function loadDashboardData() {
    console.log('Loading dashboard data...');

    $.ajax({
        url: '/Dashboard/GetAdminDashboard',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            console.log('Dashboard data received:', response);

            if (response.status) {
                bindDashboardData(response.data);
            } else {
                console.error('API returned error:', response.message);
                showError('Failed to load dashboard data: ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error('AJAX Error:', {
                status: status,
                error: error,
                response: xhr.responseText
            });
            showError('Error loading dashboard data. Please refresh the page.');

            // Show error in charts
            $('#attendance_chart').html('<div class="alert alert-danger">Failed to load chart data</div>');
            $('#leave_chart').html('<div class="alert alert-danger">Failed to load chart data</div>');
        }
    });
}

function bindDashboardData(data) {
    console.log('Binding dashboard data:', data);

    // Bind KPI Cards with animation
    animateValue('totalEmployees', 0, data.totalEmployees || 0, 1000);
    animateValue('presentToday', 0, data.presentToday || 0, 1000);
    animateValue('leaveToday', 0, data.leaveToday || 0, 1000);
    animateValue('wfhCount', 0, data.wfhCount || 0, 1000);

    // Update percentages
    updatePercentages(data);

    // Bind Charts
    renderAttendanceChart(data.attendanceChart || []);
    renderLeaveChart(data.leaveTypeSummary || []);

    // Bind Recent Attendance Table
    bindRecentAttendance(data.recentAttendance || []);
}

function updatePercentages(data) {
    // Present percentage
    $('#presentPercentage').html(
        `<span class="text-success">
            <i class="mdi mdi-trending-up"></i>${data.todayPresentPercentage || 0}%
        </span> Attendance Rate`
    );

    // Leave percentage
    $('#leavePercentage').html(
        `<span class="text-danger">
            <i class="mdi mdi-trending-down"></i>${data.leavePercentage || 0}%
        </span> Employees On Leave`
    );

    // WFH percentage
    $('#wfhPercentage').html(
        `<span class="text-success">
            <i class="mdi mdi-trending-up"></i>${data.wfhPercentage || 0}%
        </span> WFH Today`
    );
}

function animateValue(id, start, end, duration) {
    const element = document.getElementById(id);
    if (!element) {
        console.warn('Element not found:', id);
        return;
    }

    const range = end - start;
    const increment = range / (duration / 16);
    let current = start;

    const timer = setInterval(function () {
        current += increment;
        if ((increment > 0 && current >= end) || (increment < 0 && current <= end)) {
            current = end;
            clearInterval(timer);
        }
        element.textContent = Math.floor(current);
    }, 16);
}

function renderAttendanceChart(chartData) {
    console.log('Rendering attendance chart with data:', chartData);

    if (!chartData || chartData.length === 0) {
        $('#attendance_chart').html('<div class="alert alert-info text-center">No attendance data available for the last 30 days</div>');
        return;
    }

    const dates = chartData.map(d => formatDate(d.date));
    const presentData = chartData.map(d => d.presentCount || 0);
    const absentData = chartData.map(d => d.absentCount || 0);
    const wfhData = chartData.map(d => d.wfhCount || 0);

    const options = {
        series: [
            {
                name: 'Present',
                data: presentData,
                color: '#10b981'
            },
            {
                name: 'Absent',
                data: absentData,
                color: '#ef4444'
            },
            {
                name: 'Work From Home',
                data: wfhData,
                color: '#3b82f6'
            }
        ],
        chart: {
            type: 'area',
            height: 350,
            toolbar: {
                show: true,
                tools: {
                    download: true,
                    zoom: true,
                    zoomin: true,
                    zoomout: true,
                    pan: false,
                    reset: true
                }
            },
            animations: {
                enabled: true,
                speed: 800,
                animateGradually: {
                    enabled: true,
                    delay: 150
                }
            }
        },
        dataLabels: {
            enabled: false
        },
        stroke: {
            curve: 'smooth',
            width: 2
        },
        xaxis: {
            categories: dates,
            labels: {
                rotate: -45,
                rotateAlways: false,
                style: {
                    fontSize: '12px'
                }
            }
        },
        yaxis: {
            title: {
                text: 'Number of Employees'
            },
            labels: {
                formatter: function (val) {
                    return Math.floor(val);
                }
            }
        },
        tooltip: {
            shared: true,
            intersect: false,
            y: {
                formatter: function (val) {
                    return val + ' employees';
                }
            }
        },
        legend: {
            position: 'top',
            horizontalAlign: 'left',
            offsetY: 0
        },
        grid: {
            borderColor: '#f1f1f1',
            strokeDashArray: 3
        },
        fill: {
            type: 'gradient',
            gradient: {
                shadeIntensity: 1,
                opacityFrom: 0.4,
                opacityTo: 0.1,
                stops: [0, 90, 100]
            }
        }
    };

    // Destroy existing chart if it exists
    if (attendanceChart) {
        attendanceChart.destroy();
    }

    // Clear the container
    $('#attendance_chart').html('');

    // Create new chart
    attendanceChart = new ApexCharts(document.querySelector("#attendance_chart"), options);
    attendanceChart.render();

    console.log('Attendance chart rendered successfully');
}

function renderLeaveChart(leaveData) {
    console.log('Rendering leave chart with data:', leaveData);

    if (!leaveData || leaveData.length === 0) {
        $('#leave_chart').html('<div class="alert alert-info text-center small">No leave data available</div>');
        updateLeaveTable([]);
        return;
    }

    const labels = leaveData.map(d => d.type);
    const counts = leaveData.map(d => d.count || 0);
    const colors = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899', '#06b6d4'];

    const options = {
        series: counts,
        chart: {
            type: 'donut',
            height: 280,
            animations: {
                enabled: true,
                speed: 800
            }
        },
        labels: labels,
        colors: colors,
        legend: {
            show: false
        },
        plotOptions: {
            pie: {
                donut: {
                    size: '70%',
                    labels: {
                        show: true,
                        name: {
                            show: true,
                            fontSize: '14px',
                            fontWeight: 600
                        },
                        value: {
                            show: true,
                            fontSize: '20px',
                            fontWeight: 700,
                            formatter: function (val) {
                                return val;
                            }
                        },
                        total: {
                            show: true,
                            label: 'Total Leaves',
                            fontSize: '14px',
                            fontWeight: 600,
                            formatter: function (w) {
                                return w.globals.seriesTotals.reduce((a, b) => a + b, 0);
                            }
                        }
                    }
                }
            }
        },
        dataLabels: {
            enabled: false
        },
        tooltip: {
            y: {
                formatter: function (val) {
                    return val + ' requests';
                }
            }
        },
        responsive: [{
            breakpoint: 480,
            options: {
                chart: {
                    height: 250
                }
            }
        }]
    };

    // Destroy existing chart if it exists
    if (leaveChart) {
        leaveChart.destroy();
    }

    // Clear the container
    $('#leave_chart').html('');

    // Create new chart
    leaveChart = new ApexCharts(document.querySelector("#leave_chart"), options);
    leaveChart.render();

    console.log('Leave chart rendered successfully');

    // Update the table below the chart
    updateLeaveTable(leaveData);
}

function updateLeaveTable(leaveData) {
    const tbody = $('#leaveTypeTable tbody');
    tbody.empty();

    if (leaveData && leaveData.length > 0) {
        leaveData.forEach(item => {
            tbody.append(`
                <tr>
                    <td>${item.type}</td>
                    <td class="text-end fw-semibold">${item.count}</td>
                </tr>
            `);
        });
    } else {
        tbody.append('<tr><td colspan="2" class="text-center text-muted"><small>No data available</small></td></tr>');
    }
}

function bindRecentAttendance(attendanceData) {
    console.log('Binding recent attendance:', attendanceData);

    const tbody = $('#recentAttendanceTable tbody');
    tbody.empty();

    if (attendanceData && attendanceData.length > 0) {
        attendanceData.forEach(row => {
            const statusBadge = getStatusBadge(row.status);
            tbody.append(`
                <tr>
                    <td>${escapeHtml(row.employeeName)}</td>
                    <td>${escapeHtml(row.date)}</td>
                    <td>${escapeHtml(row.checkIn)}</td>
                    <td>${escapeHtml(row.checkOut)}</td>
                    <td>${statusBadge}</td>
                </tr>
            `);
        });
    } else {
        tbody.append(`
            <tr>
                <td colspan="5" class="text-center text-muted py-4">
                    <i class="mdi mdi-information-outline me-1"></i>
                    No recent attendance data available
                </td>
            </tr>
        `);
    }
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

function formatDate(dateString) {
    if (!dateString) return '';

    const date = new Date(dateString);
    const options = { month: 'short', day: 'numeric' };
    return date.toLocaleDateString('en-US', options);
}

function escapeHtml(text) {
    if (!text) return '';
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.toString().replace(/[&<>"']/g, function (m) { return map[m]; });
}

function showError(message) {
    console.error(message);

    // You can integrate with a toast notification library here
    // For now, using a simple alert
    if (typeof toastr !== 'undefined') {
        toastr.error(message, 'Error', {
            closeButton: true,
            progressBar: true,
            timeOut: 5000
        });
    } else {
        // Fallback: Show error in console and optionally alert
        console.error('ERROR:', message);
        // Uncomment below if you want browser alerts
        showToast(message, 'error');
    }
}

// Optional: Add a manual refresh function
window.refreshDashboard = function () {
    loadDashboardData();
};

console.log('Dashboard JavaScript loaded successfully');
