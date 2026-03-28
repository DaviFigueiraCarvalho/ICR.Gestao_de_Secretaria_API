using ICR.API.Authorization;
using ICR.Domain.DTOs;
using ICR.Domain.Model.DashboardAggregate;
using ICR.Domain.Model.UserRoleAgreggate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserModel = ICR.Domain.Model.UserRoleAgreggate.User;

namespace ICR.API.Controllers
{
    [ApiController]
    [Route("api/v1/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepository _repository;
        public DashboardController(IDashboardRepository repository) => _repository = repository;

        [HttpGet("national")]
        [AuthorizeScope(UserModel.UserScope.NATIONAL)]
        public async Task<IActionResult> GetNational() => Ok(await _repository.GetNationalStatsAsync());

        [HttpGet("federation/{id:long}")]
        [AuthorizeScope(UserModel.UserScope.FEDERATION)]
        public async Task<IActionResult> GetFederation(long id) => Ok(await _repository.GetFederationStatsAsync(id));

        [HttpGet("church/{id:long}")]
        public async Task<IActionResult> GetChurch(long id) => Ok(await _repository.GetChurchStatsAsync(id));
    }
}