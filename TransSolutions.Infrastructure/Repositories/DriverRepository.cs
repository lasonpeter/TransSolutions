using Microsoft.EntityFrameworkCore;
using TransSolutions.Domain.Interfaces.Repositories;
using TransSolutions.Domain.Models.BusinessLogic;
using TransSolutions.Infrastructure.DbContext;

namespace TransSolutions.Infrastructure.Services;

public class DriverRepository : IDriverRepository
{
    private readonly AppDbContext _context;

    public DriverRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(Driver driver, CancellationToken ct)
    {
        await _context.Drivers.AddAsync(driver, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Driver driver, CancellationToken ct)
    {
        _context.Drivers.Update(driver);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Driver?> GetByIdAsync(Guid id, bool track = true, CancellationToken ct = default)
    {
        var query = _context.Drivers.AsQueryable();
        if (!track) query = query.AsNoTracking();
        
        return await query.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public IQueryable<Driver> GetQueryable()
    {
        return _context.Drivers.AsNoTracking();
    }
}