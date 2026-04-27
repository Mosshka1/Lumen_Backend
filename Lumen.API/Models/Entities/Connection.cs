namespace Lumen.API.Models.Entities;

public class Connection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = [];
}
