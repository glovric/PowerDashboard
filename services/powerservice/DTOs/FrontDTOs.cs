using PowerService.DTOs.Projections;

namespace PowerService.DTOs.Front
{

    public class FrontDTOBase
    {
        public double?[]? LoadValues { get; set; }

        public double?[]? RampValues { get; set; }

        public int[]? HistValues { get; set; }

        public string[]? HistLabels { get; set; }
    }

    public class LatestData : FrontDTOBase
    {
        public string[]? Labels { get; set; }
    }

    public class HistoricData : LatestData {}

    public class ForecastData : FrontDTOBase {}

    public class TransmissionStatus
    {
        public Dictionary<string, List<LoadMeasurement>> Data { get; set; } = new();
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

    public class ExportResult
    {
        public Stream Stream { get; init; } = default!;
        public string ContentType { get; init; } = default!;
    }

}