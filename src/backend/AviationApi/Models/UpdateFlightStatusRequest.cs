namespace AviationApi.Models;

public class UpdateFlightStatusRequest
{
    public FlightStatus Status { get; set; }

    /// <summary>
    /// Required (non-empty, allowed vocabulary) when <see cref="Status"/> is <see cref="FlightStatus.Delayed"/>; ignored otherwise.
    /// </summary>
    public string DelayReason { get; set; }
}
