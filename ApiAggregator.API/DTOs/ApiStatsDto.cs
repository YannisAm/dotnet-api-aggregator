namespace ApiAggregator.API.DTOs
{
    public class ApiStatsDto
    {
        public int TotalRequests { get; set; }
        public double AverageResponseTime { get; set; }
        public PerformanceBucketsDto PerformanceBuckets { get; set; } = new();
    }
}