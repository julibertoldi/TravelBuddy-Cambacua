using System;

namespace TravelBuddy.Statistics;

public class AuditLogExceptionDto
{
    public DateTime ExecutionTime { get; set; }
    public string? HttpMethod { get; set; }
    public string? Url { get; set; }
    public int? HttpStatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
}
