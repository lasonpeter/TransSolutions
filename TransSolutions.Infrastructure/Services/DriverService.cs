using Microsoft.EntityFrameworkCore;
using TransSolutions.Domain.Interfaces.Repositories;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Domain.Models.BusinessLogic;
using TransSolutions.Shared.Contracts.Driver;

public class DriverService : IDriverService
{
    private readonly IDriverRepository _driverRepository;

    public DriverService(IDriverRepository driverRepository) 
        => _driverRepository = driverRepository;

    public async Task<CreateDriverResponse> CreateDriver(CreateDriverRequest request, CancellationToken ct)
    {
        var existingDriver = await _driverRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.AppUserId == request.UserId.ToString(), ct);

        if (existingDriver != null)
        {
            existingDriver.IsActive = true;
            existingDriver.DrivingLicenseCategories = request.DrivingLicenseCategories;
            await _driverRepository.UpdateAsync(existingDriver, ct);
            return new CreateDriverResponse { Id = existingDriver.Id };
        }

        var driver = new Driver
        {
            Id = Guid.NewGuid(),
            AppUserId = request.UserId.ToString(),
            DrivingLicenseCategories = request.DrivingLicenseCategories,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _driverRepository.AddAsync(driver, ct);

        return new CreateDriverResponse { Id = driver.Id };
    }

    public async Task UpdateDriver(UpdateDriverRequest request, CancellationToken ct)
    {
        var driver= await _driverRepository.GetByIdAsync(request.Id, true,ct);
        if(driver is null)
            throw new KeyNotFoundException("Driver not found");
        driver.DrivingLicenseCategories = request.DrivingLicenseCategories;
        await _driverRepository.UpdateAsync(driver, ct);
    }

    public async Task<GetDriverResponse> GetDriver(GetDriverRequest request, CancellationToken ct)
    {
        var driver = await _driverRepository.GetByIdAsync(request.Id, track: false, ct);

        if (driver is null || !driver.IsActive)
            throw new KeyNotFoundException("Driver not found");

        return new GetDriverResponse
        {
            Id = driver.Id,
            Name = driver.User?.Name ?? string.Empty,
            Surname = driver.User?.Surname ?? string.Empty,
            DrivingLicenseCategories = driver.DrivingLicenseCategories.ToList()
        };
    }

    public async Task<GetDriversResponse> GetDrivers(GetDriversRequest request, CancellationToken ct)
    {
        var query = _driverRepository.GetQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            query = query.Where(x => EF.Functions.ILike(x.User.FullNameComputed, $"%{request.FullName}%"));
        }

        var totalCount = await query.CountAsync(ct);

        var drivers = await query
            .OrderBy(x => x.User.Surname)
            .Skip(request.PageSize * (request.PageNumber - 1))
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new GetDriversResponse
        {
            TotalCount = totalCount,
            Drivers = drivers.Select(d => new GetDriverResponse
            {
                Id = d.Id,
                Name = d.User?.Name ?? string.Empty,
                Surname = d.User?.Surname ?? string.Empty,
                DrivingLicenseCategories = d.DrivingLicenseCategories.ToList()
            })
        };
    }

    public async Task DeleteDriver(DeleteDriverRequest request, CancellationToken ct)
    {
        var driver = await _driverRepository.GetByIdAsync(request.Id, track: true, ct);
        
        if (driver == null) throw new KeyNotFoundException();

        driver.IsActive = false; 
        await _driverRepository.UpdateAsync(driver, ct);
    }
}
