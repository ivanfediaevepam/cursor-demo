using AviationApi.Models;
using AviationApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AviationApi.Controllers;

[ApiController]
[Route("[controller]")]
public class FlightsController : ControllerBase
{
    private readonly ILogger<FlightsController> _logger;
    private readonly IFlightRepository _flightRepository;

    public FlightsController(ILogger<FlightsController> logger, IFlightRepository flightRepository)
    {
        _logger = logger;
        _flightRepository = flightRepository;
    }

    [HttpGet]
    public ActionResult<List<Flight>> Get()
    {
        _logger.LogInformation("GET ✈✈✈ All Flights ✈✈✈");

        var flights = _flightRepository.GetAllFlights();

        return Ok(flights);
    }

    [HttpGet("{id}")]
    public ActionResult<Flight> GetById(int id)
    {
        _logger.LogInformation($"GET ✈✈✈ {id} ✈✈✈");

        var flight = _flightRepository.GetFlightById(id);

        if (flight == null)
        {
            return NotFound();
        }

        return Ok(flight);
    }

    [HttpPost]
    public ActionResult<Flight> Post([FromBody] Flight flight)
    {
        _logger.LogInformation($"POST ✈✈✈ {flight} ✈✈✈");

        if (flight == null)
        {
            return BadRequest("Flight data is required.");
        }

        _flightRepository.AddFlight(flight);

        return CreatedAtAction(nameof(GetById), new { id = flight.Id }, flight);
    }

    /// <summary>Updates a flight's status when the requested transition satisfies business rules.</summary>
    /// <param name="id">The identifier of the flight whose status should be updated.</param>
    /// <param name="request">The desired next status and optional delay reason (required when status is <see cref="FlightStatus.Delayed"/>).</param>
    /// <returns>
    /// HTTP 200 OK with the updated <see cref="Flight"/> when the flight exists and the transition is allowed;
    /// HTTP 404 Not Found when no flight matches <paramref name="id"/>;
    /// HTTP 400 Bad Request when the body is missing, the status is unsupported, the transition is invalid,
    /// status is Delayed without a valid <see cref="UpdateFlightStatusRequest.DelayReason"/>, or the delay reason is not in the allowed vocabulary.
    /// </returns>
    [HttpPut("{id:int}/status")]
    public ActionResult<Flight> UpdateFlightStatus(int id, [FromBody] UpdateFlightStatusRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        var flight = _flightRepository.GetFlightById(id);
        if (flight == null)
        {
            return NotFound();
        }

        var newStatus = request.Status;

        switch (newStatus)
        {
            case FlightStatus.Boarding:
                if (DateTime.Now > flight.DepartureTime)
                {
                    return BadRequest("Cannot board, past departure time.");
                }
                break;

            case FlightStatus.Departed:
                if (flight.Status != FlightStatus.Boarding)
                {
                    return BadRequest("Flight must be in 'Boarding' status before it can be 'Departed'.");
                }
                break;

            case FlightStatus.InAir:
                if (flight.Status != FlightStatus.Departed)
                {
                    return BadRequest("Flight must be in 'Departed' status before it can be 'In Air'.");
                }
                break;

            case FlightStatus.Landed:
                if (flight.Status != FlightStatus.InAir)
                {
                    return BadRequest("Flight must be in 'In Air' status before it can be 'Landed'.");
                }
                break;

            case FlightStatus.Cancelled:
                if (DateTime.Now > flight.DepartureTime)
                {
                    return BadRequest("Cannot cancel, past departure time.");
                }
                break;

            case FlightStatus.Delayed:
                if (flight.Status == FlightStatus.Cancelled)
                {
                    return BadRequest("Cannot delay, flight is cancelled.");
                }
                break;

            default:
                return BadRequest("Unknown or unsupported flight status.");
        }

        if (newStatus == FlightStatus.Delayed)
        {
            var reason = request.DelayReason?.Trim();
            if (string.IsNullOrEmpty(reason))
            {
                return BadRequest("Delay reason is required when status is Delayed.");
            }

            if (!DelayReasonCatalog.IsAllowed(reason))
            {
                return BadRequest("Invalid delay reason.");
            }

            flight.DelayReason = reason;
        }
        else
        {
            flight.DelayReason = null;
        }

        flight.Status = newStatus;
        _flightRepository.UpdateFlight(flight);

        return Ok(flight);
    }

    [HttpPost("{id}/calculateAerodynamics")]
    public ActionResult CalculateAerodynamics(int id)
    {
        _logger.LogInformation($"POST ✈✈✈ Calculating Aerodynamics for {id} ✈✈✈");

        var flight = _flightRepository.GetFlightById(id);

        if (flight == null)
        {
            return NotFound();
        }

        return Ok(new { Message = "Aerodynamics calculated successfully." });
    }

    [HttpPost("{id}/takeFlight/{altitude}")]
    public ActionResult TakeFlight(int id, int altitude)
    {
        _logger.LogInformation($"POST ✈✈✈ Flight {id} taking flight to {altitude}ft ✈✈✈");

        var flight = _flightRepository.GetFlightById(id);

        if (flight == null)
        {
            return NotFound();
        }

        return Ok(new { Message = $"Flight {id} is now at {altitude}ft." });
    }

    [HttpPost("{id}/lightningStrike")]
    public ActionResult LightningStrike(int id)
    {
        _logger.LogInformation($"POST ✈✈✈ Lightning strike on flight {id}! ⚡⚡⚡");

        var flight = _flightRepository.GetFlightById(id);

        if (flight == null)
        {
            return NotFound();
        }

        return Ok(new { Message = "Lightning strike handled." });
    }
}