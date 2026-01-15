using ICR.Domain.DTOs.UserRole;
using ICR.Domain.Model.UserRoleAgreggate;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICR.API.Controllers
{
    [ApiController]
    [Route("api/user-roles")]
    public class UserRoleController : ControllerBase
    {
        private readonly IUserRoleRepository _repository;

        public UserRoleController(IUserRoleRepository repository)
        {
            _repository = repository;
        }

        // ===== USER =====

        [HttpPost("users")]
        public async Task<ActionResult<UserResponseDTO>> AddUser(UserDTO dto)
        {
            var result = await _repository.AddUserAsync(dto);
            return Ok(result);
        }

        [HttpGet("users/{id:long}")]
        public async Task<ActionResult<UserResponseDTO>> GetUserById(long id)
        {
            var result = await _repository.GetUserByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("users/by-username/{username}")]
        public async Task<ActionResult<UserResponseDTO>> GetUserByUsername(string username)
        {
            var result = await _repository.GetUserByUsernameAsync(username);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserResponseDTO>>> GetAllUsers()
        {
            return Ok(await _repository.GetAllUsersAsync());
        }

        [HttpPatch("users/{id:long}")]
        public async Task<ActionResult<UserResponseDTO>> UpdateUser(long id, UserPatchDTO dto)
        {
            var result = await _repository.UpdateUserAsync(id, dto);
            if (result.Id == 0) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("users/{id:long}")]
        public async Task<ActionResult<UserResponseDTO>> DeleteUser(long id)
        {
            var result = await _repository.DeleteUserAsync(id);
            if (result.Id == 0) return NotFound(result);
            return Ok(result);
        }

        // ===== ROLE =====

        [HttpPost("roles")]
        public async Task<ActionResult<RoleResponseDTO>> AddRole(RoleDTO dto)
        {
            return Ok(await _repository.AddRoleAsync(dto));
        }

        [HttpGet("roles/{id:long}")]
        public async Task<ActionResult<RoleResponseDTO>> GetRoleById(long id)
        {
            var result = await _repository.GetRoleByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("roles/by-name/{name}")]
        public async Task<ActionResult<RoleResponseDTO>> GetRoleByName(string name)
        {
            var result = await _repository.GetRoleByNameAsync(name);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<RoleResponseDTO>>> GetAllRoles()
        {
            return Ok(await _repository.GetAllRolesAsync());
        }

        [HttpPatch("roles/{id:long}")]
        public async Task<ActionResult<RoleResponseDTO>> UpdateRole(long id, RolePatchDTO dto)
        {
            var result = await _repository.UpdateRoleAsync(id, dto);
            if (result.Id == 0) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("roles/{id:long}")]
        public async Task<ActionResult<RoleResponseDTO>> DeleteRole(long id)
        {
            var result = await _repository.DeleteRoleAsync(id);
            if (result.Id == 0) return NotFound(result);
            return Ok(result);
        }

        // ===== USER_ROLE =====

        [HttpPost("assign")]
        public async Task<ActionResult<UserRoleResponseDTO>> AddUserRole(long userId, long roleId)
        {
            return Ok(await _repository.AddUserRoleAsync(userId, roleId));
        }

        [HttpDelete("assign")]
        public async Task<ActionResult<UserRoleResponseDTO>> RemoveUserRole(long userId, long roleId)
        {
            return Ok(await _repository.RemoveUserRoleAsync(userId, roleId));
        }

        [HttpGet("users/{userId:long}/roles")]
        public async Task<ActionResult<IEnumerable<RoleResponseDTO>>> GetRolesByUser(long userId)
        {
            return Ok(await _repository.GetRolesByUserIdAsync(userId));
        }

        [HttpGet("roles/{roleId:long}/users")]
        public async Task<ActionResult<IEnumerable<UserResponseDTO>>> GetUsersByRole(long roleId)
        {
            return Ok(await _repository.GetUsersByRoleIdAsync(roleId));
        }
    }
}
