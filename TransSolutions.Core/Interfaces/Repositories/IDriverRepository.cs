using TransSolutions.Shared.Contracts.Driver;

namespace TransSolutions.Domain.Interfaces.Repositories;

public interface IDriverRepository
{
    public Task CreateDriver(CreateDriverRequest request);
    public Task DeleteDriver(DeleteDriverRequest request);
    public Task UpdateDriver(UpdateDriverRequest request);
    public Task<GetDriverResponse> GetDriver(GetDriverRequest request);
    public Task<GetDriversResponse> GetDrivers(GetDriversRequest request);
}