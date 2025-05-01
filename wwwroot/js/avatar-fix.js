/**
 * Script sửa lỗi avatar không hiển thị đúng
 */
document.addEventListener('DOMContentLoaded', function() {
    console.log('Avatar fix script loaded');
    
    // Tắt xử lý lỗi mặc định - chỉ log ra console
    const avatars = document.querySelectorAll('img[src*="avatar"]');
    console.log(`Tìm thấy ${avatars.length} avatar trên trang`);
    
    avatars.forEach((img, index) => {
        // Lưu URL ban đầu
        const originalSrc = img.getAttribute('src').split('?')[0];
        img.setAttribute('data-original-src', originalSrc);
        
        // Log thông tin
        console.log(`Avatar #${index + 1}: ${originalSrc}`);
        
        // Theo dõi trạng thái tải
        img.addEventListener('load', function() {
            console.log(`✓ Avatar #${index + 1} đã tải thành công: ${this.src}`);
        });
        
        // Thay thế xử lý lỗi hiện tại để tránh tự động chuyển sang avatar mặc định
        img.removeAttribute('onerror');
        img.onerror = function() {
            console.log(`❌ Không thể tải avatar: ${this.src}`);
            const timestamp = new Date().getTime();
            const originalPath = img.getAttribute('data-original-src');
            
            // Thử tải lại với timestamp mới để tránh cache
            if (!this.src.includes('?v=')) {
                console.log(`Thử tải lại với timestamp: ${originalPath}?v=${timestamp}`);
                this.src = `${originalPath}?v=${timestamp}`;
            } 
            // Nếu đã thử tải lại rồi mà vẫn lỗi, mới dùng ảnh mặc định
            else if (!this.src.includes('default-avatar.png')) {
                console.log('Chuyển sang avatar mặc định');
                this.src = `/images/default-avatar.png?v=${timestamp}`;
            }
        };
    });
    
    // Thêm nút làm mới avatar vào trang profile
    if (window.location.pathname.includes('/Account/Profile')) {
        const profileHeader = document.querySelector('.card-header');
        if (profileHeader) {
            const refreshBtn = document.createElement('button');
            refreshBtn.className = 'btn btn-sm btn-outline-info float-end';
            refreshBtn.innerHTML = '<i class="fas fa-sync-alt"></i> Làm mới avatar';
            refreshBtn.addEventListener('click', function() {
                const timestamp = new Date().getTime();
                const avatarImages = document.querySelectorAll('img[src*="avatar"]');
                
                avatarImages.forEach(img => {
                    const originalSrc = img.getAttribute('data-original-src') || img.src.split('?')[0];
                    img.src = `${originalSrc}?v=${timestamp}`;
                });
                
                alert('Đã làm mới avatar!');
            });
            profileHeader.appendChild(refreshBtn);
        }
    }
});
