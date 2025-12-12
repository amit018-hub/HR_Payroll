
//document.getElementById('togglePassword').addEventListener('click', function () {
//    const passwordInput = document.getElementById('userpassword');
//    const toggleIcon = document.getElementById('toggleIcon');

//    if (passwordInput.type === 'password') {
//        passwordInput.type = 'text';
//        toggleIcon.classList.remove('fa-eye-slash');
//        toggleIcon.classList.add('fa-eye');
//    } else {
//        passwordInput.type = 'password';
//        toggleIcon.classList.remove('fa-eye');
//        toggleIcon.classList.add('fa-eye-slash');
//    }
//});
const pwd = document.getElementById('userpassword');
const toggle = document.getElementById('togglePwd');
toggle.addEventListener('click', () => {
    if (pwd.type === 'password') {
        pwd.type = 'text';
        toggle.textContent = '🙈';
    } else {
        pwd.type = 'password';
        toggle.textContent = '👁️';
    }
});
$("#submitbtn").on("click", async function (e) {
    e.preventDefault();

    const username = $("#username").val().trim();
    const password = $("#userpassword").val().trim();
    const remember = $("#rememberMe").is(":checked");

    if (!username && !password) {
        showToast('Username & Password Required!', 'error');
        return;
    }
    if (!username) {
        showToast('Username Required!', 'error');
        return;
    }
    if (!password) {
        showToast('Password Required!', 'error');
        return;
    }

    const btn = $("#submitbtn");
    btn.prop("disabled", true);
    btn.html(`<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Loading...`);

    try {
        const response = await fetch("/Home/Login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                Username: username,
                Password: password,
                RememberMe: remember
            })
        });

        const result = await response.json();

        if (result.success) {

            btn.text("Redirecting...");
            btn.addClass("btn-success");

            showToast(result.message || 'Login successful!', 'success');

            setTimeout(() => {
                window.location.href = result.redirectUrl || "/Dashboard/AdminDashboard";
            }, 1000);

        } else {
            showToast(result.message || "Incorrect username or password.", 'error');

            $("#username").val("");
            $("#userpassword").val("");

            btn.prop("disabled", false);
            btn.html(`Log In <i class="fas fa-sign-in-alt ms-1"></i>`);
            btn.removeClass("btn-success");
        }

    } catch (err) {
        console.error("Fetch error:", err);
        showToast("An unexpected error occurred. Please try again.", 'error');

        btn.prop("disabled", false);
        btn.html(`Log In <i class="fas fa-sign-in-alt ms-1"></i>`);
    }
});
