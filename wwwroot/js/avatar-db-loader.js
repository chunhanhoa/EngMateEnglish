/**
 * Script này giúp tải avatar trực tiếp từ database, tránh vấn đề cache
 */
document.addEventListener('DOMContentLoaded', function() {
    console.log("Avatar Database Loader loaded");

    // Lấy avatar mới từ server và cập nhật tất cả avatar trên trang
    function fetchLatestAvatar() {
        const url = '/debug/get-avatar-path?_=' + new Date().getTime();

        fetch(url, {
            method: 'GET',
            headers: {
                'Cache-Control': 'no-cache, no-store, must-revalidate',
                'Pragma': 'no-cache',
                'Expires': '0'
            },
            credentials: 'same-origin'
        })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok: ' + response.status);
            }
            return response.json();
        })
        .then(data => {
            console.log("Avatar response:", data);

            if (data && data.success && data.avatarPath) {
                const timestamp = new Date().getTime();
                const avatarUrl = data.avatarPath + "?v=" + timestamp;

                console.log("Đã nhận avatar mới từ DB:", data.avatarPath);
                console.log("Avatar URL với timestamp:", avatarUrl);

                // Cập nhật tất cả avatar trên trang
                document.querySelectorAll('img.avatar, .user-menu img.rounded-circle, td img.rounded-circle, #avatar-preview, .avatar-image, #modal-avatar-preview').forEach(img => {
                    console.log("Cập nhật avatar:", img.src, "->", avatarUrl);
                    img.src = avatarUrl;
                });

                // Lưu vào localStorage
                localStorage.setItem('currentUserAvatar', avatarUrl);
                localStorage.setItem('avatarTimestamp', timestamp.toString());

                console.log("Avatar đã được cập nhật thành công");
                showNotification("Đã tải avatar mới thành công!");
            } else {
                console.warn("Không nhận được avatar từ server:", data);
                showNotification("Không thể tải avatar: " + (data.message || "Lỗi không xác định"), "warning");
            }
        })
        .catch(error => {
            console.error("Lỗi khi tải avatar:", error);
            showNotification("Lỗi khi tải avatar: " + error.message, "error");
        });
    }

    // Hiển thị thông báo
    function showNotification(message, type = "success") {
        console.log("Notification:", message, type);
        // createNotificationElement(message, type); // Bỏ comment nếu cần
    }

    // Tạo phần tử thông báo
    function createNotificationElement(message, type = "success") {
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.innerHTML = message;
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 10px 20px;
            border-radius: 4px;
            z-index: 9999;
            opacity: 0;
            transition: opacity 0.3s ease;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        `;
        document.body.appendChild(notification);
        setTimeout(() => {
            notification.style.opacity = '1';
            setTimeout(() => {
                notification.style.opacity = '0';
                setTimeout(() => notification.remove(), 300);
            }, 3000);
        }, 10);
    }

    // Xử lý lỗi tải ảnh
    function setupImageErrorHandlers() {
        document.querySelectorAll('img').forEach(img => {
            img.onerror = function() {
                console.error("Lỗi tải ảnh:", this.src, "Status:", this.complete ? "Hoàn tất nhưng lỗi" : "Không tải được");
                if (this.src.includes('avatar') && !this.src.includes('default-avatar.png')) {
                    console.log("Chuyển sang ảnh mặc định:", this.src);
                    this.src = '/images/default-avatar.png?v=' + new Date().getTime();
                    this.onerror = null;
                }
            };
        });
    }

    // Cập nhật tất cả avatar với timestamp
    function updateAllAvatarSrc() {
        document.querySelectorAll('img[src*="avatar"]').forEach(img => {
            const currentSrc = img.getAttribute('src');
            if (!currentSrc.includes('default-avatar') && !currentSrc.includes('?v=')) {
                const timestamp = new Date().getTime();
                const newSrc = currentSrc.split('?')[0] + '?v=' + timestamp;
                console.log(`Cập nhật src avatar: ${currentSrc} -> ${newSrc}`);
                img.setAttribute('src', newSrc);
            }
        });
    }

    // Force reload tất cả avatar
    function forceReloadAllAvatars() {
        document.querySelectorAll('img[src*="avatar"]').forEach(img => {
            const timestamp = new Date().getTime();
            const baseUrl = img.src.split('?')[0];
            const newSrc = baseUrl + '?v=' + timestamp;

            const newImg = new Image();
            newImg.onload = function() {
                console.log("Đã tải lại avatar:", newSrc);
                img.src = newSrc;
            };
            newImg.onerror = function() {
                console.error("Không thể tải lại avatar:", newSrc);
                if (!newSrc.includes('default-avatar')) {
                    img.src = '/images/default-avatar.png?v=' + timestamp;
                }
            };
            newImg.src = newSrc;
        });
    }

    // Kiểm tra đăng nhập và chạy
    if (document.querySelector('.user-menu')) {
        const cachedAvatar = localStorage.getItem('currentUserAvatar');
        if (cachedAvatar && !cachedAvatar.includes('default-avatar')) {
            document.querySelectorAll('img.avatar').forEach(img => {
                img.src = cachedAvatar;
            });
        }
        setTimeout(() => {
            fetchLatestAvatar();
            setupImageErrorHandlers();
            updateAllAvatarSrc();
        }, 1000);
        setTimeout(() => {
            forceReloadAllAvatars();
        }, 1000);

        document.querySelectorAll('.user-menu img.rounded-circle').forEach(img => {
            img.addEventListener('click', function(e) {
                e.stopPropagation();
                fetchLatestAvatar();
            });
        });
    }

    window.addEventListener('beforeunload', function() {
        localStorage.setItem('lastAvatarRefresh', new Date().toISOString());
    });
});