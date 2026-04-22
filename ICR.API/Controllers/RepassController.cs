using ICR.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using ICR.Domain.DTOs;
using ICR.Domain.Model.RepassAggregate;
using ICR.Domain.Model.UserRoleAgreggate;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserModel = ICR.Domain.Model.UserRoleAgreggate.User;

namespace ICR.API.Controllers
{
    [ApiController]
    [Route("api/repasses")]
    [Authorize]
    [AuthorizeScope(UserModel.UserScope.NATIONAL)]
    public class RepassController : ControllerBase
    {
        private readonly IRepassRepository _repository;

        public RepassController(IRepassRepository repository)
        {
            _repository = repository;
        }

        // POST: api/repasses
        [HttpPost]
        public async Task<ActionResult<RepassResponseDTO>> Create([FromBody] RepassDTO dto)
        {
            var result = await _repository.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // GET: api/repasses/{id}
        [HttpGet("{id:long}")]
        public async Task<ActionResult<RepassResponseDTO>> GetById(long id)
        {
            var result = await _repository.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // GET: api/repasses?pageNumber=1&pageQuantity=10
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RepassResponseDTO>>> GetAll(
            [FromQuery(Name = "pageNumber")] int pageNumber = 1,
            [FromQuery(Name = "pageQuantity")] int pageQuantity = 50)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageQuantity < 1) pageQuantity = 50;

            var result = await _repository.GetAllAsync(pageNumber, pageQuantity);
            return Ok(result);
        }

        // GET: api/repasses/church/{churchId}
        [HttpGet("church/{churchId:long}")]
        public async Task<ActionResult<IEnumerable<RepassResponseDTO>>> GetByChurch(long churchId)
        {
            var result = await _repository.GetByChurchIdAsync(churchId);
            return Ok(result);
        }

        // GET: api/repasses/reference/{reference}
        [HttpGet("reference/{reference:long}")]
        public async Task<ActionResult<IEnumerable<RepassResponseDTO>>> GetByReference(long reference)
        {
            var result = await _repository.GetByReferenceIdAsync(reference);
            return Ok(result);
        }

        // PATCH: api/repasses/{id}
        [HttpPatch("{id:long}")]
        public async Task<ActionResult<RepassResponseDTO>> Update(
            long id,
            [FromBody] RepassUpdateDTO dto)
        {
            var result = await _repository.UpdateAsync(id, dto);

            if (result.Id == 0)
                return NotFound(result);

            if (!string.IsNullOrEmpty(result.ResultMessage))
                return BadRequest(result);

            return Ok(result);
        }

        // DELETE: api/repasses/{id}
        [HttpDelete("{id:long}")]
        public async Task<ActionResult<RepassResponseDTO>> Delete(long id)
        {
            var result = await _repository.DeleteAsync(id);

            if (result.Id == 0)
                return NotFound(result);

            return Ok(result);
        }
        [HttpGet("references")]
        public async Task<ActionResult<IEnumerable<Reference>>> GetAllRefences()
        {
            var references = await _repository.GetAllReferencesAsync();
            return Ok(references);
        }

        [HttpGet("references/{id:long}")]
        public async Task<ActionResult<Reference>> GetReferenceById(long id)
        {
            var reference = await _repository.GetReferenceByIdAsync(id);

            if (reference == null)
                return NotFound(reference);

            return Ok(reference);
        }
    }
}
