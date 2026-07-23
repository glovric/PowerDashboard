namespace PowerService.DTOs.Projections
{

    public class LoadMeasurement
    {
        public DateTimeOffset Timestamp { get; set; } = default!;
        public double? LoadValue { get; set; }
    }

    public class TransmissionStatusProjection
    {
        public DateTimeOffset Timestamp { get; set; } = default!;
        public Dictionary<string, double?> Loads { get; set; } = new();
    }

}