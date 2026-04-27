namespace Lumen.API.Models.DTOs.Connections;

public record ConnectRequest(string Code);

public record PartnerDto(
    Guid Id,
    string DisplayName,
    string CharacterAssetKey,
    bool IsConnected
);

public record MyCodeResponse(string InviteCode);
