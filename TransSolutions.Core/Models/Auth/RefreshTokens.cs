namespace TransSolutions.Domain.Models.Auth;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class RefreshTokens
{
    [Key]
    public Guid Id { get; set; }
    
    [MaxLength(512)]
    public string Token { get; set; } = string.Empty;
    
    public string JwtId { get; set; } = string.Empty;  
    
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiryDate { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    
    [ForeignKey(nameof(UserId))]
    public virtual AppUser User { get; set; } = null!;
}