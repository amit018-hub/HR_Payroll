$(function () {

    const steps = ["#step1", "#step2", "#step3", "#step4"];

    function showStep(stepIndex) {

        $(".tab-pane").removeClass("show active");

        $(steps[stepIndex]).addClass("show active");

        $(".nav-link").each(function (idx) {
            $(this).toggleClass("active", idx === stepIndex);
        });
    }
    window.showStep = showStep;

    $("#step1Next").on("click", () => showStep(1));
    $("#step2Prev").on("click", () => showStep(0));
    $("#step2Next").on("click", () => showStep(2));
    $("#step3Prev").on("click", () => showStep(1));
    $("#step3Next").on("click", () => showStep(3));
    $("#step4Prev").on("click", () => showStep(2));

    getDepartments();

    $('#step2Next, #step3Next').on('click', function () {
        fetchSalaryComponents(function () {
            let salary = parseFloat($('#salaryPerMonth').val()) || 0;
            renderSalaryBreakup(salary);
        });
    });


    //$('#salaryPerMonth').on('input', function () {
    //    var perMonth = parseFloat($(this).val()) || 0;
    //    $('#salaryPerYear').val(perMonth > 0 ? (perMonth * 12).toFixed(2) : '');
    //    updateTotals();
    //});

    //$('#salaryPerYear').on('input', function () {
    //    var perYear = parseFloat($(this).val()) || 0;
    //    $('#salaryPerMonth').val(perYear > 0 ? (perYear / 12).toFixed(2) : '');
    //    updateTotals();
    //});
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
                    <td class="align-middle">${idx + 1}. ${escapeHtml(compName)}</td>
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
            <table class="table table-bordered align-middle text-center">
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
    $('#salaryPerMonth').val(saved.salaryPerMonth || saved.SalaryPerMonth || saved.salary || '');
    $('#salaryPerYear').val(saved.salaryPerYear || saved.SalaryPerYear || '');
}

function showFieldError($el, message) {
    $el.addClass('is-invalid');
    let $feedback = $el.siblings('.invalid-feedback');
    if ($feedback.length === 0) {
        $feedback = $('<div class="invalid-feedback"></div>');
        $el.after($feedback);
    }
    $feedback.text(message);
}




//function validateStep1() {
//    const $step = $('#step1');
//    clearAllErrors($step);

//    const $firstName = $step.find('input[placeholder="First Name"]');
//    const $department = $step.find('#department');
//    const $joiningDate = $step.find('input[type="date"]');

//    let valid = true;

//    if (!$firstName.val() || !$firstName.val().trim()) {
//        showFieldError($firstName, 'First name is required.');
//        valid = false;
//    }

//    if (!$department.val() || $department.val() === '') {
//        showFieldError($department, 'Department is required.');
//        valid = false;
//    }

//    if (!$joiningDate.val() || $joiningDate.val().trim() === '') {
//        showFieldError($joiningDate, 'Joining date is required.');
//        valid = false;
//    }

//    return valid;
//}

//// Validate Step 2 (Payroll) - essential fields
//function validateStep2() {
//    const $step = $('#step2');
//    clearAllErrors($step);

//    const $salaryPerMonth = $('#salaryPerMonth');
//    const $salaryPerYear = $('#salaryPerYear');

//    let perMonth = parseFloat($salaryPerMonth.val()) || 0;
//    let perYear = parseFloat($salaryPerYear.val()) || 0;
//    let valid = true;

//    if (perMonth <= 0 && perYear <= 0) {
//        showFieldError($salaryPerMonth, 'Enter Salary Per Month or Salary Per Year.');
//        showFieldError($salaryPerYear, 'Enter Salary Per Month or Salary Per Year.');
//        valid = false;
//    }

//    return valid;
//}

//function validateStep3() {
//    const $step = $('#step3');
//    clearAllErrors($step);

//    const $beneficiary = $step.find('input[placeholder="Beneficiary Name"]');
//    const $accountNumber = $step.find('input[placeholder="Account Number"]');
//    const $ifsc = $step.find('input[placeholder="IFSC Code"]');

//    let valid = true;

//    if (!$accountNumber.val() || !$accountNumber.val().trim()) {
//        showFieldError($accountNumber, 'Account number is required.');
//        valid = false;
//    }

//    if (!$ifsc.val() || !$ifsc.val().trim()) {
//        showFieldError($ifsc, 'IFSC code is required.');
//        valid = false;
//    }

//    if ($beneficiary.length && $beneficiary.val() && !$beneficiary.val().trim()) {
//        showFieldError($beneficiary, 'Provide beneficiary name or remove value.');
//        valid = false;
//    }

//    return valid;
//}

//function validateStep4() {
//    const $table = $('#salaryBreakupTable');
//    if ($table.find('tbody tr').length === 0) {
//        alert('Salary breakup is empty. Enter Salary Per Month to generate breakup.');
//        return false;
//    }

//    let total = 0;
//    $table.find('tbody tr').each(function () {
//        const val = parseFloat($(this).find('input').val()) || 0;
//        total += val;
//    });

//    if (total <= 0) {
//        alert('Salary breakup total must be greater than 0.');
//        return false;
//    }

//    return true;
//}

function validateBySelector(selector) {
    if (!selector) return true;
    const sel = selector.startsWith('#') || selector.startsWith('.') ? selector : '.' + selector;
    const $container = $(sel);
    if ($container.length === 0) return true; // nothing to validate

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


$('#step1Next').off('click').on('click', function () {
    if (!validateStep1()) return;
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
            showStep(1);
        },
        error: function () {
            alert('Failed to save basic info.');
        }
    });
});

$('#step2Next').off('click').on('click', function () {
    if (!validateStep2()) return;
    var data = {
        EmployeeId: $('#employeeId').val(),
        SalaryPerMonth: parseFloat($('#salaryPerMonth').val()) || 0,
        SalaryPerYear: parseFloat($('#salaryPerYear').val()) || 0,
        RecoveryMode: $('select').eq(6).val(),
        InstallmentAmount: $('#installment').val(),
        RecoveryCycle: $('select').eq(7).val(),
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
            fetchSalaryComponents(function () {
                let salary = parseFloat($('#salaryPerMonth').val()) || 0;
                renderSalaryBreakup(salary);
            });
            showStep(2);
        },
        error: function () {
            alert('Failed to save payroll info.');
        }
    });
});

$('#step3Next').off('click').on('click', function () {
    if (!validateStep3()) return;
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
            showStep(3);
        },
        error: function () {
            alert('Failed to save bank details.');
        }
    });
});

$('#step4Finish').off('click').on('click', function () {
    if (!validateStep4()) return;

    if (!updateTotals()) {
        alert('Fix errors in salary breakup (values exceed salary or invalid).');
        return;
    }

    const salary = parseFloat($('#salaryPerMonth').val()) || 0;
    const components = collectCheckedComponents();
    if (components.length === 0) {
        if (!confirm('No components selected. Do you want to submit with none selected?')) return;
    }

    const sum = components.reduce((s, c) => s + (c.Amount || 0), 0);
    if (salary > 0 && sum > salary) {
        alert('Selected components total exceeds Salary Per Month. Adjust values.');
        return;
    }

    var data = {
        EmployeeId: $('#employeeId').val(),
        SalaryComponents: components
    };
    $.ajax({
        url: '/Employee/SaveSalaryBreakup',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            alert('Salary breakup saved successfully.');
        },
        error: function () {
            alert('Failed to save salary breakup.');
        }
    });
});