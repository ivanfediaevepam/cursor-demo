namespace AviationApi.Models;

/// <summary>Allowed delay reason values for flights marked <see cref="FlightStatus.Delayed"/>.</summary>
public static class DelayReasonCatalog
{
    public static readonly IReadOnlyList<string> AllowedValues = new[]
    {
        "Weather",
        "Technical",
        "Operational",
    };

    public static bool IsAllowed(string value) => AllowedValues.Contains(value);
}
