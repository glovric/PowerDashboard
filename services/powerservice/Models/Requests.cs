using System.ComponentModel.DataAnnotations;

namespace PowerService.Models {

    public class LatestDataRequest
    {
        [Required(ErrorMessage = "Country cannot be empty")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "Count cannot be empty")]
        public int Count { get; set; }

        [Required(ErrorMessage = "Interval cannot be empty")]
        public int Interval { get; set; }
    }

    public class HistoricDataRequest
    {
        [Required(ErrorMessage = "Country cannot be empty")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "Interval cannot be empty")]
        public int Interval { get; set; }
        
        [Required(ErrorMessage = "Start Date cannot be empty")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Start Date cannot be empty")]
        public DateTime EndDate { get; set; }
    }

    public class ForecastDataRequest
    {
        [Required(ErrorMessage = "Country cannot be empty")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "Interval cannot be empty")]
        public int Interval { get; set; }

        [Required(ErrorMessage = "Horizon cannot be empty")]
        public int Horizon { get; set; }
        
        [Required(ErrorMessage = "Forecast Date cannot be empty")]
        public DateTime ForecastDate { get; set; }

    }

    public class ExportRequest
    {
        [Required(ErrorMessage = "Export Format cannot be empty")]
        public string ExportFormat {get; set;} = string.Empty;

        [Required(ErrorMessage = "Column Keys cannot be empty")]
        public List<string> ColumnKeys { get; set; } = [];

        [Required(ErrorMessage = "Headers cannot be empty")]
        public List<string> Headers { get; set; } = [];

        [Required(ErrorMessage = "Rows cannot be empty")]
        public List<Dictionary<string, object>> Rows { get; set; } = [];
    }

}