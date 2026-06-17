using Microsoft.EntityFrameworkCore;
using TransSolutions.Domain.Interfaces.Repositories;
using TransSolutions.Domain.Interfaces.Services;
using TransSolutions.Domain.Models.BusinessLogic;
using TransSolutions.Shared.Contracts.Vehicle;

namespace TransSolutions.Infrastructure.Services;

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IIssueTicketRepository _issueTicketRepository;

    public VehicleService(IVehicleRepository vehicleRepository, IIssueTicketRepository issueTicketRepository)
    {
        _vehicleRepository = vehicleRepository;
        _issueTicketRepository = issueTicketRepository;
    }

    public async Task<CreateVehicleResponse> CreateVehicle(CreateVehicleRequest request, CancellationToken ct)
    {
        var vehicle = new Vehicle()
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Name = request.Name,
            RegistrationPlateNumber = request.RegistrationPlateNumber,
            VehicleType = request.VehicleType,
            IsActive = true
        };
        await _vehicleRepository.AddAsync(vehicle, ct);
        return new CreateVehicleResponse { Id = vehicle.Id };
    }

    public async Task UpdateVehicle(UpdateVehicleRequest request, CancellationToken ct)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.Id, track: true, ct: ct);

        if (vehicle is null || !vehicle.IsActive)
            throw new KeyNotFoundException($"Vehicle with ID {request.Id} not found.");

        vehicle.Name = request.Name;
        vehicle.RegistrationPlateNumber = request.RegistrationPlateNumber;
        vehicle.VehicleType = request.VehicleType;
        
        await _vehicleRepository.UpdateAsync(vehicle, ct);
    }

    public async Task DeleteVehicle(DeleteVehicleRequest request, CancellationToken ct)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.Id, track: true, ct: ct);

        if (vehicle is null)
            throw new KeyNotFoundException("Vehicle not found.");

        // Logical delete
        vehicle.IsActive = false;
        await _vehicleRepository.UpdateAsync(vehicle, ct);
    }

    public async Task<GetVehicleResponse> GetVehicle(GetVehicleRequest request, CancellationToken ct)
    {
        var vehicle = await _vehicleRepository.GetQueryable()
            .Where(v => v.Id == request.Id)
            .Select(v => new GetVehicleResponse
            {
                Id = v.Id,
                Name = v.Name,
                RegistrationPlateNumber = v.RegistrationPlateNumber,
                VehicleType = v.VehicleType,
                CreatedAt = v.CreatedAt,
                IsActive = v.IsActive,
                IssueTickets = v.IssueTickets.Select(it => new IssueTicketResponse
                {
                    Id = it.Id,
                    Description = it.Description,
                    Timestamp = it.Timestamp,
                    Severity = it.Severity,
                    AuthorName = it.Author != null ? it.Author.Name + " " + it.Author.Surname : "Unknown",
                    IsResolved = it.IsResolved,
                    ResolvedAt = it.ResolvedAt,
                    ResolvedByName = it.ResolvedBy != null ? it.ResolvedBy.Name + " " + it.ResolvedBy.Surname : null
                }).ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (vehicle is null || !vehicle.IsActive)
            throw new KeyNotFoundException("Vehicle not found.");

        return vehicle;
    }

    public async Task<GetVehiclesResponse> GetVehicles(GetVehiclesRequest request, CancellationToken ct)
    {
        var query = _vehicleRepository.GetQueryable();

        if (!string.IsNullOrWhiteSpace(request.Name))
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{request.Name}%"));

        if (!string.IsNullOrWhiteSpace(request.RegistrationPlateNumber))
            query = query.Where(x => EF.Functions.ILike(x.RegistrationPlateNumber, $"%{request.RegistrationPlateNumber}%"));

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(request.PageSize * (request.PageNumber - 1))
            .Take(request.PageSize)
            .Select(v => new GetVehicleResponse
            {
                Id = v.Id,
                Name = v.Name,
                RegistrationPlateNumber = v.RegistrationPlateNumber,
                VehicleType = v.VehicleType,
                CreatedAt = v.CreatedAt,
                IsActive = v.IsActive,
                IssueTickets = v.IssueTickets.Select(it => new IssueTicketResponse
                {
                    Id = it.Id,
                    Description = it.Description,
                    Timestamp = it.Timestamp,
                    Severity = it.Severity,
                    AuthorName = it.Author != null ? it.Author.Name + " " + it.Author.Surname : "Unknown",
                    IsResolved = it.IsResolved,
                    ResolvedAt = it.ResolvedAt,
                    ResolvedByName = it.ResolvedBy != null ? it.ResolvedBy.Name + " " + it.ResolvedBy.Surname : null
                }).ToList()
            })
            .ToListAsync(ct);

        return new GetVehiclesResponse
        {
            Vehicles = items,
            TotalCount = totalCount
        };
    }

    public async Task AddIssueTicket(CreateIssueTicketRequest request, string userId, CancellationToken ct)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId, track: false, ct: ct);
        
        if (vehicle is null || !vehicle.IsActive)
            throw new KeyNotFoundException("Vehicle not found.");

        var issueTicket = new IssueTicket
        {
            Id = Guid.NewGuid(),
            Description = request.Description,
            Severity = request.Severity,
            Timestamp = DateTime.UtcNow,
            VehicleId = request.VehicleId,
            AuthorId = userId
        };

        await _issueTicketRepository.AddAsync(issueTicket, ct);
    }

    public async Task ResolveIssueTicket(ResolveIssueTicketRequest request, string userId, CancellationToken ct)
    {
        var ticket = await _issueTicketRepository.GetByIdAsync(request.TicketId, track: true, ct: ct);

        if (ticket is null)
            throw new KeyNotFoundException("Issue ticket not found.");

        if (ticket.IsResolved)
            return;

        ticket.IsResolved = true;
        ticket.ResolvedAt = DateTime.UtcNow;
        ticket.ResolvedById = userId;

        await _issueTicketRepository.UpdateAsync(ticket, ct);
    }
}