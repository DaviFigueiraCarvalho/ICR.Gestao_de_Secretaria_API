using Microsoft.AspNetCore.Authorization;
using ICR.Domain.DTOs;
using ICR.Domain.Model.CellAggregate;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ICR.API.Controllers
{
    [ApiController]
    [Route("api/cells")]
    [Authorize]
    public class CellController : ControllerBase
    {
        private readonly ICellRepository _repository;

        public CellController(ICellRepository cellRepository)
        {
            _repository = cellRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var cells = await _repository.GetAllAsync();
            return Ok(cells);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetFiltered([FromQuery] long? federationId, [FromQuery] long? churchId)
        {
            var cells = await _repository.GetFilteredAsync(federationId, churchId);
            return Ok(cells);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var cell = await _repository.GetByIdAsync(id);

            if (cell == null)
                return NotFound(new { message = "Cell not found" });

            return Ok(cell);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CellDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _repository.AddAsync(dto);

            if (result.Id == 0)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPatch("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] CellPatchDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _repository.UpdateAsync(id, dto);

            if (result.Id == 0)
                return NotFound(result);

            return Ok(result);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _repository.DeleteAsync(id);

            if (result.Id == 0)
                return NotFound(result);

            return Ok(result);
        }

        [HttpDelete("bulk/{id:long}")]
        public async Task<IActionResult> DeleteBulk(long id, [FromQuery] long? targetCellId)
        {
            var result = await _repository.DeleteWithRelationsAsync(id, targetCellId);

            if (result == null || result.Id == 0)
                return NotFound(new { message = "Cell not found" });

            return Ok(new { message = targetCellId.HasValue ? "Cell deleted and relations moved successfully." : "Cell and relations deleted successfully.", data = result });
        }
    }
}
