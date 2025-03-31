/* EngMate Animations */

// Wait for the DOM to be fully loaded
document.addEventListener('DOMContentLoaded', function() {
    // Initialize animations for elements with specific classes
    initializeAnimations();
    
    // Add scroll animations
    addScrollAnimations();
    
    // Add hover effects
    addHoverEffects();
    
    // Add audio button functionality
    setupAudioButtons();
});

// Initialize animations
function initializeAnimations() {
    // Topic cards animation
    const topicCards = document.querySelectorAll('.topic-card');
    
    topicCards.forEach((card, index) => {
        // Add a slight delay to each card for a staggered effect
        gsap.from(card, {
            duration: 0.6,
            opacity: 0,
            y: 30,
            delay: 0.2 + (index * 0.1),
            ease: "power2.out"
        });
    });
    
    // Vocabulary cards animation
    const vocabularyCards = document.querySelectorAll('.vocabulary-card');
    
    vocabularyCards.forEach((card, index) => {
        gsap.from(card, {
            duration: 0.6,
            opacity: 0,
            y: 20,
            delay: 0.3 + (index * 0.1),
            ease: "power2.out"
        });
    });
    
    // Main page hero section animation
    const heroTitle = document.querySelector('.hero-title');
    const heroText = document.querySelector('.hero-text');
    const heroButtons = document.querySelectorAll('.hero-section .btn');
    
    if (heroTitle) {
        gsap.from(heroTitle, {
            duration: 1,
            y: 50,
            opacity: 0,
            ease: "power3.out"
        });
    }
    
    if (heroText) {
        gsap.from(heroText, {
            duration: 1,
            y: 30,
            opacity: 0,
            delay: 0.3,
            ease: "power3.out"
        });
    }
    
    if (heroButtons.length > 0) {
        gsap.from(heroButtons, {
            duration: 0.8,
            y: 20,
            opacity: 0,
            stagger: 0.2,
            delay: 0.6,
            ease: "power3.out"
        });
    }
}

// Add scroll animations
function addScrollAnimations() {
    // Animate elements when they come into view
    const elementsToAnimate = document.querySelectorAll('.card, .topic-card, h2, .lead');
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                // If element is h2 or lead, fade in from bottom
                if (entry.target.tagName === 'H2' || entry.target.classList.contains('lead')) {
                    gsap.fromTo(entry.target, 
                        {opacity: 0, y: 30}, 
                        {duration: 0.8, opacity: 1, y: 0, ease: "power2.out"}
                    );
                } 
                // For cards, add a scale + fade animation
                else if (entry.target.classList.contains('card') || entry.target.classList.contains('topic-card')) {
                    gsap.fromTo(entry.target, 
                        {opacity: 0, y: 20, scale: 0.95}, 
                        {duration: 0.6, opacity: 1, y: 0, scale: 1, ease: "back.out(1.7)"}
                    );
                }
                
                // Once the animation is complete, stop observing this element
                observer.unobserve(entry.target);
            }
        });
    }, {
        threshold: 0.1, // Trigger when at least 10% of the element is visible
        rootMargin: "0px 0px -10% 0px" // Slightly offset to trigger before the element is fully in view
    });
    
    elementsToAnimate.forEach(element => {
        observer.observe(element);
    });
}

// Add hover effects
function addHoverEffects() {
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
    
    // Vocabulary cards hover effect
    const vocabularyCards = document.querySelectorAll('.vocabulary-card');
    
    vocabularyCards.forEach(card => {
        card.addEventListener('mouseenter', () => {
            gsap.to(card, {duration: 0.3, y: -5, boxShadow: "0 10px 25px rgba(0, 0, 0, 0.1)", ease: "power2.out"});
        });
        
        card.addEventListener('mouseleave', () => {
            gsap.to(card, {duration: 0.3, y: 0, boxShadow: "0 5px 15px rgba(0, 0, 0, 0.05)", ease: "power2.out"});
        });
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
