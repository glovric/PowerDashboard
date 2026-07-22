namespace PowerService.Dtos.Front
{

    public class LatestData
    {
        public string[]? Labels { get; set; }
        public double?[]? LoadValues { get; set; }

        public double?[]? RampValues { get; set; }

        public int[]? HistValues { get; set; }

        public string[]? HistLabels { get; set; }

    }

    public class HistoricData : LatestData {}

    public class ForecastData
    {
        public double?[]? LoadValues { get; set; }

        public double?[]? RampValues { get; set; }

        public int[]? HistValues { get; set; }

        public string[]? HistLabels { get; set; }
    }

    public class TransmissionStatus
    {
        public DateTimeOffset Timestamp { get; set; } = default!;
        public Dictionary<string, double?> Loads { get; set; } = new();
    }

    public class DbStatus
    {
        public bool IsOnline { get; set; } = false;
        public string LastDataTimeHour { get; set; } = "N/A";
        public long TotalRecordsHour { get; set; } = 0;
        public long SizeHour { get; set; } = 0;
        public string LastDataTimeQuarter { get; set; } = "N/A";
        public long TotalRecordsQuarter { get; set; } = 0;
        public long SizeQuarter { get; set; } = 0;
        public long SizeDatabase { get; set; } = 0;
    }
}