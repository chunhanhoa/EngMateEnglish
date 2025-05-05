/**
 * Avatar Manager - Improved avatar handling for the TiengAnh application
 */
document.addEventListener('DOMContentLoaded', function() {
    console.log('Avatar Manager loaded');
    
    // Configuration
    const config = {
        defaultAvatarPath: '/images/default-avatar.png',
        maxRetries: 3,
        retryDelay: 1000,
        debug: true
    };
    
    // Log messages conditionally
    function log(...args) {
        if (config.debug) {
            console.log('[AvatarManager]', ...args);
        }
    }
    
    // Handle avatar error with robust fallback
    window.handleAvatarError = function(img, originalSrc, defaultPath) {
        // Get retry count from element or initialize it
        let retryCount = parseInt(img.getAttribute('data-retry-count') || '0');
        log(`Avatar load error: ${img.src}, retry: ${retryCount}`);
        
        // Mark as error for styling
        img.classList.add('avatar-error');
        
        // If using default avatar or exceeded retries, stop trying
        if (retryCount >= config.maxRetries || img.src.includes('default-avatar')) {
            log('Using default avatar');
            img.src = `${defaultPath || config.defaultAvatarPath}?v=${Date.now()}`;
            img.onerror = null; // Prevent infinite loop
            return;
        }
        
        // Increment retry count
        retryCount++;
        img.setAttribute('data-retry-count', retryCount.toString());
        
        // Try server-side avatar check API
        setTimeout(() => {
            fetch('/api/avatar/get-current?' + new URLSearchParams({
                userId: img.dataset.userid || '',
                timestamp: Date.now()
            }), {
                method: 'GET',
                headers: {
                    'Cache-Control': 'no-cache',
                    'Pragma': 'no-cache'
                },
                credentials: 'same-origin'
            })
            .then(response => response.json())
            .then(data => {
                if (data.success && data.avatarUrl && !data.avatarUrl.includes('default-avatar')) {
                    log(`Got avatar from API: ${data.avatarUrl}`);
                    img.src = `${data.avatarUrl}?v=${Date.now()}`;
                    img.classList.remove('avatar-error');
                } else {
                    log('API returned default avatar or error');
                    img.src = `${defaultPath || config.defaultAvatarPath}?v=${Date.now()}`;
                    img.onerror = null;
                }
            })
            .catch(error => {
                log('API error:', error);
                img.src = `${defaultPath || config.defaultAvatarPath}?v=${Date.now()}`;
                img.onerror = null;
            });
        }, config.retryDelay);
    };
    
    // Initialize all avatar images on the page
    function initializeAvatars() {
        document.querySelectorAll('img[src*="avatar"], .avatar-image, .rounded-circle[src*="avatar"]').forEach(img => {
            // Store original source
            const originalSrc = img.src.split('?')[0];
            img.setAttribute('data-original-src', originalSrc);
            
            // Reset retry count
            img.setAttribute('data-retry-count', '0');
            
            // Set up error handling
            img.onerror = function() {
                handleAvatarError(this, originalSrc, config.defaultAvatarPath);
            };
            
            // Set up load handler
            img.onload = function() {
                log(`Avatar loaded: ${this.src}`);
                this.classList.add('avatar-success');
                this.classList.remove('avatar-error');
            };
            
            // Add timestamp to prevent caching issues
            if (!img.src.includes('?v=')) {
                img.src = `${originalSrc}?v=${Date.now()}`;
                log(`Added timestamp to: ${originalSrc}`);
            }
        });
    }
    
    // Function to refresh all avatars
    window.refreshAllAvatars = function() {
        document.querySelectorAll('img[src*="avatar"], .avatar-image, .rounded-circle[src*="avatar"]').forEach(img => {
            const originalSrc = img.getAttribute('data-original-src') || img.src.split('?')[0];
            img.setAttribute('data-retry-count', '0');
            img.src = `${originalSrc}?v=${Date.now()}`;
            log(`Refreshed avatar: ${originalSrc}`);
        });
    };
    
    // Initialize avatars on page load
    initializeAvatars();
    
    // Re-initialize after AJAX requests
    if (window.jQuery) {
        $(document).ajaxComplete(function() {
            setTimeout(initializeAvatars, 100);
        });
    }
    
    // Refresh avatars periodically for long-running sessions
    setInterval(function() {
        if (document.visibilityState === 'visible') {
            log('Periodic avatar refresh');
            refreshAllAvatars();
        }
    }, 30 * 60 * 1000); // 30 minutes
});
