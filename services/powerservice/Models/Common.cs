namespace PowerService.Models.Common
{

    public class LoadPoint
    {
        public DateTimeOffset Timestamp { get; set; } = default!;
        public double? LoadValue { get; set; }
    }

}