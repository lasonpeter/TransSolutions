using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TransSolutions.Domain.Models.Auth;
using TransSolutions.Shared.Enums.Vehicle;

namespace TransSolutions.Domain.Models.BusinessLogic;

public class Driver
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public IEnumerable<DrivingLicenseCategory> DrivingLicenseCategories { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public string? AppUserId { get; set; }
    public AppUser?  User { get; set; }
    
    public IEnumerable<RoadTrip>? RoadTrips { get; set; }
}