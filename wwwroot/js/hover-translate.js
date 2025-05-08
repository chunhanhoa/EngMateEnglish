document.addEventListener('DOMContentLoaded', function() {
    // Khởi tạo tooltip cho chức năng dịch
    const tooltip = document.createElement('div');
    tooltip.className = 'hover-translate-tooltip';
    tooltip.style.display = 'none';
    document.body.appendChild(tooltip);

    // Các phần tử tooltip
    const tooltipContent = document.createElement('div');
    tooltipContent.className = 'tooltip-content';
    
    const tooltipWord = document.createElement('div');
    tooltipWord.className = 'tooltip-word';
    
    const tooltipTranslation = document.createElement('div');
    tooltipTranslation.className = 'tooltip-translation';
    
    const tooltipActions = document.createElement('div');
    tooltipActions.className = 'tooltip-actions';
    
    const speakButton = document.createElement('button');
    speakButton.innerHTML = '<i class="fas fa-volume-up"></i>';
    speakButton.className = 'tooltip-speak-btn';
    
    tooltipActions.appendChild(speakButton);
    tooltipContent.appendChild(tooltipWord);
    tooltipContent.appendChild(tooltipTranslation);
    tooltipContent.appendChild(tooltipActions);
    tooltip.appendChild(tooltipContent);

    // Biến theo dõi trạng thái
    let selectedText = '';
    let isTooltipVisible = false;
    let currentTooltipWord = '';
    let isTranslating = false;

    // Bắt sự kiện mouseup để phát hiện văn bản được chọn
    document.addEventListener('mouseup', function(e) {
        const selection = window.getSelection();
        selectedText = selection.toString().trim();
        
        // Ẩn tooltip nếu không có văn bản được chọn
        if (!selectedText) {
            hideTooltip();
            return;
        }
        
        // Hiển thị tooltip gần văn bản được chọn
        const range = selection.getRangeAt(0);
        const rect = range.getBoundingClientRect();
        
        // Xử lý mọi loại văn bản (không giới hạn chỉ từ tiếng Anh đơn)
        showTooltip(rect, selectedText);
    });
    
    // Bắt sự kiện click trên trang để ẩn tooltip khi click ra ngoài
    document.addEventListener('mousedown', function(e) {
        if (isTooltipVisible && !tooltip.contains(e.target)) {
            hideTooltip();
        }
    });

    // Xử lý nút phát âm
    speakButton.addEventListener('click', function() {
        speakWord(currentTooltipWord);
    });

    // Hiển thị tooltip với dịch nghĩa
    function showTooltip(rect, text) {
        currentTooltipWord = text;
        
        // Điều chỉnh hiển thị dựa trên độ dài văn bản
        if (text.length > 50) {
            tooltipWord.textContent = text.substring(0, 50) + '...';
            tooltip.style.width = '350px'; // Mở rộng tooltip cho văn bản dài
        } else {
            tooltipWord.textContent = text;
            tooltip.style.width = text.length > 30 ? '300px' : '250px';
        }
        
        tooltipTranslation.innerHTML = '<div class="loading-spinner"></div>';
        
        // Định vị tooltip - điều chỉnh vị trí dựa trên kích thước màn hình
        const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
        const viewportWidth = Math.max(document.documentElement.clientWidth || 0, window.innerWidth || 0);
        
        // Xác định vị trí Y
        let topPosition = rect.bottom + scrollTop + 10;
        
        // Xác định vị trí X, tránh hiển thị ra ngoài màn hình
        let leftPosition = rect.left + rect.width/2 - parseInt(tooltip.style.width)/2;
        
        // Đảm bảo tooltip không bị lệch ra khỏi màn hình
        if (leftPosition < 10) leftPosition = 10;
        if (leftPosition + parseInt(tooltip.style.width) > viewportWidth - 10) {
            leftPosition = viewportWidth - parseInt(tooltip.style.width) - 10;
        }
        
        tooltip.style.top = `${topPosition}px`;
        tooltip.style.left = `${leftPosition}px`;
        tooltip.style.display = 'block';
        isTooltipVisible = true;
        
        // Gọi API dịch
        translateText(text);
    }

    // Ẩn tooltip
    function hideTooltip() {
        tooltip.style.display = 'none';
        isTooltipVisible = false;
    }

    // Phát âm từ/câu - hỗ trợ đọc cả câu dài
    function speakWord(word) {
        if ('speechSynthesis' in window) {
            // Dừng đang phát âm nếu có
            window.speechSynthesis.cancel();
            
            // Chia văn bản dài thành các phần nhỏ hơn nếu cần
            if (word.length > 200) {
                // Chia thành các đoạn dựa trên dấu câu
                const segments = word.match(/[^\.!\?]+[\.!\?]+/g) || [word];
                
                // Phát âm từng phần
                let index = 0;
                function speakNext() {
                    if (index < segments.length) {
                        const utterance = new SpeechSynthesisUtterance(segments[index]);
                        utterance.lang = 'en-US';
                        utterance.rate = 0.9;
                        
                        utterance.onend = function() {
                            index++;
                            speakNext();
                        };
                        
                        window.speechSynthesis.speak(utterance);
                    }
                }
                
                speakNext();
            } else {
                // Phát âm bình thường cho văn bản ngắn
                const utterance = new SpeechSynthesisUtterance(word);
                utterance.lang = 'en-US';
                utterance.rate = 0.9;
                window.speechSynthesis.speak(utterance);
            }
        }
    }

    // Dịch văn bản sử dụng API thực tế
    async function translateText(text) {
        if (isTranslating) return;
        isTranslating = true;
        
        try {
            // API CORS proxy để vượt qua giới hạn CORS trên các trình duyệt khác nhau
            // Sử dụng cors-anywhere hoặc một proxy CORS tương tự
            const proxyUrl = 'https://cors-anywhere.herokuapp.com/';
            const targetUrl = `https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=vi&dt=t&q=${encodeURIComponent(text)}`;
            
            // Thử phương pháp 1: Sử dụng proxy CORS
            try {
                const response = await fetch(proxyUrl + targetUrl, {
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'  // Yêu cầu bởi cors-anywhere
                    }
                });
                
                if (response.ok) {
                    const data = await response.json();
                    let translationResult = '';
                    
                    if (data && data[0]) {
                        data[0].forEach(item => {
                            if (item[0]) translationResult += item[0];
                        });
                    }
                    
                    tooltipTranslation.textContent = translationResult || 'Không thể dịch văn bản này';
                    return; // Thoát khỏi hàm nếu thành công
                }
            } catch (proxyError) {
                console.log("Proxy method failed, trying direct method", proxyError);
                // Tiếp tục thử phương pháp khác
            }
            
            // Phương pháp 2: Gọi trực tiếp API (hoạt động trên Edge)
            try {
                // Sử dụng API LibreTranslate - không cần API key
                const url = `https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=vi&dt=t&q=${encodeURIComponent(text)}`;
                const response = await fetch(url);
                
                if (response.ok) {
                    const data = await response.json();
                    let translationResult = '';
                    
                    if (data && data[0]) {
                        data[0].forEach(item => {
                            if (item[0]) translationResult += item[0];
                        });
                    }
                    
                    tooltipTranslation.textContent = translationResult || 'Không thể dịch văn bản này';
                    return; // Thoát khỏi hàm nếu thành công
                }
            } catch (directError) {
                console.log("Direct method failed", directError);
                // Tiếp tục thử phương pháp khác
            }
            
            // Phương pháp 3: Sử dụng API MyMemory
            const langPair = 'en|vi';
            const myMemoryUrl = `https://api.mymemory.translated.net/get?q=${encodeURIComponent(text)}&langpair=${langPair}`;
            
            const myMemoryResponse = await fetch(myMemoryUrl);
            
            if (myMemoryResponse.ok) {
                const data = await myMemoryResponse.json();
                
                if (data && data.responseData) {
                    tooltipTranslation.textContent = data.responseData.translatedText;
                    return; // Thoát khỏi hàm nếu thành công
                }
            }
            
            // Nếu tất cả phương pháp đều thất bại, sử dụng từ điển cục bộ
            throw new Error("All translation methods failed");
            
        } catch (error) {
            console.error('Translation error:', error);
            
            // Fallback: sử dụng từ điển cục bộ
            const commonTranslations = {
                'hello': 'xin chào',
                'world': 'thế giới',
                'computer': 'máy tính',
                'language': 'ngôn ngữ',
                'english': 'tiếng Anh',
                'vietnamese': 'tiếng Việt',
                'dictionary': 'từ điển',
                'translate': 'dịch',
                'vocabulary': 'từ vựng',
                'learn': 'học',
                'study': 'nghiên cứu',
                'student': 'học sinh',
                'teacher': 'giáo viên',
                'school': 'trường học',
                'university': 'đại học',
                'book': 'sách',
                'knowledge': 'kiến thức',
                'word': 'từ',
                'sentence': 'câu',
                'grammar': 'ngữ pháp',
                'speaking': 'nói',
                'writing': 'viết',
                'reading': 'đọc',
                'listening': 'nghe',
                'test': 'bài kiểm tra',
                'exam': 'kỳ thi',
                'lesson': 'bài học',
                'practice': 'thực hành',
                'exercise': 'bài tập',
                'question': 'câu hỏi',
                'answer': 'câu trả lời',
                'correct': 'đúng',
                'wrong': 'sai',
                'mistake': 'lỗi'
            };
            
            const lowerText = text.toLowerCase().trim();
            
            if (commonTranslations[lowerText]) {
                tooltipTranslation.textContent = commonTranslations[lowerText];
            } else {
                // Sử dụng heuristics đơn giản để tạo bản dịch tạm thời
                if (text.length < 5) {
                    tooltipTranslation.textContent = `"${text}" (không có trong từ điển)`;
                } else {
                    // Nếu là câu dài, thử tạo bản dịch dựa trên các từ đã biết
                    const words = text.toLowerCase().split(/\s+/);
                    let translated = '';
                    
                    words.forEach(word => {
                        const cleanWord = word.replace(/[.,!?;:'"()[\]{}]/g, '');
                        if (commonTranslations[cleanWord]) {
                            translated += commonTranslations[cleanWord] + ' ';
                        } else {
                            translated += word + ' ';
                        }
                    });
                    
                    tooltipTranslation.textContent = translated.trim() || 'Không thể dịch văn bản này';
                }
            }
        } finally {
            isTranslating = false;
        }
    }
});
