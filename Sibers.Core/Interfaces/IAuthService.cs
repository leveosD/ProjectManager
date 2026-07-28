using Sibers.Core.DTOs;

namespace Sibers.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> LoginAsync(LoginDto dto);
}
