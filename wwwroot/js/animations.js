/* EngMate Animations */

// Wait for the DOM to be fully loaded
document.addEventListener('DOMContentLoaded', function() {
    // Initialize animations for elements with specific classes
    initializeAnimations();
    
    // Add hover effects
    addHoverEffects();
    
    // Add scroll animations
    initScrollAnimations();
    
    // Add audio button functionality
    setupAudioButtons();
});

// Initialize animations
function initializeAnimations() {
    // Tìm các phần tử cần hiển thị
    const heroTitle = document.querySelector('.hero-section h1');
    const heroText = document.querySelector('.hero-section p');
    const heroButtons = document.querySelectorAll('.hero-section .btn');
    
    // Hiển thị ngay lập tức thay vì animation
    if (heroTitle) {
        heroTitle.style.opacity = 1;
        heroTitle.style.transform = 'translateY(0)';
    }
    
    if (heroText) {
        heroText.style.opacity = 1;
        heroText.style.transform = 'translateY(0)';
    }
    
    if (heroButtons.length > 0) {
        heroButtons.forEach(button => {
            button.style.opacity = 1;
            button.style.transform = 'translateY(0)';
        });
    }

    // Hiển thị ngay các danh sách chủ đề và từ vựng
    const topicCards = document.querySelectorAll('.topic-card, .vocabulary-card');
    if (topicCards.length > 0) {
        topicCards.forEach(card => {
            card.style.opacity = 1;
            card.style.transform = 'translateY(0)';
        });
    }
}

// Add hover effects
function addHoverEffects() {
    // Vẫn giữ hiệu ứng hover nhưng loại bỏ animation khi load
    const cards = document.querySelectorAll('.feature-card, .topic-card');
    
    // Audio buttons hover effect
    const audioButtons = document.querySelectorAll('.audio-btn');
    
    audioButtons.forEach(button => {
        button.addEventListener('mouseenter', () => {
            gsap.to(button, {duration: 0.3, scale: 1.1, ease: "power1.out"});
        });
        
        button.addEventListener('mouseleave', () => {
            gsap.to(button, {duration: 0.3, scale: 1, ease: "power1.out"});
        });
    });
    
    // Topic cards hover effect
    const topicCards = document.querySelectorAll('.topic-card');
    
    topicCards.forEach(card => {
        card.addEventListener('mouseenter', () => {
            gsap.to(card, {duration: 0.3, y: -5, boxShadow: "0 10px 25px rgba(0, 0, 0, 0.1)", ease: "power2.out"});
        });
        
        card.addEventListener('mouseleave', () => {
            gsap.to(card, {duration: 0.3, y: 0, boxShadow: "0 5px 15px rgba(0, 0, 0, 0.05)", ease: "power2.out"});
        });
    });
    
    // Vocabulary cards hover effect - giữ nguyên hiệu ứng hover
    const vocabularyCards = document.querySelectorAll('.vocabulary-card');
    
    vocabularyCards.forEach(card => {
        // Set hiển thị mặc định ngay lập tức
        card.style.opacity = 1;
        card.style.transform = 'translateY(0)';
        
        card.addEventListener('mouseenter', () => {
            gsap.to(card, {duration: 0.3, y: -5, boxShadow: "0 10px 25px rgba(0, 0, 0, 0.1)", ease: "power2.out"});
        });
        
        card.addEventListener('mouseleave', () => {
            gsap.to(card, {duration: 0.3, y: 0, boxShadow: "0 5px 15px rgba(0, 0, 0, 0.05)", ease: "power2.out"});
        });
    });
}

// Sửa đổi phần scroll animation để không có hiệu ứng fading
function initScrollAnimations() {
    const elementsToAnimate = document.querySelectorAll('.fade-in, .fade-in-left, .fade-in-right');
    
    elementsToAnimate.forEach(element => {
        // Hiển thị ngay lập tức không cần scroll trigger
        element.style.opacity = 1;
        element.style.transform = 'translateY(0) translateX(0)';
    });
}

// Setup audio buttons
function setupAudioButtons() {
    const audioButtons = document.querySelectorAll('.audio-btn');
    
    // Giả lập âm thanh phát âm cho các từ
    const sampleWords = {
        'Cat': '/audio/cat.mp3',
        'Dog': '/audio/dog.mp3',
        'Elephant': '/audio/elephant.mp3',
        'Lion': '/audio/lion.mp3',
        'Apple': '/audio/apple.mp3',
        'Banana': '/audio/banana.mp3',
        'Orange': '/audio/orange.mp3',
        'Pencil': '/audio/pencil.mp3',
        'Book': '/audio/book.mp3'
    };
    
    audioButtons.forEach(button => {
        button.addEventListener('click', function() {
            // Tìm từ vựng gần nhất
            const card = button.closest('.vocabulary-card');
            const wordElement = card.querySelector('.word');
            
            if (wordElement) {
                const word = wordElement.textContent;
                
                // Check if we have an audio for this word
                if (sampleWords[word]) {
                    // Create and play audio
                    const audio = new Audio(sampleWords[word]);
                    
                    // Add animation to button while playing
                    button.classList.add('playing');
                    button.innerHTML = '<i class="fas fa-volume-up fa-beat"></i>';
                    
                    audio.play();
                    
                    // Reset button after audio ends
                    audio.onended = function() {
                        button.classList.remove('playing');
                        button.innerHTML = '<i class="fas fa-volume-up"></i>';
                    };
                }
            }
        });
    });
}

// Quiz functionality
function setupQuiz() {
    const quizOptions = document.querySelectorAll('.quiz-option');
    
    quizOptions.forEach(option => {
        option.addEventListener('click', function() {
            // First remove selected class from all options
            const allOptions = this.closest('.quiz-question').querySelectorAll('.quiz-option');
            allOptions.forEach(opt => opt.classList.remove('selected'));
            
            // Add selected class to clicked option
            this.classList.add('selected');
            
            // Check if correct
            if (this.dataset.correct === 'true') {
                // Wait a moment before showing the result
                setTimeout(() => {
                    this.classList.add('correct');
                    
                    // Animate the correct answer
                    gsap.to(this, {
                        duration: 0.5,
                        backgroundColor: '#e6fff2',
                        borderColor: '#5cb85c',
                        ease: "power2.out"
                    });
                }, 500);
            } else {
                // Wait a moment before showing the result
                setTimeout(() => {
                    this.classList.add('incorrect');
                    
                    // Animate the incorrect answer
                    gsap.to(this, {
                        duration: 0.5,
                        backgroundColor: '#ffe6e6',
                        borderColor: '#d9534f',
                        ease: "power2.out"
                    });
                    
                    // Find and highlight the correct option
                    allOptions.forEach(opt => {
                        if (opt.dataset.correct === 'true') {
                            opt.classList.add('correct');
                            
                            // Animate the correct option
                            gsap.to(opt, {
                                duration: 0.5,
                                backgroundColor: '#e6fff2',
                                borderColor: '#5cb85c',
                                ease: "power2.out"
                            });
                        }
                    });
                }, 500);
            }
        });
    });
}

// Initialize animations on page load
window.addEventListener('load', function() {
    // If quiz exists on the page, set it up
    if (document.querySelector('.quiz-option')) {
        setupQuiz();
    }
});
