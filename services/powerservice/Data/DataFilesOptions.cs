using System.ComponentModel.DataAnnotations;

namespace PowerService.Data
{
    public class DataFilesOptions
    {
        [Required(ErrorMessage = "TimeSeries60 path cannot be empty! Make sure you set a value in settings.")]
        public string TimeSeries60 { get; set; } = string.Empty;
        [Required(ErrorMessage = "TimeSeries15 path cannot be empty! Make sure you set a value in settings.")]
        public string TimeSeries15 { get; set; } = string.Empty;
    }
}