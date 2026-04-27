using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Lumen.API.Data;
using Lumen.API.Helpers;
using Lumen.API.Models.DTOs.Auth;
using Lumen.API.Models.Entities;
using Lumen.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Lumen.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email already in use.");

        if (await _db.Users.AnyAsync(u => u.Login == request.Login))
            throw new InvalidOperationException("Login already in use.");

        // Generate unique invite code
        string inviteCode;
        do { inviteCode = InviteCodeGenerator.Generate(); }
        while (await _db.Users.AnyAsync(u => u.InviteCode == inviteCode));

        var user = new User
        {
            Login        = request.Login.Trim(),
            Email        = request.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName  = request.Login.Trim(),
            InviteCode   = inviteCode,
            CharacterId  = 1   // default Panda
        };

        _db.Users.Add(user);

        // Pre-create all 3 food logs for today
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dailyRecord = new DailyRecord { UserId = user.Id, Date = today };
        _db.DailyRecords.Add(dailyRecord);
        await _db.SaveChangesAsync();

        _db.FoodLogs.AddRange(
            new FoodLog { DailyRecordId = dailyRecord.Id, MealType = MealType.Breakfast },
            new FoodLog { DailyRecordId = dailyRecord.Id, MealType = MealType.Lunch },
            new FoodLog { DailyRecordId = dailyRecord.Id, MealType = MealType.Dinner }
        );
        await _db.SaveChangesAsync();

        var fullUser = await _db.Users
            .Include(u => u.Character)
            .FirstAsync(u => u.Id == user.Id);

        return new AuthResponse(GenerateToken(fullUser), MapToDto(fullUser));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users
            .Include(u => u.Character)
            .Include(u => u.Connection)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return new AuthResponse(GenerateToken(user), MapToDto(user));
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private string GenerateToken(User user)
    {
        var jwt      = _config.GetSection("JwtSettings");
        var key      = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
        var creds    = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires  = DateTime.UtcNow.AddMinutes(double.Parse(jwt["ExpiresInMinutes"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Login)
        };

        var token = new JwtSecurityToken(
            issuer:             jwt["Issuer"],
            audience:           jwt["Audience"],
            claims:             claims,
            expires:            expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserDto MapToDto(User user) => new(
        user.Id,
        user.Login,
        user.Email,
        user.DisplayName,
        user.CharacterId,
        user.Character?.AssetKey ?? "panda",
        user.InviteCode,
        user.ConnectionId.HasValue
    );
}
