using System.ComponentModel.DataAnnotations;

namespace NET.Api.Domain.Entities;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Token { get; set; } = string.Empty;
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public DateTime ExpiryDate { get; set; }
    
    public bool IsRevoked { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? RevokedAt { get; set; }
    
    // Navigation property
    public ApplicationUser User { get; set; } = null!;
    
    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
    
    public bool IsActive => !IsRevoked && !IsExpired;
}