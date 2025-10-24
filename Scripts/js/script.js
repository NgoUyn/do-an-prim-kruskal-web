// Product data
const products = {
    1: {
        title: "Giày Thể Thao X1",
        price: "1.250.000 VNĐ",
        image: "https://images.unsplash.com/photo-1542291026-7eec264c27ff?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=1000&q=80",
        description: "Giày thể thao cao cấp với thiết kế hiện đại, chất liệu da thật, êm ái và bền bỉ. Phù hợp cho cả tập luyện và sử dụng hàng ngày.",
        features: [
            "Chất liệu da thật cao cấp",
            "Đế giày chống trượt",
            "Thiết kế thoáng khí",
            "Màu sắc thời trang",
            "Bảo hành 12 tháng"
        ]
    },
    2: {
        title: "Tai Nghe Chụp Tai",
        price: "850.000 VNĐ",
        image: "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=1000&q=80",
        description: "Tai nghe chụp tai cao cấp với âm thanh sống động, khử tiếng ồn hiệu quả. Thiết kế êm ái, phù hợp sử dụng trong nhiều giờ.",
        features: [
            "Công nghệ khử tiếng ồn",
            "Âm thanh surround 7.1",
            "Pin sử dụng 20 giờ",
            "Kết nối Bluetooth 5.0",
            "Thiết kế gập gọn tiện lợi"
        ]
    },
    3: {
        title: "Đồng Hồ Thông Minh",
        price: "2.500.000 VNĐ",
        image: "https://images.unsplash.com/photo-1523275335684-37898b6baf30?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=1000&q=80",
        description: "Đồng hồ thông minh đa chức năng, theo dõi sức khỏe, thông báo thông minh. Màn hình AMOLED sắc nét, thời lượng pin lên đến 7 ngày.",
        features: [
            "Theo dõi nhịp tim 24/7",
            "Đo lượng calo tiêu thụ",
            "Thông báo cuộc gọi, tin nhắn",
            "Chống nước IP68",
            "Hơn 20 chế độ tập luyện"
        ]
    },
    4: {
        title: "Bàn Phím Cơ",
        price: "1.750.000 VNĐ",
        image: "https://images.unsplash.com/photo-1485955900006-10f4d324d411?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=1000&q=80",
        description: "Bàn phím cơ chuyên game với đèn LED RGB, switch Blue, độ bền cao. Thiết kế TKL tiết kiệm không gian, phím điều khiển đa phương tiện.",
        features: [
            "Switch Blue cơ học",
            "Đèn LED RGB 16.8 triệu màu",
            "Thiết kế TKL không numpad",
            "Cáp USB-C tháo rời",
            "Phím điều khiển đa phương tiện"
        ]
    }
};

// Function to show specific page
function showPage(pageId) {
    // Hide all pages
    document.querySelectorAll('.page').forEach(page => {
        page.classList.remove('active');
    });

    // Show the selected page
    document.getElementById(pageId).classList.add('active');

    // Scroll to top
    window.scrollTo(0, 0);
}

// Function to show product detail
function showProductDetail(productId) {
    const product = products[productId];

    if (product) {
        document.getElementById('detail-title').textContent = product.title;
        document.getElementById('detail-price').textContent = product.price;
        document.getElementById('detail-image').src = product.image;
        document.getElementById('detail-image').alt = product.title;
        document.getElementById('detail-description').textContent = product.description;

        // Clear and add features
        const featuresList = document.getElementById('detail-features');
        featuresList.innerHTML = '';
        product.features.forEach(feature => {
            const li = document.createElement('li');
            li.textContent = feature;
            featuresList.appendChild(li);
        });

        // Show product detail page
        showPage('product-detail');
    }
}

// Form submission handlers
document.getElementById('registerForm').addEventListener('submit', function (e) {
    e.preventDefault();
    alert('Đăng ký thành công! Vui lòng kiểm tra email để xác nhận tài khoản.');
    showPage('home');
});

document.getElementById('loginForm').addEventListener('submit', function (e) {
    e.preventDefault();
    alert('Đăng nhập thành công!');
    showPage('home');
});