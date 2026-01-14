using ICR.Domain.DTOs;
using ICR.Domain.Model.FamilyAggregate;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ICR.API.Controllers
{
    [ApiController]
    [Route("api/families")]
    public class FamilyController : ControllerBase
    {
        private readonly IFamilyRepository _familyRepository;

        public FamilyController(IFamilyRepository familyRepository)
        {
            _familyRepository = familyRepository;
        }

        // GET api/families?pageNumber=1&pageQuantity=10
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageQuantity = 10)
        {
            var families = await _familyRepository.GetAsync(pageNumber, pageQuantity);
            return Ok(families);
        }

        // GET api/families/5
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var family = await _familyRepository.GetByIdAsync(id);

            if (family == null)
                return NotFound(new { message = "Family not found" });

            return Ok(family);
        }

        // GET api/families/church/10
        [HttpGet("church/{churchId:long}")]
        public async Task<IActionResult> GetByChurch(long churchId)
        {
            var families = await _familyRepository.GetByChurchId(churchId);
            return Ok(families);
        }

        // GET api/families/cell/5
        [HttpGet("cell/{cellId:long}")]
        public async Task<IActionResult> GetByCell(long cellId)
        {
            var families = await _familyRepository.GetByCellIdAsync(cellId);
            return Ok(families);
        }

        // GET api/families/wedding/month/6
        [HttpGet("wedding/month/{month:int}")]
        public async Task<IActionResult> GetByWeddingMonth(int month)
        {
            if (month < 1 || month > 12)
                return BadRequest(new { message = "Invalid month" });

            var families = await _familyRepository
                .GetFamiliesByWeddingBirthdayMonthAsync(month);

            return Ok(families);
        }

        // POST api/families
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FamilyDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var family = new Family(
                0,
                dto.Name,
                dto.churchId,
                dto.CellId,
                dto.ManId,
                dto.WomanId,
                dto.WeddingDate
            );

            var result = await _familyRepository.AddAsync(family);

            if (result.Id == 0)
                return BadRequest(result);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result
            );
        }

        // PATCH api/families/5
        [HttpPatch("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] FamilyDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedFamily = new Family(
                id,
                dto.Name,
                dto.churchId,
                dto.CellId,
                dto.ManId,
                dto.WomanId,
                dto.WeddingDate
            );

            var result = await _familyRepository.UpdateAsync(id, updatedFamily);

            if (result.Id == 0)
                return NotFound(result);

            return Ok(result);
        }

        // DELETE api/families/5
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _familyRepository.DeleteAsync(id);

            if (result.Id == 0)
                return NotFound(result);

            return Ok(result);
        }
    }
}
