$(function () {
    $(document).on("cookieDataLoaded", function () {
        const user = window.globalUserData || {};
        const userRole = user.role;

        if (userRole === "Manager" || userRole === "Team Lead") {
            $("#approvalTabContainer").show();
        } else {
            $("#approvalTabContainer").hide();
        }

    });

    // ════════════════════════════════════════════════════════════
    // PUBLIC API (Expose functions for Razor onclick bindings)
    // ════════════════════════════════════════════════════════════

    window.emp = {
        changeWeek: empChangeWeek,
        onYearChange: empOnYearChange,
        refresh: empRefresh,
        viewHistory: empViewHistory,
        addRow: empAddRow,
        uploadAttachment: empUploadAttachment,
        saveSheet: empSaveSheet,
        submitSheet: empSubmitSheet
    };

    window.mgr = {
        changeWeek: mgrChangeWeek,
        onYearChange: mgrOnYearChange,
        refresh: mgrRefresh,
        setFilter: mgrSetFilter,
        approveReject: mgrApproveReject
    };

    // ════════════════════════════════════════════════════════════
    //  INIT
    // ════════════════════════════════════════════════════════════
    empInit();
});

// ════════════════════════════════════════════════════════════
//  SHARED PURE HELPERS  (true globals, no namespace)
// ════════════════════════════════════════════════════════════
const DAY_NAMES = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

function getCurrentMonday() {
    const now = new Date();
    const day = now.getDay();
    const diff = (day === 0) ? -6 : 1 - day;
    const mon = new Date(now);
    mon.setDate(now.getDate() + diff);
    mon.setHours(0, 0, 0, 0);
    return mon;
}

function getWeekDates(offset) {
    const monday = getCurrentMonday();
    monday.setDate(monday.getDate() + offset * 7);
    return Array.from({ length: 7 }, (_, i) => {
        const d = new Date(monday);
        d.setDate(monday.getDate() + i);
        return d;
    });
}

function fmt(d) {
    return d.toLocaleDateString('en-GB', {
        day: '2-digit', month: 'short', year: 'numeric'
    }).replace(/ /g, '-');
}

function isoDate(d) {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

function getFirstMondayOfYear(year) {
    const jan1 = new Date(year, 0, 1);
    const day = jan1.getDay();
    const diff = (day === 0) ? 1 : (day === 1) ? 0 : (8 - day);
    const mon = new Date(year, 0, 1 + diff);
    mon.setHours(0, 0, 0, 0);
    return mon;
}

function showToast(msg) {
    const t = document.getElementById('tsToast');
    t.textContent = msg;
    t.classList.add('show');
    setTimeout(() => t.classList.remove('show'), 2800);
}

// ════════════════════════════════════════════════════════════
//  TAB SWITCHER
// ════════════════════════════════════════════════════════════
function switchTab(name, btn) {
    document.querySelectorAll('.ts-tab-pane').forEach(p => p.classList.remove('active'));
    document.querySelectorAll('.ts-tab-btn').forEach(b => b.classList.remove('active'));
    document.getElementById(`tab-${name}`).classList.add('active');
    btn.classList.add('active');

    if (name === 'manager' && !mgr_loaded) {
        mgr_loaded = true;
        mgrUpdateWeekLabel();
        mgrLoad();
    }
}

// ════════════════════════════════════════════════════════════
//  EMPLOYEE STATE  (prefixed globals)
// ════════════════════════════════════════════════════════════
var emp_weekOffset = 0;
var emp_timesheetId = 0;
var emp_rows = [];
var emp_dropdowns = { projects: [], activities: [], categories: [], shifts: [] };
var emp_HOLIDAYS = new Set();

// ── Helpers ──────────────────────────────────────────────────
function empGetDayType(d) {
    const iso = isoDate(d);
    const day = d.getDay();
    const today = isoDate(new Date());
    if (emp_HOLIDAYS.has(iso)) return 'holiday';
    if (day === 0 || day === 6) return 'weekend';
    if (iso === today) return 'today';
    return 'present';
}

function empUpdateWeekLabel() {
    const dates = getWeekDates(emp_weekOffset);
    const label = `${fmt(dates[0])} – ${fmt(dates[6])}`;
    document.getElementById('emp-weekRange').innerText = label;
    document.getElementById('emp-weekSub').innerText = label;
}

function empSetLoading(show) {
    document.getElementById('emp-loadingMsg').style.display = show ? 'block' : 'none';
    document.getElementById('emp-tableSection').style.display = show ? 'none' : 'block';
    document.getElementById('emp-noDataMsg').style.display = 'none';
}

function empShowNoData() {
    document.getElementById('emp-loadingMsg').style.display = 'none';
    document.getElementById('emp-tableSection').style.display = 'none';
    document.getElementById('emp-noDataMsg').style.display = 'block';
}

function empBuildOptions(items, selectedId) {
    return items.map(i =>
        `<option value="${i.id}"${String(i.id) === String(selectedId) ? ' selected' : ''}>${i.displayName}</option>`
    ).join('');
}

function empMakeDefaultRow() {
    const lastRow = emp_rows.length ? emp_rows[emp_rows.length - 1] : null;
    return {
        rowId: 0,
        projectId: emp_dropdowns.projects[0]?.id ?? null,
        activityId: emp_dropdowns.activities[0]?.id ?? null,
        categoryId: emp_dropdowns.categories[0]?.id ?? null,
        shiftId: emp_dropdowns.shifts[0]?.id ?? null,
        remarks: lastRow?.remarks || '',
        hours: Array(7).fill(null)
    };
}

// ── Load ─────────────────────────────────────────────────────
async function empLoad() {
    empSetLoading(true);
    try {
        const res = await fetch(`/TimeSheet/GetTimesheet?weekOffset=${emp_weekOffset}`);
        const result = await res.json();
        if (!result.status || !result.data) { empShowNoData(); return; }

        const data = result.data;
        if (data.projects?.length) emp_dropdowns.projects = data.projects;
        if (data.activities?.length) emp_dropdowns.activities = data.activities;
        if (data.categories?.length) emp_dropdowns.categories = data.categories;
        if (data.shifts?.length) emp_dropdowns.shifts = data.shifts;

        emp_HOLIDAYS = new Set((data.holidays || []).map(h => h.split('T')[0]));

        const h = data.header || {};
        emp_timesheetId = h.timesheetId || 0;

        document.getElementById('emp-empInfo').innerText = `${h.employeeCode || ''} ${h.employeeName || ''}`.trim() || '--';
        document.getElementById('emp-approverInfo').innerText = `${h.approverCode || ''} ${h.approverName || ''}`.trim() || '--';
        document.getElementById('emp-empNameFull').innerText = h.employeeName || '--';
        document.getElementById('emp-approverFull').innerText = h.approverCode ? `${h.approverCode} : ${h.approverName}` : '--';

        const statusMap = { DRAFT: 'Draft', PENDING: 'Pending with User', APPROVED: 'Approved', REJECTED: 'Rejected' };
        document.getElementById('emp-statusBadge').innerText = statusMap[h.statusCode] || h.status || 'Draft';

        empUpdateWeekLabel();

        emp_rows = (data.rows || []).map(r => ({
            rowId: r.rowId || 0,
            projectId: r.projectId || null,
            activityId: r.activityId || null,
            categoryId: r.categoryId || null,
            shiftId: r.shiftId || null,
            remarks: r.remarks || '',
            hours: Array.isArray(r.hours) && r.hours.length === 7
                ? r.hours.map(v => (v === null || v === undefined) ? null : parseFloat(v))
                : Array(7).fill(null)
        }));

        if (emp_rows.length === 0) emp_rows.push(empMakeDefaultRow());

        document.getElementById('emp-loadingMsg').style.display = 'none';
        document.getElementById('emp-tableSection').style.display = 'block';
        document.getElementById('emp-noDataMsg').style.display = 'none';
        empRenderAll();

    } catch (err) {
        console.error('empLoad:', err);
        empShowNoData();
    }
}

// ── Render ───────────────────────────────────────────────────
function empRenderAll() {
    const dates = getWeekDates(emp_weekOffset);
    empRenderHeaders(dates);
    empRenderBody(dates);
    empRenderSummary(dates);
}

function empRenderHeaders(dates) {
    const dateRow = document.getElementById('emp-dateRow');
    dateRow.querySelectorAll('th.date-th').forEach(t => t.remove());
    const remarksTh = dateRow.querySelectorAll('th')[1];
    dates.forEach(d => {
        const th = document.createElement('th');
        th.className = `date-th ${empGetDayType(d)}`;
        th.textContent = fmt(d);
        dateRow.insertBefore(th, remarksTh);
    });

    const dayRow = document.getElementById('emp-dayRow');
    dayRow.innerHTML = '';
    dates.forEach(d => {
        const th = document.createElement('th');
        const t = empGetDayType(d);
        th.textContent = DAY_NAMES[d.getDay()];
        th.className = (t === 'present') ? '' : t;
        dayRow.appendChild(th);
    });
}

function empRenderBody(dates) {
    const tbody = document.getElementById('emp-timesheetBody');
    tbody.innerHTML = '';

    emp_rows.forEach((row, ri) => {
        const tr = document.createElement('tr');

        const projTd = document.createElement('td');
        projTd.className = 'proj-cell';
        projTd.innerHTML = `
            <select class="proj-select" onchange="empSetField(${ri},'projectId',+this.value)">
                <option value="">-- Select Project --</option>
                ${empBuildOptions(emp_dropdowns.projects, row.projectId)}
            </select>
            <select class="proj-select" onchange="empSetField(${ri},'activityId',+this.value)">
                <option value="">-- Activity --</option>
                ${empBuildOptions(emp_dropdowns.activities, row.activityId)}
            </select>
            <select class="proj-select" onchange="empSetField(${ri},'categoryId',+this.value)">
                <option value="">-- Category --</option>
                ${empBuildOptions(emp_dropdowns.categories, row.categoryId)}
            </select>
            <select class="proj-select" onchange="empSetField(${ri},'shiftId',this.value)">
                <option value="">-- Shift --</option>
                ${empBuildOptions(emp_dropdowns.shifts, row.shiftId)}
            </select>`;
        tr.appendChild(projTd);

        dates.forEach((d, di) => {
            const type = empGetDayType(d);
            const rawVal = row.hours[di];
            const hasHours = rawVal !== null && rawVal !== undefined && parseFloat(rawVal) > 0;
            const isAbsent = !hasHours && (type === 'present' || type === 'today');
            const cellType = (type === 'present' && isAbsent) ? 'absent' : type;
            const dis = (type === 'weekend' || type === 'holiday') ? 'disabled' : '';
            const val = rawVal != null ? parseFloat(rawVal).toFixed(2) : '';

            let tag = '';
            if (type === 'holiday') tag = `<span class="day-tag">Holiday</span>`;
            else if (type === 'weekend') tag = `<span class="day-tag">Weekend</span>`;
            else if (type === 'today' && hasHours) tag = `<span class="day-tag">Today ✓</span>`;
            else if (type === 'today') tag = `<span class="day-tag">Today</span>`;
            else if (isAbsent) tag = `<span class="day-tag">Absent</span>`;
            else tag = `<span class="day-tag">Present</span>`;

            const td = document.createElement('td');
            td.className = `day-cell ${cellType}`;
            td.innerHTML = `
                <input class="time-input" type="number"
                    min="0" max="24" step="0.5"
                    value="${val}" placeholder="0.00" ${dis}
                    onchange="empOnHoursChange(${ri},${di},this.value)">
                ${tag}`;
            tr.appendChild(td);
        });

        const remTd = document.createElement('td');
        remTd.className = 'remarks-cell';
        remTd.innerHTML = `<textarea placeholder="Enter remarks..." oninput="empSetField(${ri},'remarks',this.value)">${row.remarks}</textarea>`;
        tr.appendChild(remTd);

        const actTd = document.createElement('td');
        actTd.className = 'action-cell';
        actTd.innerHTML = `
            <div class="action-icons">
                <button class="icon-btn ref-btn" title="Reset row" onclick="empResetRow(${ri})">&#8635;</button>
                <button class="icon-btn del-btn" title="Delete row" onclick="empDeleteRow(${ri})">&#128465;</button>
            </div>`;
        tr.appendChild(actTd);
        tbody.appendChild(tr);
    });

    // Total row
    const totalTr = document.createElement('tr');
    totalTr.className = 'total-row';
    const lbl = document.createElement('td');
    lbl.className = 'proj-cell';
    lbl.innerHTML = '<strong style="font-size:12px;">Total Hours</strong>';
    totalTr.appendChild(lbl);
    dates.forEach((d, di) => {
        const td = document.createElement('td');
        td.className = `day-cell ${empGetDayType(d)}`;
        td.style.textAlign = 'center';
        const total = emp_rows.reduce((s, r) => s + (parseFloat(r.hours[di]) || 0), 0);
        td.innerHTML = `<span class="total-hours">${total > 0 ? total.toFixed(2) : '—'}</span>`;
        totalTr.appendChild(td);
    });
    const remT = document.createElement('td'); remT.className = 'remarks-cell';
    const actT = document.createElement('td'); actT.className = 'action-cell';
    totalTr.appendChild(remT);
    totalTr.appendChild(actT);
    tbody.appendChild(totalTr);
}

function empRenderSummary(dates) {
    if (!dates) dates = getWeekDates(emp_weekOffset);
    let totalHrs = 0, presentDays = 0, absentDays = 0, holidays = 0, weekends = 0;
    dates.forEach((d, di) => {
        const type = empGetDayType(d);
        if (type === 'holiday') { holidays++; return; }
        if (type === 'weekend') { weekends++; return; }
        const dayTotal = emp_rows.reduce((s, r) => s + (parseFloat(r.hours[di]) || 0), 0);
        if (dayTotal > 0) { presentDays++; totalHrs += dayTotal; } else absentDays++;
    });
    document.getElementById('emp-summaryBar').innerHTML = `
        <div class="s-item">Total Hours: <span>${totalHrs.toFixed(2)}</span></div>
        <div class="s-item">Present Days: <span>${presentDays}</span></div>
        <div class="s-item">Absent Days: <span>${absentDays}</span></div>
        <div class="s-item">Holidays: <span>${holidays}</span></div>
        <div class="s-item">Weekends: <span>${weekends}</span></div>`;
}

// ── Public employee actions (called from HTML) ────────────────
function empSetField(ri, field, val) {
    if (emp_rows[ri]) emp_rows[ri][field] = val;
}

function empOnHoursChange(ri, di, value) {
    emp_rows[ri].hours[di] = value ? parseFloat(value) : null;
    const dates = getWeekDates(emp_weekOffset);
    const type = empGetDayType(dates[di]);
    const hasHours = value && parseFloat(value) > 0;
    const isAbsent = !hasHours && type === 'present';
    const cellType = isAbsent ? 'absent' : type;

    const bodyRows = document.querySelectorAll('#emp-timesheetBody tr:not(.total-row)');
    const targetRow = bodyRows[ri];
    if (targetRow) {
        const cell = targetRow.querySelectorAll('.day-cell')[di];
        if (cell) {
            cell.className = `day-cell ${cellType}`;
            const tag = cell.querySelector('.day-tag');
            if (tag) {
                if (type === 'holiday') tag.textContent = 'Holiday';
                else if (type === 'weekend') tag.textContent = 'Weekend';
                else if (type === 'today' && hasHours) tag.textContent = 'Today ✓';
                else if (type === 'today') tag.textContent = 'Today';
                else if (isAbsent) tag.textContent = 'Absent';
                else tag.textContent = 'Present';
            }
        }
        const totals = document.querySelectorAll('#emp-timesheetBody tr.total-row .day-cell');
        if (totals[di]) {
            const total = emp_rows.reduce((s, r) => s + (parseFloat(r.hours[di]) || 0), 0);
            const span = totals[di].querySelector('.total-hours');
            if (span) span.textContent = total > 0 ? total.toFixed(2) : '—';
        }
    }
    empRenderSummary();
}

function empChangeWeek(dir) {
    emp_weekOffset += dir;
    empUpdateWeekLabel();
    const year = getWeekDates(emp_weekOffset)[0].getFullYear().toString();
    const sel = document.getElementById('emp-yearSelect');
    if ([...sel.options].some(o => o.value === year)) sel.value = year;
    empLoad();
}

function empOnYearChange(year) {
    const firstMon = getFirstMondayOfYear(parseInt(year));
    const curMon = getCurrentMonday();
    emp_weekOffset = Math.round((firstMon - curMon) / (7 * 86400000));
    empUpdateWeekLabel();
    empLoad();
}

function empAddRow() {
    emp_rows.push(empMakeDefaultRow());
    empRenderAll();
    showToast('✅ New row added');
}

async function empSaveSheet() {
    if (emp_rows.length === 0) { showToast('⚠️ Nothing to save'); return; }
    const payload = {
        timesheetId: emp_timesheetId || 0,
        weekOffset: emp_weekOffset || 0,
        rows: emp_rows.map((r, i) => ({
            rowId: r.rowId || 0,
            projectId: r.projectId || 0,
            activityId: r.activityId || 0,
            categoryId: r.categoryId || 0,
            shiftId: r.shiftId || '',
            remarks: r.remarks || '',
            sortOrder: i + 1,
            hours: r.hours.map(h => h ?? null)
        }))
    };
    try {
        const res = await fetch('/TimeSheet/SaveTimesheet', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
        const result = await res.json();
        if (!result.status) { showToast('❌ ' + (result.message || 'Save failed')); return; }
        if (result.data?.timesheetId[0]) emp_timesheetId = result.data.timesheetId[0];
        showToast('💾 Saved successfully');
    } catch (err) { console.error('empSaveSheet:', err); showToast('❌ Save failed'); }
}

async function empSubmitSheet() {
    if (!confirm('Submit timesheet for approval?')) return;
    await empSaveSheet();
    try {
        const res = await fetch('/TimeSheet/SubmitTimesheet', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ timesheetId: emp_timesheetId }) });
        const result = await res.json();
        if (!result.status) { showToast('❌ ' + (result.message || 'Submit failed')); return; }
        document.getElementById('emp-statusBadge').innerText = 'Pending with User';
        showToast('✅ Submitted for approval');
    } catch (err) { console.error('empSubmitSheet:', err); showToast('❌ Submit failed'); }
}

async function empDeleteRow(index) {
    if (emp_rows.length === 1) { showToast('⚠️ At least one row is required'); return; }
    const row = emp_rows[index];
    if (!row.rowId) { emp_rows.splice(index, 1); empRenderAll(); showToast('🗑️ Row removed'); return; }
    if (!confirm('Delete this row?')) return;
    try {
        const res = await fetch(`/TimeSheet/DeleteRow?rowId=${row.rowId}`, { method: 'POST' });
        const result = await res.json();
        if (!result.status) { showToast('❌ ' + (result.message || 'Delete failed')); return; }
        emp_rows.splice(index, 1); empRenderAll(); showToast('🗑️ Row deleted');
    } catch (err) { console.error('empDeleteRow:', err); showToast('❌ Delete failed'); }
}

async function empResetRow(index) {
    const row = emp_rows[index];
    if (!row.rowId) { emp_rows[index].hours = Array(7).fill(null); emp_rows[index].remarks = ''; empRenderAll(); showToast('🔄 Row reset'); return; }
    try {
        const res = await fetch(`/TimeSheet/ResetRow?rowId=${row.rowId}`, { method: 'POST' });
        const result = await res.json();
        if (!result.status) { showToast('❌ ' + (result.message || 'Reset failed')); return; }
        emp_rows[index].hours = Array(7).fill(null); emp_rows[index].remarks = ''; empRenderAll(); showToast('🔄 Row reset');
    } catch (err) { console.error('empResetRow:', err); showToast('❌ Reset failed'); }
}

function empRefresh() {
    const spin = document.getElementById('emp-spinIcon');
    spin.style.transition = 'transform 0.65s ease';
    spin.style.transform = 'rotate(360deg)';
    setTimeout(() => { spin.style.transform = ''; spin.style.transition = ''; }, 700);
    empLoad();
}

async function empViewHistory() {
    if (!emp_timesheetId) { showToast('⚠️ No saved timesheet yet'); return; }
    document.getElementById('historyBody').innerHTML = '<p class="text-center text-muted">Loading...</p>';
    new bootstrap.Modal(document.getElementById('historyModal')).show();
    try {
        const res = await fetch(`/TimeSheet/GetHistory?timesheetId=${emp_timesheetId}`);
        const result = await res.json();
        if (!result.status || !result.data?.length) {
            document.getElementById('historyBody').innerHTML = '<p class="text-center text-muted">No history found.</p>';
            return;
        }
        const trs = result.data.map(l => `
            <tr>
                <td><span class="badge bg-secondary">${l.actionType ?? '—'}</span></td>
                <td>${l.actionByName ?? '—'} <small class="text-muted">(${l.actionByCode ?? ''})</small></td>
                <td>${l.actionOn ? new Date(l.actionOn).toLocaleString() : '—'}</td>
                <td>${l.remarks ?? '—'}</td>
            </tr>`).join('');
        document.getElementById('historyBody').innerHTML = `
            <table class="table table-sm table-bordered table-hover">
                <thead class="table-dark"><tr><th>Action</th><th>By</th><th>Date / Time</th><th>Remarks</th></tr></thead>
                <tbody>${trs}</tbody>
            </table>`;
    } catch { document.getElementById('historyBody').innerHTML = '<p class="text-center text-danger">Failed to load history.</p>'; }
}

function empUploadAttachment() { showToast('📎 Upload feature coming soon'); }

function empInit() { empUpdateWeekLabel(); empLoad(); }

// ════════════════════════════════════════════════════════════
//  MANAGER STATE  (prefixed globals)
// ════════════════════════════════════════════════════════════
var mgr_weekOffset = 0;
var mgr_loaded = false;
var mgr_teamList = [];
var mgr_selectedEmpId = null;
var mgr_selectedTsId = null;
var mgr_currentFilter = 'ALL';
var mgr_HOLIDAYS = new Set();

// ── Helpers ──────────────────────────────────────────────────
function mgrGetDayType(d) {
    const iso = isoDate(d);
    const day = d.getDay();
    const today = isoDate(new Date());
    if (mgr_HOLIDAYS.has(iso)) return 'holiday';
    if (day === 0 || day === 6) return 'weekend';
    if (iso === today) return 'today';
    return 'present';
}

function mgrUpdateWeekLabel() {
    const dates = getWeekDates(mgr_weekOffset);
    const label = `${fmt(dates[0])} – ${fmt(dates[6])}`;
    document.getElementById('mgr-weekRange').innerText = label;
}

function mgrSetLoading(show) {
    document.getElementById('mgr-loadingMsg').style.display = show ? 'block' : 'none';
    document.getElementById('mgr-mainSection').style.display = show ? 'none' : 'block';
    document.getElementById('mgr-noDataMsg').style.display = 'none';
}

function mgrShowNoData() {
    document.getElementById('mgr-loadingMsg').style.display = 'none';
    document.getElementById('mgr-mainSection').style.display = 'none';
    document.getElementById('mgr-noDataMsg').style.display = 'block';
}

function mgrLabelFor(code) {
    return { DRAFT: 'Draft', PENDING: 'Pending', APPROVED: 'Approved', REJECTED: 'Rejected' }[code] || 'Draft';
}

function mgrClearDetail() {
    mgr_selectedEmpId = null;
    mgr_selectedTsId = null;
    document.getElementById('mgr-noEmpMsg').style.display = 'block';
    document.getElementById('mgr-timesheetDetail').style.display = 'none';
}

// ── Load ─────────────────────────────────────────────────────
async function mgrLoad() {
    mgrSetLoading(true);
    try {
        const res = await fetch(`/TimeSheet/GetTeamTimesheets?weekOffset=${mgr_weekOffset}`);
        const result = await res.json();
        if (!result.status || !result.data) { mgrShowNoData(); return; }

        mgr_HOLIDAYS = new Set((result.data.holidays || []).map(h => h.split('T')[0]));
        mgr_teamList = result.data.team || [];

        if (mgr_teamList.length === 0) { mgrShowNoData(); return; }

        mgrSetLoading(false);
        mgrRenderCards();
        mgrClearDetail();
    } catch (err) {
        console.error('mgrLoad:', err);
        mgrShowNoData();
    }
}

// ── Render ───────────────────────────────────────────────────
function mgrRenderCards() {
    const filtered = mgr_currentFilter === 'ALL'
        ? mgr_teamList
        : mgr_teamList.filter(e => e.statusCode === mgr_currentFilter);

    const container = document.getElementById('mgr-empCards');
    container.innerHTML = '';

    if (filtered.length === 0) {
        container.innerHTML = `<div style="color:#7a94aa;font-size:13px;padding:10px 0;">No employees with status "${mgr_currentFilter}" this week.</div>`;
        return;
    }

    filtered.forEach(emp => {
        const card = document.createElement('div');
        card.className = `emp-card${emp.employeeId === mgr_selectedEmpId ? ' selected' : ''}`;
        card.onclick = () => mgrSelectEmployee(emp);
        card.innerHTML = `
            <div class="emp-card-name">${emp.employeeName || '—'}</div>
            <div class="emp-card-code">${emp.employeeCode || ''}</div>
            <div class="emp-card-status">
                <span class="status-pill ${emp.statusCode || 'DRAFT'}">${mgrLabelFor(emp.statusCode)}</span>
            </div>`;
        container.appendChild(card);
    });
}

async function mgrSelectEmployee(empObj) {
    mgr_selectedEmpId = empObj.employeeId;
    mgr_selectedTsId = empObj.timesheetId || 0;

    mgrRenderCards();

    document.getElementById('mgr-noEmpMsg').style.display = 'none';
    document.getElementById('mgr-timesheetDetail').style.display = 'block';
    document.getElementById('mgr-detailEmpName').innerText = empObj.employeeName || '—';
    document.getElementById('mgr-detailEmpCode').innerText = empObj.employeeCode || '';

    const sc = empObj.statusCode || 'DRAFT';
    const pill = document.getElementById('mgr-detailStatus');
    pill.className = `status-pill ${sc}`;
    pill.innerText = mgrLabelFor(sc);
    document.getElementById('mgr-detailWeekLbl').innerText = document.getElementById('mgr-weekRange').innerText;

    document.getElementById('mgr-approveBar').style.display = (sc === 'PENDING') ? 'flex' : 'none';

    document.getElementById('mgr-timesheetBody').innerHTML =
        '<tr><td colspan="20" style="text-align:center;padding:20px;color:#2e5f8a;">⏳ Loading...</td></tr>';
    try {
        const res = await fetch(`/TimeSheet/GetTimesheetById?timesheetId=${mgr_selectedTsId}&employeeId=${empObj.employeeId}&weekOffset=${mgr_weekOffset}`);
        const result = await res.json();
        if (!result.status || !result.data) {
            document.getElementById('mgr-timesheetBody').innerHTML =
                '<tr><td colspan="20" style="text-align:center;padding:20px;color:#888;">No data</td></tr>';
            return;
        }
        mgrRenderDetail(result.data);
    } catch (err) {
        console.error('mgrSelectEmployee:', err);
        document.getElementById('mgr-timesheetBody').innerHTML =
            '<tr><td colspan="20" style="text-align:center;padding:20px;color:#c00;">Failed to load</td></tr>';
    }
}

function mgrRenderDetail(data) {
    const dates = getWeekDates(mgr_weekOffset);
    const rows = (data.rows || []).map(r => ({
        projectName: r.projectName || '—',
        activityName: r.activityName || '—',
        categoryName: r.categoryName || '—',
        shiftName: r.shiftName || '—',
        remarks: r.remarks || '',
        hours: Array.isArray(r.hours) && r.hours.length === 7
            ? r.hours.map(v => (v === null || v === undefined) ? null : parseFloat(v))
            : Array(7).fill(null)
    }));

    // Headers
    const dateRow = document.getElementById('mgr-dateRow');
    dateRow.querySelectorAll('th.date-th').forEach(t => t.remove());
    const remarksTh = dateRow.querySelectorAll('th')[1];
    dates.forEach(d => {
        const th = document.createElement('th');
        th.className = `date-th ${mgrGetDayType(d)}`;
        th.textContent = fmt(d);
        dateRow.insertBefore(th, remarksTh);
    });

    const dayRow = document.getElementById('mgr-dayRow');
    dayRow.innerHTML = '';
    dates.forEach(d => {
        const th = document.createElement('th');
        const t = mgrGetDayType(d);
        th.textContent = DAY_NAMES[d.getDay()];
        th.className = (t === 'present') ? '' : t;
        dayRow.appendChild(th);
    });

    // Body (read-only)
    const tbody = document.getElementById('mgr-timesheetBody');
    tbody.innerHTML = '';

    rows.forEach(row => {
        const tr = document.createElement('tr');

        const projTd = document.createElement('td');
        projTd.className = 'proj-cell';
        projTd.style.fontSize = '11px';
        projTd.innerHTML = `
            <div style="color:#1a3a5c;font-weight:600;">${row.projectName}</div>
            <div style="color:#4a6080;">${row.activityName} · ${row.categoryName} · ${row.shiftName}</div>`;
        tr.appendChild(projTd);

        dates.forEach((d, di) => {
            const type = mgrGetDayType(d);
            const rawVal = row.hours[di];
            const hasHours = rawVal !== null && rawVal !== undefined && parseFloat(rawVal) > 0;
            const isAbsent = !hasHours && (type === 'present' || type === 'today');
            const cellType = (type === 'present' && isAbsent) ? 'absent' : type;
            const val = rawVal != null && parseFloat(rawVal) > 0 ? parseFloat(rawVal).toFixed(2) : '';

            const td = document.createElement('td');
            td.className = `day-cell ${cellType}`;
            td.style.fontSize = '12px';
            td.style.fontFamily = "'IBM Plex Mono', monospace";
            td.innerHTML = val
                ? `<strong>${val}</strong>`
                : (type === 'weekend' || type === 'holiday')
                    ? `<span style="color:#aaa;">—</span>`
                    : `<span style="color:#f28b5e;font-size:10px;">—</span>`;
            tr.appendChild(td);
        });

        const remTd = document.createElement('td');
        remTd.className = 'remarks-cell';
        remTd.style.fontSize = '11px';
        remTd.textContent = row.remarks || '';
        tr.appendChild(remTd);

        tbody.appendChild(tr);
    });

    // Total row
    const totalTr = document.createElement('tr');
    totalTr.className = 'total-row';
    const lbl = document.createElement('td');
    lbl.className = 'proj-cell';
    lbl.innerHTML = '<strong style="font-size:12px;">Total Hours</strong>';
    totalTr.appendChild(lbl);
    dates.forEach((d, di) => {
        const td = document.createElement('td');
        td.className = `day-cell ${mgrGetDayType(d)}`;
        td.style.textAlign = 'center';
        const total = rows.reduce((s, r) => s + (parseFloat(r.hours[di]) || 0), 0);
        td.innerHTML = `<span class="total-hours">${total > 0 ? total.toFixed(2) : '—'}</span>`;
        totalTr.appendChild(td);
    });
    const remT = document.createElement('td'); remT.className = 'remarks-cell';
    totalTr.appendChild(remT);
    tbody.appendChild(totalTr);

    // Summary
    let totalHrs = 0, presentDays = 0, absentDays = 0, holidays = 0, weekends = 0;
    dates.forEach((d, di) => {
        const type = mgrGetDayType(d);
        if (type === 'holiday') { holidays++; return; }
        if (type === 'weekend') { weekends++; return; }
        const dayTotal = rows.reduce((s, r) => s + (parseFloat(r.hours[di]) || 0), 0);
        if (dayTotal > 0) { presentDays++; totalHrs += dayTotal; } else absentDays++;
    });
    document.getElementById('mgr-summaryBar').innerHTML = `
        <div class="s-item">Total Hours: <span>${totalHrs.toFixed(2)}</span></div>
        <div class="s-item">Present Days: <span>${presentDays}</span></div>
        <div class="s-item">Absent Days: <span>${absentDays}</span></div>
        <div class="s-item">Holidays: <span>${holidays}</span></div>
        <div class="s-item">Weekends: <span>${weekends}</span></div>`;
}

// ── Public manager actions (called from HTML) ─────────────────
function mgrSetFilter(filter, btn) {
    mgr_currentFilter = filter;
    document.querySelectorAll('.mgr-filter-btns .filter-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
    mgrRenderCards();
    mgrClearDetail();
}

function mgrChangeWeek(dir) {
    mgr_weekOffset += dir;
    mgrUpdateWeekLabel();
    const year = getWeekDates(mgr_weekOffset)[0].getFullYear().toString();
    const sel = document.getElementById('mgr-yearSelect');
    if ([...sel.options].some(o => o.value === year)) sel.value = year;
    mgrClearDetail();
    mgrLoad();
}

function mgrOnYearChange(year) {
    const firstMon = getFirstMondayOfYear(parseInt(year));
    const curMon = getCurrentMonday();
    mgr_weekOffset = Math.round((firstMon - curMon) / (7 * 86400000));
    mgrUpdateWeekLabel();
    mgrClearDetail();
    mgrLoad();
}

function mgrRefresh() {
    const spin = document.getElementById('mgr-spinIcon');
    spin.style.transition = 'transform 0.65s ease';
    spin.style.transform = 'rotate(360deg)';
    setTimeout(() => { spin.style.transform = ''; spin.style.transition = ''; }, 700);
    mgrLoad();
}

async function mgrApproveReject(action) {
    if (!mgr_selectedTsId) { showToast('⚠️ No timesheet selected'); return; }
    const remarks = document.getElementById('mgr-approveRemarks').value.trim();
    if (action === 'REJECTED' && !remarks) { showToast('⚠️ Please enter rejection remarks'); return; }
    if (!confirm(`${action === 'APPROVED' ? 'Approve' : 'Reject'} this timesheet?`)) return;

    try {
        const res = await fetch('/TimeSheet/ApproveReject', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ timesheetId: mgr_selectedTsId, action, remarks })
        });
        const result = await res.json();
        if (!result.status) { showToast('❌ ' + (result.message || 'Action failed')); return; }

        showToast(action === 'APPROVED' ? '✅ Timesheet approved' : '🚫 Timesheet rejected');

        const entry = mgr_teamList.find(e => e.timesheetId === mgr_selectedTsId);
        if (entry) entry.statusCode = action;

        mgrRenderCards();
        document.getElementById('mgr-approveBar').style.display = 'none';
        const sc = document.getElementById('mgr-detailStatus');
        sc.className = `status-pill ${action}`;
        sc.innerText = mgrLabelFor(action);
        document.getElementById('mgr-approveRemarks').value = '';
    } catch (err) {
        console.error('mgrApproveReject:', err);
        showToast('❌ Action failed');
    }
}