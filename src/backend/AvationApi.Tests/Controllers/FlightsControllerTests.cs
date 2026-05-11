using AviationApi.Controllers;
using AviationApi.Models;
using AviationApi.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AviationApi.Tests.Controllers
{
    public class FlightsControllerTests
    {
        private readonly ILogger<FlightsController> _logger;
        private readonly IFlightRepository _flightRepository;
        private readonly FlightsController _sut;

        public FlightsControllerTests()
        {
            _logger = Substitute.For<ILogger<FlightsController>>();
            _flightRepository = Substitute.For<IFlightRepository>();
            _sut = new FlightsController(_logger, _flightRepository);
        }

        [Fact]
        public void Get_ReturnsOkWithFlightsFromRepository()
        {
            var flights = new List<Flight>
            {
                CreateFlight(id: 1, status: FlightStatus.Scheduled),
            };
            _flightRepository.GetAllFlights().Returns(flights);

            var result = _sut.Get();

            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(flights);
        }

        [Fact]
        public void GetById_WhenFlightExists_ReturnsOkWithFlight()
        {
            var flight = CreateFlight(id: 2, status: FlightStatus.Scheduled);
            _flightRepository.GetFlightById(2).Returns(flight);

            var result = _sut.GetById(2);

            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeSameAs(flight);
        }

        [Fact]
        public void GetById_WhenFlightMissing_ReturnsNotFound()
        {
            _flightRepository.GetFlightById(999).Returns((Flight?)null);

            var result = _sut.GetById(999);

            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void Post_WhenBodyIsNull_ReturnsBadRequest()
        {
            var result = _sut.Post(null!);

            result.Result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be("Flight data is required.");
            _flightRepository.DidNotReceive().AddFlight(Arg.Any<Flight>());
        }

        [Fact]
        public void Post_WhenValid_ReturnsCreatedAtAction()
        {
            var flight = CreateFlight(id: 10, status: FlightStatus.Scheduled);
            _flightRepository.AddFlight(flight).Returns(flight);

            var result = _sut.Post(flight);

            var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            created.ActionName.Should().Be(nameof(FlightsController.GetById));
            created.RouteValues!["id"].Should().Be(flight.Id);
            created.Value.Should().BeSameAs(flight);
            _flightRepository.Received(1).AddFlight(flight);
        }

        [Fact]
        public void UpdateFlightStatus_WhenFlightExists_ReturnsOkWithUpdatedFlight()
        {
            var departure = DateTime.UtcNow.AddDays(1);
            var existing = CreateFlight(id: 1, status: FlightStatus.Scheduled, departureTime: departure);
            _flightRepository.GetFlightById(1).Returns(existing);

            var request = new UpdateFlightStatusRequest { Status = FlightStatus.Boarding };

            var result = _sut.UpdateFlightStatus(1, request);

            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeSameAs(existing);
            existing.Status.Should().Be(FlightStatus.Boarding);
            _flightRepository.Received(1).UpdateFlight(Arg.Is<Flight>(f => f.Id == 1 && f.Status == FlightStatus.Boarding));
        }

        [Fact]
        public void UpdateFlightStatus_WhenFlightDoesNotExist_ReturnsNotFoundAndDoesNotUpdate()
        {
            const int unknownId = 9999;
            _flightRepository.GetFlightById(unknownId).Returns((Flight?)null);

            var request = new UpdateFlightStatusRequest { Status = FlightStatus.Boarding };

            var result = _sut.UpdateFlightStatus(unknownId, request);

            result.Result.Should().BeOfType<NotFoundResult>();
            _flightRepository.DidNotReceive().UpdateFlight(Arg.Any<Flight>());
        }

        private static Flight CreateFlight(
            int id,
            FlightStatus status,
            DateTime? departureTime = null)
        {
            var dep = departureTime ?? DateTime.UtcNow.AddDays(1);
            return new Flight
            {
                Id = id,
                FlightNumber = $"WB{id:D3}",
                Origin = "Origin",
                Destination = "Destination",
                DepartureTime = dep,
                ArrivalTime = dep.AddHours(1),
                Status = status,
                FuelRange = 100,
                FuelTankLeak = false,
                FlightLogSignature = "log-sig",
                AerobaticSequenceSignature = "aero-sig",
            };
        }
    }
}
