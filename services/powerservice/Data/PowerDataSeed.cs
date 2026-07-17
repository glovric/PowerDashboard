using Microsoft.EntityFrameworkCore;
using Npgsql;
using PowerService.Models;
using PowerService.Utils;

namespace PowerService.Data
{
    
    public class DbSeeder(
        ILogger<DbSeeder> logger, 
        IConfiguration configuration,
        PowerDataContext context)
    {
        public async Task SeedPowerDataAsync()
        {

            DataFilesOptions dataPaths = configuration.GetSection("DataFiles").Get<DataFilesOptions>()!;

            bool hasDataHour = await context.Set<PowerDataHour>().AnyAsync();
            bool hasDataQuarter = await context.Set<PowerDataQuarter>().AnyAsync();

            string copyCommandHour = $@"
                    COPY ""PowerDataHour"" (""timestamp"", ""at_load_value"", ""be_load_value"", ""bg_load_value"", ""ch_load_value"", ""cy_load_value"", ""cz_load_value"", ""de_load_value"", ""dk_load_value"", ""ee_load_value"", ""es_load_value"", ""fi_load_value"", ""fr_load_value"", ""gb_load_value"", ""gr_load_value"", ""hr_load_value"", ""hu_load_value"", ""ie_load_value"", ""it_load_value"", ""lt_load_value"", ""lu_load_value"", ""lv_load_value"", ""me_load_value"", ""nl_load_value"", ""no_load_value"", ""pl_load_value"", ""pt_load_value"", ""ro_load_value"", ""rs_load_value"", ""se_load_value"", ""si_load_value"", ""sk_load_value"", ""ua_load_value"")  
                    FROM STDIN 
                    WITH (FORMAT csv, HEADER true);";
            
            string copyCommandQuarter = $@"
                    COPY ""PowerDataQuarter"" (""timestamp"", ""at_load_value"", ""be_load_value"", ""de_load_value"", ""hu_load_value"", ""lu_load_value"", ""nl_load_value"")  
                    FROM STDIN 
                    WITH (FORMAT csv, HEADER true);";

            if (hasDataHour && hasDataQuarter)
            {
                logger.LogInformation("[Database Seeding] Seeding skipped: Data already exists in both 'PowerData' tables.");
                return;
            }

            logger.LogInformation("[Database Seeding] Tables are empty. Starting seed process...");

            var connectionString = context.Database.GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
                throw new Exception("[Database Seeding] Connection string not found.");

            if(!hasDataHour)
            {
                await SeedViaCopyAsync(connectionString, dataPaths.TimeSeries60, "PowerDataHour", copyCommandHour);
            }

            if(!hasDataQuarter)
            {
                await SeedViaCopyAsync(connectionString, dataPaths.TimeSeries15, "PowerDataQuarter", copyCommandQuarter);
            }
        
        }

        private async Task SeedViaCopyAsync(string connectionString, string csvFilePath, string tableName, string copyCommand)
        {
            if (!File.Exists(csvFilePath))
                throw new FileNotFoundException($"CSV file not found at {csvFilePath}");

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            await using var transaction = await conn.BeginTransactionAsync();

            try
            {

                await using (var importer = conn.BeginTextImport(copyCommand))
                {
                    using var fileStream = File.OpenRead(csvFilePath);
                    await fileStream.CopyToAsync(importer.BaseStream);     
                }

                await transaction.CommitAsync();
                logger.LogInformation("[Database Seeding] Seeding of {TableName} completed successfully.", tableName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[Database Seeding] Seeding failed");
                await transaction.RollbackAsync();
                throw;
            }
        }

    }

}