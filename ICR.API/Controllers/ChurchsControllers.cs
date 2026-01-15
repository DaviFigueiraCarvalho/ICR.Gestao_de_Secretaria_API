using ICR.Domain.DTOs;
using ICR.Domain.Model.ChurchAggregate;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICR.API.Controllers
{
    [ApiController]
    [Route("api/churches")]
    public class ChurchController : ControllerBase
    {
        private readonly IChurchRepository _repository;

        public ChurchController(IChurchRepository repository)
        {
            _repository = repository;
        }

        // GET: api/churches?pageNumber=1&pageQuantity=10
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChurchResponseDto>>> GetAll()
        {
            var churches = await _repository.GetAllChurchsAsync();
            return Ok(churches);
        }

        // GET: api/churches/{id}
        [HttpGet("{id:long}")]
        public async Task<ActionResult<ChurchResponseDto>> GetById(long id)
        {
            var church = await _repository.GetByIdAsync(id);
            if (church == null)
                return NotFound();

            return Ok(church);
        }

        // GET: api/churches/federation/{federationId}
        [HttpGet("federation/{federationId:long}")]
        public async Task<ActionResult<IEnumerable<ChurchResponseDto>>> GetByFederation(long federationId)
        {
            var churches = await _repository.GetChurchsbyFederationId(federationId);
            return Ok(churches);
        }

        // POST: api/churches
        [HttpPost]
        public async Task<ActionResult<ChurchResponseDto>> Create([FromBody] ChurchDTO dto)
        {
            var church = await _repository.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = church.Id }, church);
        }

        // PATCH: api/churches/{id}
        [HttpPatch("{id:long}")]
        public async Task<ActionResult<ChurchResponseDto>> Update(long id, [FromBody] ChurchPatchDTO dto)
        {
            var updated = await _repository.UpdateAsync(id, dto);

            if (updated == null)
                return NotFound();

            return Ok(updated);
        }



        // DELETE: api/churches/{id}
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var deleted = await _repository.DeleteAsync(id);

            if (deleted == null)
                return NotFound();

            return NoContent();
        }

    }
}
