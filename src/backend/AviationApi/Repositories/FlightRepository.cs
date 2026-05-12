using AviationApi.Models;

namespace AviationApi.Repositories
{
    public class FlightRepository : IFlightRepository
    {
        private List<Flight> Flights = new List<Flight>
    {
        new Flight
        {
            Id = 1,
            FlightNumber = "WB001",
            Origin = "Kitty Hawk, NC",
            Destination = "Manteo, NC",
            DepartureTime = new DateTime(1903, 12, 17, 10, 35, 0),
            ArrivalTime = new DateTime(1903, 12, 17, 10, 35, 0).AddMinutes(12),
            Status = FlightStatus.Scheduled,
            FuelRange = 100,
            FuelTankLeak = false,
            FlightLogSignature = "171203-DEP-ARR-WB001",
            AerobaticSequenceSignature = "L4B-H2C-R3A-S1D-T2E"
        },
        new Flight
        {
            Id = 2,
            FlightNumber = "WB002",
            Origin = "Kitty Hawk, NC",
            Destination = "Manteo, NC",
            DepartureTime = new DateTime(1903, 12, 17, 10, 35, 0),
            ArrivalTime = new DateTime(1903, 12, 17, 10, 35, 0).AddMinutes(12),
            Status = FlightStatus.Scheduled,
            FuelRange = 100,
            FuelTankLeak = false,
            FlightLogSignature = "171203-DEP-ARR-WB002",
            AerobaticSequenceSignature = "L1A-H1B-R1C-T1E"
        },
        new Flight
        {
            Id = 3,
            FlightNumber = "WB003",
            Origin = "Fort Myer, VA",
            Destination = "Fort Myer, VA",
            DepartureTime = new DateTime(1908, 9, 17, 10, 35, 0),
            ArrivalTime = new DateTime(1908, 9, 17, 10, 35, 0).AddMinutes(12),
            Status = FlightStatus.Scheduled,
            FuelRange = 100,
            FuelTankLeak = true,
            FlightLogSignature = "170908-DEP-ARR-WB003",
            AerobaticSequenceSignature = "L2A-H2B-R2C"
        },
        new Flight { Id = 4, FlightNumber = "WB004", Origin = "Dayton, OH", Destination = "Dayton, OH", DepartureTime = new DateTime(1907, 5, 20), ArrivalTime = new DateTime(1907, 5, 20).AddMinutes(12), Status = FlightStatus.Scheduled, FuelRange = 100, FuelTankLeak = false, FlightLogSignature = "070520-WB004", AerobaticSequenceSignature = "L1A" },
        new Flight { Id = 5, FlightNumber = "WB005", Origin = "Dayton, OH", Destination = "Dayton, OH", DepartureTime = new DateTime(1909, 5, 20), ArrivalTime = new DateTime(1909, 5, 20).AddMinutes(12), Status = FlightStatus.Scheduled, FuelRange = 110, FuelTankLeak = false, FlightLogSignature = "090520-WB005", AerobaticSequenceSignature = "L1B" },
        new Flight { Id = 6, FlightNumber = "WB006", Origin = "Dayton, OH", Destination = "Dayton, OH", DepartureTime = new DateTime(1909, 6, 20), ArrivalTime = new DateTime(1909, 6, 20).AddMinutes(12), Status = FlightStatus.Scheduled, FuelRange = 120, FuelTankLeak = false, FlightLogSignature = "090620-WB006", AerobaticSequenceSignature = "L1C" },
        new Flight { Id = 7, FlightNumber = "WB007", Origin = "Dayton, OH", Destination = "Dayton, OH", DepartureTime = new DateTime(1910, 5, 20), ArrivalTime = new DateTime(1910, 5, 20).AddMinutes(12), Status = FlightStatus.Scheduled, FuelRange = 130, FuelTankLeak = false, FlightLogSignature = "100520-WB007", AerobaticSequenceSignature = "L1D" },
        new Flight { Id = 8, FlightNumber = "WB008", Origin = "Dayton, OH", Destination = "Dayton, OH", DepartureTime = new DateTime(1910, 6, 20), ArrivalTime = new DateTime(1910, 6, 20).AddMinutes(12), Status = FlightStatus.Scheduled, FuelRange = 140, FuelTankLeak = false, FlightLogSignature = "100620-WB008", AerobaticSequenceSignature = "L1E" },
        new Flight { Id = 9, FlightNumber = "WB009", Origin = "Dayton, OH", Destination = "Dayton, OH", DepartureTime = new DateTime(1911, 5, 20), ArrivalTime = new DateTime(1911, 5, 20).AddMinutes(12), Status = FlightStatus.Scheduled, FuelRange = 150, FuelTankLeak = false, FlightLogSignature = "110520-WB009", AerobaticSequenceSignature = "L2A" },
        new Flight { Id = 10, FlightNumber = "WB010", Origin = "Dayton, OH", Destination = "Dayton, OH", DepartureTime = new DateTime(1912, 5, 20), ArrivalTime = new DateTime(1912, 5, 20).AddMinutes(12), Status = FlightStatus.Scheduled, FuelRange = 160, FuelTankLeak = false, FlightLogSignature = "120520-WB010", AerobaticSequenceSignature = "L2B" },
        new Flight { Id = 11, FlightNumber = "WB011", Origin = "Dayton, OH", Destination = "Dayton, OH", DepartureTime = new DateTime(1912, 6, 20), ArrivalTime = new DateTime(1912, 6, 20).AddMinutes(12), Status = FlightStatus.Scheduled, FuelRange = 170, FuelTankLeak = false, FlightLogSignature = "120620-WB011", AerobaticSequenceSignature = "L2C" },
        new Flight { Id = 12, FlightNumber = "WB012", Origin = "Dayton, OH", Destination = "Dayton, OH", DepartureTime = new DateTime(1913, 5, 20), ArrivalTime = new DateTime(1913, 5, 20).AddMinutes(12), Status = FlightStatus.Scheduled, FuelRange = 180, FuelTankLeak = false, FlightLogSignature = "130520-WB012", AerobaticSequenceSignature = "L2D" },
        new Flight { Id = 13, FlightNumber = "WB013", Origin = "Dayton, OH", Destination = "Dayton, OH", DepartureTime = new DateTime(1913, 6, 20), ArrivalTime = new DateTime(1913, 6, 20).AddMinutes(12), Status = FlightStatus.Scheduled, FuelRange = 190, FuelTankLeak = false, FlightLogSignature = "130620-WB013", AerobaticSequenceSignature = "L2E" },
        new Flight { Id = 14, FlightNumber = "WB014", Origin = "Dayton, OH", Destination = "Dayton, OH", DepartureTime = new DateTime(1913, 7, 20), ArrivalTime = new DateTime(1913, 7, 20).AddMinutes(12), Status = FlightStatus.Scheduled, FuelRange = 200, FuelTankLeak = false, FlightLogSignature = "130720-WB014", AerobaticSequenceSignature = "L3A" },
        new Flight { Id = 15, FlightNumber = "WB015", Origin = "Dayton, OH", Destination = "Dayton, OH", DepartureTime = new DateTime(1913, 8, 20), ArrivalTime = new DateTime(1913, 8, 20).AddMinutes(12), Status = FlightStatus.Scheduled, FuelRange = 210, FuelTankLeak = false, FlightLogSignature = "130820-WB015", AerobaticSequenceSignature = "L3B" },
        new Flight { Id = 16, FlightNumber = "WB016", Origin = "Dayton, OH", Destination = "Dayton, OH", DepartureTime = new DateTime(1914, 5, 20), ArrivalTime = new DateTime(1914, 5, 20).AddMinutes(12), Status = FlightStatus.Scheduled, FuelRange = 220, FuelTankLeak = false, FlightLogSignature = "140520-WB016", AerobaticSequenceSignature = "L3C" },
        new Flight { Id = 17, FlightNumber = "WB017", Origin = "Dayton, OH", Destination = "Dayton, OH", DepartureTime = new DateTime(1914, 6, 20), ArrivalTime = new DateTime(1914, 6, 20).AddMinutes(12), Status = FlightStatus.Scheduled, FuelRange = 230, FuelTankLeak = false, FlightLogSignature = "140620-WB017", AerobaticSequenceSignature = "L3D" },
        new Flight { Id = 18, FlightNumber = "WB018", Origin = "Dayton, OH", Destination = "Dayton, OH", DepartureTime = new DateTime(1915, 5, 20), ArrivalTime = new DateTime(1915, 5, 20).AddMinutes(12), Status = FlightStatus.Scheduled, FuelRange = 240, FuelTankLeak = false, FlightLogSignature = "150520-WB018", AerobaticSequenceSignature = "L3E" },
        new Flight { Id = 19, FlightNumber = "WB019", Origin = "Dayton, OH", Destination = "Dayton, OH", DepartureTime = new DateTime(1916, 5, 20), ArrivalTime = new DateTime(1916, 5, 20).AddMinutes(12), Status = FlightStatus.Scheduled, FuelRange = 250, FuelTankLeak = false, FlightLogSignature = "160520-WB019", AerobaticSequenceSignature = "L4A" },
        new Flight { Id = 20, FlightNumber = "WB020", Origin = "Dayton, OH", Destination = "Dayton, OH", DepartureTime = new DateTime(1918, 5, 20), ArrivalTime = new DateTime(1918, 5, 20).AddMinutes(12), Status = FlightStatus.Scheduled, FuelRange = 260, FuelTankLeak = false, FlightLogSignature = "180520-WB020", AerobaticSequenceSignature = "L4B" },
        new Flight
        {
            Id = 21,
            FlightNumber = "WB021",
            Origin = "Dayton, OH",
            Destination = "Dayton, OH",
            DepartureTime = new DateTime(1919, 5, 20, 10, 35, 0),
            ArrivalTime = new DateTime(1919, 5, 20, 10, 47, 0),
            Status = FlightStatus.Scheduled,
            FuelRange = 250,
            FuelTankLeak = false,
            FlightLogSignature = "200519-DEP-ARR-WB021",
            AerobaticSequenceSignature = "L5A-H3B-R4C-T1E"
        },

    };

        public List<Flight> GetAllFlights()
        {
            return Flights;
        }

        public Flight GetFlightById(int id)
        {
            return Flights.FirstOrDefault(f => f.Id == id);
        }

        public Flight AddFlight(Flight flight)
        {
            Flights.Add(flight);

            return flight;
        }

        public Flight UpdateFlight(Flight updatedFlight)
        {
            var flight = Flights.FirstOrDefault(f => f.Id == updatedFlight.Id);
            if (flight != null)
            {
                flight.FlightNumber = updatedFlight.FlightNumber;
                flight.Origin = updatedFlight.Origin;
                flight.Destination = updatedFlight.Destination;
                flight.DepartureTime = updatedFlight.DepartureTime;
                flight.ArrivalTime = updatedFlight.ArrivalTime;
                flight.Status = updatedFlight.Status;
                flight.DelayReason = updatedFlight.DelayReason;
                flight.FuelRange = updatedFlight.FuelRange;
                flight.FuelTankLeak = updatedFlight.FuelTankLeak;
                flight.FlightLogSignature = updatedFlight.FlightLogSignature;
                flight.AerobaticSequenceSignature = updatedFlight.AerobaticSequenceSignature;
            }

            return flight;
        }
    }
}