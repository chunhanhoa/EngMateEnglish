/* EngMate - JavaScript cho phần bài kiểm tra */

// Biến toàn cục
let currentQuestion = 1;
let totalQuestions = 0;
let timeLeft = 0;
let timerInterval;
let answeredQuestions = [];
let userAnswers = [];

// Khởi tạo bài kiểm tra
function initTest(duration, questions) {
    totalQuestions = questions;
    timeLeft = duration * 60; // Chuyển từ phút sang giây
    
    // Khởi tạo mảng theo dõi câu trả lời của người dùng
    answeredQuestions = new Array(totalQuestions).fill(false);
    userAnswers = new Array(totalQuestions).fill(null);
    
    // Khởi tạo bộ đếm thời gian
    startTimer();
    
    // Khởi tạo hiển thị câu hỏi
    showQuestion(currentQuestion);
    
    // Xử lý khi người dùng chọn đáp án
    $('.test-option').on('click', function() {
        // Loại bỏ class selected khỏi tất cả các option trong câu hỏi này
        $(this).closest('.test-options').find('.test-option').removeClass('selected');
        
        // Thêm class selected vào option được chọn
        $(this).addClass('selected');
        
        // Cập nhật trạng thái câu hỏi đã trả lời
        const questionCard = $(this).closest('.test-question-card');
        const questionIndex = parseInt(questionCard.data('question')) - 1;
        answeredQuestions[questionIndex] = true;
        userAnswers[questionIndex] = $(this).data('answer');
        
        // Cập nhật tiến trình
        updateProgressBar();
    });
    
    // Xử lý khi click nút Câu tiếp theo
    $('#nextBtn').on('click', function() {
        if (currentQuestion < totalQuestions) {
            currentQuestion++;
            showQuestion(currentQuestion);
        }
    });
    
    // Xử lý khi click nút Câu trước
    $('#prevBtn').on('click', function() {
        if (currentQuestion > 1) {
            currentQuestion--;
            showQuestion(currentQuestion);
        }
    });
}

// Hiển thị câu hỏi theo số thứ tự
function showQuestion(questionNumber) {
    // Ẩn tất cả các câu hỏi
    $('.test-question-card').hide();
    
    // Hiển thị câu hỏi với số thứ tự tương ứng
    $('#question-' + questionNumber).show();
    
    // Cập nhật trạng thái các nút điều hướng
    updateNavigationButtons();
    
    // Cập nhật thanh tiến trình
    updateProgressBar();
}

// Cập nhật trạng thái các nút điều hướng
function updateNavigationButtons() {
    // Nút Câu trước: vô hiệu hóa nếu đang ở câu đầu tiên
    $('#prevBtn').prop('disabled', currentQuestion === 1);
    
    // Nút Câu tiếp theo và Nộp bài
    if (currentQuestion === totalQuestions) {
        $('#nextBtn').hide();
        $('#submitBtn').show();
    } else {
        $('#nextBtn').show();
        $('#submitBtn').hide();
    }
    
    // Cập nhật text hiển thị tiến trình
    $('#progressText').text('Câu ' + currentQuestion + '/' + totalQuestions);
}

// Cập nhật thanh tiến trình
function updateProgressBar() {
    // Đếm số câu đã trả lời
    let answeredCount = answeredQuestions.filter(Boolean).length;
    
    // Cập nhật thanh tiến trình
    let progressPercentage = Math.round((answeredCount / totalQuestions) * 100);
    $('#progressBar').css('width', progressPercentage + '%');
    $('#progressBar').attr('aria-valuenow', progressPercentage);
    $('#progressBar').text(answeredCount + '/' + totalQuestions);
}

// Khởi tạo bộ đếm thời gian
function startTimer() {
    timerInterval = setInterval(function() {
        // Giảm thời gian còn lại
        timeLeft--;
        
        // Định dạng thời gian (MM:SS)
        let minutes = Math.floor(timeLeft / 60);
        let seconds = timeLeft % 60;
        
        // Cập nhật hiển thị thời gian
        $('#timer').text(minutes + ':' + (seconds < 10 ? '0' : '') + seconds);
        
        // Nếu thời gian hết
        if (timeLeft <= 0) {
            // Dừng bộ đếm thời gian
            clearInterval(timerInterval);
            
            // Tự động nộp bài
            alert('Hết thời gian! Bài kiểm tra của bạn sẽ được nộp tự động.');
            submitTest();
        }
    }, 1000);
}

// Nộp bài kiểm tra
function submitTest() {
    // Dừng bộ đếm thời gian
    clearInterval(timerInterval);
    
    // Tính toán kết quả
    let correctAnswers = 0;
    let answeredCount = answeredQuestions.filter(Boolean).length;
    
    // Đếm số câu trả lời đúng (trong thực tế sẽ cần so sánh với đáp án đúng)
    $('.test-question-card').each(function(index) {
        const selectedOption = $(this).find('.test-option.selected');
        if (selectedOption.length && selectedOption.data('correct') === true) {
            correctAnswers++;
        }
    });
    
    // Tính điểm
    const score = Math.round((correctAnswers / totalQuestions) * 100);
    
    // Lưu kết quả vào localStorage để hiển thị ở trang kết quả
    localStorage.setItem('testResult', JSON.stringify({
        testId: getTestId(),
        correctAnswers: correctAnswers,
        totalQuestions: totalQuestions,
        score: score,
        timeSpent: $('#timer').text(),
        answeredCount: answeredCount
    }));
    
    // Chuyển hướng đến trang kết quả
    window.location.href = '/Test/Result/' + getTestId();
}

// Lấy ID của bài kiểm tra từ URL
function getTestId() {
    // Trong thực tế, ID bài kiểm tra có thể được lấy từ nhiều nguồn khác nhau
    // Đây chỉ là một cách đơn giản để mô phỏng
    let pathParts = window.location.pathname.split('/');
    return pathParts[pathParts.length - 1];
}
