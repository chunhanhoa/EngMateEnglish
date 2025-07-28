/**
 * Unified Avatar Manager - Giải quyết tất cả vấn đề về avatar
 */
document.addEventListener('DOMContentLoaded', function() {
    console.log('🔄 Unified Avatar Manager loaded');
    
    const CONFIG = {
        defaultAvatar: '/images/default-avatar.png',
        avatarPath: '/images/avatar/',
        maxRetries: 2,
        retryDelay: 1000,
        debug: true,
        endpoints: {
            getCurrentAvatar: '/Account/GetAvatarUrl',
            updateAvatar: '/Account/UpdateAvatar'
        }
    };
    
    function log(...args) {
        if (CONFIG.debug) {
            console.log('🖼️ [AvatarManager]', ...args);
        }
    }
    
    class AvatarManager {
        constructor() {
            this.initialize();
        }
        
        initialize() {
            log('Initializing Avatar Manager');
            this.setupErrorHandlers();
            this.validateExistingAvatars();
            this.setupRefreshHandlers();
        }
        
        // Kiểm tra xem file avatar có tồn tại không
        async checkAvatarExists(avatarUrl) {
            try {
                const cleanUrl = avatarUrl.split('?')[0]; // Remove query params
                const response = await fetch(cleanUrl, { method: 'HEAD' });
                return response.ok;
            } catch (error) {
                log('Error checking avatar existence:', error);
                return false;
            }
        }
        
        // Xử lý lỗi avatar và fallback
        async handleAvatarError(img, originalSrc) {
            const retryCount = parseInt(img.getAttribute('data-retry-count') || '0');
            log(`Avatar error: ${img.src}, retry: ${retryCount}`);
            
            img.classList.add('avatar-error');
            
            // Nếu đã thử nhiều lần hoặc đang dùng default avatar
            if (retryCount >= CONFIG.maxRetries || img.src.includes('default-avatar')) {
                log('Max retries reached or using default avatar');
                this.setDefaultAvatar(img);
                return;
            }
            
            // Thử lấy avatar mới từ server
            img.setAttribute('data-retry-count', (retryCount + 1).toString());
            
            try {
                const response = await fetch(CONFIG.endpoints.getCurrentAvatar, {
                    method: 'GET',
                    headers: {
                        'Cache-Control': 'no-cache',
                        'Pragma': 'no-cache'
                    },
                    credentials: 'same-origin'
                });
                
                const data = await response.json();
                
                if (data.success && data.avatarPath) {
                    const avatarExists = await this.checkAvatarExists(data.avatarPath);
                    
                    if (avatarExists && !data.avatarPath.includes('default-avatar')) {
                        log(`Using avatar from server: ${data.avatarPath}`);
                        img.src = `${data.avatarPath}?v=${Date.now()}`;
                        img.classList.remove('avatar-error');
                        return;
                    }
                }
                
                log('Server avatar not available, using default');
                this.setDefaultAvatar(img);
                
            } catch (error) {
                log('Server request failed:', error);
                this.setDefaultAvatar(img);
            }
        }
        
        // Set default avatar
        setDefaultAvatar(img) {
            img.src = `${CONFIG.defaultAvatar}?v=${Date.now()}`;
            img.onerror = null; // Prevent infinite loop
            img.classList.add('avatar-default');
        }
        
        // Setup error handlers cho tất cả avatar
        setupErrorHandlers() {
            this.findAllAvatars().forEach(img => {
                if (!img.hasAttribute('data-avatar-managed')) {
                    img.setAttribute('data-avatar-managed', 'true');
                    img.setAttribute('data-retry-count', '0');
                    
                    const originalSrc = img.src.split('?')[0];
                    img.setAttribute('data-original-src', originalSrc);
                    
                    // Remove existing error handlers
                    img.onerror = null;
                    
                    // Set new error handler
                    img.onerror = () => this.handleAvatarError(img, originalSrc);
                    
                    // Set load handler
                    img.onload = () => {
                        log(`Avatar loaded successfully: ${img.src}`);
                        img.classList.add('avatar-success');
                        img.classList.remove('avatar-error', 'avatar-default');
                    };
                    
                    log(`Setup handlers for: ${originalSrc}`);
                }
            });
        }
        
        // Validate existing avatars
        async validateExistingAvatars() {
            const avatars = this.findAllAvatars();
            log(`Validating ${avatars.length} avatars`);
            
            for (const img of avatars) {
                const src = img.src.split('?')[0];
                
                // Skip default avatars and data URLs
                if (src.includes('default-avatar') || src.startsWith('data:')) {
                    continue;
                }
                
                // Check if avatar file exists
                const exists = await this.checkAvatarExists(src);
                
                if (!exists) {
                    log(`Avatar file not found: ${src}`);
                    await this.handleAvatarError(img, src);
                } else {
                    log(`Avatar validated: ${src}`);
                }
            }
        }
        
        // Find all avatar images
        findAllAvatars() {
            return document.querySelectorAll(`
                img[src*="avatar"],
                .avatar-image,
                .user-menu img.rounded-circle,
                .avatar,
                #avatar-preview,
                #modal-avatar-preview
            `.replace(/\s+/g, ''));
        }
        
        // Setup refresh handlers
        setupRefreshHandlers() {
            // Click to refresh avatar
            document.addEventListener('click', (e) => {
                const avatar = e.target.closest('.user-menu img.rounded-circle');
                if (avatar) {
                    e.preventDefault();
                    e.stopPropagation();
                    this.refreshAvatar(avatar);
                }
            });
            
            // Auto refresh after AJAX
            if (window.jQuery) {
                $(document).ajaxComplete(() => {
                    setTimeout(() => this.setupErrorHandlers(), 100);
                });
            }
        }
        
        // Refresh specific avatar
        async refreshAvatar(img) {
            log('Manually refreshing avatar');
            
            try {
                const response = await fetch(CONFIG.endpoints.getCurrentAvatar, {
                    method: 'GET',
                    headers: {
                        'Cache-Control': 'no-cache',
                        'Pragma': 'no-cache'
                    },
                    credentials: 'same-origin'
                });
                
                const data = await response.json();
                
                if (data.success && data.avatarPath) {
                    const avatarExists = await this.checkAvatarExists(data.avatarPath);
                    
                    if (avatarExists) {
                        const newSrc = `${data.avatarPath}?v=${Date.now()}`;
                        log(`Refreshing to: ${newSrc}`);
                        
                        // Update all avatars on page
                        this.findAllAvatars().forEach(avatar => {
                            avatar.src = newSrc;
                            avatar.setAttribute('data-retry-count', '0');
                            avatar.classList.remove('avatar-error');
                        });
                        
                        this.showNotification('Đã làm mới avatar thành công!', 'success');
                    } else {
                        log('Refreshed avatar file does not exist');
                        this.setDefaultAvatar(img);
                        this.showNotification('Avatar không tồn tại, sử dụng avatar mặc định', 'warning');
                    }
                } else {
                    log('Failed to get avatar from server');
                    this.showNotification('Không thể tải avatar từ server', 'error');
                }
                
            } catch (error) {
                log('Refresh avatar error:', error);
                this.showNotification('Lỗi khi làm mới avatar', 'error');
            }
        }
        
        // Show notification
        showNotification(message, type = 'info') {
            console.log(`📢 ${type.toUpperCase()}: ${message}`);
            
            // Create notification element if needed
            const notification = document.createElement('div');
            notification.className = `alert alert-${type === 'error' ? 'danger' : type === 'success' ? 'success' : 'warning'} position-fixed`;
            notification.style.cssText = `
                top: 20px;
                right: 20px;
                z-index: 9999;
                max-width: 300px;
                opacity: 0;
                transition: opacity 0.3s ease;
            `;
            notification.innerHTML = `
                <i class="fas fa-${type === 'success' ? 'check' : type === 'error' ? 'exclamation-triangle' : 'info'}-circle me-2"></i>
                ${message}
            `;
            
            document.body.appendChild(notification);
            
            // Show and auto-hide
            setTimeout(() => notification.style.opacity = '1', 10);
            setTimeout(() => {
                notification.style.opacity = '0';
                setTimeout(() => notification.remove(), 300);
            }, 3000);
        }
        
        // Public method to refresh all avatars
        refreshAllAvatars() {
            log('Refreshing all avatars');
            this.findAllAvatars().forEach(img => {
                const originalSrc = img.getAttribute('data-original-src') || img.src.split('?')[0];
                img.src = `${originalSrc}?v=${Date.now()}`;
                img.setAttribute('data-retry-count', '0');
            });
        }
    }
    
    // Initialize the avatar manager
    window.avatarManager = new AvatarManager();
    
    // Global functions for backward compatibility
    window.handleAvatarError = (img, originalSrc, defaultPath) => {
        window.avatarManager.handleAvatarError(img, originalSrc);
    };
    
    window.refreshAllAvatars = () => {
        window.avatarManager.refreshAllAvatars();
    };
    
    // Periodic avatar validation for long sessions
    setInterval(() => {
        if (document.visibilityState === 'visible') {
            log('Periodic avatar validation');
            window.avatarManager.validateExistingAvatars();
        }
    }, 10 * 60 * 1000); // 10 minutes
});
