using Lumen.API.Models.DTOs.Connections;

namespace Lumen.API.Services.Interfaces;

public interface IConnectionService
{
    Task<MyCodeResponse> GetMyCodeAsync(Guid userId);
    Task<PartnerDto> ConnectAsync(Guid userId, string partnerCode);
    Task DisconnectAsync(Guid userId);
    Task<PartnerDto?> GetPartnerAsync(Guid userId);
}
