using Microsoft.AspNetCore.Mvc;
using TiengAnh.Models;

namespace TiengAnh.Controllers
{
    public class TestController : Controller
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var tests = GetSampleTests();
            return View(tests);
        }

        public IActionResult Details(int id)
        {
            var test = GetSampleTests().FirstOrDefault(t => t.TestID == id);
            
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        public IActionResult Take(int id)
        {
            var test = GetSampleTests().FirstOrDefault(t => t.TestID == id);
            
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
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
    }
}
