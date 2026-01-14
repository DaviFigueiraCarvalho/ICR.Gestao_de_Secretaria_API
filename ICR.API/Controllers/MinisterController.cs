using ICR.Domain.DTOs;
using ICR.Domain.Model.MinisterAggregate;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICR.API.Controllers
{
    [ApiController]
    [Route("api/ministers")]
    public class MinisterController : ControllerBase
    {
        private readonly IMinisterRepository _repository;

        public MinisterController(IMinisterRepository repository)
        {
            _repository = repository;
        }

        // POST: api/ministers
        [HttpPost]
        public async Task<ActionResult<MinisterResponseDTO>> Create([FromBody] Minister minister)
        {
            var result = await _repository.AddAsync(minister);

            if (result.Id == 0)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // GET: api/ministers/{id}
        [HttpGet("{id:long}")]
        public async Task<ActionResult<MinisterResponseDTO>> GetById(long id)
        {
            var minister = await _repository.GetByIdAsync(id);

            if (minister == null)
                return NotFound();

            return Ok(minister);
        }

        // GET: api/ministers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MinisterResponseDTO>>> GetAll()
        {
            var ministers = await _repository.GetAllAsync();
            return Ok(ministers);
        }

        // GET: api/ministers/church/{churchId}
        [HttpGet("church/{churchId:long}")]
        public async Task<ActionResult<IEnumerable<MinisterResponseDTO>>> GetByChurch(long churchId)
        {
            var ministers = await _repository.GetByChurchIdAsync(churchId);
            return Ok(ministers);
        }

        // GET: api/ministers/birthdays?month=5
        [HttpGet("birthdays")]
        public async Task<ActionResult<IEnumerable<MinisterBirthdayDTO>>> GetBirthdays([FromQuery] int month)
        {
            var result = await _repository.GetByBirthdaydatesIdAsync(month);
            return Ok(result);
        }

        // PATCH: api/ministers/{id}
        [HttpPatch("{id:long}")]
        public async Task<ActionResult<MinisterResponseDTO>> Patch(long id, [FromBody] MinisterPatchDTO dto)
        {
            var result = await _repository.UpdateAsync(id, dto);

            if (result.Id == 0)
                return BadRequest(result);

            return Ok(result);
        }

        // DELETE: api/ministers/{id}
        [HttpDelete("{id:long}")]
        public async Task<ActionResult<MinisterResponseDTO>> Delete(long id)
        {
            var result = await _repository.DeleteAsync(id);

            if (result.Id == 0)
                return NotFound(result);

            return Ok(result);
        }
    }
}
