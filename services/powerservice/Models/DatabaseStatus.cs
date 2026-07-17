namespace PowerService.Models
{
    
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