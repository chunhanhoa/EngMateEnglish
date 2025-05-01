// Quản lý chức năng yêu thích trên các trang
class FavoriteHandler {
    constructor() {
        // Store the current page information
        this.currentPage = window.location.pathname;
    }
    
    // Cập nhật UI sau khi thêm/xóa yêu thích
    updateFavoriteUI(button, isFavorite) {
        const icon = button.querySelector('i');
        
        if (isFavorite) {
            if (icon.classList.contains('far')) {
                icon.classList.replace('far', 'fas');
            }
            if (button.classList.contains('btn-outline-danger')) {
                button.classList.replace('btn-outline-danger', 'btn-danger');
            }
            button.setAttribute('title', 'Đã thêm vào yêu thích');
        } else {
            if (icon.classList.contains('fas')) {
                icon.classList.replace('fas', 'far');
            }
            if (button.classList.contains('btn-danger')) {
                button.classList.replace('btn-danger', 'btn-outline-danger');
            }
            button.setAttribute('title', 'Thêm vào yêu thích');
        }
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
    
    // Cập nhật trang yêu thích nếu đang mở
    updateFavoritesPage(action, itemId, itemType) {
        // Tìm các tab hoặc cửa sổ khác có thể đang mở trang yêu thích
        if (window.opener) {
            // Trường hợp tab mới được mở từ trang yêu thích
            try {
                if (window.opener.location.pathname.includes('/Progress/Favorites')) {
                    window.opener.postMessage({
                        action: action,
                        itemId: itemId,
                        itemType: itemType
                    }, window.location.origin);
                }
            } catch (e) {
                console.error('Không thể cập nhật cửa sổ mở:', e);
            }
        }
        
        // Gửi thông điệp tới tất cả các tab khác
        if (window.localStorage) {
            // Sử dụng localStorage để giao tiếp giữa các tab
            const message = {
                action: action,
                itemId: itemId,
                itemType: itemType,
                timestamp: new Date().getTime()
            };
            window.localStorage.setItem('favoriteUpdate', JSON.stringify(message));
        }
    }
    
    // Xử lý click vào nút yêu thích
    handleFavoriteClick(button, itemId, itemType) {
        // Gọi API để toggle trạng thái yêu thích
        window.progressTracker.toggleFavorite(itemId, itemType)
            .then(data => {
                if (data.success) {
                    // Kiểm tra trạng thái hiện tại
                    const icon = button.querySelector('i');
                    const isFavorite = icon.classList.contains('far'); // Sẽ chuyển thành yêu thích
                    
                    // Cập nhật UI
                    this.updateFavoriteUI(button, isFavorite);
                    
                    // Thông báo
                    if (isFavorite) {
                        this.showNotification(`Đã thêm vào danh sách yêu thích`, 'success');
                        // Cập nhật trang yêu thích
                        this.updateFavoritesPage('add', itemId, itemType);
                    } else {
                        this.showNotification(`Đã xóa khỏi danh sách yêu thích`, 'info');
                        // Cập nhật trang yêu thích
                        this.updateFavoritesPage('remove', itemId, itemType);
                    }
                }
            })
            .catch(error => {
                console.error('Lỗi khi cập nhật trạng thái yêu thích:', error);
                this.showNotification('Có lỗi xảy ra. Vui lòng đăng nhập và thử lại', 'danger');
            });
    }
    
    // Khởi tạo lắng nghe sự kiện
    initFavoriteButtons() {
        const buttons = document.querySelectorAll('.favorite-btn, .remove-favorite-btn');
        buttons.forEach(button => {
            button.addEventListener('click', (e) => {
                e.preventDefault();
                
                // Lấy thông tin itemId và itemType
                const itemId = button.getAttribute('data-id');
                
                // Xác định itemType dựa trên trang hiện tại hoặc attribute
                let itemType = button.getAttribute('data-type');
                if (!itemType) {
                    if (this.currentPage.includes('/Vocabulary')) {
                        itemType = 'Vocabulary';
                    } else if (this.currentPage.includes('/Grammar')) {
                        itemType = 'Grammar';
                    } else if (this.currentPage.includes('/Progress/Favorites')) {
                        // Trong trang yêu thích, xác định loại dựa trên tab hiện tại
                        itemType = button.closest('.tab-pane').id === 'vocabulary' ? 'Vocabulary' : 'Grammar';
                    }
                }
                
                // Xử lý click
                this.handleFavoriteClick(button, itemId, itemType);
            });
        });
        
        // Lắng nghe thông điệp từ các tab khác thông qua localStorage
        window.addEventListener('storage', (event) => {
            if (event.key === 'favoriteUpdate') {
                const message = JSON.parse(event.newValue);
                
                // Kiểm tra xem có phải là trang yêu thích không
                if (this.currentPage.includes('/Progress/Favorites')) {
                    this.handleFavoritesPageUpdate(message);
                }
            }
        });
    }
    
    // Xử lý cập nhật trang yêu thích
    handleFavoritesPageUpdate(message) {
        const { action, itemId, itemType } = message;
        const tabId = itemType.toLowerCase();
        
        if (action === 'remove') {
            // Xóa mục khỏi danh sách yêu thích
            const item = document.querySelector(`#${tabId} .favorite-item[data-item-id="${itemId}"]`);
            if (item) {
                item.remove();
                
                // Cập nhật UI
                this.updateFavoritesTab(tabId);
            }
        } else if (action === 'add') {
            // Reload để lấy dữ liệu mới
            window.location.reload();
        }
    }
    
    // Cập nhật tab yêu thích
    updateFavoritesTab(tabId) {
        const tabPane = document.getElementById(tabId);
        if (!tabPane) return;
        
        const items = tabPane.querySelectorAll('.favorite-item');
        
        // Cập nhật badge
        const badge = document.querySelector(`#${tabId}-tab .badge`);
        if (badge) {
            badge.textContent = items.length;
            if (items.length === 0) {
                badge.style.display = 'none';
            } else {
                badge.style.display = 'inline-block';
            }
        }
        
        // Kiểm tra xem còn mục nào không
        if (items.length === 0) {
            const emptyMessage = this.createEmptyMessage(tabId === 'vocabulary' ? 'Vocabulary' : 'Grammar');
            tabPane.querySelector('.row').appendChild(emptyMessage);
        }
    }
    
    // Tạo thông báo trống cho tab
    createEmptyMessage(itemType) {
        const div = document.createElement('div');
        div.className = 'col-12 text-center py-5';
        div.innerHTML = `
            <div class="mb-4">
                <i class="fas fa-heart-broken text-muted" style="font-size: 4rem;"></i>
            </div>
            <h3 class="mb-3">Chưa có mục yêu thích</h3>
            <p class="text-muted">Hãy nhấn vào biểu tượng trái tim để thêm ${itemType === 'Vocabulary' ? 'từ vựng' : 'ngữ pháp'} vào danh sách yêu thích của bạn.</p>
            <a href="/${itemType}" class="btn btn-primary mt-2">
                <i class="fas fa-search me-2"></i> Khám phá ${itemType === 'Vocabulary' ? 'từ vựng' : 'ngữ pháp'}
            </a>`;
        return div;
    }
}

// Khởi tạo và export
window.favoriteHandler = new FavoriteHandler();
document.addEventListener('DOMContentLoaded', () => {
    window.favoriteHandler.initFavoriteButtons();
});
