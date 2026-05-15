using System.Globalization;

namespace AgileActors.API.DTOs
{
    public class AggregateQueryParameters
    {
        public string? NewsKeyword { get; set; } = "technology";
        public string? Category { get; set; }
        public string? Language { get; set; } = "en";
        public int PageSize { get; set; } = 10;
        public int Page { get; set; } = 1;

        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public string Units { get; set; } = "metric";

        public string? BaseCurrency { get; set; } = "EUR";

        public string? SortBy { get; set; }
        public string SortOrder { get; set; } = "desc";

        public DateOnly? FromDate { get; set; }

        public string FromDateFormatted => FromDate?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
        public string LatString => Lat?.ToString(CultureInfo.InvariantCulture) ?? "37.98";
        public string LonString => Lon?.ToString(CultureInfo.InvariantCulture) ?? "23.72";
    }
}