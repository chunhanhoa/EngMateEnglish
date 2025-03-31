// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

/**
 * EngMate - Hàm chức năng chung cho ứng dụng
 */

// Thiết lập chung khi trang đã tải xong
document.addEventListener('DOMContentLoaded', function() {
    // Đánh dấu menu active dựa trên URL hiện tại
    highlightActiveMenu();
    
    // Thiết lập tooltip cho các phần tử có thuộc tính data-bs-toggle="tooltip"
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
    
    // Thiết lập popover cho các phần tử có thuộc tính data-bs-toggle="popover"
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
    
    // Bắt sự kiện trên các liên kết có class "language-link"
    document.querySelectorAll('.dropdown-item').forEach(function(link) {
        link.addEventListener('click', function(e) {
            if (!this.classList.contains('active')) {
                // Lưu ngôn ngữ được chọn vào localStorage
                localStorage.setItem('appLanguage', this.textContent.trim());
                
                // Reload trang để áp dụng ngôn ngữ mới
                // Trong ứng dụng thực, việc này sẽ sử dụng AJAX hoặc cơ chế chuyển đổi ngôn ngữ tốt hơn
                // window.location.reload();
                
                // Cập nhật UI để hiển thị ngôn ngữ đã chọn
                document.getElementById('languageDropdown').innerHTML = '<i class="fas fa-globe me-1"></i> ' + this.textContent.trim();
                
                // Đánh dấu ngôn ngữ hiện tại là active
                document.querySelectorAll('.dropdown-item').forEach(function(l) {
                    l.classList.remove('active');
                    l.innerHTML = l.textContent.trim();
                });
                this.classList.add('active');
                this.innerHTML = '<i class="fas fa-check me-1"></i> ' + this.textContent.trim();
            }
        });
    });
});

// Đánh dấu menu active dựa trên URL hiện tại
function highlightActiveMenu() {
    // Lấy đường dẫn hiện tại
    const currentPath = window.location.pathname;
    
    // Tìm và đánh dấu menu active
    document.querySelectorAll('.navbar-nav .nav-link').forEach(function(link) {
        const href = link.getAttribute('href');
        
        // Kiểm tra xem liên kết có khớp với đường dẫn hiện tại không
        if (href && currentPath.includes(href) && href !== '/') {
            link.classList.add('active');
        } else if (href === '/' && currentPath === '/') {
            link.classList.add('active');
        }
    });
}

// Scroll mượt đến phần tử với ID tương ứng
function scrollToElement(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
}

// Hiển thị thông báo
function showNotification(message, type = 'success') {
    // Tạo phần tử thông báo
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.innerHTML = message;
    
    // Thêm vào body
    document.body.appendChild(notification);
    
    // Hiệu ứng hiển thị
    setTimeout(() => {
        notification.style.right = '20px';
    }, 100);
    
    // Tự động ẩn sau 3 giây
    setTimeout(() => {
        notification.style.right = '-300px';
        setTimeout(() => {
            document.body.removeChild(notification);
        }, 500);
    }, 3000);
}

// Hàm format date
function formatDate(date) {
    let d = new Date(date);
    let month = '' + (d.getMonth() + 1);
    let day = '' + d.getDate();
    let year = d.getFullYear();

    if (month.length < 2) month = '0' + month;
    if (day.length < 2) day = '0' + day;

    return [day, month, year].join('/');
}
