namespace PowerService.Services
{
    public abstract class BaseService<T> where T : BaseService<T>
    {
        protected readonly ILogger<T> _logger;

        public readonly HashSet<string> allowedCountriesHour = new(StringComparer.OrdinalIgnoreCase) 
        { "AT", "BE", "BG", "CH", "CY", "CZ", "DE", "DK", "EE", "ES", "FI", "FR", "GB", "GR", "HR", "HU", "IE", "IT", "LT", "LU", "LV", "ME", "NL", "NO", "PL", "PT", "RO", "RS", "SE", "SI", "SK", "UA" };

        public readonly HashSet<string> allowedCountriesQuarter = new(StringComparer.OrdinalIgnoreCase) 
        { "AT", "BE", "DE", "HU", "LU", "NL" };

        public BaseService(ILogger<T> logger)
        {
            _logger = logger;
        }

        public (string[] BinLabels, int[] Counts) CreateHistogramLabels(double?[] values, int binCount = 5)
        {
            if (values == null || values.Length == 0 || binCount <= 0)
                return (Array.Empty<string>(), Array.Empty<int>());

            // Only use non-null values for min/max
            var validValues = values.OfType<double>().ToArray();
            if (validValues.Length == 0)
                return (Array.Empty<string>(), Array.Empty<int>());

            double min = validValues.Min();
            double max = validValues.Max();

            if (min == max) max = min + 1;

            double binSize = (max - min) / binCount;

            string[] binLabels = new string[binCount];
            int[] counts = new int[binCount];

            for (int i = 0; i < binCount; i++)
            {
                double binStart = min + i * binSize;
                double binEnd = min + (i + 1) * binSize;
                if (i == 0)
                {
                    // First bin: < value
                    binLabels[i] = $"< {Math.Round((decimal)binEnd, 2)}";
                    continue;
                }
                else if (i == binCount - 1)
                {
                    // Last bin: > value
                    binLabels[i] = $"> {Math.Round((decimal)binStart, 2)}";
                    continue;
                }
                binLabels[i] = $"{Math.Round((decimal)binStart, 2)} - {Math.Round((decimal)binEnd, 2)}";
            }

            foreach (var v in values)
            {
                if (!v.HasValue) continue; // skip nulls

                double value = v.Value;
                int binIndex = (value == max) ? binCount - 1 : (int)((value - min) / binSize);
                counts[binIndex]++;
            }

            return (binLabels, counts);
        }

    }
}