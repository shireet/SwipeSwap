using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SwipeSwap.Infrastructure.Jwt.Services.Interfaces;

namespace SwipeSwap.Infrastructure.Jwt.Services.Implementations;


public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public int UserId =>
        int.Parse(httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
}