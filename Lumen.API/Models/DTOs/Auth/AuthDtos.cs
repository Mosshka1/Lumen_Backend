namespace Lumen.API.Models.DTOs.Auth;

public record RegisterRequest(
    string Login,
    string Email,
    string Password
);

public record LoginRequest(
    string Email,
    string Password
);

public record AuthResponse(
    string Token,
    UserDto User
);

public record UserDto(
    Guid Id,
    string Login,
    string Email,
    string DisplayName,
    int CharacterId,
    string CharacterAssetKey,
    string InviteCode,
    bool HasPartner
);
