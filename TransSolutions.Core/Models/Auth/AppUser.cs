using Microsoft.AspNetCore.Identity;
using TransSolutions.Domain.Models.BusinessLogic;

namespace TransSolutions.Domain.Models.Auth;

public class AppUser : IdentityUser 
{
    public string? FullName { get; set; }
    public Driver? Driver { get; set; }
}