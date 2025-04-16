using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiengAnh.Data;
using TiengAnh.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TiengAnh.Controllers
{
    public class TestController : Controller
    {
        private readonly ILogger<TestController> _logger;
        private readonly HocTiengAnhContext _context;

        public TestController(ILogger<TestController> logger, HocTiengAnhContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Đang tải danh sách bài kiểm tra từ database");
                
                // Lấy tất cả bài kiểm tra từ database, bao gồm thông tin chủ đề và câu hỏi
                var kiemTras = await _context.KiemTras
                    .Include(k => k.IdCdNavigation)
                    .Include(k => k.CauHoiKts)
                    .ToListAsync();
                
                _logger.LogInformation($"Đã tìm thấy {kiemTras.Count} bài kiểm tra");

                // Chuyển đổi từ KiemTra sang TestModel
                var tests = kiemTras.Select(kt => new TestModel
                {
                    TestID = kt.IdKt,
                    TestName = kt.TitleKt ?? "Bài kiểm tra không tên",
                    Title = kt.TitleKt ?? "Bài kiểm tra không tên",
                    TopicID = kt.IdCd,
                    TopicName = kt.IdCdNavigation?.NameCd ?? "Chưa phân loại",
                    TotalQuestions = kt.CauHoiKts.Count,
                    // Xác định Level dựa trên ID chủ đề để có dữ liệu đa dạng
                    Level = kt.IdCd % 3 == 0 ? "B1" : (kt.IdCd % 2 == 0 ? "A2" : "A1"),
                    Duration = 15,
                    TimeLimit = 15,
                    Description = $"Bài kiểm tra: {kt.TitleKt}",
                    // Tạo đường dẫn ảnh mặc định nếu không có
                    ImageUrl = "/images/test/default.jpg",
                    // Xác định Category dựa trên tên chủ đề
                    Category = GetCategory(kt.IdCdNavigation?.NameCd)
                }).ToList();

                // Nếu không tìm thấy bài kiểm tra nào, hiển thị bài kiểm tra mẫu
                if (!tests.Any())
                {
                    _logger.LogWarning("Không tìm thấy bài kiểm tra nào trong database, sử dụng dữ liệu mẫu");
                    return View(GetSampleTests());
                }

                return View(tests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách bài kiểm tra: {Message}", ex.Message);
                // Trong trường hợp lỗi, trả về dữ liệu mẫu để không hiển thị lỗi cho người dùng
                return View(GetSampleTests());
            }
        }

        public IActionResult Details(int id)
        {
            try
            {
                // Tìm bài kiểm tra từ database
                var test = _context.KiemTras
                    .Include(k => k.IdCdNavigation)
                    .Include(k => k.CauHoiKts)
                    .FirstOrDefault(k => k.IdKt == id);

                if (test == null)
                {
                    // Nếu không tìm thấy trong database, thử tìm trong dữ liệu mẫu
                    var sampleTest = GetSampleTests().FirstOrDefault(t => t.TestID == id);
                    if (sampleTest == null)
                    {
                        return NotFound();
                    }
                    return View(sampleTest);
                }

                // Chuyển đổi từ KiemTra sang TestModel
                var testModel = new TestModel
                {
                    TestID = test.IdKt,
                    TestName = test.TitleKt ?? "Bài kiểm tra không tên",
                    Title = test.TitleKt ?? "Bài kiểm tra không tên",
                    TopicID = test.IdCd,
                    TopicName = test.IdCdNavigation?.NameCd ?? "Chưa phân loại",
                    TotalQuestions = test.CauHoiKts.Count,
                    Level = test.IdCd % 3 == 0 ? "B1" : (test.IdCd % 2 == 0 ? "A2" : "A1"),
                    Duration = 15,
                    TimeLimit = 15,
                    Description = $"Bài kiểm tra: {test.TitleKt}",
                    ImageUrl = "/images/test/default.jpg",
                    Category = GetCategory(test.IdCdNavigation?.NameCd)
                };

                return View(testModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải thông tin chi tiết bài kiểm tra: {Message}", ex.Message);
                return RedirectToAction("Index");
            }
        }

        public IActionResult Take(int id)
        {
            try
            {
                // Tìm bài kiểm tra từ database
                var test = _context.KiemTras
                    .Include(k => k.IdCdNavigation)
                    .Include(k => k.CauHoiKts)
                    .FirstOrDefault(k => k.IdKt == id);

                if (test == null)
                {
                    // Nếu không tìm thấy trong database, thử tìm trong dữ liệu mẫu
                    var sampleTest = GetSampleTests().FirstOrDefault(t => t.TestID == id);
                    if (sampleTest == null)
                    {
                        return NotFound();
                    }
                    
                    _logger.LogInformation($"Đang hiển thị bài kiểm tra mẫu ID = {id}");
                    return View(sampleTest);
                }

                // Chuyển đổi từ KiemTra sang TestModel
                var testModel = new TestModel
                {
                    TestID = test.IdKt,
                    TestName = test.TitleKt ?? "Bài kiểm tra không tên",
                    Title = test.TitleKt ?? "Bài kiểm tra không tên",
                    TopicID = test.IdCd,
                    TopicName = test.IdCdNavigation?.NameCd ?? "Chưa phân loại",
                    TotalQuestions = test.CauHoiKts.Count,
                    Level = test.IdCd % 3 == 0 ? "B1" : (test.IdCd % 2 == 0 ? "A2" : "A1"),
                    Duration = 15,
                    TimeLimit = 15,
                    Description = $"Bài kiểm tra: {test.TitleKt}",
                    ImageUrl = "/images/test/default.jpg",
                    Category = GetCategory(test.IdCdNavigation?.NameCd)
                };

                _logger.LogInformation($"Đang hiển thị bài kiểm tra ID = {id} từ database");
                return View(testModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tải bài kiểm tra ID = {id}");
                TempData["Error"] = "Đã xảy ra lỗi khi tải bài kiểm tra. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }

        public IActionResult Result(int id)
        {
            var test = GetSampleTests().FirstOrDefault(t => t.TestID == id);
            
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        public IActionResult Review(int id)
        {
            var test = GetSampleTests().FirstOrDefault(t => t.TestID == id);
            
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }
        
        // API để kiểm tra kết nối database và đếm số lượng bài kiểm tra
        [HttpGet]
        public IActionResult CheckTests()
        {
            try
            {
                int testCount = _context.KiemTras.Count();
                var testList = _context.KiemTras.Select(k => new { k.IdKt, k.TitleKt }).ToList();
                return Json(new { success = true, count = testCount, tests = testList });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // Phương thức để thêm dữ liệu mẫu vào database
        [HttpGet]
        public async Task<IActionResult> SeedTestData()
        {
            try
            {
                // Kiểm tra xem đã có dữ liệu chưa
                if (_context.KiemTras.Any())
                {
                    TempData["Message"] = "Đã có dữ liệu bài kiểm tra trong database!";
                    return RedirectToAction("Index");
                }

                // Thêm dữ liệu vào bảng KiemTra
                var sampleTests = new List<KiemTra>
                {
                    new KiemTra { 
                        IdCd = 1, // Chủ đề Animals
                        TitleKt = "Go to the Zoo! - Animal Vocabulary Test"
                    },
                    new KiemTra { 
                        IdCd = 2, // Chủ đề Home
                        TitleKt = "A Day at Home - Home Vocabulary Test"
                    },
                    new KiemTra { 
                        IdCd = 3, // Chủ đề School
                        TitleKt = "First Day at School - Learn School Words"
                    },
                    new KiemTra { 
                        IdCd = 4, // Chủ đề Colors
                        TitleKt = "Paint the World with Colors - Color Vocabulary"
                    },
                    new KiemTra { 
                        IdCd = 5, // Chủ đề Numbers
                        TitleKt = "Shopping Time - Count with Me! - Numbers Test"
                    },
                    new KiemTra { 
                        IdCd = 6, // Chủ đề Fruits
                        TitleKt = "Visit the Fruit Market - Fruit Vocabulary"
                    }
                };

                await _context.KiemTras.AddRangeAsync(sampleTests);
                await _context.SaveChangesAsync();

                // Thêm câu hỏi mẫu cho mỗi bài kiểm tra
                foreach (var test in sampleTests)
                {
                    // Thêm 10 câu hỏi mẫu cho mỗi bài kiểm tra
                    for (int i = 1; i <= 10; i++)
                    {
                        var question = new CauHoiKt
                        {
                            IdKt = test.IdKt,
                            QuestionCh = $"Question {i} for test: {test.TitleKt}?",
                            OptionA = $"Option A for question {i}",
                            OptionB = $"Option B for question {i}",
                            OptionC = $"Option C for question {i}",
                            OptionD = $"Option D for question {i}",
                            AnswerCh = "A" // Đáp án mặc định là A
                        };

                        await _context.CauHoiKts.AddAsync(question);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Message"] = "Đã thêm dữ liệu mẫu thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm dữ liệu mẫu: {Message}", ex.Message);
                TempData["Error"] = $"Lỗi khi thêm dữ liệu mẫu: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        private List<TestModel> GetSampleTests()
        {
            return new List<TestModel>
            {
                new TestModel
                {
                    TestID = 1,
                    TestName = "Bài kiểm tra cơ bản (Basic Test)",
                    Description = "Bài kiểm tra trình độ cơ bản bao gồm từ vựng và ngữ pháp đơn giản",
                    Level = "A1",
                    Duration = 15,
                    TotalQuestions = 20,
                    ImageUrl = "/images/test/basic-test.jpg",
                    Category = "General"
                },
                new TestModel
                {
                    TestID = 2,
                    TestName = "Kiểm tra từ vựng động vật",
                    Description = "Bài kiểm tra kiến thức về các loại động vật bằng tiếng Anh",
                    Level = "A1-A2",
                    Duration = 10,
                    TotalQuestions = 15,
                    ImageUrl = "/images/test/animals-test.jpg",
                    Category = "Vocabulary"
                },
                new TestModel
                {
                    TestID = 3,
                    TestName = "Kiểm tra ngữ pháp thì hiện tại",
                    Description = "Bài kiểm tra về thì hiện tại đơn và hiện tại tiếp diễn",
                    Level = "A2",
                    Duration = 20,
                    TotalQuestions = 25,
                    ImageUrl = "/images/test/grammar-test.jpg",
                    Category = "Grammar"
                },
                new TestModel
                {
                    TestID = 4,
                    TestName = "Kiểm tra trung cấp (Intermediate Test)",
                    Description = "Bài kiểm tra trình độ trung cấp với từ vựng và ngữ pháp nâng cao hơn",
                    Level = "B1",
                    Duration = 30,
                    TotalQuestions = 30,
                    ImageUrl = "/images/test/intermediate-test.jpg",
                    Category = "General"
                }
            };
        }

        private string GetCategory(string? topicName)
        {
            if (string.IsNullOrEmpty(topicName))
                return "General";
                
            switch (topicName.ToLower())
            {
                case "animals": return "Vocabulary";
                case "home": return "Vocabulary";
                case "school": return "Vocabulary";
                case "colors": return "Vocabulary";
                case "numbers": return "Vocabulary";
                case "fruits": return "Vocabulary";
                case "verbs": return "Grammar";
                case "tenses": return "Grammar";
                default: return "General";
            }
        }
    }
}
