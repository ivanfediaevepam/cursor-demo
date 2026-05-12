using AviationApi.Models;

namespace AviationApi.Repositories;

public interface IPlaneRepository
{
    List<Plane> GetAllPlanes();

    Plane GetPlaneById(int id);

    Plane AddPlane(Plane plane);

    IReadOnlyList<MaintenanceRecord> GetMaintenanceRecordsForPlane(int planeId);

    MaintenanceRecord GetMaintenanceRecord(int planeId, int recordId);

    MaintenanceRecord AddMaintenanceRecord(int planeId, MaintenanceRecord record);
}
