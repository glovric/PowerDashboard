using Microsoft.AspNetCore.Mvc;
using PowerService.DTOs.Inference;
using PowerService.Requests;
using PowerService.Services;
using Microsoft.AspNetCore.Authorization;
using Shared;

namespace PowerService.Controllers
{
    [ApiController]
    [Route("powerdata/inference")]
    public class InferenceDataController : BaseController<InferenceDataController>
    {
        private readonly InferenceDataService _service;

        public InferenceDataController(ILogger<InferenceDataController> logger, InferenceDataService service) : base(logger)
        {
            _service = service;
        }

        [HttpPost("latest")]
        [Authorize(Policy = AuthPolicies.Service)]
        public async Task<ActionResult<LatestData>> GetLastDbRowsInference([FromBody] LatestDataRequest request)
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

            var result = _service.GetLastRecords(request.Country.ToUpper(), request.Count, request.Interval);

            return Ok(result);
        }

        [HttpPost("history")]
        [Authorize(Policy = AuthPolicies.Service)]
        public async Task<ActionResult<HistoricData>> GetHistoricDbRowsInference([FromBody] HistoricDataRequest request)
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

            var result = _service.GetHistoricRecords(request.Country.ToUpper(), request.StartDate, request.EndDate, request.Interval);

            return Ok(result);
        }

        [HttpPost("forecast")]
        [Authorize(Policy = AuthPolicies.Service)]
        public async Task<ActionResult<ForecastData>> GetForecastRecords([FromBody] ForecastDataRequest request)
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

    }

}