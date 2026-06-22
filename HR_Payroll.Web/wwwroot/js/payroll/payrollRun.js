$(function () {

    // Load departments into filter dropdown on page load
    $.getJSON('/Payroll/LoadDepartments')
        .done(function (res) {
            if (!res?.status) return;
            var depts = res.result?.data ?? res.data ?? [];
            $.each(depts, function (_, d) {
                var id = d.departmentId ?? d.DepartmentId;
                var name = d.departmentName ?? d.DepartmentName ?? '';
                $('#filterDepartment').append($('<option>', { value: id, text: name }));
            });
        });

    // Load employees into grid
    $('#btnLoadEmployees').on('click', function () {
        var month = $('#payrollMonth').val();
        if (!month) { alert('Select a payroll month.'); return; }
        loadPayrollEmployees(month);
    });

    // Select all / deselect all
    $('#selectAll').on('change', function () {
        $('#payrollTable tbody').find('input.emp-check').prop('checked', $(this).is(':checked'));
    });

    // Calculate payroll for selected employees
    $('#btnCalculatePayroll').on('click', function () {
        var month = $('#payrollMonth').val();
        if (!month) { alert('Select a payroll month.'); return; }

        var selected = $('#payrollTable tbody').find('input.emp-check:checked')
            .map(function () { return parseInt($(this).data('empid'), 10); }).get();

        if (!selected.length) { alert('Select at least one employee.'); return; }

        $('#btnCalculatePayroll').prop('disabled', true).text('Calculating…');

        $.ajax({
            url: '/Payroll/CalculatePayroll',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ Month: month, EmployeeIds: selected }),
            success: function (res) {
                if (res?.status) {
                    loadPayrollEmployees(month); // refresh grid with persisted values
                } else {
                    alert(res?.message || 'Calculation failed.');
                }
            },
            error: function () { alert('Calculation request failed.'); },
            complete: function () {
                $('#btnCalculatePayroll').prop('disabled', false).text('Calculate Payroll');
            }
        });
    });

    // Open salary slips for all checked employees in new tabs
    $('#btnGeneratePayslips').on('click', function () {
        var month = $('#payrollMonth').val();
        var selected = $('#payrollTable tbody').find('input.emp-check:checked')
            .map(function () { return $(this).data('empid'); }).get();

        if (!selected.length) { alert('Select at least one employee.'); return; }

        var parts = month.split('-');
        $.each(selected, function (_, empId) {
            window.open('/Payroll/SalarySlip?employeeId=' + empId + '&month=' + month, '_blank');
        });
    });

    // -----------------------------------------------------------------------
    // Modal — show payroll details for a single employee
    // -----------------------------------------------------------------------
    $('#payrollTable').on('click', '.btn-view-detail', function () {
        var row = $(this).closest('tr');
        var name = row.data('name') || '-';
        var code = row.data('code') || '-';
        var gross = parseFloat(row.data('gross')) || 0;
        var ded = parseFloat(row.data('ded')) || 0;
        var net = parseFloat(row.data('net')) || 0;
        var empId = row.data('empid');
        var month = $('#payrollMonth').val();

        $('#modalEmployeeName').text(name);
        $('#modalEmployeeCode').text(code);
        $('#modalMonth').text(month || '-');
        $('#modalGross').text('₹' + gross.toLocaleString());
        $('#modalDeductions').text('₹' + ded.toLocaleString());
        $('#modalNetPay').text('₹' + net.toLocaleString());

        $('#modalViewSlipLink').off('click').on('click', function () {
            window.open('/Payroll/SalarySlip?employeeId=' + empId + '&month=' + month, '_blank');
        });

        new bootstrap.Modal(document.getElementById('employeePayrollModal')).show();
    });

    // -----------------------------------------------------------------------
    // Internal helpers
    // -----------------------------------------------------------------------

    function loadPayrollEmployees(month) {
        var deptId = $('#filterDepartment').val();
        var url = '/Payroll/LoadPayrollEmployees?month=' + encodeURIComponent(month);
        if (deptId) url += '&departmentId=' + deptId;

        $('#payrollTable tbody').html('<tr><td colspan="9" class="text-center">Loading…</td></tr>');

        $.getJSON(url)
            .done(function (res) {
                if (!res?.status) {
                    $('#payrollTable tbody').html('<tr><td colspan="9" class="text-center text-danger">Failed to load employees.</td></tr>');
                    return;
                }
                renderGrid(res.result?.data ?? res.data ?? []);
            })
            .fail(function () {
                $('#payrollTable tbody').html('<tr><td colspan="9" class="text-center text-danger">Request failed.</td></tr>');
            });
    }

    function renderGrid(rows) {
        if (!rows.length) {
            $('#payrollTable tbody').html('<tr><td colspan="9" class="text-center text-muted">No employees found.</td></tr>');
            return;
        }

        var html = rows.map(function (r) {
            var empId = r.employeeId ?? r.EmployeeId ?? 0;
            var code = r.employeeCode ?? r.EmployeeCode ?? '';
            var name = r.employeeName ?? r.EmployeeName ?? '';
            var dept = r.departmentName ?? r.DepartmentName ?? '-';
            var hasComps = r.hasComponents ?? r.HasSalaryComponents ?? r.hasSalaryComponents ?? false;
            var gross = r.gross ?? r.Gross ?? null;
            var ded = r.deductions ?? r.Deductions ?? null;
            var net = r.netPay ?? r.NetPay ?? null;
            var status = r.status ?? r.Status ?? 'Not Calculated';

            var grossStr = gross != null ? '₹' + parseFloat(gross).toLocaleString() : '-';
            var dedStr = ded != null ? '₹' + parseFloat(ded).toLocaleString() : '-';
            var netStr = net != null ? '₹' + parseFloat(net).toLocaleString() : '-';

            var badgeClass = status === 'Paid' ? 'bg-success'
                : status === 'Approved' ? 'bg-primary'
                    : status === 'Calculated' ? 'bg-info text-dark'
                        : 'bg-secondary';

            var compWarn = hasComps ? '' : ' table-warning';

            return '<tr class="' + compWarn + '"'
                + ' data-empid="' + empId + '"'
                + ' data-name="' + name + '"'
                + ' data-code="' + code + '"'
                + ' data-gross="' + (gross ?? 0) + '"'
                + ' data-ded="' + (ded ?? 0) + '"'
                + ' data-net="' + (net ?? 0) + '">'
                + '<td><input class="emp-check form-check-input" type="checkbox" data-empid="' + empId + '" /></td>'
                + '<td>' + code + '</td>'
                + '<td>' + name + '</td>'
                + '<td>' + dept + '</td>'
                + '<td class="text-end">' + grossStr + '</td>'
                + '<td class="text-end">' + dedStr + '</td>'
                + '<td class="text-end">' + netStr + '</td>'
                + '<td><span class="badge ' + badgeClass + '">' + status + '</span></td>'
                + '<td class="text-center"><button class="btn btn-sm btn-outline-secondary btn-view-detail">View</button></td>'
                + '</tr>';
        }).join('');

        $('#payrollTable tbody').html(html);
        $('#selectAll').prop('checked', false);
    }
});