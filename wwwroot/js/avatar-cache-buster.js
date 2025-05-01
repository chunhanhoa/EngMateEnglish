/**
 * JavaScript để giải quyết vấn đề cache cho avatar
 */
document.addEventListener('DOMContentLoaded', function() {
    // Debug avatar URL
    console.log('Avatar-cache-buster.js loaded');
    
    // Cập nhật tất cả avatar với timestamp mới và sửa lỗi
    function updateAvatarSrcs() {
        const timestamp = new Date().getTime();
        document.querySelectorAll('img[src*="avatar"]').forEach(img => {
            const currentSrc = img.getAttribute('src');
            if (currentSrc && !img.classList.contains('avatar-updated') && !currentSrc.includes('default-avatar.png?v=')) {
                // Kiểm tra xem ảnh đã tải xong chưa và có kích thước hay không
                if (img.complete && img.naturalWidth === 0) {
                    console.warn("Avatar không tải được:", currentSrc);
                    return; // Bỏ qua ảnh lỗi
                }
                
                // Loại bỏ timestamp cũ nếu có
                const baseUrl = currentSrc.split('?')[0];
                img.setAttribute('data-original-src', baseUrl); // Lưu URL gốc
                const newSrc = `${baseUrl}?v=${timestamp}`;
                img.setAttribute('src', newSrc);
                img.classList.add('avatar-updated');
                console.log(`Đã cập nhật avatar: ${baseUrl} => ${newSrc}`);
            }
        });
    }

    // Xử lý lỗi khi avatar không tải được
    function handleAvatarErrors() {
        document.querySelectorAll('img[src*="avatar"]').forEach(img => {
            if (!img.hasAttribute('data-error-handled')) {
                img.setAttribute('data-error-handled', 'true');
                
                // Lưu trình xử lý lỗi gốc
                const originalErrorHandler = img.onerror;
                
                img.onerror = function() {
                    console.warn("Lỗi tải avatar:", this.src);
                    
                    // Chỉ sử dụng avatar mặc định nếu ảnh hiện tại không phải avatar mặc định
                    if (!this.src.includes('default-avatar.png')) {
                        const timestamp = new Date().getTime();
                        console.log('Chuyển sang avatar mặc định');
                        this.src = `/images/default-avatar.png?v=${timestamp}`;
                    }
                    
                    // Tắt xử lý lỗi để tránh vòng lặp vô tận
                    this.onerror = null;
                };
            }
        });
    }
    
    // Kiểm tra liệu avatar đã tồn tại trong hệ thống chưa
    function checkAvatarExists() {
        document.querySelectorAll('img[src*="avatar"]').forEach(img => {
            if (img.complete && img.naturalWidth > 0) {
                console.log("Avatar hợp lệ:", img.src);
            } else if (img.complete) {
                console.log("Avatar không hợp lệ:", img.src);
            }
        });
    }
    
    // Cập nhật ngay khi trang tải xong
    setTimeout(function() {
        updateAvatarSrcs();
        handleAvatarErrors();
        
        // Kiểm tra avatar sau khi xử lý
        setTimeout(checkAvatarExists, 1000);
    }, 100);
    
    // Cập nhật sau mỗi lần AJAX hoàn thành
    if (typeof $ !== 'undefined') {
        $(document).ajaxComplete(function() {
            setTimeout(function() {
                updateAvatarSrcs();
                handleAvatarErrors();
            }, 100);
        });
    }
});
