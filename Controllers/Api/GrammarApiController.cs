using Microsoft.AspNetCore.Mvc;
using TiengAnh.Models;
using TiengAnh.Repositories;

namespace TiengAnh.Controllers.Api
{
    [ApiController]
    [Route("api/grammars")]
    [Produces("application/json")]
    public class GrammarApiController : ControllerBase
    {
        private readonly GrammarRepository _grammarRepo;

        public GrammarApiController(GrammarRepository grammarRepo)
        {
            _grammarRepo = grammarRepo;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<GrammarModel>>> GetAll([FromQuery] string? level = null)
        {
            if (!string.IsNullOrWhiteSpace(level))
            {
                var list = await _grammarRepo.GetGrammarsByLevelAsync(level);
                return Ok(list);
            }
            var all = await _grammarRepo.GetAllAsync();
            return Ok(all);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<GrammarModel>> GetById(int id)
        {
            var item = await _grammarRepo.GetByGrammarIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        // Tạo mới grammar (Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GrammarModel>> Create([FromBody] GrammarModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            model.ID_NP = await _grammarRepo.GetNextIdAsync();
            model.FavoriteByUsers ??= new List<string>();

            await _grammarRepo.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = model.ID_NP }, model);
        }

        // Cập nhật grammar theo ID_NP (Admin)
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] GrammarModel model)
        {
            var existing = await _grammarRepo.GetByGrammarIdAsync(id);
            if (existing == null) return NotFound();

            model.Id = existing.Id;
            model.ID_NP = existing.ID_NP;

            var ok = await _grammarRepo.UpdateAsync(existing.Id, model);
            return ok ? NoContent() : StatusCode(500, "Không thể cập nhật");
        }

        // Xóa grammar theo ID_NP (Admin)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _grammarRepo.GetByGrammarIdAsync(id);
            if (existing == null) return NotFound();

            var ok = await _grammarRepo.DeleteAsync(existing.Id);
            return ok ? NoContent() : StatusCode(500, "Không thể xóa");
        }
    }
}
