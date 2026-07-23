using PowerService.Data.Entities;
using PowerService.Requests;
using Npgsql;
using CsvHelper;
using ClosedXML.Excel;
using System.Globalization;

namespace PowerService.Services.Helpers
{

    public static class ServiceHelpers
    {
        
        public static double?[] CreateRampData(double?[] loadValues)
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

        public static async Task<(string LastTime, long Count)?> GetTableStatsAsync(NpgsqlConnection conn, string tableName)
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

        public static async Task<long> GetTableSizeAsync(NpgsqlConnection conn, string tableName)
        {
            // pg_total_relation_size includes table data, indexes, and toast tables
            var sql = $"SELECT pg_total_relation_size('{tableName}');";
            
            await using var cmd = new NpgsqlCommand(sql, conn);
            var result = await cmd.ExecuteScalarAsync();

            if (result == null || result == DBNull.Value) return 0;

            long bytes = Convert.ToInt64(result);
            return bytes;
        }

        public static async Task<long> GetDatabaseSizeAsync(NpgsqlConnection conn)
        {
            const string sql = "SELECT pg_database_size(current_database());";

            await using var cmd = new NpgsqlCommand(sql, conn);
            var result = await cmd.ExecuteScalarAsync();

            if (result == null || result == DBNull.Value) return 0;

            long bytes = Convert.ToInt64(result);
            return bytes;
        }

        public static MemoryStream ExportExcel(ExportRequest request)
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

        public static async Task<MemoryStream> ExportCsv(ExportRequest request)
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

        public static Dictionary<string, double?> BuildHourCountryLoads(PowerDataHour p)
        {
            return new Dictionary<string, double?> {
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
            };
        }

        public static Dictionary<string, double?> BuildQuarterCountryLoads(PowerDataQuarter p)
        {
            return new Dictionary<string, double?> {
                { "AT", p.ATLoadValue },
                { "BE", p.BELoadValue },
                { "DE", p.DELoadValue },
                { "HU", p.HULoadValue },
                { "LU", p.LULoadValue },
                { "NL", p.NLLoadValue },
            };
        }

    }
}