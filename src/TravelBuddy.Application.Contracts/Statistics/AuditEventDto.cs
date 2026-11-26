using System;

namespace TravelBuddy.Statistics;

public class AuditEventDto
{
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime ExecutionTime { get; set; }
    public string? ClientIpAddress { get; set; }
    public string Result { get; set; } = string.Empty;
}
