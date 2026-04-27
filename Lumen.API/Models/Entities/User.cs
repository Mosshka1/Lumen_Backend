namespace Lumen.API.Models.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }

    public int CharacterId { get; set; } = 1;
    public Character Character { get; set; } = null!;

    public string InviteCode { get; set; } = string.Empty;

    public Guid? ConnectionId { get; set; }
    public Connection? Connection { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<DailyRecord> DailyRecords { get; set; } = [];
}
