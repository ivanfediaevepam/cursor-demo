using AviationApi.Models;

namespace AviationApi.Repositories;

public class PlaneRepository : IPlaneRepository
{
    private readonly List<Plane> _planes =
    [
        new Plane
        {
            Id = 1,
            Name = "Wright Flyer",
            Year = 1903,
            Description = "The first successful heavier-than-air powered aircraft.",
            RangeInKm = 12,
        },
        new Plane
        {
            Id = 2,
            Name = "Wright Flyer II",
            Year = 1904,
            Description = "A refinement of the original Flyer with better performance.",
            RangeInKm = 24,
        },
        new Plane
        {
            Id = 3,
            Name = "Wright Model A",
            Year = 1908,
            Description = "The first commercially successful airplane.",
            RangeInKm = 40,
        },
        new Plane { Id = 4, Name = "Wright Model A", Year = 1907, Description = "The first commercially successful airplane.", RangeInKm = 45 },
        new Plane { Id = 5, Name = "Military Flyer", Year = 1909, Description = "Developed for the U.S. Army Signal Corps.", RangeInKm = 50 },
        new Plane { Id = 6, Name = "Transitional Model AB", Year = 1909, Description = "A transitional design between Model A and B.", RangeInKm = 55 },
        new Plane { Id = 7, Name = "Wright Model B", Year = 1910, Description = "The first Wright airplane with a rear elevator.", RangeInKm = 60 },
        new Plane { Id = 8, Name = "Wright Model R", Year = 1910, Description = "A small racing airplane known as the Baby Wright.", RangeInKm = 65 },
        new Plane { Id = 9, Name = "Wright Model EX", Year = 1911, Description = "Famous for the first transcontinental flight across the U.S.", RangeInKm = 70 },
        new Plane { Id = 10, Name = "Wright Model C", Year = 1912, Description = "A hydroaeroplane design for the U.S. Navy.", RangeInKm = 75 },
        new Plane { Id = 11, Name = "Wright Model D", Year = 1912, Description = "A fast scout plane for the military.", RangeInKm = 80 },
        new Plane { Id = 12, Name = "Wright Model CH", Year = 1913, Description = "A modified Model C for hydro operations.", RangeInKm = 85 },
        new Plane { Id = 13, Name = "Wright Model E", Year = 1913, Description = "The first Wright plane with a single propeller.", RangeInKm = 90 },
        new Plane { Id = 14, Name = "Wright Model F", Year = 1913, Description = "The first Wright plane with a fuselage.", RangeInKm = 95 },
        new Plane { Id = 15, Name = "Wright Model G", Year = 1913, Description = "A flying boat designed for the Navy.", RangeInKm = 100 },
        new Plane { Id = 16, Name = "Wright Model H", Year = 1914, Description = "A heavier version of the Model F.", RangeInKm = 110 },
        new Plane { Id = 17, Name = "Wright Model HS", Year = 1914, Description = "A shorter wingspan version of the Model H.", RangeInKm = 120 },
        new Plane { Id = 18, Name = "Wright Model K", Year = 1915, Description = "A seaplane with a new wing profile.", RangeInKm = 130 },
        new Plane { Id = 19, Name = "Wright Model L", Year = 1916, Description = "A single-seat military scout.", RangeInKm = 140 },
        new Plane { Id = 20, Name = "Liberty Eagle", Year = 1918, Description = "A late-war design with increased power.", RangeInKm = 145 },
        new Plane
        {
            Id = 21,
            Name = "OW.1 Aerial Coupe",
            Year = 1919,
            Description = "A pioneer in enclosed cabin designs, offering a glimpse into the future of passenger aviation.",
            RangeInKm = 150,
        },
    ];

    private readonly Dictionary<int, List<MaintenanceRecord>> _maintenanceByPlane = new();

    public List<Plane> GetAllPlanes() => _planes;

    public Plane GetPlaneById(int id) => _planes.FirstOrDefault(p => p.Id == id);

    public Plane AddPlane(Plane plane)
    {
        _planes.Add(plane);
        return plane;
    }

    public IReadOnlyList<MaintenanceRecord> GetMaintenanceRecordsForPlane(int planeId)
    {
        if (!_maintenanceByPlane.TryGetValue(planeId, out var list))
        {
            return Array.Empty<MaintenanceRecord>();
        }

        return list.OrderByDescending(r => r.PerformedAt).ThenByDescending(r => r.Id).ToList();
    }

    public MaintenanceRecord GetMaintenanceRecord(int planeId, int recordId)
    {
        if (!_maintenanceByPlane.TryGetValue(planeId, out var list))
        {
            return null;
        }

        return list.FirstOrDefault(r => r.Id == recordId);
    }

    public MaintenanceRecord AddMaintenanceRecord(int planeId, MaintenanceRecord record)
    {
        if (!_maintenanceByPlane.TryGetValue(planeId, out var list))
        {
            list = new List<MaintenanceRecord>();
            _maintenanceByPlane[planeId] = list;
        }

        var nextId = list.Count == 0 ? 1 : list.Max(r => r.Id) + 1;
        record.Id = nextId;
        record.PlaneId = planeId;
        list.Add(record);
        return record;
    }
}
