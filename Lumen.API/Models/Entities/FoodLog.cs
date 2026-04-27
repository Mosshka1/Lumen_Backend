namespace Lumen.API.Models.Entities;

public enum MealType
{
    Breakfast = 0,
    Lunch     = 1,
    Dinner    = 2
}

public class FoodLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DailyRecordId { get; set; }
    public DailyRecord DailyRecord { get; set; } = null!;

    public MealType MealType { get; set; }
    public bool IsChecked { get; set; } = false;
}
