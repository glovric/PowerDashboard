using PowerService.Data;
using PowerService.Models;
using Npgsql;
using CsvHelper;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace PowerService.Services
{

    public class FrontDataService : BaseService<FrontDataService>
    {
        private readonly PowerDataContext _context;

        public FrontDataService(ILogger<FrontDataService> logger, PowerDataContext context) : base(logger)
        {
            _context = context;
        }

        public object? GetLastRecords(string country, int count, int interval)
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
                            .Select(p => new {
                                Timestamp = p.Timestamp,
                                LoadValue = EF.Property<double?>(p, country + "LoadValue")
                                }
                            )
                            .ToList();

            var timestamps = dbResults
                            .Select(p => p.Timestamp?.ToString("yyyy-M-d HH:mm") ?? "")
                            .ToArray();

            var loadValues = dbResults
                            .Select(p => p.LoadValue)
                            .ToArray();

            var (histLabels, histValues) = CreateHistogramLabels(loadValues);
            var rampValues = CreateRampData(loadValues);

            return new { labels = timestamps, loadValues, rampValues, histValues, histLabels };
        }

        public object? GetHistoricRecords(string country, DateTime startDate, DateTime endDate, int interval)
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
                            .Select(p => new {
                                Timestamp = p.Timestamp,
                                LoadValue = EF.Property<double?>(p, country + "LoadValue")
                                }
                            )
                            .ToList();

            var timestamps = dbResults
                            .Select(p => p.Timestamp?.ToString("yyyy-M-d HH:mm") ?? "")
                            .ToArray();

            var loadValues = dbResults
                            .Select(p => p.LoadValue)
                            .ToArray();

            var (histLabels, histValues) = CreateHistogramLabels(loadValues);
            var rampValues = CreateRampData(loadValues);

            return new { labels = timestamps, loadValues, rampValues, histValues, histLabels };
        } 

        public object? GetForecastRecords(string country, DateTime forecastDate, int interval, int horizon )
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
                            .Select(p => new {
                                Timestamp = p.Timestamp,
                                LoadValue = EF.Property<double?>(p, country + "LoadValue")
                                }
                            )
                            .ToList();

            var timestamps = dbResults
                            .Select(p => p.Timestamp?.ToString("yyyy-M-d HH:mm") ?? "")
                            .ToArray();

            var loadValues = dbResults
                            .Take(interval == 60 ? horizon : horizon*4)
                            .Select(p => p.LoadValue)
                            .ToArray();

            var (histLabels, histValues) = CreateHistogramLabels(loadValues);
            var rampValues = CreateRampData(loadValues);

            return new { loadValues, rampValues, histValues, histLabels };
        }

        public double?[] CreateRampData(double?[] loadValues)
        {
            var rampData = new List<double?>();

            for(int i = 1; i < loadValues.Length; i++)
            {
                if(loadValues[i] == null || loadValues[i - 1] == null)
                {
                    rampData.Add(null);
                }
                else {
                    rampData.Add(loadValues[i] - loadValues[i - 1]);
                }
            }

            return rampData.ToArray();
        }

        public async Task<(string LastTime, long Count)?> GetTableStatsAsync(NpgsqlConnection conn, string tableName)
        {
            const string sql = @"
                SELECT 
                    TO_CHAR(MAX(""timestamp""), 'YYYY-MM-DD HH24:MI:SS') AS last_time,
                    COUNT(*) AS total_count
                FROM {0};";

            var query = string.Format(sql, tableName);

            await using var cmd = new NpgsqlCommand(query, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var lastTime = reader.IsDBNull(0) ? "No Data" : reader.GetString(0);
                var count = reader.IsDBNull(1) ? 0L : reader.GetInt64(1);
                
                return (lastTime, count);
            }

            return null;
        }

        public async Task<long> GetTableSizeAsync(NpgsqlConnection conn, string tableName)
        {
            // pg_total_relation_size includes table data, indexes, and toast tables
            var sql = $"SELECT pg_total_relation_size('{tableName}');";
            
            await using var cmd = new NpgsqlCommand(sql, conn);
            var result = await cmd.ExecuteScalarAsync();

            if (result == null || result == DBNull.Value) return 0;

            long bytes = Convert.ToInt64(result);
            return bytes;
        }

        public async Task<long> GetDatabaseSizeAsync(NpgsqlConnection conn)
        {
            const string sql = "SELECT pg_database_size(current_database());";

            await using var cmd = new NpgsqlCommand(sql, conn);
            var result = await cmd.ExecuteScalarAsync();

            if (result == null || result == DBNull.Value) return 0;

            long bytes = Convert.ToInt64(result);
            return bytes;
        }

        public async Task<DbStatus> GetDatabaseStatus()
        {
            var connectionString = _context.Database.GetConnectionString();
            
            var status = new DbStatus();

            try
            {
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                var hourStats = await GetTableStatsAsync(connection, "\"PowerDataHour\"");
                if (hourStats.HasValue)
                {
                    status.IsOnline = true;
                    status.LastDataTimeHour = hourStats.Value.LastTime;
                    status.TotalRecordsHour = hourStats.Value.Count;
                }

                var quarterStats = await GetTableStatsAsync(connection, "\"PowerDataQuarter\"");
                if (quarterStats.HasValue)
                {
                    status.IsOnline = true;
                    status.LastDataTimeQuarter = quarterStats.Value.LastTime;
                    status.TotalRecordsQuarter = quarterStats.Value.Count;
                }

                if (status.IsOnline)
                {
                    status.SizeHour = await GetTableSizeAsync(connection, "\"PowerDataHour\"");
                    status.SizeQuarter = await GetTableSizeAsync(connection, "\"PowerDataQuarter\"");
                    status.SizeDatabase = await GetDatabaseSizeAsync(connection);
                }
            }
            catch(Exception)
            {
                return status;
            }

            return status;
        }

        private MemoryStream ExportExcel(ExportRequest request)
        {
            var memoryStream = new MemoryStream();

            // Create a new workbook
            using (var workbook = new XLWorkbook())
            {
                // Add a worksheet
                var worksheet = workbook.Worksheets.Add("Sheet1");

                // Write Headers
                for (int i = 0; i < request.Headers.Count; i++)
                {
                    worksheet.Cell(1, i + 1).Value = request.Headers[i];
                }

                // Write Rows
                for (int rowIndex = 0; rowIndex < request.Rows.Count; rowIndex++)
                {
                    var row = request.Rows[rowIndex];
                    for (int colIndex = 0; colIndex < request.ColumnKeys.Count; colIndex++)
                    {
                        var key = request.ColumnKeys[colIndex];
                        var value = row.ContainsKey(key) ? row[key]?.ToString() : string.Empty;
                        worksheet.Cell(rowIndex + 2, colIndex + 1).Value = value;
                    }
                }

                // Save to MemoryStream
                workbook.SaveAs(memoryStream);
            }

            // Reset stream position so it can be read by controller
            memoryStream.Position = 0;

            return memoryStream;
        }

        private async Task<MemoryStream> ExportCsv(ExportRequest request)
        {
            var memoryStream = new MemoryStream();
        
            // Use 'using' to ensure resources are disposed properly, 
            // but keep the stream open for the controller to read.
            using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // Write Headers
                foreach (var header in request.Headers)
                {
                    csv.WriteField(header);
                }
                csv.NextRecord();

                // Write Rows
                foreach (var row in request.Rows)
                {
                    foreach (var key in request.ColumnKeys)
                    {
                        // Safely get value, handle nulls
                        var value = row.ContainsKey(key) ? row[key]?.ToString() : string.Empty;
                        csv.WriteField(value);
                    }
                    csv.NextRecord();
                }
                
                // Flush the CSV writer to ensure all data is written to the StreamWriter
                await csv.FlushAsync();
                
                // Flush the StreamWriter to ensure all data is written to the MemoryStream
                await writer.FlushAsync();
            }

            // Reset position to the beginning so the Controller can read from start
            memoryStream.Position = 0;

            return memoryStream;
        }

        public async Task<MemoryStream> ExportTableData(ExportRequest request)
        {
            return request.ExportFormat == "csv"
                    ? await ExportCsv(request)
                    : ExportExcel(request);
        }

        public List<TransmissionResponse> GetTransmissionStatus(DateTime date, int interval)
        {

            var startDate = date;
            var endDate = date.AddDays(1);

            if (interval == 60)
            {
                IQueryable<PowerDataHour> query = _context.PowerDataHour;
                var dbResults = query
                            .Where(p => p.Timestamp >= startDate && p.Timestamp < endDate)
                            .OrderBy(p => p.Timestamp)
                            .Select(p => new TransmissionResponse {
                                Timestamp = p.Timestamp.ToString(),
                                Loads = new Dictionary<string, double?> {
                                    { "AT", p.ATLoadValue },
                                    { "BE", p.BELoadValue },
                                    { "BG", p.BGLoadValue },
                                    { "CH", p.CHLoadValue },
                                    { "CY", p.CYLoadValue },
                                    { "CZ", p.CZLoadValue },
                                    { "DE", p.DELoadValue },
                                    { "DK", p.DKLoadValue },
                                    { "EE", p.EELoadValue },
                                    { "ES", p.ESLoadValue },
                                    { "FI", p.FILoadValue },
                                    { "FR", p.FRLoadValue },
                                    { "GB", p.GBLoadValue },
                                    { "GR", p.GRLoadValue },
                                    { "HR", p.HRLoadValue },
                                    { "HU", p.HULoadValue },
                                    { "IE", p.IELoadValue },
                                    { "IT", p.ITLoadValue },
                                    { "LT", p.LTLoadValue },
                                    { "LU", p.LULoadValue },
                                    { "LV", p.LVLoadValue },
                                    { "ME", p.MELoadValue },
                                    { "NL", p.NLLoadValue },
                                    { "NO", p.NOLoadValue },
                                    { "PL", p.PLLoadValue },
                                    { "PT", p.PTLoadValue },
                                    { "RO", p.ROLoadValue },
                                    { "RS", p.RSLoadValue },
                                    { "SE", p.SELoadValue },
                                    { "SI", p.SILoadValue },
                                    { "SK", p.SKLoadValue },
                                    { "UA", p.UALoadValue }
                                    }
                                })
                            .ToList();
                Console.WriteLine($"Kolicina dbresults: {dbResults.Count}");
                return dbResults;
            }
            else {
                IQueryable<PowerDataQuarter> query = _context.PowerDataQuarter;
                var dbResults = query
                            .Where(p => p.Timestamp >= startDate && p.Timestamp < endDate)
                            .OrderBy(p => p.Timestamp)
                            .Select(p => new TransmissionResponse {
                                Timestamp = p.Timestamp.ToString(),
                                Loads = new Dictionary<string, double?> {
                                    { "AT", p.ATLoadValue },
                                    { "BE", p.BELoadValue },
                                    { "DE", p.DELoadValue },
                                    { "HU", p.HULoadValue },
                                    { "LU", p.LULoadValue },
                                    { "NL", p.NLLoadValue },
                                    }
                                })
                            .ToList();
                Console.WriteLine($"Kolicina dbresults: {dbResults.Count}");
                return dbResults;
            }

        }        

    }
}