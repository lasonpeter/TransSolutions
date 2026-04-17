using Microsoft.AspNetCore.Identity;
using TransSolutions.Domain.Models.BusinessLogic;

namespace TransSolutions.Domain.Models.Auth;

public class AppUser : IdentityUser 
{
    
    public string? FullName { get; set; }
    
    // Good for searching/filtering in SQL
    public string FullNameComputed { get; private set; }
    public Driver? Driver { get; set; }
    /*public Guid DriverId { get; set; }*/
}