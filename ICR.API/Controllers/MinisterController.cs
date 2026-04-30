using ICR.API.Authorization;
using ICR.Domain.DTOs;
using ICR.Domain.Model.MinisterAggregate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserModel = ICR.Domain.Model.UserRoleAgreggate.User;

namespace ICR.API.Controllers
{
    [ApiController]
    [Route("api/ministers")]
    [AuthorizeScope(UserModel.UserScope.NATIONAL)]
    public class MinisterController : ControllerBase
    {
        private readonly IMinisterRepository _repository;

        public MinisterController(IMinisterRepository repository)
        {
            _repository = repository;
        }

        // POST: api/ministers
        [HttpPost]
        public async Task<ActionResult<MinisterResponseDTO>> Create([FromBody] MinisterDTO minister)
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
        public async Task<ActionResult<IEnumerable<MinisterResponseDTO>>> GetAll(
            [FromQuery(Name = "pageNumber")] int page = 1,
            [FromQuery(Name = "pageQuantity")] int pageQuantity = 50)
        {
            if (page < 1) page = 1;
            if (pageQuantity < 1) pageQuantity = 50;

            var ministers = await _repository.GetAllAsync(page, pageQuantity);
            return Ok(ministers);
        }

        // GET: api/ministers/church/{churchId}
        [HttpGet("church/{churchId:long}")]
        public async Task<ActionResult<IEnumerable<MinisterResponseDTO>>> GetByChurch(long churchId)
        {
            var ministers = await _repository.GetByChurchIdAsync(churchId);
            return Ok(ministers);
        }

        // GET: api/ministers/insured
        [HttpGet("insured")]
        public async Task<ActionResult<IEnumerable<MinisterInsuredListDTO>>> GetInsured()
        {
            var result = await _repository.GetInsuredAsync();
            return Ok(result);
        }

        // GET: api/ministers/birthdays/month/5
        [HttpGet("birthdays/month/{month:int}")]
        public async Task<ActionResult<IEnumerable<MinisterBirthdayDTO>>> GetBirthdays(int month)
        {
            var result = await _repository.GetBirthdaysByMonthAsync(month);
            return Ok(result);
        }

        // GET: api/ministers/weddings/month/5
        [HttpGet("weddings/month/{month:int}")]
        public async Task<ActionResult<IEnumerable<MinisterBirthdayDTO>>> GetWeddings(int month)
        {
            var result = await _repository.GetWeddingAnniversariesByMonthAsync(month);
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
