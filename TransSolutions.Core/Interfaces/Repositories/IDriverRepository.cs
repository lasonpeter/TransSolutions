using TransSolutions.Domain.Models.BusinessLogic;

namespace TransSolutions.Domain.Interfaces.Repositories;

public interface IDriverRepository
{
    Task<Driver?> GetByIdAsync(Guid id, bool track = true, CancellationToken ct = default);
    Task AddAsync(Driver driver, CancellationToken ct);
    Task UpdateAsync(Driver driver, CancellationToken ct);
    
    IQueryable<Driver> GetQueryable();
}