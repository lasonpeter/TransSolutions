namespace TransSolutions.Domain.Interfaces.Repositories;

public interface IVehicleRepository
{
    public Task AddVehicle();
    public Task RemoveVehicle();
    public Task UpdateVehicle();
    public Task GetVehicles();
    public Task GetVehicleInfo();
}