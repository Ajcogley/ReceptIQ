
using ReciptIQ.DTOs.Auth;

namespace ReciptIQ.API.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<UserDto?> GetUserByIdAsync(Guid userId);
}
