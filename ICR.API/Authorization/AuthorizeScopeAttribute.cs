using ICR.Domain.Model.UserRoleAgreggate;
using Microsoft.AspNetCore.Authorization;

namespace ICR.API.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AuthorizeScopeAttribute : AuthorizeAttribute
    {
        public AuthorizeScopeAttribute(User.UserScope minimumScope)
            : base(BuildPolicyName(minimumScope))
        {
        }

        internal static string BuildPolicyName(User.UserScope scope) => $"MinimumScope:{scope}";
    }
}
