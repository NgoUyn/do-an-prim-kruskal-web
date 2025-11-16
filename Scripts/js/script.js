// Form submission handlers
const registerForm = document.getElementById('registerForm');
if (registerForm) { // <-- SỬA LỖI: Chỉ chạy nếu tìm thấy
    registerForm.addEventListener('submit', function (e) {
        e.preventDefault();
        alert('Đăng ký thành công! Vui lòng kiểm tra email để xác nhận tài khoản.');
        showPage('home');
    });
}

const loginForm = document.getElementById('loginForm');
if (loginForm) { // <-- SỬA LỖI: Chỉ chạy nếu tìm thấy
    loginForm.addEventListener('submit', function (e) {
        e.preventDefault();
        alert('Đăng nhập thành công!');
        showPage('home');
    });
}