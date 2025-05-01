/* EngMate - JavaScript cho phần bài kiểm tra */

// Biến toàn cục
let currentQuestion = 1;
let totalQuestions = 0;
let timeLeft = 0;
let timerInterval;

// Khởi tạo bài kiểm tra
function initTest(duration, questions) {
    totalQuestions = questions;
    timeLeft = duration * 60; // Chuyển từ phút sang giây
    
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
    let progressPercentage = (currentQuestion / totalQuestions) * 100;
    $('#progressBar').css('width', progressPercentage + '%');
    $('#progressBar').attr('aria-valuenow', progressPercentage);
    $('#progressBar').text(currentQuestion + '/' + totalQuestions);
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
        
        // Cảnh báo khi còn ít thời gian
        if (timeLeft === 60) { // Còn 1 phút
            $('#timer').addClass('text-danger').addClass('fw-bold');
            $('#timer').parent().addClass('animate__animated animate__pulse animate__infinite');
        }
    }, 1000);
}

// Nộp bài kiểm tra
function submitTest() {
    // Dừng bộ đếm thời gian
    clearInterval(timerInterval);
    
    // Tính toán kết quả
    // Trong ứng dụng thực tế, dữ liệu này sẽ được gửi đến server
    let correctAnswers = 0;
    let wrongAnswers = 0;
    let unanswered = 0;
    
    // Mô phỏng việc tính điểm
    // Trong thực tế, điểm sẽ được tính dựa trên câu trả lời thực tế của người dùng
    correctAnswers = 15;
    wrongAnswers = 5;
    unanswered = totalQuestions - correctAnswers - wrongAnswers;
    
    // Lưu kết quả vào localStorage để hiển thị ở trang kết quả
    // Trong ứng dụng thực tế, kết quả sẽ được lưu trữ trong database
    localStorage.setItem('testResult', JSON.stringify({
        correctAnswers: correctAnswers,
        wrongAnswers: wrongAnswers,
        unanswered: unanswered,
        score: (correctAnswers / totalQuestions) * 100
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
