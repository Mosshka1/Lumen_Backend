using Lumen.API.Data;
using Lumen.API.Models.DTOs.Connections;
using Lumen.API.Models.Entities;
using Lumen.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lumen.API.Services;

public class ConnectionService : IConnectionService
{
    private readonly AppDbContext _db;

    public ConnectionService(AppDbContext db) => _db = db;

    public async Task<MyCodeResponse> GetMyCodeAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        return new MyCodeResponse(user.InviteCode);
    }

    public async Task<PartnerDto> ConnectAsync(Guid userId, string partnerCode)
    {
        var me = await _db.Users
            .Include(u => u.Connection)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (me.ConnectionId.HasValue)
            throw new InvalidOperationException("You are already connected to someone.");

        var partner = await _db.Users
            .Include(u => u.Character)
            .Include(u => u.Connection)
            .FirstOrDefaultAsync(u => u.InviteCode == partnerCode.ToUpper())
            ?? throw new KeyNotFoundException("No user found with this invite code.");

        if (partner.Id == userId)
            throw new InvalidOperationException("You cannot connect to yourself.");

        if (partner.ConnectionId.HasValue)
            throw new InvalidOperationException("This user is already in a pair.");

        // Create a shared connection
        var connection = new Connection();
        _db.Connections.Add(connection);
        await _db.SaveChangesAsync();

        me.ConnectionId      = connection.Id;
        partner.ConnectionId = connection.Id;
        await _db.SaveChangesAsync();

        return new PartnerDto(
            partner.Id,
            partner.DisplayName,
            partner.Character?.AssetKey ?? "panda",
            true
        );
    }

    public async Task DisconnectAsync(Guid userId)
    {
        var me = await _db.Users
            .Include(u => u.Connection)
            .ThenInclude(c => c!.Users)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (me.Connection is null)
            throw new InvalidOperationException("You are not connected to anyone.");

        var connectionId = me.ConnectionId!.Value;

        // Remove connection from both users
        foreach (var user in me.Connection.Users)
            user.ConnectionId = null;

        await _db.SaveChangesAsync();

        // Delete the connection record
        var conn = await _db.Connections.FindAsync(connectionId);
        if (conn is not null)
        {
            _db.Connections.Remove(conn);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<PartnerDto?> GetPartnerAsync(Guid userId)
    {
        var me = await _db.Users
            .Include(u => u.Connection)
            .ThenInclude(c => c!.Users)
            .ThenInclude(u => u.Character)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (me.Connection is null) return null;

        var partner = me.Connection.Users.FirstOrDefault(u => u.Id != userId);
        if (partner is null) return null;

        return new PartnerDto(
            partner.Id,
            partner.DisplayName,
            partner.Character?.AssetKey ?? "panda",
            true
        );
    }
}
