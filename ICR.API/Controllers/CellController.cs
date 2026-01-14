using ICR.Domain.DTOs;
using ICR.Domain.Model.CellAggregate;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ICR.API.Controllers
{
    [ApiController]
    [Route("api/cells")]
    public class CellController : ControllerBase
    {
        private readonly ICellRepository _cellRepository;

        public CellController(ICellRepository cellRepository)
        {
            _cellRepository = cellRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageQuantity = 10)
        {
            var cells = await _cellRepository.GetAllAsync(pageNumber, pageQuantity);
            return Ok(cells);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var cell = await _cellRepository.GetByIdAsync(id);

            if (cell == null)
                return NotFound(new { message = "Cell not found" });

            return Ok(cell);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CellDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _cellRepository.AddAsync(dto);

            if (result.Id == 0)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPatch("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] CellDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Mapeia DTO → entidade (porque tua interface é torta)
            var cell = new Cell(
                id,
                dto.Name,
                dto.ChurchId,
                dto.ResponsibleId ?? 0
            );

            var result = await _cellRepository.UpdateAsync(id, cell);

            if (result.Id == 0)
                return NotFound(result);

            return Ok(result);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _cellRepository.DeleteAsync(id);

            if (result.Id == 0)
                return NotFound(result);

            return Ok(result);
        }
    }
}
