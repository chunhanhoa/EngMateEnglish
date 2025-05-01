// Script để theo dõi và cập nhật tiến trình học tập

class ProgressTracker {
    constructor() {
        this.userId = document.querySelector('meta[name="user-id"]')?.content;
        this.pendingToggles = {};
    }
    
    // Gửi cập nhật tiến trình học tập lên server
    updateProgress(data) {
        // Kiểm tra xem người dùng đã đăng nhập chưa
        if (!this.userId) {
            console.warn("Chưa đăng nhập, không thể cập nhật tiến độ");
            this.showNotification("Vui lòng đăng nhập để lưu tiến độ học tập", "warning");
            return Promise.reject("Chưa đăng nhập");
        }
        
        // Chuẩn bị dữ liệu gửi đi
        const progressData = {
            type: data.type, // 'Vocabulary', 'Grammar', 'Exercise'
            title: data.title,
            score: data.score || 0,
            pointsEarned: data.pointsEarned || 10,
            topicId: data.topicId || 0,
            topicName: data.topicName || '',
            completionPercentage: data.completionPercentage || 0
        };
        
        // Lấy token CSRF từ form
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        
        // Gửi request AJAX
        return fetch('/Progress/UpdateProgress', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token || ''
            },
            body: JSON.stringify(progressData)
        })
        .then(response => {
            if (!response.ok) {
                throw new Error('Lỗi khi cập nhật tiến độ');
            }
            return response.json();
        })
        .then(data => {
            console.log('Đã cập nhật tiến độ thành công:', data);
            return data;
        })
        .catch(error => {
            console.error('Lỗi khi cập nhật tiến độ:', error);
            this.showNotification("Có lỗi xảy ra khi cập nhật tiến độ", "danger");
            throw error;
        });
    }
    
    // Thêm/xóa khỏi danh sách yêu thích với cải tiến
    toggleFavorite(id, type) {
        // Kiểm tra xem người dùng đã đăng nhập chưa
        if (!this.userId) {
            console.warn("Chưa đăng nhập, không thể thêm vào yêu thích");
            this.showNotification("Vui lòng đăng nhập để sử dụng tính năng yêu thích", "warning");
            return Promise.reject("Chưa đăng nhập");
        }
        
        // Tạo key để theo dõi request
        const requestKey = `${type}-${id}`;
        
        // Nếu đang có request đang xử lý cho item này, không gửi request mới
        if (this.pendingToggles[requestKey]) {
            return this.pendingToggles[requestKey];
        }
        
        // Chuẩn bị dữ liệu gửi đi
        const favoriteData = {
            id: id,
            type: type // 'Vocabulary' hoặc 'Grammar'
        };
        
        // Lấy token CSRF từ form
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        
        // Gửi request AJAX và lưu promise để theo dõi
        this.pendingToggles[requestKey] = fetch('/Progress/ToggleFavorite', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token || ''
            },
            body: JSON.stringify(favoriteData)
        })
        .then(response => {
            if (!response.ok) {
                throw new Error('Lỗi khi cập nhật yêu thích');
            }
            return response.json();
        })
        .then(data => {
            console.log('Đã cập nhật yêu thích thành công:', data);
            return data;
        })
        .catch(error => {
            console.error('Lỗi khi cập nhật yêu thích:', error);
            this.showNotification("Có lỗi xảy ra khi cập nhật mục yêu thích", "danger");
            throw error;
        })
        .finally(() => {
            // Xóa request khỏi danh sách đang xử lý
            delete this.pendingToggles[requestKey];
        });
        
        return this.pendingToggles[requestKey];
    }
    
    // Hiển thị thông báo
    showNotification(message, type) {
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} notification-toast`;
        notification.style.position = 'fixed';
        notification.style.top = '20px';
        notification.style.right = '20px';
        notification.style.zIndex = '9999';
        notification.style.minWidth = '300px';
        notification.style.padding = '15px';
        notification.style.boxShadow = '0 0.5rem 1rem rgba(0,0,0,0.15)';
        notification.innerHTML = message;
        
        document.body.appendChild(notification);
        
        setTimeout(() => {
            notification.remove();
        }, 3000);
    }
}

// Khởi tạo đối tượng theo dõi tiến trình
window.progressTracker = new ProgressTracker();
