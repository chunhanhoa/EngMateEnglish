/**
 * Mô-đun theo dõi tiến trình học tập
 */
const ProgressTracker = (function() {
    // Hàm gọi API lưu tiến trình
    function saveProgress(itemId, type, score, topicId) {
        const requestData = {
            itemId: itemId,
            type: type,
            score: score,
            topicId: topicId
        };
        
        return fetch('/Progress/SaveProgress', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify(requestData)
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                console.log('Đã lưu tiến trình học tập');
            } else {
                console.error('Lỗi khi lưu tiến trình:', data.message);
            }
            return data;
        })
        .catch(error => {
            console.error('Lỗi kết nối:', error);
            return { success: false, message: 'Lỗi kết nối' };
        });
    }
    
    // Hàm gọi API cập nhật yêu thích
    function toggleFavorite(itemId, itemType) {
        const requestData = {
            itemId: itemId,
            itemType: itemType
        };
        
        return fetch('/Progress/ToggleFavorite', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify(requestData)
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                console.log('Đã cập nhật trạng thái yêu thích');
            } else {
                console.error('Lỗi khi cập nhật yêu thích:', data.message);
            }
            return data;
        })
        .catch(error => {
            console.error('Lỗi kết nối:', error);
            return { success: false, message: 'Lỗi kết nối' };
        });
    }
    
    return {
        saveProgress: saveProgress,
        toggleFavorite: toggleFavorite
    };
})();
