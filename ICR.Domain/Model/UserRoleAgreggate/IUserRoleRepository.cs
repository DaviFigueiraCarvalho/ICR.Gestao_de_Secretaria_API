using ICR.Domain.DTOs.UserRole;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICR.Domain.Model.UserRoleAgreggate
{
    public interface IUserRoleRepository
    {
        // ===== USER =====
        Task<UserResponseDTO> AddUserAsync(UserDTO user);
        Task<UserResponseDTO?> GetUserByIdAsync(long userId);
        Task<UserResponseDTO?> GetUserByUsernameAsync(string username);
        Task<IEnumerable<UserResponseDTO>> GetAllUsersAsync();
        Task<UserResponseDTO> UpdateUserAsync(long userId, UserPatchDTO user);
        Task<UserResponseDTO> DeleteUserAsync(long userId);

        // ===== ROLE =====
        Task<RoleResponseDTO> AddRoleAsync(RoleDTO role);
        Task<RoleResponseDTO?> GetRoleByIdAsync(long roleId);
        Task<RoleResponseDTO?> GetRoleByNameAsync(string name);
        Task<IEnumerable<RoleResponseDTO>> GetAllRolesAsync();
        Task<RoleResponseDTO> UpdateRoleAsync(long roleId, RolePatchDTO role);
        Task<RoleResponseDTO> DeleteRoleAsync(long roleId);

        // ===== USER_ROLE =====
        Task<UserRoleResponseDTO> AddUserRoleAsync(long userId, long roleId);
        Task<UserRoleResponseDTO> RemoveUserRoleAsync(long userId, long roleId);

        Task<IEnumerable<RoleResponseDTO>> GetRolesByUserIdAsync(long userId);
        Task<IEnumerable<UserResponseDTO>> GetUsersByRoleIdAsync(long roleId);
    }
}
