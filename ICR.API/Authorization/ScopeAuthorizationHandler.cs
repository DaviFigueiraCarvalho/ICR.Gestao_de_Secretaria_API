using ICR.Domain.Model.UserRoleAgreggate;
using Microsoft.AspNetCore.Authorization;

namespace ICR.API.Authorization
{
    public class MinimumScopeRequirement : IAuthorizationRequirement
    {
        public User.UserScope MinimumScope { get; }

        public MinimumScopeRequirement(User.UserScope minimumScope)
        {
            MinimumScope = minimumScope;
        }
    }

    public class ScopeAuthorizationHandler : AuthorizationHandler<MinimumScopeRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MinimumScopeRequirement requirement)
        {
            var scopeClaim = context.User.FindFirst("scope");

            if (scopeClaim != null
                && Enum.TryParse<User.UserScope>(scopeClaim.Value, out var userScope)
                && userScope >= requirement.MinimumScope)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
