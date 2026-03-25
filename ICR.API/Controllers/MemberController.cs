using Microsoft.AspNetCore.Authorization;
using ICR.Domain.DTOs;
using ICR.Domain.Model.MemberAggregate;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICR.API.Controllers
{
    [ApiController]
    [Route("api/members")]
    [Authorize]
    public class MemberController : ControllerBase
    {
        private readonly IMemberRepository _repository;

        public MemberController(IMemberRepository repository)
        {
            _repository = repository;
        }

        // GET: api/members
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberResponseDTO>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        { 
            return Ok(await _repository.GetAllAsync(page, pageSize));
        }

        // GET: api/members/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<MemberResponseDTO>> GetById(long id)
        {
            var member = await _repository.GetByIdAsync(id);
            return member == null ? NotFound() : Ok(member);
        }

        // GET: api/members/family/{familyId}
        [HttpGet("family/{familyId}")]
        public async Task<ActionResult<IEnumerable<MemberResponseDTO>>> GetByFamily(long familyId)
        {
            return Ok(await _repository.GetByFamilyAsync(familyId));
        }

        // GET: api/members/birthdays/{month}/church/{churchId}
        [HttpGet("birthdays/{month}/church/{churchId}")]
        public async Task<ActionResult<IEnumerable<MemberResponseDTO>>> GetBirthdaysByMonth(
            int month,
            long churchId)
        {
            return Ok(await _repository.GetBirthdaysByMonthAsync(month, churchId));
        }

        // POST: api/members
        [HttpPost]
        public async Task<ActionResult<MemberResponseDTO>> Create([FromBody] MemberDTO member)
        {
            var result = await _repository.AddAsync(member);

            if (result.Id == 0)
                return BadRequest(result);

            return Ok(result);
        }

        // PATCH: api/members/{id}
        [HttpPatch("{id}")]
        public async Task<ActionResult<MemberResponseDTO>> Patch(long id, [FromBody] MemberPatchDTO member)
        {
            var result = await _repository.UpdateAsync(id, member);

            if (result == null)
                return NotFound();

            if (result.Id == 0)
                return BadRequest(result);

            return Ok(result);
        }

        // DELETE: api/members/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _repository.RemoveAsync(id);

            if (result.Id == 0)
                return NotFound(result);

            return Ok(result);
        }
    }
}
