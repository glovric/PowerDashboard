namespace PowerService.DTOs.Inference
{

    public class InferenceDTOBase
    {
        public string[]? Timestamp { get; set; }
        public double?[]? Load { get; set; }
        public string[]? HistLabels { get; set; }
    }

    public class LatestData : InferenceDTOBase {}

    public class HistoricData : InferenceDTOBase {}

    public class ForecastData : InferenceDTOBase {}

}