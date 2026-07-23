using PowerService.Data;
using PowerService.Data.Entities;
using PowerService.DTOs.Front;
using PowerService.DTOs.Projections;
using PowerService.Services.Helpers;
using PowerService.Requests;
using Npgsql;
using Microsoft.EntityFrameworkCore;

namespace PowerService.Services
{

    public class FrontDataService : BaseService<FrontDataService>
    {
        private readonly PowerDataContext _context;

        public FrontDataService(ILogger<FrontDataService> logger, PowerDataContext context) : base(logger)
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
                            .Take(interval == 60 ? count : count * 4)
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

            var (histLabels, histValues) = CreateHistogramLabels(loadValues);
            var rampValues = ServiceHelpers.CreateRampData(loadValues);

            return new LatestData{ 
                Labels = timestamps, 
                LoadValues = loadValues,
                RampValues = rampValues, 
                HistValues = histValues, 
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

            var dbResults = query
                            .Where(p => p.Timestamp >= startDate && p.Timestamp < endDate)
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

            var (histLabels, histValues) = CreateHistogramLabels(loadValues);
            var rampValues = ServiceHelpers.CreateRampData(loadValues);

            return new HistoricData { 
                Labels = timestamps, 
                LoadValues = loadValues,
                RampValues = rampValues, 
                HistValues = histValues, 
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


            DateTime startDate = forecastDate.AddHours(-1 * horizon);

            var dbResults = query
                            .Where(p => p.Timestamp >= startDate && p.Timestamp < forecastDate)
                            .OrderBy(p => p.Timestamp)
                            .Select(p => new LoadMeasurement {
                                Timestamp = p.Timestamp,
                                LoadValue = EF.Property<double?>(p, country + "LoadValue")
                                }
                            )
                            .ToList();

            var timestamps = dbResults
                            .Select(p => p.Timestamp.ToString("yyyy-M-d HH:mm"))
                            .ToArray();

            var loadValues = dbResults
                            .Take(interval == 60 ? horizon : horizon*4)
                            .Select(p => p.LoadValue)
                            .ToArray();

            var (histLabels, histValues) = CreateHistogramLabels(loadValues);
            var rampValues = ServiceHelpers.CreateRampData(loadValues);

            return new ForecastData { 
                LoadValues = loadValues,
                RampValues = rampValues, 
                HistValues = histValues, 
                HistLabels = histLabels 
            };
        }

        public TransmissionStatus GetTransmissionStatus(DateTime date, int interval)
        {

            var startDate = date;
            var endDate = date.AddDays(1);

            if (interval == 60)
            {
                IQueryable<PowerDataHour> query = _context.PowerDataHour;
                var dbResults = query
                            .Where(p => p.Timestamp >= startDate && p.Timestamp < endDate)
                            .OrderBy(p => p.Timestamp)
                            .Select(p => new TransmissionStatusProjection {
                                Timestamp = p.Timestamp,
                                Loads = ServiceHelpers.BuildHourCountryLoads(p)
                            })
                            .ToList();

                var result = new TransmissionStatus
                {
                    Data = allowedCountriesHour.ToDictionary(
                        country => country,
                        country => dbResults.Select(x => new LoadMeasurement
                        {
                            Timestamp = x.Timestamp,
                            LoadValue = x.Loads[country]
                        }).ToList()
                    )
                };

                return result;
            }
            else {
                IQueryable<PowerDataQuarter> query = _context.PowerDataQuarter;
                var dbResults = query
                            .Where(p => p.Timestamp >= startDate && p.Timestamp < endDate)
                            .OrderBy(p => p.Timestamp)
                            .Select(p => new TransmissionStatusProjection {
                                Timestamp = p.Timestamp,
                                Loads = ServiceHelpers.BuildQuarterCountryLoads(p)
                                })
                            .ToList();

                var result = new TransmissionStatus
                {
                    Data = allowedCountriesQuarter.ToDictionary(
                        country => country,
                        country => dbResults.Select(x => new LoadMeasurement
                        {
                            Timestamp = x.Timestamp,
                            LoadValue = x.Loads[country]
                        }).ToList()
                    )
                };
                
                return result;
            }

        }

        public async Task<DbStatus> GetDatabaseStatus()
        {
            var connectionString = _context.Database.GetConnectionString();
            
            var status = new DbStatus();

            try
            {
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                var hourStats = await ServiceHelpers.GetTableStatsAsync(connection, "\"PowerDataHour\"");
                if (hourStats.HasValue)
                {
                    status.IsOnline = true;
                    status.LastDataTimeHour = hourStats.Value.LastTime;
                    status.TotalRecordsHour = hourStats.Value.Count;
                }

                var quarterStats = await ServiceHelpers.GetTableStatsAsync(connection, "\"PowerDataQuarter\"");
                if (quarterStats.HasValue)
                {
                    status.IsOnline = true;
                    status.LastDataTimeQuarter = quarterStats.Value.LastTime;
                    status.TotalRecordsQuarter = quarterStats.Value.Count;
                }

                if (status.IsOnline)
                {
                    status.SizeHour = await ServiceHelpers.GetTableSizeAsync(connection, "\"PowerDataHour\"");
                    status.SizeQuarter = await ServiceHelpers.GetTableSizeAsync(connection, "\"PowerDataQuarter\"");
                    status.SizeDatabase = await ServiceHelpers.GetDatabaseSizeAsync(connection);
                }
            }
            catch(Exception)
            {
                return status;
            }

            return status;
        }

        public async Task<ExportResult> ExportTableData(ExportRequest request)
        {
            var stream = request.ExportFormat == "csv"
                    ? await ServiceHelpers.ExportCsv(request)
                    : ServiceHelpers.ExportExcel(request);

            var contentType = request.ExportFormat switch
            {
                "csv"  => "text/csv",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => throw new InvalidOperationException()
            };

            return new ExportResult
            {
                Stream = stream,
                ContentType = contentType,
            };

        }  

    }
}