using AviationApi.Controllers;
using AviationApi.Models;
using AviationApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AviationApi.Tests.Controllers;

public class PlanesControllerTests
{
    private readonly ILogger<PlanesController> _logger;
    private readonly IPlaneRepository _planeRepository;
    private readonly PlanesController _planesController;

    public PlanesControllerTests()
    {
        _logger = Substitute.For<ILogger<PlanesController>>();
        _planeRepository = new PlaneRepository();
        _planesController = new PlanesController(_logger, _planeRepository);
    }

    [Fact]
    public void GetAll_ReturnsListOfPlanes()
    {
        var result = _planesController.GetAll();

        var okObjectResult = (OkObjectResult)result.Result!;
        var returnedPlanes = (List<Plane>)okObjectResult.Value!;
        returnedPlanes.Should().NotBeEmpty();
        returnedPlanes.Should().HaveCount(21);
    }

    [Fact]
    public void GetMaintenanceRecords_WhenPlaneMissing_ReturnsNotFound()
    {
        var result = _planesController.GetMaintenanceRecords(9999);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void GetMaintenanceRecords_WhenPlaneExists_ReturnsOkAndEmptyListInitially()
    {
        var result = _planesController.GetMaintenanceRecords(1);

        var ok = (OkObjectResult)result.Result!;
        var list = (List<MaintenanceRecord>)ok.Value!;
        list.Should().BeEmpty();
    }

    [Fact]
    public void PostMaintenanceRecord_WhenPlaneMissing_ReturnsNotFound()
    {
        var request = new CreateMaintenanceRecordRequest
        {
            PerformedAt = new DateTime(1910, 1, 1),
            Kind = MaintenanceKind.Inspection,
            Summary = "Annual inspection",
        };

        var result = _planesController.PostMaintenanceRecord(9999, request);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void PostMaintenanceRecord_WhenSummaryMissing_ReturnsBadRequest()
    {
        var request = new CreateMaintenanceRecordRequest
        {
            PerformedAt = new DateTime(1910, 1, 1),
            Kind = MaintenanceKind.Inspection,
            Summary = "   ",
        };

        var result = _planesController.PostMaintenanceRecord(1, request);

        var badRequest = (BadRequestObjectResult)result.Result!;
        badRequest.Value.Should().Be("Summary is required.");
    }

    [Fact]
    public void PostMaintenanceRecord_WhenValid_ReturnsCreatedWithRecord()
    {
        var request = new CreateMaintenanceRecordRequest
        {
            PerformedAt = new DateTime(1910, 6, 1),
            Kind = MaintenanceKind.ScheduledService,
            Summary = "Oil change and control cable check",
            Details = "No defects noted.",
            PerformedBy = "O. Wright",
            WorkOrderReference = "WO-1910-0042",
        };

        var result = _planesController.PostMaintenanceRecord(1, request);

        var created = (CreatedAtActionResult)result.Result!;
        created.ActionName.Should().Be(nameof(PlanesController.GetMaintenanceRecord));
        var record = (MaintenanceRecord)created.Value!;
        record.Id.Should().Be(1);
        record.PlaneId.Should().Be(1);
        record.Summary.Should().Be("Oil change and control cable check");
        record.Kind.Should().Be(MaintenanceKind.ScheduledService);
    }

    [Fact]
    public void GetMaintenanceRecord_AfterPost_ReturnsOk()
    {
        var request = new CreateMaintenanceRecordRequest
        {
            PerformedAt = new DateTime(1910, 7, 1),
            Kind = MaintenanceKind.Repair,
            Summary = "Replaced fabric on lower wing",
        };
        _planesController.PostMaintenanceRecord(2, request);

        var result = _planesController.GetMaintenanceRecord(2, 1);

        var ok = (OkObjectResult)result.Result!;
        var record = (MaintenanceRecord)ok.Value!;
        record.PlaneId.Should().Be(2);
        record.Summary.Should().Be("Replaced fabric on lower wing");
    }

    [Fact]
    public void GetMaintenanceRecord_WhenRecordMissing_ReturnsNotFound()
    {
        var result = _planesController.GetMaintenanceRecord(1, 99);

        result.Result.Should().BeOfType<NotFoundResult>();
    }
}
