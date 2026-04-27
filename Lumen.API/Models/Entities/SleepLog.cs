namespace Lumen.API.Models.Entities;

public class SleepLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DailyRecordId { get; set; }
    public DailyRecord DailyRecord { get; set; } = null!;

    public float HoursSlept { get; set; }
    public string? Note { get; set; }

    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
}
