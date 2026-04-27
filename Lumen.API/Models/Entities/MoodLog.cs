namespace Lumen.API.Models.Entities;

public enum TimeSlot
{
    T0800 = 0,
    T1100 = 1,
    T1400 = 2,
    T1700 = 3,
    T2000 = 4,
    T2300 = 5   // 23:00 – 07:59
}

public enum MoodLevel
{
    VeryHappy = 1,
    Happy     = 2,
    Neutral   = 3,
    Sad       = 4,
    VerySad   = 5
}

public class MoodLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DailyRecordId { get; set; }
    public DailyRecord DailyRecord { get; set; } = null!;

    public TimeSlot TimeSlot { get; set; }
    public MoodLevel Mood { get; set; }

    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
}
