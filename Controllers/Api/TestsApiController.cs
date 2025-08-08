using Microsoft.AspNetCore.Mvc;
using TiengAnh.Models;
using TiengAnh.Repositories;

namespace TiengAnh.Controllers.Api
{
    [ApiController]
    [Route("api/tests")]
    [Produces("application/json")]
    public class TestsApiController : ControllerBase
    {
        private readonly TestRepository _testRepo;

        public TestsApiController(TestRepository testRepo)
        {
            _testRepo = testRepo;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TestModel>>> GetAll([FromQuery] string? category = null, [FromQuery] string? level = null)
        {
            if (!string.IsNullOrWhiteSpace(category))
            {
                var byCat = await _testRepo.GetTestsByCategoryAsync(category);
                return Ok(byCat);
            }
            if (!string.IsNullOrWhiteSpace(level))
            {
                var byLevel = await _testRepo.GetTestsByLevelAsync(level);
                return Ok(byLevel);
            }
            var all = await _testRepo.GetAllTestsAsync();
            return Ok(all);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<TestModel>> GetById(string id)
        {
            var test = await _testRepo.GetByStringIdAsync(id);
            if (test == null && int.TryParse(id, out var numericId))
            {
                test = await _testRepo.GetByTestIdAsync(numericId);
            }
            return test is null ? NotFound() : Ok(test);
        }

        // Tạo mới test (Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TestModel>> Create([FromBody] TestModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var ok = await _testRepo.SaveTestAsync(model);
            if (!ok) return StatusCode(500, "Không thể tạo bài test");

            var routeId = !string.IsNullOrEmpty(model.TestIdentifier) ? model.TestIdentifier : model.Id;
            return CreatedAtAction(nameof(GetById), new { id = routeId }, model);
        }

        // Cập nhật test theo id (admin). id có thể là TestIdentifier/JsonId/Id
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(string id, [FromBody] TestModel model)
        {
            var existing = await _testRepo.GetByStringIdAsync(id);
            if (existing == null) return NotFound();

            // Giữ khóa Mongo
            model.Id = existing.Id;

            var ok = await _testRepo.SaveTestAsync(model);
            return ok ? NoContent() : StatusCode(500, "Không thể cập nhật");
        }

        // Xóa test theo id (admin). id có thể là TestIdentifier/JsonId/Id
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _testRepo.GetByStringIdAsync(id);
            if (existing == null) return NotFound();

            var ok = await _testRepo.DeleteAsync(existing.Id);
            return ok ? NoContent() : StatusCode(500, "Không thể xóa");
        }
    }
}
