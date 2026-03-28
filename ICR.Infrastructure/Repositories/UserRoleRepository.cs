using ICR.Application.Services;
using ICR.Domain.DTOs.UserRole;
using ICR.Domain.Model.UserRoleAgreggate;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICR.Infra.Data.Repositories
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly ConnectionContext _context;

        public UserRoleRepository(ConnectionContext context)
        {
            _context = context;
        }

        // ===== USER =====
        public async Task<UserResponseDTO> AddUserAsync(UserDTO dto)
        {
            if (dto.MemberId.HasValue)
            {
                var member = await _context.Members
                    .FirstOrDefaultAsync(m => m.Id == dto.MemberId);

                if (member == null)
                    throw new KeyNotFoundException($"Membro de ID:{dto.MemberId} não existe");
            }
            else
            {
                var userroot = Environment.GetEnvironmentVariable("ROOTUSERNAME");
                var rootpass = Environment.GetEnvironmentVariable("ROOTPASSWORD");
                // Se MemberId for nulo, garantir que o usuário está sendo criado com Username "admin"
                // Apenas o root (admin) pode não ter um Member vinculado.
                if (dto.Username.Trim().ToLower() != userroot)
                {
                    throw new ArgumentException($"Somente o usuário root ('{userroot}') pode ser criado sem um MemberId vinculado.");
                }
            }

            var normalizedUsername = dto.Username.Trim().ToLower();

            var userExists = await _context.Users.AnyAsync(u =>
                u.Username.ToLower() == normalizedUsername ||
                (dto.MemberId.HasValue && u.MemberId == dto.MemberId.Value)
            );

            if (userExists)
                return new UserResponseDTO
                {
                    Id = 0,
                };

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                MemberId = dto.MemberId,
                Username = normalizedUsername,
                PasswordHash = passwordHash, // nunca a senha pura
                Scope = dto.Scope
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserResponseDTO
            {
                Id = user.Id,
                MemberId = user.MemberId,
                Username = user.Username,
                Scope = user.Scope,
            };
        }

        public async Task<UserResponseDTO?> GetUserByIdAsync(long userId)
        {
            var user = await _context.Users
                .Include(u => u.Member)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            return new UserResponseDTO
            {
                Id = user.Id,
                MemberId = user.MemberId,
                MemberName = user.Member?.Name ?? "",
                Username = user.Username,
                Scope = user.Scope,
            };
        }

        public async Task<User?> GetUserEntityByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<UserResponseDTO?> GetUserByUsernameAsync(string username)
        {
            var user = await _context.Users
                .Include(u => u.Member)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null) return null;

            return new UserResponseDTO
            {
                Id = user.Id,
                MemberId = user.MemberId,
                MemberName = user.Member?.Name ?? "",
                Username = user.Username,
                Scope = user.Scope,
            };
        }

        public async Task<IEnumerable<UserResponseDTO>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Member)
                .Select(u => new UserResponseDTO
                {
                    Id = u.Id,
                    MemberId = u.MemberId,
                    MemberName = u.Member.Name ?? "",
                    Username = u.Username,
                    Scope = u.Scope,
                })
                .ToListAsync();
        }

        public async Task<UserResponseDTO> UpdateUserAsync(long userId, UserPatchDTO dto)
        {
            var user = await _context.Users
                .Include(u => u.Member)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return new UserResponseDTO
                {
                    Id = 0,
                };

            // FK: Member
            if (dto.MemberId.HasValue)
            {
                var memberExists = await _context.Members
                    .AnyAsync(m => m.Id == dto.MemberId.Value);

                if (!memberExists)
                    return new UserResponseDTO
                    {
                        Id = user.Id,
                    };

                user.MemberId = dto.MemberId.Value;
            }
            else if (user.Username.Trim().ToLower() != "admin")
            {
                // Apenas o usuário root ('admin') pode ter e manter o MemberId como null. Os demais não podem remover a associação.
                // Se a requisição enviar MemberId explicitamente como null para um usuário não-admin (via JSON partial patch, por exemplo), impedimos.
                // Obs: Dependendo de como o DTO é construído no frontend, o MemberId vir nulo no Patch significa "não alterar".
                // Então só faremos algo se a intenção fosse desvincular o membro.
            }

            if (!string.IsNullOrWhiteSpace(dto.Username))
                user.Username = dto.Username;

            if (!string.IsNullOrWhiteSpace(dto.PasswordHash))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash);
            ;

            if (dto.Scope.HasValue)
                user.Scope = dto.Scope.Value;

            await _context.SaveChangesAsync();

            return new UserResponseDTO
            {
                Id = user.Id,
                MemberId = user.MemberId,
                MemberName = user.Member?.Name ?? "",
                Username = user.Username,
                Scope = user.Scope,
            };
        }



        public async Task<UserResponseDTO> DeleteUserAsync(long userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return new UserResponseDTO
                {
                    Id = 0,
                };

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return new UserResponseDTO
            {
                Id = user.Id,
                Username = user.Username,
            };
        }

        // ===== ROLE =====
        public async Task<RoleResponseDTO> AddRoleAsync(RoleDTO dto)
        {
            

            var role = new Role
            {
                Name = dto.Name,
                MinimalScope = dto.MinimalScope,
                Description = dto.Description,
                Active = dto.Active
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return new RoleResponseDTO
            {
                Id = role.Id,
                Name = role.Name,
                MinimalScope = role.MinimalScope,
                Description = role.Description,
                Active = role.Active,
            };
        }

        public async Task<RoleResponseDTO?> GetRoleByIdAsync(long roleId)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
            if (role == null) return null;

            return new RoleResponseDTO
            {
                Id = role.Id,
                Name = role.Name,
                MinimalScope = role.MinimalScope,
                Description = role.Description,
                Active = role.Active,
            };
        }

        public async Task<RoleResponseDTO?> GetRoleByNameAsync(string name)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == name);
            if (role == null) return null;

            return new RoleResponseDTO
            {
                Id = role.Id,
                Name = role.Name,
                MinimalScope = role.MinimalScope,
                Description = role.Description,
                Active = role.Active,
            };
        }

        public async Task<IEnumerable<RoleResponseDTO>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Select(r => new RoleResponseDTO
                {
                    Id = r.Id,
                    Name = r.Name,
                    MinimalScope = r.MinimalScope,
                    Description = r.Description,
                    Active = r.Active,
                })
                .ToListAsync();
        }

        public async Task<RoleResponseDTO> UpdateRoleAsync(long roleId, RolePatchDTO dto)
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
                return new RoleResponseDTO
                {
                    Id = 0,
                };

            if (!string.IsNullOrWhiteSpace(dto.Name))
                role.Name = dto.Name;

            if (dto.MinimalScope.HasValue)
                role.MinimalScope = dto.MinimalScope.Value;

            if (dto.Description != null)
                role.Description = dto.Description;

            if (dto.Active.HasValue)
                role.Active = dto.Active.Value;

            await _context.SaveChangesAsync();

            return new RoleResponseDTO
            {
                Id = role.Id,
                Name = role.Name,
                MinimalScope = role.MinimalScope,
                Description = role.Description,
                Active = role.Active,
            };
        }


        public async Task<RoleResponseDTO> DeleteRoleAsync(long roleId)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
            if (role == null)
                return new RoleResponseDTO
                {
                    Id = 0,
                };

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return new RoleResponseDTO
            {
                Id = role.Id,
                Name = role.Name,
            };
        }

        // ===== USER_ROLE =====
        public async Task<UserRoleResponseDTO> AddUserRoleAsync(long userId, long roleId)
        {
            var user = await _context.Users.FindAsync(userId);
            var role = await _context.Roles.FindAsync(roleId);

            if (user == null)
                return new UserRoleResponseDTO
                {
                    UserId = 0,
                };
            if(role == null)
                return new UserRoleResponseDTO
                {
                    RoleId = 0,
                };

            var exists = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (!exists)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = userId,
                    RoleId = roleId
                });
                await _context.SaveChangesAsync();
            }

            return new UserRoleResponseDTO { };
        }

        public async Task<UserRoleResponseDTO> RemoveUserRoleAsync(long userId, long roleId)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (userRole != null)
            {
                _context.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();
            }

            var user = await _context.Users.FindAsync(userId);
            var role = await _context.Roles.FindAsync(roleId);

            return new UserRoleResponseDTO
            {
                UserId = userId,
                RoleId = roleId,
                UserName = user?.Member.Name ?? "NULL",
                RoleName = role?.Name ?? "NULL"
            };
        }

        public async Task<IEnumerable<RoleResponseDTO>> GetRolesByUserIdAsync(long userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Include(ur => ur.Role)
                .Select(ur => new RoleResponseDTO
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    MinimalScope = ur.Role.MinimalScope,
                    Description = ur.Role.Description,
                    Active = ur.Role.Active,
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<UserResponseDTO>> GetUsersByRoleIdAsync(long roleId)
        {
            return await _context.UserRoles
                .Where(ur => ur.RoleId == roleId)
                .Include(ur => ur.User)
                .ThenInclude(u => u.Member)
                .Select(ur => new UserResponseDTO
                {
                    Id = ur.User.Id,
                    MemberId = ur.User.MemberId,
                    MemberName = ur.User.Member.Name ?? "",
                    Username = ur.User.Username,
                    Scope = ur.User.Scope,
                })
                .ToListAsync();
        }
    }
}
