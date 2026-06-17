using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransSolutions.Domain.Models.BusinessLogic;

public class RoadTrip
{
    [Key]
    public Guid Id { get; set; }
    public float Distance { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public float AverageFuelConsumption { get; set; }
    
    public Vehicle Vehicle { get; set; }
    public Guid VehicleId { get; set; }
    public Driver Driver { get; set; }
    public Guid DriverId { get; set; }
    
}