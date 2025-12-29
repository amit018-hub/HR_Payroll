// Multi-Select Dropdown Functionality
document.addEventListener('DOMContentLoaded', function () {
    const multiSelectButtons = document.querySelectorAll('.multi-select-button');

    // Toggle dropdown
    multiSelectButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.stopPropagation();
            const targetId = this.getAttribute('data-target');
            const dropdown = document.getElementById(targetId);
            const allDropdowns = document.querySelectorAll('.multi-select-dropdown');
            const allButtons = document.querySelectorAll('.multi-select-button');

            // Close all other dropdowns
            allDropdowns.forEach(d => {
                if (d.id !== targetId) {
                    d.classList.remove('show');
                }
            });
            allButtons.forEach(b => {
                if (b !== this) {
                    b.classList.remove('active');
                }
            });

            // Toggle current dropdown
            dropdown.classList.toggle('show');
            this.classList.toggle('active');
        });
    });

    // Close dropdown when clicking outside
    document.addEventListener('click', function (e) {
        if (!e.target.closest('.multi-select-wrapper')) {
            document.querySelectorAll('.multi-select-dropdown').forEach(d => d.classList.remove('show'));
            document.querySelectorAll('.multi-select-button').forEach(b => b.classList.remove('active'));
        }
    });

    // Prevent dropdown close when clicking inside
    document.querySelectorAll('.multi-select-dropdown').forEach(dropdown => {
        dropdown.addEventListener('click', function (e) {
            e.stopPropagation();
        });
    });

    // Handle option selection
    document.querySelectorAll('.multi-select-option').forEach(option => {
        option.addEventListener('click', function () {
            this.classList.toggle('selected');
            updateSelectedText(this.closest('.multi-select-dropdown'));
        });
    });

    // Select All functionality
    document.querySelectorAll('.select-all-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            const dropdown = this.closest('.multi-select-dropdown');
            const visibleOptions = dropdown.querySelectorAll('.multi-select-option:not([style*="display: none"])');
            visibleOptions.forEach(option => option.classList.add('selected'));
            updateSelectedText(dropdown);
        });
    });

    // Clear All functionality
    document.querySelectorAll('.clear-all-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            const dropdown = this.closest('.multi-select-dropdown');
            dropdown.querySelectorAll('.multi-select-option').forEach(option => {
                option.classList.remove('selected');
            });
            updateSelectedText(dropdown);
        });
    });

    // Search functionality
    document.querySelectorAll('.search-input').forEach(input => {
        input.addEventListener('input', function () {
            const searchTerm = this.value.toLowerCase();
            const dropdown = this.closest('.multi-select-dropdown');
            const options = dropdown.querySelectorAll('.multi-select-option');

            options.forEach(option => {
                const text = option.textContent.toLowerCase();
                if (text.includes(searchTerm)) {
                    option.style.display = '';
                } else {
                    option.style.display = 'none';
                }
            });
        });
    });
});

$(function () {
    loadOffices();
    loadShifts();
    loadDepartment();
    loadSubDepartments(0);
    loadBranchWiseUsers(0);
    $('#dept').on('change', function () {
        const deptId = $(this).val();
        loadSubDepartments(deptId);

        // reset employees immediately
        $('#employeeDropdown .multi-select-options').empty();
        resetEmployeePlaceholder();
    });
    $('#subDept').on('change', function () {
        const subDeptId = $(this).val();
        if (subDeptId) {
            loadBranchWiseUsers(subDeptId);
        } else {
            $('#employeeDropdown .multi-select-options').empty();
            resetEmployeePlaceholder();
        }
    });

});

function loadOffices() {
    $.ajax({
        url: '/Assign/GetOfficeLocations',
        type: 'GET',
        success: function (response) {

            const $container = $('#officeDropdown .multi-select-options');
            $container.empty();

            if (!response.status || !response.data || response.data.length === 0) {
                $container.append(`<div class="no-results">No offices found</div>`);
                resetOfficePlaceholder();
                return;
            }

            response.data.forEach(office => {
                const html = `
                    <div class="multi-select-option" data-value="${office.officeID}">
                        <div class="multi-select-checkbox"></div>
                        <span class="multi-select-label">${office.officeName}</span>
                    </div>
                `;
                $container.append(html);
            });

            bindOfficeSelection();
            resetOfficePlaceholder();
        },
        error: function (xhr) {
            console.error('Office load failed:', xhr.responseText);
            resetOfficePlaceholder();
        }
    });
}
function bindOfficeSelection() {
    $('#officeDropdown .multi-select-option')
        .off('click')
        .on('click', function () {
            $(this).toggleClass('selected');
            updateSelectedText(document.getElementById('officeDropdown'));
        });
}
function resetOfficePlaceholder() {
    $('#officeSelected')
        .removeClass('selected-text')
        .html('-- Select Office --');
}

function loadShifts() {
    $.ajax({
        url: '/Assign/GetEmployeeShifts',
        type: 'GET',
        success: function (response) {

            const $container = $('#shiftDropdown .multi-select-options');
            $container.empty();

            if (!response.status || !response.data || response.data.length === 0) {
                $container.append(`<div class="no-results">No shifts found</div>`);
                resetShiftPlaceholder();
                return;
            }

            response.data.forEach(shift => {
                const start = formatTime12H(shift.startTime);
                const end = formatTime12H(shift.endTime);

                const html = `
                    <div class="multi-select-option" data-value="${shift.shiftCode}">
                        <div class="multi-select-checkbox"></div>
                        <span class="multi-select-label">
                            ${shift.shiftName} (${start} - ${end})
                        </span>
                    </div>
                `;
                $container.append(html);
            });

            bindShiftSelection();
            resetShiftPlaceholder();
        },
        error: function (xhr) {
            console.error('Shift load failed:', xhr.responseText);
            resetShiftPlaceholder();
        }
    });
}
function bindShiftSelection() {
    $('#shiftDropdown .multi-select-option')
        .off('click')
        .on('click', function () {
            //$(this).toggleClass('selected');
            $('#shiftDropdown .multi-select-option').removeClass('selected');
            $(this).addClass('selected');

            updateSelectedText(document.getElementById('shiftDropdown'));

            // optional: auto close
            $('#shiftDropdown').removeClass('show');
            $('.multi-select-button[data-target="shiftDropdown"]').removeClass('active');
        });
}
function resetShiftPlaceholder() {
    $('#shiftSelected')
        .removeClass('selected-text')
        .html('-- Select Shift --');
}

function updateSelectedText(dropdown) {
    const dropdownId = dropdown.id;
    const selectedOptions = dropdown.querySelectorAll('.multi-select-option.selected');

    let selectedTextId = '';
    let defaultText = '';

    if (dropdownId === 'officeDropdown') {
        selectedTextId = 'officeSelected';
        defaultText = '-- Select Office --';
    } else if (dropdownId === 'shiftDropdown') {
        selectedTextId = 'shiftSelected';
        defaultText = '-- Select Shift --';
    } else if (dropdownId === 'employeeDropdown') {
        selectedTextId = 'employeeSelected';
        defaultText = '-- Select Employee --';
    }

    const $label = $('#' + selectedTextId);

    if (selectedOptions.length === 0) {
        $label
            .removeClass('selected-text')
            .html(defaultText);
        return;
    }

    if (selectedOptions.length === 1) {
        const text =
            selectedOptions[0].querySelector('.multi-select-label, .employee-name')
                ?.textContent.trim();

        $label
            .addClass('selected-text')
            .html(`${text} <span class="selected-count">1</span>`);
    } else {
        $label
            .addClass('selected-text')
            .html(`${selectedOptions.length} selected <span class="selected-count">${selectedOptions.length}</span>`);
    }
}

function loadDepartment() {
    $.ajax({
        url: '/Assign/GetDepartments',
        type: 'GET',
        success: function (response) {
            if (response.status && response.data) {
                $('#dept').empty().append('<option value="">-- Select Department --</option>');
                $.each(response.data, function (index, dept) {
                    $('#dept').append(
                        $('<option></option>').val(dept.departmentId).text(dept.departmentName)
                    );
                });
            } else {
                console.warn('No departments found:', response.message);
            }
        },
        error: function (xhr) {
            console.error('Error fetching departments:', xhr.responseText);
        }
    });
}

function loadSubDepartments(departmentId) {
    $('#subDept').empty().append('<option value="">-- Select Sub Department --</option>');

    $.ajax({
        url: '/Assign/GetSubDepartments?departmentId=' + departmentId,
        type: 'GET',
        success: function (response) {
            if (response.status && response.data) {
                $.each(response.data, function (index, sub) {
                    $('#subDept').append(
                        $('<option></option>').val(sub.subDepartmentId).text(sub.subDepartmentName)
                    );
                });
            }
        },
        error: function (xhr) {
            console.error('Error loading sub-departments:', xhr.responseText);
        }
    });
}

function loadBranchWiseUsers(subDepartmentId) {
    $.ajax({
        url: '/Assign/GetEmployeeBySubDept?subDepartmentId=' + subDepartmentId,
        type: 'GET',
        success: function (response) {

            const $optionsContainer = $('#employeeDropdown .multi-select-options');
            $optionsContainer.empty();

            if (!response.status || !response.data || response.data.length === 0) {
                $optionsContainer.append(`
                    <div class="no-results">No employees found</div>
                `);
                resetEmployeePlaceholder();
                return;
            }

            response.data.forEach(user => {

                const initials = getInitials(user.employeeName);

                const html = `
                    <div class="multi-select-option" data-value="${user.employeeID}">
                        <div class="multi-select-checkbox"></div>

                        <div class="employee-option">
                            <div class="employee-avatar">${initials}</div>
                            <div class="employee-info">
                                <div class="employee-name">${user.employeeName}</div>
                                <div class="employee-id">${user.employeeCode || 'EMP-' + user.employeeID}</div>
                            </div>
                        </div>
                    </div>
                `;

                $optionsContainer.append(html);
            });

            // Rebind click handlers after dynamic render
            bindEmployeeSelection();

            // Reset selected text
            resetEmployeePlaceholder();
        },
        error: function (xhr) {
            console.error('Error loading users:', xhr.responseText);
            resetEmployeePlaceholder();
        }
    });
}
function bindEmployeeSelection() {
    $('#employeeDropdown .multi-select-option').off('click').on('click', function () {
        $(this).toggleClass('selected');
        updateSelectedText(document.getElementById('employeeDropdown'));
    });
}
function resetEmployeePlaceholder() {
    $('#employeeSelected')
        .removeClass('selected-text')
        .html('-- Select Employee --');
}
function getInitials(name) {
    if (!name) return 'NA';
    return name
        .split(' ')
        .map(x => x[0])
        .join('')
        .substring(0, 2)
        .toUpperCase();
}

function formatTime12H(time) {
    if (!time) return '';

    const [hh, mm] = time.split(':');
    let hour = parseInt(hh, 10);
    const min = mm;
    const ampm = hour >= 12 ? 'PM' : 'AM';

    hour = hour % 12 || 12;
    return `${hour}:${min} ${ampm}`;
}

$('#btnCancel').on('click', function () {

    // Optional confirmation
    if (!confirm('Are you sure you want to reset all changes?')) {
        return;
    }

    resetAssignShiftForm();
});

function resetAssignShiftForm() {

    // 🔹 Clear multi-select selections
    $('.multi-select-option').removeClass('selected');

    // 🔹 Reset placeholders
    resetOfficePlaceholder();
    resetShiftPlaceholder();
    resetEmployeePlaceholder();

    // 🔹 Clear dates
    $('#fromDate').val('');
    $('#toDate').val('');

    // 🔹 Reset department & sub-department
    $('#dept').val('');
    $('#subDept').empty().append('<option value="">-- Select Sub Department --</option>');

    // 🔹 Clear employee list
    $('#employeeDropdown .multi-select-options').empty();

    // 🔹 Close all dropdowns
    $('.multi-select-dropdown').removeClass('show');
    $('.multi-select-button').removeClass('active');
}

$('#btnAssignShift').on('click', async function () {

    const officeIds = getSelectedValues('officeDropdown');
    const shiftCodes = getSelectedValues('shiftDropdown');
    const employeeIds = getSelectedValues('employeeDropdown');

    const fromDate = $('#fromDate').val();
    const toDate = $('#toDate').val();

    let errors = [];
    if (!officeIds.length || !shiftCodes.length || !employeeIds.length || !fromDate) {
        errors.push('Please fill all required fields.');
    }
    if (toDate && new Date(toDate) < new Date(fromDate)) {
        errors.push('To Date cannot be earlier than From Date.');
    }
    if (errors.length) {
        showToast(errors.join('\n'), 'error');
        return;
    }

    $('#btnAssignShift').prop('disabled', true);

    // 🔥 BUILD ASSIGNMENTS
    const assignments = [];
    employeeIds.forEach(empId => {
        officeIds.forEach(officeId => {
            shiftCodes.forEach(shiftCode => {
                assignments.push({
                    employeeId: empId,
                    officeId: officeId,
                    shiftCode: shiftCode,
                    fromDate: fromDate,
                    toDate: toDate || ''
                });
            });
        });
    });

    // 🔥 BUILD FORMDATA (IMPORTANT)
    const formData = new FormData();

    assignments.forEach((item, index) => {
        formData.append(`requests[${index}].EmployeeId`, item.employeeId);
        formData.append(`requests[${index}].OfficeId`, item.officeId);
        formData.append(`requests[${index}].ShiftCode`, item.shiftCode);
        formData.append(`requests[${index}].FromDate`, item.fromDate);
        formData.append(`requests[${index}].ToDate`, item.toDate);
    });

    try {
        const res = await $.ajax({
            url: '/Assign/AssignEmployeeShift',
            type: 'POST',
            data: formData,
            processData: false,   // 🔥 REQUIRED
            contentType: false    // 🔥 REQUIRED
        });

        if (res.status) {
            showToast(res.message, 'success');
            resetAssignShiftForm();
        } else {
            showToast(res.message || 'Failed to assign shift', 'error');
        }

    } catch (err) {
        console.error(err);
        showToast('Error occurred while assigning shifts.', 'error');
    }
    finally {
        $('#btnAssignShift').prop('disabled', false);
    }
});

function getSelectedValues(dropdownId) {
    const values = [];

    $('#' + dropdownId)
        .find('.multi-select-option.selected')
        .each(function () {
            values.push($(this).attr('data-value'));
        });

    return values;
}


