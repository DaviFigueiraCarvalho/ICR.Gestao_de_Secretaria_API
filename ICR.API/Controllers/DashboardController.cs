using ICR.Domain.DTOs;
using ICR.Domain.Model.DashboardAggregate;
using Microsoft.AspNetCore.Mvc;

namespace ICR.API.Controllers
{
    [ApiController]
    [Route("api/v1/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepository _repository;
        public DashboardController(IDashboardRepository repository) => _repository = repository;

        [HttpGet("national")]
        public async Task<IActionResult> GetNational() => Ok(await _repository.GetNationalStatsAsync());

        [HttpGet("federation/{id:long}")]
        public async Task<IActionResult> GetFederation(long id) => Ok(await _repository.GetFederationStatsAsync(id));

        [HttpGet("church/{id:long}")]
        public async Task<IActionResult> GetChurch(long id) => Ok(await _repository.GetChurchStatsAsync(id));
    }
}