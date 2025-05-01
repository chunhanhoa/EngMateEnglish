document.addEventListener('DOMContentLoaded', function() {
    console.log('Avatar handler loaded');

    const defaultAvatar = '/images/default-avatar.png';
    const maxRetries = 3;

    // Hàm xử lý lỗi tải avatar
    window.handleAvatarError = function(img, originalSrc, defaultPath) {
        let retryCount = parseInt(img.getAttribute('data-retry-count') || '0');
        console.warn(`Lỗi tải avatar: ${img.src}, Retry: ${retryCount}`);

        if (retryCount >= maxRetries || originalSrc.includes('default-avatar.png')) {
            console.log('Đạt giới hạn retry hoặc đã dùng avatar mặc định');
            img.src = `${defaultPath}?v=${new Date().getTime()}`;
            img.onerror = null;
            img.classList.add('avatar-error');
            return;
        }

        // Thử lấy avatar từ API
        fetch('/debug/get-avatar-url', {
            headers: {
                'Cache-Control': 'no-cache',
                'Pragma': 'no-cache'
            }
        })
        .then(response => response.json())
        .then(data => {
            if (data.success && data.avatarPath && !data.avatarPath.includes('default-avatar.png')) {
                console.log(`Thử lại với avatar từ API: ${data.avatarPath}`);
                img.src = `${data.avatarPath}?v=${new Date().getTime()}`;
                img.setAttribute('data-retry-count', retryCount + 1);
            } else {
                console.warn('API trả về avatar mặc định hoặc lỗi:', data);
                img.src = `${defaultPath}?v=${new Date().getTime()}`;
                img.onerror = null;
                img.classList.add('avatar-error');
            }
        })
        .catch(error => {
            console.error('Lỗi khi lấy avatar từ API:', error);
            img.src = `${defaultPath}?v=${new Date().getTime()}`;
            img.onerror = null;
            img.classList.add('avatar-error');
        });
    };

    // Khởi tạo tất cả avatar
    document.querySelectorAll('img[src*="avatar"], .avatar, .avatar-image, .rounded-circle').forEach(img => {
        const originalSrc = img.src.split('?')[0];
        img.setAttribute('data-original-src', originalSrc);
        img.setAttribute('data-retry-count', '0');

        img.onerror = function() {
            handleAvatarError(this, originalSrc, defaultAvatar);
        };

        img.onload = function() {
            console.log(`Avatar tải thành công: ${this.src}`);
            img.classList.add('avatar-success');
            img.classList.remove('avatar-error');
        };
    });

    // Làm mới avatar khi nhấp vào
    document.querySelectorAll('.user-menu img.rounded-circle').forEach(img => {
        img.addEventListener('click', function(e) {
            e.stopPropagation();
            fetch('/debug/get-avatar-url')
                .then(response => response.json())
                .then(data => {
                    if (data.success && data.avatarPath) {
                        const newSrc = `${data.avatarPath}?v=${new Date().getTime()}`;
                        console.log(`Làm mới avatar: ${img.src} -> ${newSrc}`);
                        img.src = newSrc;
                        img.setAttribute('data-retry-count', '0');
                        img.classList.remove('avatar-error');
                    }
                })
                .catch(error => console.error('Lỗi khi làm mới avatar:', error));
        });
    });
});