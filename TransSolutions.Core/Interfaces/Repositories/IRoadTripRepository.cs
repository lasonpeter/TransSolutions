using TransSolutions.Domain.Models.BusinessLogic;

namespace TransSolutions.Domain.Interfaces.Repositories;

public interface IRoadTripRepository
{
    public Task AddAsync(RoadTrip roadTrip, CancellationToken ct);
    public Task UpdateAsync(RoadTrip roadTrip, CancellationToken ct);
    public Task DeleteAsync(RoadTrip roadTrip, CancellationToken ct);
    public Task<RoadTrip?> GetByIdAsync(Guid id, bool track = true, CancellationToken ct = default);
    public Task<IQueryable<RoadTrip>> GetQueryable(CancellationToken ct);
}