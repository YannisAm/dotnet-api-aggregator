namespace AgileActors.API.DTOs
{
    public class ApiStats
    {
        public int TotalRequests { get; set; }
        public double TotalResponseTime { get; set; }
        public int FastRequests { get; set; }
        public int AverageRequests { get; set; }
        public int SlowRequests { get; set; }
    }
}