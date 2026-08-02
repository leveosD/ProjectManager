using System.Security.Claims;
using Sibers.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Sibers.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public string? Role => User?.FindFirstValue(ClaimTypes.Role);

    public int? EmployeeId
    {
        get
        {
            var idString = User?.FindFirstValue("EmployeeId");
            if (int.TryParse(idString, out int id))
            {
                return id;
            }
            return null;
        }
    }
}