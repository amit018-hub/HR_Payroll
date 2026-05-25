$(function () {
    // load departments
    $.getJSON('/Payroll/LoadDepartments').done(function (res) {
        if (res?.status && res.result) {
            const list = res.result.data ?? res.result;
            list.forEach(function (d) {
                $('#filterDepartment').append(`<option value="${d.departmentId}">${d.departmentName}</option>`);
            });
        }
    });

    $('#btnLoadEmployees').on('click', function () {
        loadEmployees();
    });

    $('#selectAll').on('change', function () {
        const checked = $(this).is(':checked');
        $('#payrollTable tbody').find('input.row-check').prop('checked', checked);
    });

    $('#btnCalculatePayroll').on('click', function () {
        const selected = $('#payrollTable tbody').find('input.row-check:checked').map(function () { return $(this).data('id'); }).get();
        if (!selected.length) { alert('Select at least one employee'); return; }
        const month = $('#payrollMonth').val();
        $(this).prop('disabled', true);
        $.ajax({
            url: '/Payroll/CalculatePayroll',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ Month: month, EmployeeIds: selected }),
            success: function (res) {
                if (res?.status) {
                    // server returns computed payroll per employee; re-render table rows
                    renderRows(res.result.data ?? res.result);
                } else {
                    alert('Payroll calculation failed');
                }
            },
            complete: function () { $('#btnCalculatePayroll').prop('disabled', false); }
        });
    });

    // row details button delegated
    $('#payrollTable').on('click', '.btn-view', function () {
        const id = $(this).data('id');
        $('#employeePayrollModal').modal('show');
        // reuse existing proxy: Web controller -> API GetEmployeeDetails
        $.getJSON('/Employee/GetEmployeeDetails', { id: id }).done(function (res) {
            if (res?.status) {
                // populate modal (similar to existing EmployeeList code)
                const payload = res.result.data ?? res.result;
                // populate modal fields (basic, bank, payroll, components)
                $('#employeePayrollModal .modal-body').data('payload', payload);
                // Implement rendering as needed
            }
        });
    });

    function loadEmployees() {
        const month = $('#payrollMonth').val();
        const departmentId = $('#filterDepartment').val() || '';
        $('#payrollTable tbody').html('<tr><td colspan="9" class="text-center">Loading...</td></tr>');
        $.getJSON('/Payroll/LoadPayrollEmployees', { month: month, departmentId: departmentId })
            .done(function (res) {
                if (!res?.status) {
                    $('#payrollTable tbody').html('<tr><td colspan="9" class="text-center text-danger">Failed to load</td></tr>');
                    return;
                }
                renderRows(res.result.data ?? res.result ?? []);
            }).fail(function () {
                $('#payrollTable tbody').html('<tr><td colspan="9" class="text-center text-danger">Failed to load</td></tr>');
            });
    }

    function renderRows(items) {
        if (!Array.isArray(items) || items.length === 0) {
            $('#payrollTable tbody').html('<tr><td colspan="9" class="text-center text-muted">No records</td></tr>');
            return;
        }
        const rows = items.map(function (e) {
            const gross = (e.gross ?? e.Gross ?? 0).toLocaleString();
            const ded = (e.deductions ?? e.Deductions ?? 0).toLocaleString();
            const net = (e.netPay ?? e.NetPay ?? 0).toLocaleString();
            const status = e.status ?? 'Draft';
            return `<tr>
                <td><input class="row-check form-check-input" data-id="${e.employeeId ?? e.EmployeeID}" type="checkbox" /></td>
                <td>${e.employeeCode ?? e.EmployeeCode ?? ''}</td>
                <td>${e.employeeName ?? e.EmployeeName ?? ''}</td>
                <td>${e.days ?? e.Days ?? ''}</td>
                <td class="text-end">${gross}</td>
                <td class="text-end">${ded}</td>
                <td class="text-end text-success fw-bold">${net}</td>
                <td><span class="badge ${status === 'Approved' ? 'bg-success' : status === 'Draft' ? 'bg-warning' : 'bg-secondary'}">${status}</span></td>
                <td class="text-center"><button class="btn btn-sm btn-outline-primary btn-view" data-id="${e.employeeId ?? e.EmployeeID}">View</button></td>
            </tr>`;
        }).join('');
        $('#payrollTable tbody').html(rows);
    }

});