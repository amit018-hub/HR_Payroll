$(function () {

    $('#btnLoadBankSheet').on('click', function () {
        loadBankSheet();
    });

    $('#bankSelectAll').on('change', function () {
        $('#bankPaymentTable tbody').find('input.bank-row-check').prop('checked', $(this).is(':checked'));
        toggleMarkButton();
    });

    $('#bankPaymentTable').on('change', 'input.bank-row-check', toggleMarkButton);

    $('#btnMarkPaymentDone').on('click', function () {
        var month = $('#bankPayrollMonth').val();
        if (!month) { alert('Select a payroll month.'); return; }

        // Collect PayrollEmployeeIds (not EmployeeIds) — the real PK for mark-paid
        var selected = $('#bankPaymentTable tbody').find('input.bank-row-check:checked')
            .map(function () { return parseInt($(this).data('peid'), 10); }).get();

        if (!selected.length) { alert('Select at least one employee.'); return; }
        if (!confirm('Mark payment as done for ' + selected.length + ' employee(s)? This cannot be undone.')) return;

        $('#btnMarkPaymentDone').prop('disabled', true);

        $.ajax({
            url: '/Payroll/MarkPaymentDone',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ PayrollMonth: month, PayrollEmployeeIds: selected }),
            success: function (res) {
                if (res?.status) {
                    loadBankSheet();
                } else {
                    alert(res?.message || 'Failed to mark payment.');
                }
            },
            error: function () { alert('Request failed.'); },
            complete: toggleMarkButton
        });
    });

    function toggleMarkButton() {
        var any = $('#bankPaymentTable tbody').find('input.bank-row-check:checked').length > 0;
        $('#btnMarkPaymentDone').prop('disabled', !any);
    }

    function loadBankSheet() {
        var month = $('#bankPayrollMonth').val();
        if (!month) { alert('Select a payroll month.'); return; }

        $('#bankPaymentTable tbody').html('<tr><td colspan="8" class="text-center">Loading…</td></tr>');
        $('#btnMarkPaymentDone').prop('disabled', true);

        $.getJSON('/Payroll/LoadBankPaymentSummary?month=' + encodeURIComponent(month))
            .done(function (res) {
                if (!res?.status) {
                    $('#bankPaymentTable tbody').html('<tr><td colspan="8" class="text-center text-danger">Failed to load.</td></tr>');
                    return;
                }
                var summary = res.result?.data ?? res.data ?? res;
                renderSummary(summary);
            })
            .fail(function () {
                $('#bankPaymentTable tbody').html('<tr><td colspan="8" class="text-center text-danger">Request failed.</td></tr>');
            });
    }

    function renderSummary(summary) {
        if (!summary) {
            $('#bankPaymentTable tbody').html('<tr><td colspan="8" class="text-center text-muted">No data.</td></tr>');
            return;
        }

        var month = summary.payrollMonth ?? summary.PayrollMonth ?? $('#bankPayrollMonth').val();
        var total = summary.totalEmployees ?? summary.TotalEmployees ?? 0;
        var amount = summary.totalAmount ?? summary.TotalAmount ?? 0;

        $('#summaryMonth').text(month || '-');
        $('#summaryTotalEmployees').text(total);
        $('#summaryTotalAmount').text(parseFloat(amount).toLocaleString());

        var rows = summary.rows ?? summary.Rows ?? [];
        if (!rows.length) {
            $('#bankPaymentTable tbody').html('<tr><td colspan="8" class="text-center text-muted">No payroll runs found. Run "Calculate Payroll" first.</td></tr>');
            return;
        }

        var html = rows.map(function (r) {
            var peId = r.payrollEmployeeId ?? r.PayrollEmployeeId ?? 0;
            var code = r.employeeCode ?? r.EmployeeCode ?? '';
            var name = r.employeeName ?? r.EmployeeName ?? '';
            var bank = r.bankName ?? r.BankName ?? '';
            var account = r.accountNo ?? r.AccountNo ?? '';
            var ifsc = r.ifsc ?? r.IFSC ?? '-';
            var net = parseFloat(r.netPay ?? r.NetPay ?? 0).toLocaleString();
            var status = r.status ?? r.Status ?? 'Calculated';
            var hasBankDetails = r.hasBankDetails ?? r.HasBankDetails ?? false;

            var badgeClass = status === 'Paid' ? 'bg-success'
                : status === 'Approved' ? 'bg-primary'
                    : 'bg-warning text-dark';
            var rowWarn = hasBankDetails ? '' : ' table-warning';
            var disabled = status === 'Paid' ? 'disabled' : '';

            return '<tr class="' + rowWarn + '">'
                + '<td><input class="bank-row-check form-check-input" type="checkbox" data-peid="' + peId + '" ' + disabled + ' /></td>'
                + '<td>' + code + '</td>'
                + '<td>' + name + '</td>'
                + '<td>' + (bank || '<span class="text-danger">Missing</span>') + '</td>'
                + '<td>' + (account || '<span class="text-danger">Missing</span>') + '</td>'
                + '<td>' + ifsc + '</td>'
                + '<td class="text-end">₹' + net + '</td>'
                + '<td><span class="badge ' + badgeClass + '">' + status + '</span></td>'
                + '</tr>';
        }).join('');

        $('#bankPaymentTable tbody').html(html);
        toggleMarkButton();
    }
});