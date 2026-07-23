namespace PowerService.Data.Entities
{

    public abstract class PowerDataBase
    {
        public int Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }

    public class PowerDataQuarter : PowerDataBase
    {
        public double? NLLoadValue { get; set; }
        public double? BELoadValue { get; set; }
        public double? DELoadValue { get; set; }
        public double? ATLoadValue { get; set; }
        public double? HULoadValue { get; set; }
        public double? LULoadValue { get; set; }
    }

    public class PowerDataHour : PowerDataBase
    {
        public double? ATLoadValue { get; set; }
        public double? BELoadValue { get; set; }
        public double? BGLoadValue { get; set; }
        public double? CHLoadValue { get; set; }
        public double? CYLoadValue { get; set; }
        public double? CZLoadValue { get; set; }
        public double? DELoadValue { get; set; }
        public double? DKLoadValue { get; set; }
        public double? EELoadValue { get; set; }
        public double? ESLoadValue { get; set; }
        public double? FILoadValue { get; set; }
        public double? FRLoadValue { get; set; }
        public double? GBLoadValue { get; set; }
        public double? GRLoadValue { get; set; }
        public double? HRLoadValue { get; set; }
        public double? HULoadValue { get; set; }
        public double? IELoadValue { get; set; }
        public double? ITLoadValue { get; set; }
        public double? LTLoadValue { get; set; }
        public double? LULoadValue { get; set; }
        public double? LVLoadValue { get; set; }
        public double? MELoadValue { get; set; }
        public double? NLLoadValue { get; set; }
        public double? NOLoadValue { get; set; }
        public double? PLLoadValue { get; set; }
        public double? PTLoadValue { get; set; }
        public double? ROLoadValue { get; set; }
        public double? RSLoadValue { get; set; }
        public double? SELoadValue { get; set; }
        public double? SILoadValue { get; set; }
        public double? SKLoadValue { get; set; }
        public double? UALoadValue { get; set; }
    }

}
