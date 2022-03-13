using System.Security.Claims;
using System.Security.Principal;

namespace BugTracker.Extensions
{
    public static class IdentityExtensions
    {
        public static int GetCompanyId(this IIdentity identity, ClaimsPrincipal user)
        {
            Claim claim = ((ClaimsIdentity)identity).FindFirst("CompanyId")!;
            
            return int.Parse(claim.Value);
        }
    }
}
