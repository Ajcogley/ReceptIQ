using Microsoft.EntityFrameworkCore;
using ReciptIQ.API.Services.Interfaces;
using ReciptIQ.DTOs.Auth;
using ReciptIQ.Helpers;
using ReciptIQ.Models;

namespace ReciptIQ.API.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtHelper _jwtHelper;

    public AuthService(AppDbContext context, JwtHelper jwtHelper)
    {
        _context = context;
        _jwtHelper = jwtHelper;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email o contraseña incorrectos");

        if ((bool)!user.IsActive)
            throw new UnauthorizedAccessException("Usuario inactivo");

        // Actualizar último login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var token = _jwtHelper.GenerateToken(user.Id, user.Email, user.Role, user.CompanyId);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = _jwtHelper.GetTokenExpiry(),
            User = MapToUserDto(user)
        };
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // Validar email único
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
            throw new InvalidOperationException("El email ya está registrado");

        // Crear empresa
        var company = new Company
        {
            Id = Guid.NewGuid(),
            Name = request.CompanyName,
            Slug = GenerateSlug(request.CompanyName),
            Industry = request.Industry,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Companies.Add(company);

        // Crear usuario admin
        var user = new User
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            Email = request.Email.ToLower(),
            PasswordHash = PasswordHelper.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = "admin", // Primer usuario es admin
            IsActive = true,
            EmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Crear categorías por defecto
        await CreateDefaultCategoriesAsync(company.Id);

        var token = _jwtHelper.GenerateToken(user.Id, user.Email, user.Role, user.CompanyId);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = _jwtHelper.GetTokenExpiry(),
            User = MapToUserDto(user, company.Name)
        };
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user == null ? null : MapToUserDto(user);
    }

    private async Task CreateDefaultCategoriesAsync(Guid companyId)
    {
        var defaultCategories = new List<Category>
        {
            new() { Id = Guid.NewGuid(), CompanyId = companyId, Name = "Alimentos y Bebidas", Color = "#EF4444", Icon = "utensils", IsDefault = true, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), CompanyId = companyId, Name = "Transporte", Color = "#3B82F6", Icon = "car", IsDefault = true, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), CompanyId = companyId, Name = "Oficina", Color = "#8B5CF6", Icon = "briefcase", IsDefault = true, DisplayOrder = 3 },
            new() { Id = Guid.NewGuid(), CompanyId = companyId, Name = "Software", Color = "#06B6D4", Icon = "monitor", IsDefault = true, DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), CompanyId = companyId, Name = "Marketing", Color = "#F59E0B", Icon = "megaphone", IsDefault = true, DisplayOrder = 5 },
            new() { Id = Guid.NewGuid(), CompanyId = companyId, Name = "Otros", Color = "#6B7280", Icon = "more-horizontal", IsDefault = true, DisplayOrder = 10 }
        };

        _context.Categories.AddRange(defaultCategories);
        await _context.SaveChangesAsync();
    }

    private UserDto MapToUserDto(User user, string? companyName = null)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            CompanyId = user.CompanyId,
            CompanyName = companyName ?? user.Company?.Name
        };
    }

    private string GenerateSlug(string name)
    {
        return name.ToLower()
            .Replace(" ", "-")
            .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
            .Replace("ñ", "n");
    }
}
