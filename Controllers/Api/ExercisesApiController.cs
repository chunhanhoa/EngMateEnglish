using Microsoft.AspNetCore.Mvc;
using TiengAnh.Models;
using TiengAnh.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace TiengAnh.Controllers.Api
{
    [ApiController]
    [Route("api/exercises")]
    [Produces("application/json")]
    public class ExercisesApiController : ControllerBase
    {
        private readonly ExerciseRepository _exerciseRepo;

        public ExercisesApiController(ExerciseRepository exerciseRepo)
        {
            _exerciseRepo = exerciseRepo;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetAll([FromQuery] int? topicId = null)
        {
            if (topicId.HasValue)
            {
                var byTopic = await _exerciseRepo.GetExercisesByTopicIdAsync(topicId.Value);
                return Ok(byTopic);
            }
            // Không có GetAll trong repo, trả về gộp các topic phổ biến (hoặc để trống)
            return Ok(new { message = "Vui lòng truyền topicId để lấy bài tập theo chủ đề" });
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetById(int id)
        {
            var item = await _exerciseRepo.GetByExerciseIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        // Tạo mới exercise (Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ExerciseModel>> Create([FromBody] ExerciseModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (model.ID_BT <= 0)
            {
                model.ID_BT = await _exerciseRepo.GetNextIdAsync();
            }

            await _exerciseRepo.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = model.ID_BT }, model);
        }

        // Cập nhật exercise theo ID_BT (Admin)
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] ExerciseModel model)
        {
            var existing = await _exerciseRepo.GetByExerciseIdAsync(id);
            if (existing == null) return NotFound();

            model.Id = existing.Id;
            model.ID_BT = existing.ID_BT;

            var ok = await _exerciseRepo.UpdateAsync(existing.Id, model);
            return ok ? NoContent() : StatusCode(500, "Không thể cập nhật");
        }

        // Xóa exercise theo ID_BT (Admin)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _exerciseRepo.DeleteByExerciseIdAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
