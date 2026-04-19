using Microsoft.EntityFrameworkCore;
using TransSolutions.Domain.Interfaces.Repositories;
using TransSolutions.Domain.Models.BusinessLogic;
using TransSolutions.Infrastructure.DbContext;

namespace TransSolutions.Infrastructure.Repositories;

public class RoadTripRepository : IRoadTripRepository
{
    private readonly AppDbContext _context;

    public RoadTripRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RoadTrip roadTrip, CancellationToken ct)
    {
        _context.RoadTrips.Add(roadTrip);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(RoadTrip roadTrip, CancellationToken ct)
    {
        _context.RoadTrips.Update(roadTrip);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(RoadTrip roadTrip, CancellationToken ct)
    {
        _context.RoadTrips.Remove(roadTrip);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<RoadTrip?> GetByIdAsync(Guid id, bool track = true, CancellationToken ct = default)
    {
        var query = _context.RoadTrips.AsQueryable();
        if (!track) query = query.AsNoTracking();
        
        return await query.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IQueryable<RoadTrip>> GetQueryable(CancellationToken ct)
    {
        return _context.RoadTrips
            .Include(x => x.Driver)
                .ThenInclude(x => x.User)
            .Include(x => x.Vehicle)
            .AsNoTracking();
    }
}