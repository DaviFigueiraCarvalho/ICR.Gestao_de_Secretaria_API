using ICR.Domain.Model.FederationAggregate;
using ICR.Infra;
using ICRManagement.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICR.API.Controllers
{
    [ApiController]
    [Route("api/federations")]
    public class FederationController : ControllerBase
    {
        private readonly IFederationRepository _repository;

        public FederationController(IFederationRepository repository)
        {
            _repository = repository;
        }

        // POST: api/federations
        [HttpPost]
        public async Task<ActionResult<IEnumerable<Federation>>> Create([FromBody] FederationDTO dto)
        {
            var result = await _repository.AddAsync(dto);
            return Ok(result);
        }

        // GET: api/federations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FederationResponseDTO>>> GetAll()
        {
            var federations = await _repository.GetAllFederationsAsync();
            return Ok(federations);
        }

        // GET: api/federations/{id}
        [HttpGet("{id:long}")]
        public async Task<ActionResult<FederationResponseDTO>> GetById(long id)
        {
            var federation = await _repository.GetByIdAsync(id);

            if (federation == null)
                return NotFound();

            return Ok(federation);
        }

        // PATCH: api/federations/{id}
        [HttpPatch("{id:long}")]
        public async Task<IActionResult> Patch(long id, [FromBody] FederationPatchDTO dto)
        {
            var updated = await _repository.UpdateAsync(id, dto);

            if (updated==null)
                return NotFound();

            return NoContent();
        }




        // DELETE: api/federations/{id}
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
