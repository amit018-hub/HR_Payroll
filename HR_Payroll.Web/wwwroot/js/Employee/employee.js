var savedSteps = { step1: false, step2: false, step3: false };
$(function () {

    const steps = ["#step1", "#step2", "#step3", "#step4"];
    showStep(0);

    // track which steps were saved explicitly
    

    function showStep(stepIndex) {

        $(".tp").removeClass("show active");

        $(steps[stepIndex]).addClass("show active");

        $(".nlink").each(function (idx) {
            $(this).toggleClass("active", idx === stepIndex);
        });
    }
    window.showStep = showStep;

    // Next/Prev should only navigate. Saving is performed by per-tab Save buttons.
    $("#step1Next").on("click", () => showStep(1));
    $("#step2Prev").on("click", () => showStep(0));
    $("#step2Next").on("click", () => showStep(2));
    $("#step3Prev").on("click", () => showStep(1));
    $("#step3Next").on("click", () => showStep(3));
    $("#step4Prev").on("click", () => showStep(2));

    getDepartments();

    // When navigating to payroll or bank via Next, ensure components get fetched for breakup preview.
    $('#step2Next, #step3Next').on('click', function () {
        fetchSalaryComponents(function () {
            let salary = parseFloat($('#salaryPerMonth').val()) || 0;
            renderSalaryBreakup(salary);
        });
    });


    function debounce(fn, wait) {
        let t;
        return function () {
            const ctx = this, args = arguments;
            clearTimeout(t);
            t = setTimeout(() => fn.apply(ctx, args), wait);
        };
    }

    // when salary changes, ensure components loaded then render
    const onSalaryChange = debounce(function () {
        const salary = parseFloat($('#salaryPerMonth').val()) || 0;
        $('#salaryPerYear').val(salary > 0 ? (salary * 12).toFixed(2) : '');
        ensureComponentsAndRender(salary);
        updateTotals();
    }, 300);

    $('#salaryPerMonth').on('input', onSalaryChange);

    $('#salaryPerYear').on('input', debounce(function () {
        var perYear = parseFloat($(this).val()) || 0;
        $('#salaryPerMonth').val(perYear > 0 ? (perYear / 12).toFixed(2) : '');
        const salary = parseFloat($('#salaryPerMonth').val()) || 0;
        ensureComponentsAndRender(salary);
        updateTotals();
    }, 300));

    // Clear validation error when user edits inputs/selects/textareas in steps
    $('#step1, #step2, #step3, #step4').on('input change', 'input, select, textarea', function () {
        clearFieldError($(this));
    });
});
function ensureComponentsAndRender(salary) {
    if (salary <= 0) {
        $('#salaryBreakupTable').html('<div class="text-danger">Enter Salary Per Month to view breakup.</div>');
        $('#selectedTotal').text('0.00');
        $('#remainingAmount').text('0.00');
        return;
    }

    if (Array.isArray(salaryComponents) && salaryComponents.length > 0) {
        renderSalaryBreakup(salary);
        return;
    }

    // fetch then render
    fetchSalaryComponents(function () {
        renderSalaryBreakup(salary);
    });
}
function getDepartments() {
    $.ajax({
        url: '/Employee/GetDepartments',
        type: 'GET',
        success: function (response) {
            var $deptSelect = $('#department');
            $deptSelect.empty();
            $deptSelect.append('<option value="">Select Department</option>');
            if (response.status && response.result) {
                let deptList = Array.isArray(response.result.data) ? response.result.data : [response.result.data];
                deptList.forEach(function (dept) {
                    $deptSelect.append(`<option value="${dept.departmentId}">${dept.departmentName}</option>`);
                });
            }
        }
    });
}

$('#department').on('change', function () {
    var deptId = $(this).val();
    var $subDeptSelect = $('#subDepartment');
    $subDeptSelect.empty();
    $subDeptSelect.append('<option value="">Select Designation</option>');
    if (deptId) {
        $.ajax({
            url: '/Employee/GetSubDepartments?deptid=' + deptId,
            type: 'GET',
            success: function (response) {
                if (response.status && response.result) {
                    let deptList = Array.isArray(response.result.data) ? response.result.data : [response.result.data];
                    deptList.forEach(function (subDept) {
                        $subDeptSelect.append(`<option value="${subDept.subDepartmentId}">${subDept.subDepartmentName}</option>`);
                    });
                }
            }
        });
    }
});


let salaryComponents = [];

function fetchSalaryComponents(callback) {
    $.ajax({
        url: '/Employee/GetSalaryComponents',
        type: 'GET',
        success: function (response) {
            let data = [];
            try {
                if (response && response.result) {
                    // response.result might already be the components or an API wrapper
                    if (Array.isArray(response.result)) data = response.result;
                    else if (response.result.data && Array.isArray(response.result.data)) data = response.result.data;
                    else if (Array.isArray(response.result.data?.data)) data = response.result.data.data; // extra nesting
                    else if (Array.isArray(response.result.data)) data = response.result.data;
                    else {
                        // could be raw
                        data = response.result;
                    }
                } else if (response && response.data) {
                    data = response.data;
                } else {
                    data = response;
                }
            } catch (ex) {
                data = response;
            }

            salaryComponents = Array.isArray(data) ? data : [];
            if (callback) callback();
        },
        error: function () {
            salaryComponents = [];
            if (callback) callback();
        }

    });
}

function renderSalaryBreakup(salary) {
    if (salary <= 0 || salaryComponents.length === 0) {
        $('#salaryBreakupTable').html('<div class="text-danger">Enter Salary Per Month to view breakup.</div>');
        $('#selectedTotal').text('0.00');
        $('#remainingAmount').text('0.00');
        return;
    }

    let rows = '';
    salaryComponents.forEach(function (comp, idx) {
        const compId = comp.ComponentID ?? comp.componentID ?? comp.componentId ?? comp.ComponentId ?? comp.id ?? 0;
        const compName = comp.ComponentName ?? comp.componentName ?? comp.component_name ?? '';
        const per = parseFloat(comp.Percentage ?? comp.percentage ?? comp.per ?? 0) || 0;
        const perOn = comp.PerOnComponentName ?? comp.perOnComponentName ?? comp.perOn ?? comp.perOnComponent ?? 'Gross Salary';

        let basicSalary = salary * 0.5;
        let grossSalary = salary;
        let amount = 0;
        if (/basic/i.test(compName) || compName.toLowerCase().includes('basic')) {
            amount = basicSalary;
        } else if (per > 0 && /basic/i.test(perOn)) {
            amount = basicSalary * (per / 100);
        } else if (per > 0 && /gross/i.test(perOn)) {
            amount = grossSalary * (per / 100);
        } else {
            amount = 0;
        }

        const componentType = comp.ComponentType ?? comp.componentType ?? comp.Type ?? 'Earning';
        const checked = componentType.toLowerCase() === 'earning' || componentType.toLowerCase() === 'employercontribution';

        rows += `
                <tr data-comp-id="${compId}">
                    <td class="text-center align-middle">
                        <input type="checkbox" class="form-check-input component-check" ${checked ? 'checked' : ''} />
                    </td>
                    <td class="">${idx + 1}. ${escapeHtml(compName)}</td>
                    <td class="align-middle" style="width:220px;">
                        <div class="input-group">
                            <input type="number" min="0" step="0.01" class="form-control component-amount text-center" value="${amount.toFixed(2)}" data-comp-id="${compId}" />
                            <div class="invalid-feedback component-error" style="display:none;"></div>
                        </div>
                    </td>
                </tr>
            `;
    });

    rows += `
            <tr class="fw-bold bg-light">
                <td colspan="2" class="text-end">Total Selected</td>
                <td class="text-center"><strong id="TotalSelectedText">0.00</strong></td>
            </tr>
        `;

    $('#salaryBreakupTable').html(`
            <table class="table table-bordered ">
                <thead class="table-light">
                    <tr>
                        <th style="width:48px;">Select</th>
                        <th>Component Name</th>
                        <th>Amount Per Month (₹)</th>
                    </tr>
                </thead>
                <tbody>${rows}</tbody>
            </table>
        `);

    $('.component-check').on('change', function () {
        updateTotals();
    });
    $('.component-amount').on('input', function () {
        const $input = $(this);
        clearComponentError($input);
        updateTotals();
    });

    updateTotals();
}

function escapeHtml(text) {
    if (!text) return '';
    return text.replace(/[&<>"'`=\/]/g, function (s) {
        return ({
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#39;',
            '/': '&#x2F;',
            '`': '&#x60;',
            '=': '&#x3D;'
        })[s];
    });
}

function clearComponentError($input) {
    $input.removeClass('is-invalid');
    $input.closest('td').find('.component-error').hide().text('');
}

function showComponentError($input, message) {
    $input.addClass('is-invalid');
    const $err = $input.closest('td').find('.component-error');
    $err.text(message).show();
}

// Generic field error clear (for step inputs/selects/textareas)
function clearFieldError($el) {
    if (!$el || !$el.length) return;
    $el.removeClass('is-invalid');
    let $feedback = $el.siblings('.invalid-feedback');
    if ($feedback.length === 0) {
        $feedback = $el.next('.invalid-feedback');
    }
    if ($feedback.length > 0) {
        $feedback.hide().text('');
    }
}

// clear all errors in a container
function clearAllErrors($container) {
    if (!$container || $container.length === 0) return;
    $container.find('input, select, textarea').each(function () {
        clearFieldError($(this));
    });
}

function updateTotals() {
    const salary = parseFloat($('#salaryPerMonth').val()) || 0;
    let total = 0;
    let hasError = false;

    $('#salaryBreakupTable tbody tr').each(function () {
        const $row = $(this);
        const checked = $row.find('.component-check').is(':checked');
        const $input = $row.find('.component-amount');
        const val = parseFloat($input.val()) || 0;

        clearComponentError($input);

        if (checked) {
            if (val < 0) {
                showComponentError($input, 'Value must be >= 0');
                hasError = true;
            } else if (salary > 0 && val > salary) {
                showComponentError($input, 'Cannot exceed Salary Per Month');
                hasError = true;
            }
            total += val;
        }
    });

    $('#selectedTotal').text(total.toFixed(2));
    $('#TotalSelectedText').text(total.toFixed(2));
    const remaining = (salary - total);
    $('#remainingAmount').text(remaining.toFixed(2));

    if (salary > 0 && total > salary) {
        $('#remainingAmount').addClass('text-danger');
    } else {
        $('#remainingAmount').removeClass('text-danger');
    }

    return !hasError && !(salary > 0 && total > salary);
}

function collectCheckedComponents() {
    const comps = [];
    $('#salaryBreakupTable tbody tr').each(function () {
        const $row = $(this);
        const checked = $row.find('.component-check').is(':checked');
        if (!checked) return;
        const compId = parseInt($row.attr('data-comp-id')) || 0;
        const amount = parseFloat($row.find('.component-amount').val()) || 0;
        comps.push({ ComponentId: compId, Amount: amount });
    });
    return comps;
}

function getSaved(obj) {
    if (!obj) return null;
    if (obj.result) {
        if (obj.result.data) return obj.result.data;
        return obj.result;
    }
    if (obj.data) return obj.data;
    return obj;
}

function bindBasicInfo(saved) {
    if (!saved) return;
    $('#employeeId').val(saved.employeeId || saved.EmployeeId || saved.id || saved.ID || '');
    $('#empcode').val(saved.employeeCode || saved.EmployeeCode || '');
    $('#firstname').val(saved.firstName || saved.FirstName || '');
    $('#lastname').val(saved.lastName || saved.LastName || '');
    $('#department').val(saved.departmentId || saved.DepartmentId || '');
    $('#subDepartment').val(saved.subDepartmentId || saved.SubDepartmentId || '');
    $('#state').val(saved.state || saved.State || '');
    $('#joiningdate').val(saved.joiningDate ? new Date(saved.joiningDate).toISOString().slice(0, 10) : '');
    $('#reporting').val(saved.reportingTo || saved.ReportingTo || '');
}

function bindPayrollInfo(saved) {
    if (!saved) return;
    // Avoid overwriting user-entered values with empty server fields
    const spm = saved.salaryPerMonth ?? saved.SalaryPerMonth ?? saved.salary;
    if (spm !== undefined && spm !== null && spm !== '') $('#salaryPerMonth').val(spm);
    const spy = saved.salaryPerYear ?? saved.SalaryPerYear ?? saved.salaryPerYear;
    if (spy !== undefined && spy !== null && spy !== '') $('#salaryPerYear').val(spy);
}

function showFieldError($el, message) {
    $el.addClass('is-invalid');
    let $feedback = $el.siblings('.invalid-feedback');
    if ($feedback.length === 0) {
        $feedback = $('<div class="invalid-feedback"></div>');
        $el.after($feedback);
    }
    $feedback.text(message).show();
}

function validateBySelector(selector) {
    if (!selector) return true;
    const sel = selector.startsWith('#') || selector.startsWith('.') ? selector : '.' + selector;
    const $container = $(sel);
    if ($container.length === 0) return true; // nothing to validate

    // clear previous errors before validating
    clearAllErrors($container);

    let valid = true;

    // find fields that are marked required
    $container.find('input, select, textarea').each(function () {
        const $el = $(this);
        const isRequired = $el.attr('required') !== undefined || $el.data('required') === true || $el.attr('data-required') === 'true' || $el.hasClass('required');
        if (!isRequired) return;

        const type = ($el.attr('type') || '').toLowerCase();
        const val = $el.val();

        // Determine empty
        let empty = false;
        if (type === 'checkbox' || type === 'radio') {
            empty = !$el.is(':checked');
        } else if ($el.is('select')) {
            empty = !val || val === '';
        } else if (type === 'file') {
            empty = !$el[0] || !$el[0].files || $el[0].files.length === 0;
        } else {
            empty = !val || (typeof val === 'string' && val.trim() === '');
        }

        if (empty) {
            const msg = $el.data('msg') || $el.attr('data-msg') || 'This field is required.';
            showFieldError($el, msg);
            valid = false;
            return;
        }

        // numeric min/max validation if present
        if ((type === 'number' || $el.hasClass('numeric')) && val !== '') {
            const num = parseFloat(val);
            const min = $el.data('min') ?? $el.attr('data-min') ?? $el.attr('min');
            const max = $el.data('max') ?? $el.attr('data-max') ?? $el.attr('max');
            if (min !== undefined && min !== null && min !== '' && !isNaN(num) && num < parseFloat(min)) {
                showFieldError($el, `Value must be >= ${min}`);
                valid = false;
                return;
            }
            if (max !== undefined && max !== null && max !== '' && !isNaN(num) && num > parseFloat(max)) {
                showFieldError($el, `Value must be <= ${max}`);
                valid = false;
                return;
            }
        }
    });

    return valid;
}

// Backwards-compatible functions for step validation that call the dynamic validator
function validateStep1() { return validateBySelector('#step1'); }
function validateStep2() { return validateBySelector('#step2'); }
function validateStep3() { return validateBySelector('#step3'); }
function validateStep4() { return validateBySelector('#step4'); }

// Save helper functions that return Promises so we can chain saves if needed
function saveBasicInfo() {
    return new Promise(function (resolve, reject) {
        if (!validateStep1()) {
            return reject('Validation failed for Step 1');
        }
        var fd = new FormData();
        fd.append('EmployeeCode', $('#empcode').val());
        fd.append('FirstName', $('#firstname').val());
        fd.append('LastName', $('#lastname').val());
        fd.append('DepartmentId', $('#department').val() || '');
        fd.append('SubDepartmentId', $('#subDepartment').val() || '');
        fd.append('State', $('#state').val() || '');
        fd.append('JoiningDate', $('#joiningdate').val() || '');
        fd.append('ReportingTo', $('#reporting').val() || '');
        fd.append('SourceOfHire', $('#sourceofhire').val() || '');
        fd.append('Interviewer', $('#interviewer').val() || '');
        fd.append('AttendanceRules', $('#attendance').val() || '');
        fd.append('EmploymentStatus', $('#employmentstatus').val() || '');
        fd.append('MaritalStatus', $('#maritalstatus').val() || '');
        fd.append('AadharNo', $('#aadhar').val() || '');
        fd.append('PANNo', $('#pan').val() || '');
        fd.append('PFNo', $('#pf').val() || '');
        fd.append('UANNo', $('#uan').val() || '');
        fd.append('ESINo', $('#esi').val() || '');
        fd.append('NoticePeriod', $('#noticeperiod').val() || '');

        var fileInput = $('input[type="file"]')[0];
        if (fileInput && fileInput.files && fileInput.files.length) {
            fd.append('ProfilePicture', fileInput.files[0]);
        }

        $.ajax({
            url: '/Employee/SaveBasicInfo',
            type: 'POST',
            data: fd,
            processData: false,
            contentType: false,
            success: function (response) {
                var saved = getSaved(response);
                bindBasicInfo(saved);
                savedSteps.step1 = true;
                resolve(saved);
            },
            error: function (xhr) {
                reject(xhr);
            }
        });
    });
}

function savePayrollInfo() {
    return new Promise(function (resolve, reject) {
        if (!validateStep2()) {
            return reject('Validation failed for Step 2');
        }
        var data = {
            EmployeeId: $('#employeeId').val(),
            SalaryPerMonth: parseFloat($('#salaryPerMonth').val()) || 0,
            SalaryPerYear: parseFloat($('#salaryPerYear').val()) || 0,
            RecoveryMode: $('#recoverymode').val(),
            InstallmentAmount: $('#installment').val(),
            RecoveryCycle: $('#recoverycycle').val(),
            BiometricUserId: $('#biometricdevice').val()
        };
        $.ajax({
            url: '/Employee/SavePayrollInfo',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (response) {
                var saved = getSaved(response);
                bindPayrollInfo(saved);
                savedSteps.step2 = true;
                // ensure components re-rendered after payroll save
                fetchSalaryComponents(function () {
                    let salary = parseFloat($('#salaryPerMonth').val()) || 0;
                    renderSalaryBreakup(salary);
                });
                resolve(saved);
            },
            error: function (xhr) {
                reject(xhr);
            }
        });
    });
}

function saveBankInfo() {
    return new Promise(function (resolve, reject) {
        if (!validateStep3()) {
            return reject('Validation failed for Step 3');
        }
        var data = {
            EmployeeId: $('#employeeId').val(),
            BeneficiaryName: $('#beneficiary').val(),
            BankName: $('#bankname').val(),
            AccountNumber: $('#accountno').val(),
            IFSCCode: $('#ifsc').val()
        };
        $.ajax({
            url: '/Employee/SaveBankDetails',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (response) {
                savedSteps.step3 = true;
                resolve(response);
            },
            error: function (xhr) {
                reject(xhr);
            }
        });
    });
}

// Per-tab Save button bindings
$('#save1').off('click').on('click', function () {
    saveBasicInfo().then(function () {
        showStep(1); // move to step 2 after save
    }).catch(function (err) {
        // validation errors are already shown; keep user on current step
        console.error('Save Step1 failed', err);
    });
});

$('#save2').off('click').on('click', function () {
    savePayrollInfo().then(function () {
        showStep(2); // move to step 3 after save
    }).catch(function (err) {
        console.error('Save Step2 failed', err);
    });
});

$('#save3').off('click').on('click', function () {
    saveBankInfo().then(function () {
        showStep(3); // move to step 4 after save
    }).catch(function (err) {
        console.error('Save Step3 failed', err);
    });
});

//// helper that always attempts to save Step1..Step3 sequentially
//function saveAllSteps() {
//    return Promise.resolve()
//        .then(() => saveBasicInfo().catch(err => Promise.reject({ step: 1, err })))
//        .then(() => savePayrollInfo().catch(err => Promise.reject({ step: 2, err })))
//        .then(() => saveBankInfo().catch(err => Promise.reject({ step: 3, err })));
//}

// validate all steps and navigate to the first invalid one
function validateAndGoToFirstInvalid() {
    const validators = [
        { fn: validateStep1, index: 0, name: 'Step 1' },
        { fn: validateStep2, index: 1, name: 'Step 2' },
        { fn: validateStep3, index: 2, name: 'Step 3' },
        { fn: validateStep4, index: 3, name: 'Step 4' }
    ];

    for (let v of validators) {
        // run validator — it will show field errors when returning false
        if (!v.fn()) {
            showStep(v.index); // switch to the tab that failed validation
            return { valid: false, step: v.index + 1, name: v.name };
        }
    }

    return { valid: true };
}

// Use this at final submit so validation fires and user is taken to offending tab
//$('#step4Finish').off('click').on('click', function () {
//    // run validation for all steps first and navigate to first invalid step if any
//    const res = validateAndGoToFirstInvalid();
//    if (!res.valid) {
//        // focus user on the invalid tab; message optional
//        alert('Please fill required fields in ' + res.name + ' before final submit.');
//        return;
//    }

//    // all validations passed visually — now attempt to save all steps and then breakup
//    saveAllSteps()
//        .then(function () {
//            // then validate salary breakup totals and save breakup
//            if (!updateTotals()) {
//                alert('Fix errors in salary breakup (values exceed salary or invalid).');
//                return;
//            }

//            const salary = parseFloat($('#salaryPerMonth').val()) || 0;
//            const components = collectCheckedComponents();
//            if (components.length === 0) {
//                if (!confirm('No components selected. Do you want to submit with none selected?')) return;
//            }

//            const sum = components.reduce((s, c) => s + (c.Amount || 0), 0);
//            if (salary > 0 && sum > salary) {
//                alert('Selected components total exceeds Salary Per Month. Adjust values.');
//                return;
//            }

//            var data = {
//                EmployeeId: $('#employeeId').val(),
//                SalaryComponents: components
//            };
//            $.ajax({
//                url: '/Employee/SaveSalaryBreakup',
//                type: 'POST',
//                contentType: 'application/json',
//                data: JSON.stringify(data),
//                success: function (response) {
//                    alert('Salary breakup saved successfully.');
//                },
//                error: function () {
//                    alert('Failed to save salary breakup.');
//                }
//            });
//        })
//        .catch(function (err) {
//            // If a save failed, show that step/tab and alert user
//            const step = err && err.step ? err.step : 1;
//            showStep(step - 1);
//            alert('Please fix validation / save errors in Step ' + step + ' before final submit.');
//            console.error('Failed saving step', err);
//        });
//});


// Single-payload save: collect all steps and submit in one FormData request
const saveAllUrl = '/Employee/SaveAllEmployeeData'; // adjust server endpoint if different

function collectAllPayload() {
    // Basic info
    const basic = {
        EmployeeId: $('#employeeId').val() || null,
        EmployeeCode: $('#empcode').val(),
        FirstName: $('#firstname').val(),
        LastName: $('#lastname').val(),
        DepartmentId: $('#department').val() || '',
        SubDepartmentId: $('#subDepartment').val() || '',
        State: $('#state').val() || '',
        JoiningDate: $('#joiningdate').val() || '',
        ReportingTo: $('#reporting').val() || '',
        SourceOfHire: $('#sourceofhire').val() || '',
        Interviewer: $('#interviewer').val() || '',
        AttendanceRules: $('#attendance').val() || '',
        EmploymentStatus: $('#employmentstatus').val() || '',
        MaritalStatus: $('#maritalstatus').val() || '',
        AadharNo: $('#aadhar').val() || '',
        PANNo: $('#pan').val() || '',
        PFNo: $('#pf').val() || '',
        UANNo: $('#uan').val() || '',
        ESINo: $('#esi').val() || '',
        NoticePeriod: $('#noticeperiod').val() || ''
    };

    // Payroll
    const payroll = {
        SalaryPerMonth: parseFloat($('#salaryPerMonth').val()) || 0,
        SalaryPerYear: parseFloat($('#salaryPerYear').val()) || 0,
        RecoveryMode: $('#recoverymode').val(),
        InstallmentAmount: $('#installment').val(),
        RecoveryCycle: $('#recoverycycle').val(),
        BiometricUserId: $('#biometricdevice').val()
    };

    // Bank
    const bank = {
        BeneficiaryName: $('#beneficiary').val() || '',
        BankName: $('#bankname').val() || '',
        AccountNumber: $('#accountno').val() || '',
        IFSCCode: $('#ifsc').val() || ''
    };

    // Salary components (checked)
    const components = collectCheckedComponents();

    return { basic, payroll, bank, components };
}

function saveAllOnePayload() {
    // Run validation and navigate to first invalid if any
    const res = validateAndGoToFirstInvalid();
    if (!res.valid) {
        alert('Please fill required fields in ' + res.name + ' before saving.');
        return;
    }

    // Validate salary breakup totals too
    if (!updateTotals()) {
        alert('Fix errors in salary breakup (values exceed salary or invalid).');
        return;
    }

    const payload = collectAllPayload();

    // Build FormData so file (profile) can be uploaded as well
    const fd = new FormData();

    // Append basic fields
    Object.entries(payload.basic).forEach(([k, v]) => fd.append(k, v ?? ''));

    // Append payroll & bank
    Object.entries(payload.payroll).forEach(([k, v]) => fd.append(k, v ?? ''));
    Object.entries(payload.bank).forEach(([k, v]) => fd.append(k, v ?? ''));

    // Append components as JSON
    fd.append('SalaryComponents', JSON.stringify(payload.components || []));

    // Append profile picture if provided
    const fileInput = $('#profile')[0];
    if (fileInput && fileInput.files && fileInput.files.length) {
        fd.append('ProfilePicture', fileInput.files[0]);
    }

    // POST single payload
    $.ajax({
        url: saveAllUrl,
        type: 'POST',
        data: fd,
        processData: false,
        contentType: false,
        success: function (response) {
            const saved = getSaved(response);

            function resetWizard() {
                // clear simple inputs/textarea/number/date/hidden
                $('#employeeWizard').find('input[type="text"], input[type="number"], input[type="date"], input[type="hidden"], textarea').val('');
                // reset selects
                $('#employeeWizard').find('select').prop('selectedIndex', 0);
                // uncheck checkboxes/radios
                $('#employeeWizard').find('input[type="checkbox"], input[type="radio"]').prop('checked', false);
                // clear file inputs
                $('#employeeWizard').find('input[type="file"]').val('');
                // clear salary components & table
                salaryComponents = [];
                $('#salaryBreakupTable').html('');
                // reset totals
                $('#selectedTotal').text('0.00');
                $('#TotalSelectedText').text('0.00');
                $('#remainingAmount').text('0.00').removeClass('text-danger');
                // remove all validation states
                clearAllErrors($('#employeeWizard'));
                // reset saved steps tracking
                savedSteps = { step1: false, step2: false, step3: false };
            }

            // If you want to keep returned values in form (e.g. EmployeeId) comment out resetWizard call.
            // We will reset form completely and then go to step 1.
            resetWizard();

            // If server returned id and you prefer to keep it, set it after reset:
            if (generatedId) {
                $('#employeeId').val(generatedId);
            }

            // Ensure components are re-fetched (if desired)
            fetchSalaryComponents(function () {
                let salary = parseFloat($('#salaryPerMonth').val()) || 0;
                if (salary > 0) renderSalaryBreakup(salary);
            });

            // Move back to first step
            showStep(0);

            alert('All data saved successfully.');
        },
        error: function (xhr) {
            // If server returned validation errors, try to surface them
            try {
                const json = xhr.responseJSON || JSON.parse(xhr.responseText || '{}');
                // Example: server returns { step: 2, fieldErrors: [...] } — handle as needed
                if (json && json.step) {
                    showStep(json.step - 1);
                    alert(json.message || ('Errors in step ' + json.step));
                } else {
                    alert('Failed to save data. See console for details.');
                    console.error(xhr);
                }
            } catch (ex) {
                alert('Failed to save data. See console for details.');
                console.error(xhr);
            }
        }
    });
}

// Replace final handler to call single-payload save
$('#step4Finish').off('click').on('click', function () {
    saveAllOnePayload();
});