/**
 * Toast notification system for EngMate app
 */
document.addEventListener('DOMContentLoaded', function() {
    // Function to create and show a toast
    window.showNotification = function(message, type = 'success', autoClose = true, duration = 5000) {
        const toastId = 'toast-' + Date.now();
        const icon = type === 'success' ? 'fas fa-check-circle' : 'fas fa-exclamation-circle';
        const bgClass = type === 'success' ? 'bg-success text-white' : 'bg-danger text-white';
        
        // Create toast container if it doesn't exist
        let toastContainer = document.querySelector('.toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
            toastContainer.style.zIndex = '1080';
            document.body.appendChild(toastContainer);
        }
        
        const toastHtml = `
            <div id="${toastId}" class="toast align-items-center ${bgClass} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="${icon} me-2"></i> ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;
        
        toastContainer.insertAdjacentHTML('beforeend', toastHtml);
        
        const toastElement = document.getElementById(toastId);
        const toast = new bootstrap.Toast(toastElement, {
            autohide: autoClose,
            delay: duration
        });
        
        toast.show();
        
        // Auto-remove from DOM after hiding
        toastElement.addEventListener('hidden.bs.toast', function () {
            toastElement.remove();
        });
        
        return toastId;
    };
    
    // Process any notifications from TempData
    const processPageNotifications = () => {
        const successMsg = document.getElementById('temp-success-message');
        if (successMsg && successMsg.value) {
            showNotification(successMsg.value, 'success');
        }
        
        const errorMsg = document.getElementById('temp-error-message');
        if (errorMsg && errorMsg.value) {
            showNotification(errorMsg.value, 'danger');
        }
    };
    
    processPageNotifications();
});
