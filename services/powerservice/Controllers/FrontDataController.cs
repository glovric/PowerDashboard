using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PowerService.Models;
using PowerService.Services;
using Shared;

namespace PowerService.Controllers
{
    [ApiController]
    [Route("powerdata/front")]
    public class FrontDataController : BaseController<FrontDataController>
    {
        private readonly FrontDataService _service;

        private static readonly HashSet<string> allowedExportFormats = new(StringComparer.OrdinalIgnoreCase) { "csv", "xlsx" };

        public FrontDataController(ILogger<FrontDataController> logger, FrontDataService service) : base(logger)
        {
            _service = service;
        }

        [HttpPost("latest")]
        [Authorize(Policy = AuthPolicies.FrontendPrivate)]
        public async Task<ActionResult<IEnumerable<PowerDataResponse>>> GetLatestRecords([FromBody] LatestDataRequest request)
        {
            if (request.Count <= 0 || request.Count > 1000)
            {
                return BadRequest("Count must be between 1 and 1000.");
            }

            if (!allowedCountriesHour.Contains(request.Country) && request.Interval == 60)
            {
                return BadRequest("Unsupported country code for hourly data.");
            }

            if (!allowedCountriesQuarter.Contains(request.Country) && request.Interval == 15)
            {
                return BadRequest("Unsupported country code for quarterly data.");
            }

            if (request.Interval <= 0)
            {
                return BadRequest("Interval must be a positive integer.");
            }

            var data = _service.GetLastRecords(request.Country.ToUpper(), request.Count, request.Interval);

            return Ok(data);

        }

        [HttpPost("history")]
        [Authorize(Policy = AuthPolicies.FrontendPrivate)]
        public async Task<ActionResult<IEnumerable<PowerDataResponse>>> GetHistoricRecords([FromBody] HistoricDataRequest request)
        {

            if (!allowedCountriesHour.Contains(request.Country) && request.Interval == 60)
            {
                return BadRequest("Unsupported country code for hourly data.");
            }

            if (!allowedCountriesQuarter.Contains(request.Country) && request.Interval == 15)
            {
                return BadRequest("Unsupported country code for quarterly data.");
            }

            if (request.Interval <= 0)
            {
                return BadRequest("Interval must be a positive integer.");
            }

            var data = _service.GetHistoricRecords(request.Country.ToUpper(), request.StartDate, request.EndDate, request.Interval);
            return Ok(data);

        }

        [HttpPost("forecast")]
        [Authorize(Policy = AuthPolicies.FrontendPrivate)]
        public async Task<ActionResult<IEnumerable<PowerDataResponse>>> GetForecastRecords([FromBody] ForecastDataRequest request)
        {
            if (!allowedCountriesHour.Contains(request.Country) && request.Interval == 60)
            {
                return BadRequest("Unsupported country code for hourly data.");
            }

            if (!allowedCountriesQuarter.Contains(request.Country) && request.Interval == 15)
            {
                return BadRequest("Unsupported country code for quarterly data.");
            }

            if (request.Interval <= 0)
            {
                return BadRequest("Interval must be a positive integer.");
            }

            if (request.Horizon <= 0)
            {
                return BadRequest("Interval must be a positive integer.");
            }

            var data = _service.GetForecastRecords(request.Country.ToUpper(), request.ForecastDate, request.Interval, request.Horizon);
            return Ok(data);
        }

        [HttpGet("db_status")]
        [Authorize(Policy = AuthPolicies.FrontendPrivate)]
        public async Task<ActionResult<DbStatus>> GetDatabaseStatus()
        {
            var status = await _service.GetDatabaseStatus();
            return Ok(status);
        }

        [HttpPost("transmission_status")]
        //[Authorize(Policy = AuthPolicies.FrontendPrivate)]
        public async Task<ActionResult<PowerDataBase>> GetTransmissionStatus([FromBody] TransmissionRequest request)
        {
            var status = _service.GetTransmissionStatus(request.Date, request.Interval);
            return Ok(status);
        }

        [HttpPost("export")]
        [Authorize(Policy = AuthPolicies.FrontendPrivate)]
        public async Task<IActionResult> ExportTableData([FromBody] ExportRequest request)
        {
            if (request == null || request.Rows == null || request.Rows.Count == 0)
                return BadRequest("No data to export.");

            if (!allowedExportFormats.Contains(request.ExportFormat))
            {
                return BadRequest("Unsupported export format.");
            }

            try
            {
                var stream = await _service.ExportTableData(request);

                var contentType = request.ExportFormat switch
                {
                    "csv"  => "text/csv",
                    "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    _ => throw new InvalidOperationException()
                };

                return File(stream, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting table data.");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An error occurred while generating the export file.");
            }
        }

        [HttpGet("health")]
        public async Task<IActionResult> GetServiceHealth()
        {
            return NoContent();
        }

    }

}