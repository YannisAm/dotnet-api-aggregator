using AgileActors.API.DTOs;
using AgileActors.API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AgileActors.API.Controllers
{
    // <summary>
    /// Provides endpoints for aggregating external data and monitoring API performance.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AggregateDataController : ControllerBase
    {
        private readonly IAggregationService _aggregationService;
        private readonly IStatisticsService _statsService;

        public AggregateDataController(IAggregationService aggregationService, IStatisticsService statsService)
        {
            _aggregationService = aggregationService;
            _statsService = statsService;
        }

        /// <summary>
        /// Fetches and aggregates data from Weather, News, and Exchange Rate APIs simultaneously.
        /// </summary>
        /// <remarks>
        /// This endpoint utilizes parallelism to minimize response time and implements caching for 5 minutes.
        /// </remarks>
        /// <param name="parameters">The query parameters including city, news keywords, and base currency.</param>
        /// <returns>An aggregated object containing results from all three external sources.</returns>
        /// <response code="200">
        /// Returns the aggregated data successfully.
        /// </response>
        /// <response code="400">
        /// If the provided parameters are invalid.
        /// </response>
        /// <response code="500">
        /// If an internal error occurs during aggregation.
        /// </response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetData([FromQuery] AggregateQueryParameters parameters)
        {
            var result = await _aggregationService.GetEverythingAsync(parameters);

            if (result == null)
                return StatusCode(500, "An error occurred while aggregating data.");

            return Ok(result);
        }

        /// <summary>
        /// Retrieves real-time performance metrics for all integrated external APIs.
        /// </summary>
        /// <remarks>
        /// This endpoint returns data stored in-memory, including: - **Total Requests**: Count of successful and failed
        /// attempts per API. - **Average Latency**: Mean response time in milliseconds. - **Performance Buckets**:
        /// Categorization based on speed: - Fast: less than 100ms - Average: 100ms - 200ms - Slow: more than 200ms
        /// </remarks>
        /// <response code="200">
        /// Returns a dictionary where the key is the API Source name.
        /// </response>
        /// <response code="204">
        /// If no requests have been processed since the application started.
        /// </response>
        /// <response code="500">
        /// If an internal error occurs during statistics calculation.
        /// </response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ApiStatsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("stats")]
        [EnableRateLimiting("fixed")]
        public IActionResult GetApiStatistics()
        {
            try
            {
                var stats = _statsService.GetStats();

                if (stats == null || !stats.Any())
                {
                    return NoContent();
                }

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error retrieving statistics.");
            }
        }
    }
}