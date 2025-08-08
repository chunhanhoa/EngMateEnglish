using Microsoft.AspNetCore.Mvc;
using TiengAnh.Models;
using TiengAnh.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace TiengAnh.Controllers.Api
{
    [ApiController]
    [Route("api/topics")]
    [Produces("application/json")]
    public class TopicsApiController : ControllerBase
    {
        private readonly ITopicRepository _topicRepo;
        private readonly VocabularyRepository _vocabRepo;
        private readonly ExerciseRepository _exerciseRepo;

        public TopicsApiController(ITopicRepository topicRepo, VocabularyRepository vocabRepo, ExerciseRepository exerciseRepo)
        {
            _topicRepo = topicRepo;
            _vocabRepo = vocabRepo;
            _exerciseRepo = exerciseRepo;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TopicModel>>> GetAllTopics()
        {
            var topics = await _topicRepo.GetAllTopicsAsync();
            return Ok(topics);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<TopicModel>> GetTopicById(int id)
        {
            var topic = await _topicRepo.GetTopicByIdAsync(id) ?? await _topicRepo.GetByTopicIdAsync(id);
            return topic is null ? NotFound() : Ok(topic);
        }

        [HttpGet("{id:int}/vocabularies")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<VocabularyModel>>> GetTopicVocabularies(int id)
        {
            var vocabs = await _vocabRepo.GetVocabulariesByTopicIdAsync(id);
            return Ok(vocabs);
        }

        [HttpGet("{id:int}/exercises")]
        [AllowAnonymous]
        public async Task<ActionResult> GetTopicExercises(int id)
        {
            var exercises = await _exerciseRepo.GetExercisesByTopicIdAsync(id);
            return Ok(exercises);
        }

        // Tạo mới chủ đề (Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TopicModel>> Create([FromBody] TopicModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Gán ID_CD nếu chưa có
            if (model.ID_CD <= 0)
            {
                var all = await _topicRepo.GetAllAsync();
                model.ID_CD = all.Any() ? all.Max(t => t.ID_CD) + 1 : 1;
            }

            var ok = await _topicRepo.CreateAsync(model);
            if (!ok) return StatusCode(500, "Không thể tạo chủ đề");
            return CreatedAtAction(nameof(GetTopicById), new { id = model.ID_CD }, model);
        }

        // Cập nhật chủ đề theo ID_CD (Admin)
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] TopicModel model)
        {
            var existing = await _topicRepo.GetByTopicIdAsync(id);
            if (existing == null) return NotFound();

            // Giữ khóa Mongo và ID_CD
            model.Id = existing.Id;
            model.ID_CD = existing.ID_CD;

            var ok = await _topicRepo.UpdateAsync(existing.ID_CD.ToString(), model);
            return ok ? NoContent() : StatusCode(500, "Không thể cập nhật");
        }

        // Xóa chủ đề theo ID_CD (Admin)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _topicRepo.GetByTopicIdAsync(id);
            if (existing == null) return NotFound();

            var ok = await _topicRepo.DeleteAsync(existing.ID_CD.ToString());
            return ok ? NoContent() : StatusCode(500, "Không thể xóa");
        }
    }
}
