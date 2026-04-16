using System.ComponentModel.DataAnnotations;
using TransSolutions.Shared.Enums.Vehicle;

namespace TransSolutions.Domain.Models.BusinessLogic;

public class Vehicle
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string RegistrationPlateNumber  { get; set; }
    public VehicleType VehicleType { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public IEnumerable<RoadTrip>? RoadTrips { get; set; }
}