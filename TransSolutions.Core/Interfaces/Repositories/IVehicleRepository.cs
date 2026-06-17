using TransSolutions.Domain.Models.BusinessLogic;

namespace TransSolutions.Domain.Interfaces.Repositories;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(Guid id, bool track = true, bool includeIssueTickets = false, CancellationToken ct = default);
    Task AddAsync(Vehicle vehicle, CancellationToken ct);
    Task UpdateAsync(Vehicle vehicle, CancellationToken ct);
    
    IQueryable<Vehicle> GetQueryable();
}   