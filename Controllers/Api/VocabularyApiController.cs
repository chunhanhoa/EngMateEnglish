using Microsoft.AspNetCore.Mvc;
using TiengAnh.Models;
using TiengAnh.Repositories;

namespace TiengAnh.Controllers.Api
{
    [ApiController]
    [Route("api/vocabularies")]
    [Produces("application/json")]
    public class VocabularyApiController : ControllerBase
    {
        private readonly VocabularyRepository _vocabRepo;

        public VocabularyApiController(VocabularyRepository vocabRepo)
        {
            _vocabRepo = vocabRepo;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<VocabularyModel>>> GetAll([FromQuery] int? topicId = null)
        {
            if (topicId.HasValue)
            {
                var list = await _vocabRepo.GetVocabulariesByTopicIdAsync(topicId.Value);
                return Ok(list);
            }
            var all = await _vocabRepo.GetAllAsync();
            return Ok(all);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<VocabularyModel>> GetById(int id)
        {
            var item = await _vocabRepo.GetByVocabularyIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        // Tạo mới từ vựng (Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<VocabularyModel>> Create([FromBody] VocabularyModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            model.ID_TV = await _vocabRepo.GetNextIdAsync();
            model.FavoriteByUsers ??= new List<string>();
            await _vocabRepo.CreateAsync(model);

            return CreatedAtAction(nameof(GetById), new { id = model.ID_TV }, model);
        }

        // Cập nhật từ vựng theo ID_TV (Admin)
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] VocabularyModel model)
        {
            var existing = await _vocabRepo.GetByVocabularyIdAsync(id);
            if (existing == null) return NotFound();

            // Giữ khóa chính Mongo
            model.Id = existing.Id;
            model.ID_TV = existing.ID_TV;

            var ok = await _vocabRepo.UpdateAsync(existing.Id, model);
            return ok ? NoContent() : StatusCode(500, "Không thể cập nhật");
        }

        // Xóa từ vựng theo ID_TV (Admin)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _vocabRepo.DeleteVocabularyAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
