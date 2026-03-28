using ICR.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using ICR.Domain.DTOs;
using ICR.Domain.Model.FamilyAggregate;
using ICR.Domain.Model.UserRoleAgreggate;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UserModel = ICR.Domain.Model.UserRoleAgreggate.User;

namespace ICR.API.Controllers
{
    [ApiController]
    [Route("api/families")]
    [Authorize]
    public class FamilyController : ControllerBase
    {
        private readonly IFamilyRepository _repository;

        public FamilyController(IFamilyRepository familyRepository)
        {
            _repository = familyRepository;
        }

        // GET api/families?pageNumber=1&pageQuantity=10
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var families = await _repository.GetAsync(page, pageSize);
            return Ok(families);
        }

        // GET api/families/5
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var family = await _repository.GetByIdAsync(id);

            if (family == null)
                return NotFound(new { message = "Family not found" });

            return Ok(family);
        }

        // GET api/families/church/10
        [HttpGet("church/{churchId:long}")]
        [AuthorizeScope(UserModel.UserScope.FEDERATION)]
        public async Task<IActionResult> GetByChurch(long churchId)
        {
            var families = await _repository.GetByChurchId(churchId);
            return Ok(families);
        }

        // GET api/families/cell/5
        [HttpGet("cell/{cellId:long}")]
        public async Task<IActionResult> GetByCell(long cellId)
        {
            var families = await _repository.GetByCellIdAsync(cellId);
            return Ok(families);
        }

        // GET api/families/wedding/month/6
        [HttpGet("wedding/month/{month:int}")]
        public async Task<IActionResult> GetByWeddingMonth(int month)
        {
            if (month < 1 || month > 12)
                return BadRequest(new { message = "Invalid month" });

            var families = await _repository
                .GetFamiliesByWeddingBirthdayMonthAsync(month);

            return Ok(families);
        }

        // POST api/families
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FamilyDTO dto)
        {
            var result = await _repository.AddAsync(dto);

            if (result.Id == 0)
                return BadRequest(result);

            return Ok(result);
        }

        // PATCH api/families/5
        [HttpPatch("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] FamilyPatchDTO dto)
        {
            var updated = await _repository.UpdateAsync(id, dto);

            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        // DELETE api/families/5
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _repository.DeleteAsync(id);

            if (result.Id == 0)
                return NotFound(result);

            return Ok(result);
        }
    }
}
