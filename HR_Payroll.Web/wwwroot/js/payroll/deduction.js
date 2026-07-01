var _rows = [];

// Load departments on page load
$.getJSON('/Payroll/LoadDepartments').done(function (res) {
    if (!res?.status) return;
    var depts = res.result?.data ?? res.data ?? [];
    depts.forEach(function (d) {
        $('#dedDept').append($('<option>', {
            value: d.departmentId ?? d.DepartmentId,
            text: d.departmentName ?? d.DepartmentName
        }));
    });
});

// Load grid
$('#btnLoadDed').on('click', function () { loadData(); });

function loadData() {
    var dept = $('#dedDept').val();
    var url = '/Payroll/LoadDeductionPageData';
    if (dept) url += '?departmentId=' + dept;

    $('#dedBody').html('<tr><td colspan="9" class="text-center py-4 text-muted">Loading…</td></tr>');

    $.getJSON(url).done(function (res) {
        if (!res?.status) {
            $('#dedBody').html('<tr><td colspan="9" class="text-center text-danger py-3">Failed to load.</td></tr>');
            return;
        }
        _rows = res.data ?? [];
        renderGrid(_rows);
    }).fail(function () {
        $('#dedBody').html('<tr><td colspan="9" class="text-center text-danger py-3">Request failed.</td></tr>');
    });
}

// ── Render ───────────────────────────────────────────────────────────────────
function renderGrid(rows) {
    updateTally(rows);

    if (!rows.length) {
        $('#dedBody').html('<tr><td colspan="9"><div class="empty-state"><div class="icon">👥</div><div>No employees found.</div></div></td></tr>');
        return;
    }

    var html = '';

    rows.forEach(function (r, idx) {
        var empId = r.employeeId ?? r.EmployeeId;
        var code = r.employeeCode ?? r.EmployeeCode ?? '';
        var name = r.employeeName ?? r.EmployeeName ?? '';
        var dept = r.departmentName ?? r.DepartmentName ?? '—';
        var gross = r.grossEarnings ?? r.GrossEarnings ?? 0;
        var totDed = r.totalDeductions ?? r.TotalDeductions ?? 0;
        var net = r.netPay ?? r.NetPay ?? (gross - totDed);
        var comps = r.deductionComponents ?? r.DeductionComponents ?? [];

        var configured = comps.filter(function (c) {
            return (c.isConfigured ?? c.IsConfigured) && ((c.currentAmount ?? c.CurrentAmount) > 0);
        }).length;
        var total = comps.length;

        var badge, badgeClass;
        if (configured === 0) { badge = 'Not Set'; badgeClass = 's-none'; }
        else if (configured < total) { badge = 'Partial'; badgeClass = 's-partial'; }
        else { badge = 'Complete'; badgeClass = 's-full'; }

        // ── Main row ──
        html += '<tr class="emp-main-row" data-idx="' + idx + '">'
            + '<td><button class="btn-exp" data-idx="' + idx + '">&#9660;</button></td>'
            + '<td class="mono" style="font-size:12.5px;color:var(--muted)">' + code + '</td>'
            + '<td class="fw-semibold">' + name + '</td>'
            + '<td class="c-muted">' + dept + '</td>'
            + '<td class="text-end mono c-green">₹' + fmtN(gross) + '</td>'
            + '<td class="text-end mono c-red">' + (totDed > 0 ? '₹' + fmtN(totDed) : '<span class="c-muted">—</span>') + '</td>'
            + '<td class="text-end mono c-navy">₹' + fmtN(net) + '</td>'
            + '<td><span class="s-badge ' + badgeClass + '">' + badge + '</span></td>'
            + '<td class="text-end"><button class="btn btn-sm btn-outline-primary btn-edit" '
            + 'data-idx="' + idx + '">Edit</button></td>'
            + '</tr>';

        // ── Expand row — deduction component entry form ──
        html += '<tr class="exp-row" id="exp-' + idx + '">'
            + '<td colspan="9"><div class="exp-inner">';

        html += '<h6>Deduction Components — ' + name + ' (' + code + ')</h6>';

        if (!comps.length) {
            html += '<div class="text-muted small">No deduction components defined in SalaryComponents master.</div>';
        } else {
            html += '<div class="comp-grid" id="compGrid-' + idx + '">';
            comps.forEach(function (c) {
                var cId = c.componentId ?? c.ComponentId;
                var cName = c.componentName ?? c.ComponentName ?? 'Component';
                var cAmt = c.currentAmount ?? c.CurrentAmount ?? 0;
                var isPct = c.percentage ?? c.Percentage;
                var perOn = c.perOnComponentName ?? c.PerOnComponentName;
                var isMand = c.isMandatory ?? c.IsMandatory ?? false;
                var isConf = c.isConfigured ?? c.IsConfigured ?? false;

                var cardClass = isMand ? 'comp-card mandatory' : (isConf && cAmt > 0 ? 'comp-card has-value' : 'comp-card');
                var meta = '';
                if (isPct) meta = isPct + '% of ' + (perOn || 'base');
                else if (isMand) meta = 'Mandatory';
                else meta = 'Optional';

                html += '<div class="' + cardClass + '">'
                    + '<div class="comp-name">' + cName + (isMand ? ' <span style="color:var(--gold);font-size:10px">●</span>' : '') + '</div>'
                    + '<div class="comp-meta">' + meta + '</div>'
                    + '<div class="comp-input-wrap">'
                    + '<span class="prefix">₹</span>'
                    + '<input type="number" class="comp-input" min="0" step="0.01" '
                    + 'data-cid="' + cId + '" data-empidx="' + idx + '" '
                    + 'value="' + (cAmt > 0 ? cAmt : '') + '" '
                    + 'placeholder="0.00" />'
                    + '</div>'
                    + '</div>';
            });
            html += '</div>'; // comp-grid

            html += '<div class="d-flex align-items-center gap-3">'
                + '<button class="btn-save-row" data-empid="' + empId + '" data-idx="' + idx + '">Save Deductions</button>'
                + '<span class="save-status-' + idx + ' text-muted" style="font-size:12px"></span>'
                + '</div>';
        }

        html += '</div></td></tr>';
    });

    $('#dedBody').html(html);
    applySearch();

    // Live net-pay update as HR types amounts
    $('#dedBody').off('input.ded').on('input.ded', '.comp-input', function () {
        var idx = $(this).data('empidx');
        recalcRow(idx);
    });
}

// Recalculate and update the summary row live as HR types
function recalcRow(idx) {
    var r = _rows[idx];
    if (!r) return;
    var gross = r.grossEarnings ?? r.GrossEarnings ?? 0;
    var newDed = 0;
    $('#compGrid-' + idx + ' .comp-input').each(function () {
        newDed += parseFloat($(this).val()) || 0;
    });
    var net = gross - newDed;
    var $mainRow = $('.emp-main-row[data-idx="' + idx + '"]');
    $mainRow.find('td:nth-child(6)').html(newDed > 0 ? '<span class="mono c-red">₹' + fmtN(newDed) + '</span>' : '<span class="c-muted">—</span>');
    $mainRow.find('td:nth-child(7)').html('<span class="mono c-navy">₹' + fmtN(net) + '</span>');
}

// ── Expand / collapse ─────────────────────────────────────────────────────────
$(document).on('click', '.btn-exp, .btn-edit', function () {
    var idx = $(this).data('idx');
    var $exp = $('#exp-' + idx);
    var open = $exp.hasClass('open');

    // Close all others
    $('.exp-row.open').not($exp).removeClass('open');
    $('.btn-exp.open').not($(this)).removeClass('open').html('&#9660;');

    $exp.toggleClass('open', !open);
    var $expBtn = $('.btn-exp[data-idx="' + idx + '"]');
    $expBtn.toggleClass('open', !open).html(open ? '&#9660;' : '&#9650;');
});

// ── Save ──────────────────────────────────────────────────────────────────────
$(document).on('click', '.btn-save-row', function () {
    var empId = $(this).data('empid');
    var idx = $(this).data('idx');
    var $btn = $(this);
    var $status = $('.save-status-' + idx);

    var items = [];
    $('#compGrid-' + idx + ' .comp-input').each(function () {
        var cid = parseInt($(this).data('cid'), 10);
        var amt = parseFloat($(this).val()) || 0;
        items.push({ ComponentId: cid, Amount: amt });
    });

    if (!items.length) return;

    $btn.prop('disabled', true).text('Saving…');
    $status.text('');

    $.ajax({
        url: '/Payroll/SaveDeductionComponents',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ EmployeeId: empId, Items: items }),
        success: function (res) {
            if (res?.status) {
                $status.html('<span style="color:var(--green)">✓ Saved successfully</span>');
                // Update _rows so recalc is accurate
                var r = _rows[idx];
                if (r) {
                    var newDed = 0;
                    items.forEach(function (i) { newDed += i.Amount; });
                    if (r.totalDeductions !== undefined) r.totalDeductions = newDed;
                    if (r.TotalDeductions !== undefined) r.TotalDeductions = newDed;
                }
                updateTally(_rows);
                // Update badge
                var configuredCount = items.filter(function (i) { return i.Amount > 0; }).length;
                var badge, cls;
                if (configuredCount === 0) { badge = 'Not Set'; cls = 's-none'; }
                else if (configuredCount < items.length) { badge = 'Partial'; cls = 's-partial'; }
                else { badge = 'Complete'; cls = 's-full'; }
                $('.emp-main-row[data-idx="' + idx + '"] .s-badge')
                    .attr('class', 's-badge ' + cls).text(badge);
            } else {
                $status.html('<span style="color:var(--red)">' + (res?.message || 'Save failed') + '</span>');
            }
        },
        error: function () {
            $status.html('<span style="color:var(--red)">Request failed</span>');
        },
        complete: function () {
            $btn.prop('disabled', false).text('Save Deductions');
            setTimeout(function () { $status.text(''); }, 4000);
        }
    });
});

// ── Search ────────────────────────────────────────────────────────────────────
$('#dedSearch').on('input', applySearch);
function applySearch() {
    var q = $('#dedSearch').val().toLowerCase().trim();
    $('.emp-main-row').each(function () {
        var show = !q || $(this).text().toLowerCase().includes(q);
        var idx = $(this).data('idx');
        $(this).toggle(show);
        if (!show) $('#exp-' + idx).removeClass('open');
    });
}

// ── Tally ─────────────────────────────────────────────────────────────────────
function updateTally(rows) {
    var totalDed = 0, totalNet = 0, configured = 0;
    rows.forEach(function (r) {
        var ded = r.totalDeductions ?? r.TotalDeductions ?? 0;
        var net = r.netPay ?? r.NetPay ?? ((r.grossEarnings ?? r.GrossEarnings ?? 0) - ded);
        totalDed += ded;
        totalNet += net;
        if (ded > 0) configured++;
    });
    $('#tallyEmployees').text(rows.length);
    $('#tallyConfigured').text(configured + ' / ' + rows.length);
    $('#tallyTotalDed').text(rows.length ? '₹' + fmtN(totalDed) : '—');
    $('#tallyNetPay').text(rows.length ? '₹' + fmtN(totalNet) : '—');
}

function fmtN(n) {
    return parseFloat(n || 0).toLocaleString('en-IN', { maximumFractionDigits: 2 });
}