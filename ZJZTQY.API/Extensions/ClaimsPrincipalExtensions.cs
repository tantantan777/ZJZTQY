using System.Security.Claims;

namespace ZJZTQY.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetEmail(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.Email);

        public static int GetUserId(this ClaimsPrincipal user)
        {
            var idStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idStr, out int id) ? id : 0;
        }

        public static string? GetUsername(this ClaimsPrincipal user)
             => user.FindFirstValue(ClaimTypes.Name);
    }
}