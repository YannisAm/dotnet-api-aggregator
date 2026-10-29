using ApiAggregator.API.DTOs;
using ApiAggregator.API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiAggregator.API.Controllers
{
    /// <summary>
    /// Provides endpoints for aggregating external data and monitoring API performance.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AggregateDataController : ControllerBase
    {
        private readonly IAggregationService _aggregationService;
        private readonly IStatisticsService _statsService;
        private readonly ILogger<AggregateDataController> _logger;

        public AggregateDataController(
            IAggregationService aggregationService,
            IStatisticsService statsService,
            ILogger<AggregateDataController> logger)
        {
            _aggregationService = aggregationService;
            _statsService = statsService;
            _logger = logger;
        }

        /// <summary>
        /// Fetches and aggregates data from Weather, News, and Exchange Rate APIs simultaneously.
        /// </summary>
        /// <remarks>
        /// Calls all sources in parallel. Successful responses are cached for up to 5 minutes. Rate limited to 10
        /// requests per minute.
        /// </remarks>
        /// <param name="parameters">The query parameters including city, news keywords, and base currency.</param>
        /// <returns>An aggregated object containing results from all external sources.</returns>
        /// <response code="200">
        /// Returns the aggregated data successfully.
        /// </response>
        /// <response code="400">
        /// If the provided parameters are invalid.
        /// </response>
        /// <response code="429">
        /// If the rate limit is exceeded. See the Retry-After header.
        /// </response>
        /// <response code="500">
        /// If an internal error occurs during aggregation.
        /// </response>
        [HttpGet]
        [EnableRateLimiting("fixed")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetData([FromQuery] AggregateQueryParameters parameters)
        {
            var result = await _aggregationService.GetEverythingAsync(parameters);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves real-time performance metrics for all integrated external APIs.
        /// </summary>
        /// <remarks>
        /// Returns in-memory data per API source: total requests, average latency in milliseconds, and performance
        /// buckets (Fast: under 100ms, Average: 100-200ms, Slow: over 200ms).
        /// </remarks>
        /// <response code="200">
        /// Returns a dictionary where the key is the API source name.
        /// </response>
        /// <response code="204">
        /// If no requests have been processed since the application started.
        /// </response>
        /// <response code="500">
        /// If an internal error occurs during statistics calculation.
        /// </response>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(IDictionary<string, ApiStatsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetApiStatistics()
        {
            try
            {
                var stats = _statsService.GetStats();

                if (stats.Count == 0)
                    return NoContent();

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving API statistics.");
                return StatusCode(500, "Error retrieving statistics.");
            }
        }
    }
}