using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using TiengAnh.Models;
using TiengAnh.Repositories;
using System.Linq;
using System.Text.Json;
using System.Security.Claims;

namespace TiengAnh.Controllers
{
    [Authorize] // Add this attribute to require authentication
    public class TestController : Controller
    {
        private readonly ITestRepository _testRepository;
        private readonly UserTestRepository _userTestRepository;
        private readonly ILogger<TestController> _logger;
        private readonly string _webRootPath;

        public TestController(
            ITestRepository testRepository,
            UserTestRepository userTestRepository,
            ILogger<TestController> logger,
            Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _testRepository = testRepository;
            _userTestRepository = userTestRepository;
            _logger = logger;
            _webRootPath = env.WebRootPath;
        }

        // GET: /Test
        public async Task<IActionResult> Index()
        {
            try
            {
                var tests = await _testRepository.GetAllTestsAsync();
                
                // Check for missing images and set defaults
                foreach (var test in tests)
                {
                    if (!string.IsNullOrEmpty(test.ImageUrl))
                    {
                        string imagePath = Path.Combine(_webRootPath, test.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (!System.IO.File.Exists(imagePath))
                        {
                            test.ImageUrl = "/images/tests/default-test.jpg";
                        }
                    }
                    else
                    {
                        test.ImageUrl = "/images/tests/default-test.jpg";
                    }
                }
                
                return View(tests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tests");
                TempData["Error"] = "Không thể tải danh sách bài kiểm tra. Vui lòng thử lại.";
                return View(new List<TestModel>());
            }
        }

        // GET: /Test/Details/{id}
        [Route("Test/Details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogWarning("Details called with null or empty ID");
                    return RedirectToAction("Index");
                }

                _logger.LogInformation($"Looking for test with ID: {id}");

                // Get all tests first for debugging and matching
                var allTests = await _testRepository.GetAllTestsAsync();
                _logger.LogInformation($"Found {allTests.Count} total tests");
                _logger.LogInformation($"Available test IDs: {string.Join(", ", allTests.Select(t => t.TestIdentifier))}");

                // Try to find by direct string match on TestIdentifier first (most likely case)
                TestModel test = allTests.FirstOrDefault(t => 
                    t.TestIdentifier == id || 
                    t.Id == id || 
                    t.JsonId == id);
                
                if (test == null)
                {
                    // If that fails, try the repository method
                    test = await _testRepository.GetByStringIdAsync(id);
                }

                if (test == null && int.TryParse(id, out int numericId))
                {
                    // Try to get by numeric ID
                    test = await _testRepository.GetByTestIdAsync(numericId);
                }
                
                if (test == null)
                {
                    _logger.LogWarning($"Test not found with ID: {id}");
                    return NotFound();
                }

                _logger.LogInformation($"Found test: {test.Title} with ID: {test.TestIdentifier}");
                return View(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Details action with ID: {id}");
                return RedirectToAction("Index");
            }
        }

        // GET: /Test/Category/{category}
        public async Task<IActionResult> Category(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Danh mục không hợp lệ.";
                return RedirectToAction("Index");
            }

            try
            {
                var tests = await _testRepository.GetTestsByCategoryAsync(id);
                ViewBag.Category = id;
                return View("Index", tests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving tests for category: {id}");
                TempData["Error"] = "Không thể tải bài kiểm tra cho danh mục này.";
                return RedirectToAction("Index");
            }
        }

        // GET: /Test/Level/{level}
        public async Task<IActionResult> Level(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Trình độ không hợp lệ.";
                return RedirectToAction("Index");
            }

            try
            {
                var tests = await _testRepository.GetTestsByLevelAsync(id);
                ViewBag.Level = id;
                return View("Index", tests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving tests for level: {id}");
                TempData["Error"] = "Không thể tải bài kiểm tra cho trình độ này.";
                return RedirectToAction("Index");
            }
        }

        // GET: /Test/Take/{id}
        [Route("Test/Take/{id}")]
        public async Task<IActionResult> Take(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("Index");
                }

                _logger.LogInformation($"Take action - Looking for test with ID: {id}");
                
                // Get all tests first for matching
                var allTests = await _testRepository.GetAllTestsAsync();
                _logger.LogInformation($"Take action - Found {allTests.Count} total tests");
                
                // Try to find by direct string match on TestIdentifier first
                TestModel test = allTests.FirstOrDefault(t => 
                    t.TestIdentifier == id || 
                    t.Id == id || 
                    t.JsonId == id);
                
                if (test == null)
                {
                    // If that fails, try the repository method
                    test = await _testRepository.GetByStringIdAsync(id);
                }

                if (test == null && int.TryParse(id, out int numericId))
                {
                    // Try to find by numeric ID
                    test = await _testRepository.GetByTestIdAsync(numericId);
                }
                
                if (test == null)
                {
                    _logger.LogWarning($"Test not found with ID: {id}");
                    return NotFound();
                }

                    var allQuestions = test.Questions.ToList();
                    var random = new Random();
                    var randomQuestions = allQuestions
                        .OrderBy(q => random.Next())
                        .Take(20)
                        .ToList();
                        
                    test.Questions = randomQuestions;
                    
                    // Store the selected question IDs in TempData for later use
                    TempData["SelectedQuestionIds"] = JsonSerializer.Serialize(
                        randomQuestions.Select(q => q.QuestionId).ToList()
                    );

                _logger.LogInformation($"Found test for taking: {test.Title} with ID: {test.TestIdentifier}");
                return View(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving test for taking: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        // GET: /Test/Result
        [HttpGet]
        public async Task<IActionResult> Result(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }

            try
            {
                var test = await _testRepository.GetByStringIdAsync(id);
                if (test == null)
                {
                    return NotFound();
                }

                // For GET requests, we'll just render the view with test data
                // The actual results will be populated by JavaScript
                return View(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving test for result: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        // POST: /Test/SubmitTest
        [HttpPost]
        public async Task<IActionResult> SubmitTest([FromBody] TestSubmissionModel submission)
        {
            if (submission == null)
            {
                return BadRequest();
            }

            try
            {
                var test = await _testRepository.GetByStringIdAsync(submission.TestId);
                if (test == null)
                {
                    return NotFound();
                }

                // Get Vietnam time for submission
                DateTime vietnamTime = GetVietnamTime();

                // Store the submission data in TempData so it's available across requests
                TempData["UserAnswers"] = JsonSerializer.Serialize(submission.UserAnswers);
                TempData["Score"] = submission.Score;
                TempData["CorrectCount"] = submission.CorrectCount;
                TempData["TimeTaken"] = submission.TimeTaken;
                TempData["TimeUsed"] = submission.TimeUsed;
                TempData["CompletedAt"] = vietnamTime; // Store completion time

                // If we have selected question IDs, keep them
                if (TempData.ContainsKey("SelectedQuestionIds"))
                {
                    TempData.Keep("SelectedQuestionIds");
                }

                // Get current user ID if authenticated
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Save completed test to database if user is logged in
                if (!string.IsNullOrEmpty(userId))
                {
                    await SaveCompletedTestToDatabase(userId, test, submission, vietnamTime);
                }
                
                // Always save to session as a fallback
                SaveCompletedTestToSession(test, submission, vietnamTime);

                // Return success response
                return Ok(new { redirectUrl = Url.Action("Result", "Test", new { id = submission.TestId }) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing test submission: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your submission.");
            }
        }

        // GET: /Test/Detail
        [HttpGet]
        public async Task<IActionResult> Detail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }

            try
            {
                var test = await _testRepository.GetByStringIdAsync(id);
                if (test == null)
                {
                    return NotFound();
                }

                // Check if we have test data in TempData
                if (TempData.ContainsKey("UserAnswers"))
                {
                    ViewBag.UserAnswers = JsonSerializer.Deserialize<int[]>(TempData["UserAnswers"].ToString());
                    ViewBag.Score = TempData["Score"];
                    ViewBag.CorrectCount = TempData["CorrectCount"];
                    ViewBag.TimeTaken = TempData["TimeTaken"];
                    
                    // Add completion time to ViewBag
                    if (TempData.ContainsKey("CompletedAt"))
                    {
                        ViewBag.CompletedAt = Convert.ToDateTime(TempData["CompletedAt"]);
                    }
                    
                    // Keep the data available for the next request too
                    TempData.Keep("UserAnswers");
                    TempData.Keep("Score");
                    TempData.Keep("CorrectCount");
                    TempData.Keep("TimeTaken");
                    TempData.Keep("CompletedAt");
                    
                    if (TempData.ContainsKey("SelectedQuestionIds"))
                    {
                        var selectedQuestionIds = JsonSerializer.Deserialize<List<int>>(
                            TempData["SelectedQuestionIds"].ToString());
                        
                        // Filter the questions to only include those that were shown during the test
                        test.Questions = test.Questions
                            .Where(q => selectedQuestionIds.Contains(q.QuestionId))
                            .ToList();
                        
                        TempData.Keep("SelectedQuestionIds");
                    }
                }
                else
                {
                    // If we don't have data, redirect to test taking page
                    return RedirectToAction("Take", new { id = id });
                }

                return View(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving test details: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        // GET: /Test/Completed
        public async Task<IActionResult> Completed()
        {
            try
            {
                List<TestModel> testDetailsList = new List<TestModel>();
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (!string.IsNullOrEmpty(userId))
                {
                    // User is logged in, get tests from database
                    var completedUserTests = await _userTestRepository.GetCompletedTestsByUserIdAsync(userId);
                    
                    if (completedUserTests != null && completedUserTests.Any())
                    {
                        foreach (var userTest in completedUserTests)
                        {
                            var test = await _testRepository.GetByStringIdAsync(userTest.TestId);
                            if (test != null)
                            {
                                // Convert UserTestModel to CompletedTestModel for consistent display
                                var completionInfo = new CompletedTestModel
                                {
                                    TestId = userTest.TestId,
                                    TestTitle = userTest.TestTitle,
                                    TestCategory = userTest.TestCategory,
                                    TestLevel = userTest.TestLevel,
                                    ImageUrl = userTest.ImageUrl,
                                    Score = userTest.Score,
                                    CorrectCount = userTest.CorrectCount,
                                    TotalQuestions = userTest.TotalQuestions,
                                    TimeTaken = userTest.TimeTaken,
                                    CompletedAt = userTest.CompletedAt
                                };
                                
                                test.CompletionInfo = completionInfo;
                                testDetailsList.Add(test);
                            }
                        }
                    }
                }
                else
                {
                    // User not logged in, fall back to session
                    var sessionCompletedTests = GetCompletedTestsFromSession();
                    
                    if (sessionCompletedTests != null && sessionCompletedTests.Any())
                    {
                        foreach (var completedTest in sessionCompletedTests)
                        {
                            var test = await _testRepository.GetByStringIdAsync(completedTest.TestId);
                            if (test != null)
                            {
                                test.CompletionInfo = completedTest;
                                testDetailsList.Add(test);
                            }
                        }
                    }
                }
                
                if (!testDetailsList.Any())
                {
                    ViewBag.CompletedMessage = "Bạn chưa hoàn thành bài kiểm tra nào. Hãy thử làm một bài kiểm tra và quay lại đây sau.";
                    return View("Index", new List<TestModel>());
                }
                
                ViewBag.IsCompletedPage = true;
                return View("Index", testDetailsList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving completed tests");
                TempData["Error"] = "Không thể tải danh sách bài kiểm tra đã hoàn thành. Vui lòng thử lại.";
                return View("Index", new List<TestModel>());
            }
        }

        // GET: /Test/Progress
        public async Task<IActionResult> Progress()
        {
            try
            {
                // Get all available tests for statistics
                var allTests = await _testRepository.GetAllTestsAsync();
                int totalAvailableTests = allTests.Count;
                
                // Initialize statistics model
                var progressModel = new TestProgressModel
                {
                    TotalAvailableTests = totalAvailableTests,
                    CompletedTests = new List<CompletedTestModel>(),
                    CompletedCount = 0,
                    AverageScore = 0,
                    CategoryProgress = new Dictionary<string, CategoryProgressModel>(),
                    LevelProgress = new Dictionary<string, LevelProgressModel>()
                };
                
                // Get completed tests
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                List<CompletedTestModel> completedTests = new List<CompletedTestModel>();
                
                if (!string.IsNullOrEmpty(userId))
                {
                    // User is logged in, get tests from database
                    var userTests = await _userTestRepository.GetCompletedTestsByUserIdAsync(userId);
                    
                    if (userTests != null && userTests.Any())
                    {
                        completedTests = userTests.Select(ut => new CompletedTestModel
                        {
                            TestId = ut.TestId,
                            TestTitle = ut.TestTitle,
                            TestCategory = ut.TestCategory,
                            TestLevel = ut.TestLevel,
                            ImageUrl = ut.ImageUrl,
                            Score = ut.Score,
                            CorrectCount = ut.CorrectCount,
                            TotalQuestions = ut.TotalQuestions,
                            TimeTaken = ut.TimeTaken,
                            CompletedAt = ut.CompletedAt
                        }).ToList();
                    }
                }
                else
                {
                    // User not logged in, fall back to session
                    completedTests = GetCompletedTestsFromSession() ?? new List<CompletedTestModel>();
                }
                
                // Update progress model with completed tests
                progressModel.CompletedTests = completedTests;
                progressModel.CompletedCount = completedTests.Count;
                progressModel.CompletionPercentage = totalAvailableTests > 0 
                    ? (double)completedTests.Count / totalAvailableTests * 100 
                    : 0;
                    
                // Calculate average score
                if (completedTests.Any())
                {
                    progressModel.AverageScore = completedTests.Average(t => t.Score);
                }
                
                // Group by category
                var categoryCounts = allTests.GroupBy(t => t.Category)
                    .ToDictionary(g => g.Key, g => g.Count());
                
                foreach (var category in categoryCounts.Keys)
                {
                    var categoryCompletedTests = completedTests
                        .Where(t => t.TestCategory == category)
                        .ToList();
                    
                    progressModel.CategoryProgress[category] = new CategoryProgressModel
                    {
                        TotalTests = categoryCounts[category],
                        CompletedTests = categoryCompletedTests.Count,
                        CompletionPercentage = categoryCounts[category] > 0 
                            ? (double)categoryCompletedTests.Count / categoryCounts[category] * 100 
                            : 0,
                        AverageScore = categoryCompletedTests.Any() 
                            ? categoryCompletedTests.Average(t => t.Score) 
                            : 0
                    };
                }
                
                // Group by level
                var levelCounts = allTests.GroupBy(t => t.Level)
                    .ToDictionary(g => g.Key, g => g.Count());
                
                foreach (var level in levelCounts.Keys)
                {
                    var levelCompletedTests = completedTests
                        .Where(t => t.TestLevel == level)
                        .ToList();
                    
                    progressModel.LevelProgress[level] = new LevelProgressModel
                    {
                        TotalTests = levelCounts[level],
                        CompletedTests = levelCompletedTests.Count,
                        CompletionPercentage = levelCounts[level] > 0 
                            ? (double)levelCompletedTests.Count / levelCounts[level] * 100 
                            : 0,
                        AverageScore = levelCompletedTests.Any() 
                            ? levelCompletedTests.Average(t => t.Score) 
                            : 0
                    };
                }
                
                // Get recent test completions
                progressModel.RecentCompletions = completedTests
                    .OrderByDescending(t => t.CompletedAt)
                    .Take(5)
                    .ToList();
                
                return View(progressModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving progress data");
                TempData["Error"] = "Không thể tải dữ liệu tiến trình học tập. Vui lòng thử lại.";
                return RedirectToAction("Index");
            }
        }

        // Helper method to save completed test to database
        private async Task SaveCompletedTestToDatabase(string userId, TestModel test, TestSubmissionModel submission, DateTime completionTime)
        {
            // Check if this test was already completed by the user
            var existingTest = await _userTestRepository.GetByUserAndTestIdAsync(userId, test.Id);
            
            UserTestModel userTest;
            if (existingTest != null)
            {
                // Update existing record
                userTest = existingTest;
                userTest.Score = submission.Score;
                userTest.CorrectCount = submission.CorrectCount;
                userTest.TimeTaken = submission.TimeTaken;
                userTest.CompletedAt = completionTime;
            }
            else
            {
                // Create new record
                userTest = new UserTestModel
                {
                    UserId = userId,
                    TestId = test.Id,
                    TestTitle = test.Title,
                    TestCategory = test.Category,
                    TestLevel = test.Level,
                    ImageUrl = test.ImageUrl,
                    Score = submission.Score,
                    CorrectCount = submission.CorrectCount,
                    TotalQuestions = test.Questions.Count,
                    TimeTaken = submission.TimeTaken,
                    CompletedAt = completionTime
                };
            }
            
            await _userTestRepository.SaveUserTestAsync(userTest);
        }

        // Helper method to save completed test to session
        private void SaveCompletedTestToSession(TestModel test, TestSubmissionModel submission, DateTime completionTime)
        {
            var completedTest = new CompletedTestModel
            {
                TestId = test.Id,
                TestTitle = test.Title,
                TestCategory = test.Category,
                TestLevel = test.Level,
                ImageUrl = test.ImageUrl,
                Score = submission.Score,
                CorrectCount = submission.CorrectCount,
                TotalQuestions = test.Questions.Count,
                TimeTaken = submission.TimeTaken,
                CompletedAt = completionTime
            };
            
            // Get existing completed tests
            var completedTests = GetCompletedTestsFromSession() ?? new List<CompletedTestModel>();
            
            // Remove any existing entry for this test
            completedTests.RemoveAll(t => t.TestId == test.Id);
            
            // Add the new completion
            completedTests.Add(completedTest);
            
            // Save back to session
            HttpContext.Session.SetString("CompletedTests", JsonSerializer.Serialize(completedTests));
        }

        // Helper method to get completed tests from session
        private List<CompletedTestModel> GetCompletedTestsFromSession()
        {
            var completedTestsJson = HttpContext.Session.GetString("CompletedTests");
            if (string.IsNullOrEmpty(completedTestsJson))
            {
                return new List<CompletedTestModel>();
            }
            
            return JsonSerializer.Deserialize<List<CompletedTestModel>>(completedTestsJson);
        }

        // Helper method to get Vietnam time
        private DateTime GetVietnamTime()
        {
            try
            {
                TimeZoneInfo vietnamZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                return TimeZoneInfo.ConvertTime(DateTime.Now, vietnamZone);
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback if the timezone isn't found (e.g., on Linux/macOS)
                return DateTime.Now.AddHours(7); // GMT+7
            }
            catch (Exception)
            {
                // If any other error occurs, return the server time
                return DateTime.Now;
            }
        }
    }
}