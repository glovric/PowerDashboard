using PowerService.Data;
using PowerService.Data.Entities;
using PowerService.DTOs.Inference;
using PowerService.DTOs.Projections;
using Microsoft.EntityFrameworkCore;

namespace PowerService.Services
{

    public class InferenceDataService : BaseService<InferenceDataService>
    {
        private readonly PowerDataContext _context;
        private readonly int MaxLag = 168;

        public InferenceDataService(ILogger<InferenceDataService> logger, PowerDataContext context) : base(logger)
        {
            _context = context;
        }

        public LatestData GetLastRecords(string country, int count, int interval)
        {
            IQueryable<PowerDataBase> query;

            if (interval == 60)
            {
                query = _context.PowerDataHour;
            }
            else {
                query = _context.PowerDataQuarter;
            }

            var dbResults = query
                            .OrderByDescending(p => p.Timestamp)
                            .Take(interval == 60 ? (count + MaxLag) : (count + MaxLag) * 4) 
                            .OrderBy(p => p.Timestamp)
                            .Select(p => new LoadMeasurement {
                                Timestamp = p.Timestamp,
                                LoadValue = EF.Property<double?>(p, country + "LoadValue")
                            })
                            .ToList();

            var timestamps = dbResults
                            .Select(p => p.Timestamp.ToString("yyyy-M-d HH:mm"))
                            .ToArray();

            var loadValues = dbResults
                            .Select(p => p.LoadValue)
                            .ToArray();

            var lastLoadValues = loadValues.TakeLast(interval == 60 ? count : count * 4).ToArray();
            var (histLabels, _) = CreateHistogramLabels(lastLoadValues);

            return new LatestData { 
                Timestamp = timestamps, 
                Load = loadValues, 
                HistLabels = histLabels 
            };
        }

        public HistoricData GetHistoricRecords(string country, DateTime startDate, DateTime endDate, int interval)
        {
            IQueryable<PowerDataBase> query;

            if (interval == 60)
            {
                query = _context.PowerDataHour;
            }
            else {
                query = _context.PowerDataQuarter;
            }

            var query1 = query.Where(p => p.Timestamp >= startDate && p.Timestamp < endDate);

            var query2 = query.Where(p => p.Timestamp < startDate)
                                .OrderByDescending(p => p.Timestamp)
                                .Take(interval == 60 ? MaxLag : MaxLag*4);

            var finalQuery = query2.Union(query1)
                                  .OrderBy(p => p.Timestamp)
                                  .Select(p => new LoadMeasurement {
                                    Timestamp = p.Timestamp,
                                    LoadValue = EF.Property<double?>(p, country + "LoadValue") // tricky?
                                    }
                                  );

            var dbResults = finalQuery.ToList();

            var timestamps = dbResults
                            .Select(p => p.Timestamp.ToString("yyyy-M-d HH:mm") ?? "")
                            .ToArray();

            var loadValues = dbResults
                            .Select(p => p.LoadValue)
                            .ToArray();

            var lastLoadValues = query1
                                .Select(p => new {
                                    LoadValue = EF.Property<double?>(p, country + "LoadValue")
                                })
                                .ToList()
                                .Select(p => p.LoadValue)
                                .ToArray();

            var (histLabels, _) = CreateHistogramLabels(lastLoadValues);

            return new HistoricData { 
                Timestamp = timestamps, 
                Load = loadValues, 
                HistLabels = histLabels 
            };
        } 

        public ForecastData GetForecastRecords(string country, DateTime forecastDate, int interval, int horizon )
        {
            IQueryable<PowerDataBase> query;

            if (interval == 60)
            {
                query = _context.PowerDataHour;
            }
            else {
                query = _context.PowerDataQuarter;
            }

            var query1 = query.Where(p => p.Timestamp < forecastDate)
                                .OrderByDescending(p => p.Timestamp)
                                .Take(interval == 60 ? (MaxLag+horizon) : (MaxLag+horizon)*4);

            var finalQuery = query1
                                  .OrderBy(p => p.Timestamp)
                                  .Select(p => new LoadMeasurement {
                                    Timestamp = p.Timestamp,
                                    LoadValue = EF.Property<double?>(p, country + "LoadValue")
                                    }
                                  );

            var dbResults = finalQuery.ToList();

            var timestamps = dbResults
                            .Select(p => p.Timestamp.ToString("yyyy-M-d HH:mm") ?? "")
                            .ToArray();

            var loadValues = dbResults
                            .Select(p => p.LoadValue)
                            .ToArray();

            var lastLoadValues = loadValues.TakeLast(interval == 60 ? horizon : horizon * 4).ToArray();
            var (histLabels, _) = CreateHistogramLabels(lastLoadValues);

            return new ForecastData { 
                Timestamp = timestamps, 
                Load = loadValues, 
                HistLabels = histLabels 
            };
        }

    }
}