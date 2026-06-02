using ICR.API.Authorization;
using ICR.Domain.DTOs;
using ICR.Domain.Model.ChurchAggregate;
using ICR.Domain.Model.UserRoleAgreggate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserModel = ICR.Domain.Model.UserRoleAgreggate.User;

namespace ICR.API.Controllers
{
    [ApiController]
    [Route("api/churches")]
    [Authorize]
    [AuthorizeScope(UserModel.UserScope.FEDERATION)]
    public class ChurchController : ControllerBase
    {
        private readonly IChurchRepository _repository;

        public ChurchController(IChurchRepository repository)
        {
            _repository = repository;
        }

        // GET: api/churches?pageNumber=1&pageQuantity=10
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChurchResponseDto>>> GetAll(
            [FromQuery(Name = "pageNumber")] int page = 1,
            [FromQuery(Name = "pageQuantity")] int pageQuantity = 50,
            [FromQuery(Name = "querySearch")] string? search = null)
        {
            if (page < 1) page = 1;
            if (pageQuantity < 1) pageQuantity = 50;

            var churches = await _repository.GetAllChurchesAsync(page, pageQuantity, search);
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
            var churches = await _repository.GetChurchesbyFederationId(federationId);
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
        // Desativa a Igreja em vez de deletar fisicamente
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var deactivated = await _repository.DeactivateAsync(id);

            if (deactivated == null)
                return NotFound();

            return Ok(new { message = "Church deactivated successfully.", data = deactivated });
        }

    }
}
