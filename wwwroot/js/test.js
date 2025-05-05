/* EngMate - JavaScript for test functionality */

// Global variables
let currentQuestion = 1;
let totalQuestions = 0;
let timeLeft = 0;
let timerInterval;
let answeredQuestions = new Set();

// Initialize test
function initTest(duration, questions) {
    totalQuestions = questions;
    timeLeft = duration * 60; // Convert minutes to seconds
    
    // Start timer
    startTimer();
    
    // Show first question
    showQuestion(currentQuestion);
    updateProgressBar();
    
    // Handle option selection
    $('.test-option').on('click', function() {
        const questionNumber = $(this).closest('.test-question').attr('id').split('-')[1];
        answeredQuestions.add(parseInt(questionNumber));
        
        // Update navigation buttons
        $(`.question-nav-btn[data-question="${questionNumber}"]`).removeClass('btn-outline-secondary btn-primary').addClass('btn-success');
        
        // Update progress
        updateProgressBar();
    });
    
    // Next button click
    $('#nextBtn').on('click', function() {
        if (currentQuestion < totalQuestions) {
            currentQuestion++;
            showQuestion(currentQuestion);
            updateProgressBar();
        }
    });
    
    // Previous button click
    $('#prevBtn').on('click', function() {
        if (currentQuestion > 1) {
            currentQuestion--;
            showQuestion(currentQuestion);
            updateProgressBar();
        }
    });
    
    // Question navigation buttons
    $('.question-nav-btn').on('click', function() {
        const questionNum = parseInt($(this).data('question'));
        currentQuestion = questionNum;
        showQuestion(currentQuestion);
        updateProgressBar();
    });
    
    // Submit button click
    $('#submitBtn, #finalSubmitBtn').on('click', function() {
        if (confirm('Bạn có chắc muốn nộp bài?')) {
            submitTest();
        }
    });
}

// Show specific question
function showQuestion(questionNumber) {
    // Hide all questions
    $('.test-question').addClass('d-none');
    
    // Show selected question
    $(`#question-${questionNumber}`).removeClass('d-none');
    
    // Update navigation buttons
    updateNavigationButtons();
    
    // Update question navigation highlighting
    $('.question-nav-btn').removeClass('btn-primary');
    $(`.question-nav-btn[data-question="${questionNumber}"]`).addClass('btn-primary');
    
    // Update progress text
    $('#progress-text').text(`Câu ${questionNumber}/${totalQuestions}`);
}

// Update navigation buttons
function updateNavigationButtons() {
    $('#prevBtn').prop('disabled', currentQuestion === 1);
    
    if (currentQuestion === totalQuestions) {
        $('#nextBtn').addClass('d-none');
        $('#submitBtn').removeClass('d-none');
    } else {
        $('#nextBtn').removeClass('d-none');
        $('#submitBtn').addClass('d-none');
    }
}

// Update progress bar
function updateProgressBar() {
    const progress = (answeredQuestions.size / totalQuestions) * 100;
    $('#progress-bar').css('width', `${progress}%`);
    $('#progress-percentage').text(`${Math.round(progress)}%`);
}

// Start timer
function startTimer() {
    timerInterval = setInterval(function() {
        timeLeft--;
        
        const minutes = Math.floor(timeLeft / 60);
        const seconds = timeLeft % 60;
        
        $('#time-remaining').text(`${minutes}:${seconds < 10 ? '0' : ''}${seconds}`);
        
        if (timeLeft <= 0) {
            clearInterval(timerInterval);
            alert('Hết thời gian! Bài kiểm tra của bạn sẽ được nộp tự động.');
            submitTest();
        }
    }, 1000);
}

// Submit test
function submitTest() {
    // Stop timer
    clearInterval(timerInterval);
    
    // Get answers
    const answers = [];
    for (let i = 1; i <= totalQuestions; i++) {
        const selectedOption = $(`input[name="question-${i}"]:checked`).val();
        answers.push({
            questionId: i,
            selectedOption: selectedOption !== undefined ? parseInt(selectedOption) : -1
        });
    }
    
    // Calculate score
    const answeredCount = answers.filter(a => a.selectedOption !== -1).length;
    const notAnsweredCount = totalQuestions - answeredCount;
    
    // Store data in localStorage for result page
    localStorage.setItem('testAnswers', JSON.stringify(answers));
    localStorage.setItem('testMetadata', JSON.stringify({
        testId: getTestId(),
        totalQuestions: totalQuestions,
        answeredQuestions: answeredCount,
        notAnsweredQuestions: notAnsweredCount,
        timeUsed: timeLeft
    }));
    
    // Redirect to results page
    window.location.href = `/Test/Result/${getTestId()}`;
}

// Extract test ID from URL
function getTestId() {
    const pathParts = window.location.pathname.split('/');
    return pathParts[pathParts.length - 1];
}

/* Test script for TiengAnh */

document.addEventListener('DOMContentLoaded', function() {
    // This script will handle the test functionality
    console.log('Test script loaded');
});
