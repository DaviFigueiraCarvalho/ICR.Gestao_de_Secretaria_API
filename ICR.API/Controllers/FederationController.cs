using ICR.Application.Services;
using ICRManagement.Application.ViewModel;
using ICRManagement.Domain.DTOs;
using ICRManagement.Domain.Model.FederationAggregate;
using ICRManagement.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ICRManagement.API.Controllers
{
    [ApiController]
    [Route("api/v1/federation")]
    public class FederationController : ControllerBase
    {
        private readonly IFederationRepository _repository;
        private readonly IdSequenceService _seq;

        public FederationController(
            IFederationRepository repository,
            IdSequenceService seq)
        {
            _repository = repository;
            _seq = seq;
        }

        // CREATE
        [HttpPost]
        public IActionResult Create([FromBody] FederationViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest("Invalid data");

            var id = _seq.GetNextId<Federation>();
            var federation = new Federation(model.Name, id, model.PastorId);

            _repository.Add(federation);
            return CreatedAtAction(nameof(GetById), new { id }, null);
        }

        // GET ALL
        [HttpGet]
        [HttpGet]
        public IActionResult GetAll(int pageNumber = 1, int pageQuantity = 10)
        {
            var federations = _repository.Get(pageNumber, pageQuantity);

            var result = federations.Select(f => new FederationViewModel
            {
                Id = f.Id,
                Name = f.Name,
                PastorId = f.PastorId
            }).ToList();

            return Ok(result);
        }

        // GET BY ID
        [HttpGet("{id}")]
        public IActionResult GetById(long id)
        {
            var federation = _repository.Get(id);
            if (federation == null) return NotFound();
            return Ok(federation);
        }

        // PATCH
        [HttpPatch("{id}")]
        public IActionResult Patch(
    [FromRoute] long id,
    [FromForm] FederationDTO dto)
        {
            var federation = _repository.Get(id);
            if (federation == null)
                return NotFound();

            if (dto.Name != null)
                federation.SetName(dto.Name);

            if (dto.PastorId != null)
                federation.SetPastorId(dto.PastorId.Value);

            _repository.Save();
            return NoContent();
        }
        // DELETE
        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            _repository.Delete(id);
            return NoContent();
        }
    }
}
