namespace PowerService.Dtos.Inference
{

    public class LatestData
    {
        public string[]? Timestamp { get; set; }
        public double?[]? Load { get; set; }
        public string[]? HistLabels { get; set; }

    }

    public class HistoricData : LatestData {}

    public class ForecastData : LatestData {}

}