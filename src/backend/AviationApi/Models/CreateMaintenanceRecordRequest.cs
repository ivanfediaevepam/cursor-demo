namespace AviationApi.Models;

public class CreateMaintenanceRecordRequest
{
    public DateTime PerformedAt { get; set; }

    public MaintenanceKind Kind { get; set; }

    public string Summary { get; set; }

    public string Details { get; set; }

    public string PerformedBy { get; set; }

    public string WorkOrderReference { get; set; }
}
