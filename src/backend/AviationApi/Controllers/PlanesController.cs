using AviationApi.Models;
using AviationApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AviationApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PlanesController : ControllerBase
{
    private const int MaintenanceSummaryMaxLength = 500;

    private readonly ILogger<PlanesController> _logger;
    private readonly IPlaneRepository _planeRepository;

    public PlanesController(ILogger<PlanesController> logger, IPlaneRepository planeRepository)
    {
        _logger = logger;
        _planeRepository = planeRepository;
    }

    [HttpGet]
    public ActionResult<List<Plane>> GetAll()
    {
        _logger.LogInformation("GET all ✈✈✈ NO PARAMS ✈✈✈");

        return Ok(_planeRepository.GetAllPlanes());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Plane> GetById(int id)
    {
        var plane = _planeRepository.GetPlaneById(id);

        if (plane == null)
        {
            return NotFound();
        }

        return Ok(plane);
    }

    [HttpPost]
    public ActionResult<Plane> Post([FromBody] Plane plane)
    {
        if (plane == null)
        {
            return BadRequest();
        }

        _planeRepository.AddPlane(plane);

        return CreatedAtAction(nameof(GetById), new { id = plane.Id }, plane);
    }

    [HttpGet("{planeId:int}/maintenance")]
    public ActionResult<List<MaintenanceRecord>> GetMaintenanceRecords(int planeId)
    {
        if (_planeRepository.GetPlaneById(planeId) == null)
        {
            return NotFound();
        }

        var records = _planeRepository.GetMaintenanceRecordsForPlane(planeId);
        return Ok(records.ToList());
    }

    [HttpGet("{planeId:int}/maintenance/{recordId:int}")]
    public ActionResult<MaintenanceRecord> GetMaintenanceRecord(int planeId, int recordId)
    {
        if (_planeRepository.GetPlaneById(planeId) == null)
        {
            return NotFound();
        }

        var record = _planeRepository.GetMaintenanceRecord(planeId, recordId);
        if (record == null)
        {
            return NotFound();
        }

        return Ok(record);
    }

    [HttpPost("{planeId:int}/maintenance")]
    public ActionResult<MaintenanceRecord> PostMaintenanceRecord(int planeId, [FromBody] CreateMaintenanceRecordRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var summary = request.Summary?.Trim() ?? string.Empty;
        if (summary.Length == 0)
        {
            return BadRequest("Summary is required.");
        }

        if (summary.Length > MaintenanceSummaryMaxLength)
        {
            return BadRequest($"Summary must be at most {MaintenanceSummaryMaxLength} characters.");
        }

        if (_planeRepository.GetPlaneById(planeId) == null)
        {
            return NotFound();
        }

        var record = new MaintenanceRecord
        {
            PerformedAt = request.PerformedAt,
            Kind = request.Kind,
            Summary = summary,
            Details = request.Details?.Trim() ?? string.Empty,
            PerformedBy = request.PerformedBy?.Trim() ?? string.Empty,
            WorkOrderReference = request.WorkOrderReference?.Trim() ?? string.Empty,
        };

        _planeRepository.AddMaintenanceRecord(planeId, record);

        return CreatedAtAction(nameof(GetMaintenanceRecord), new { planeId, recordId = record.Id }, record);
    }
}
