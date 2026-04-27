namespace Lumen.API.Models.Entities;

public class Character
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AssetKey { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = [];
}
