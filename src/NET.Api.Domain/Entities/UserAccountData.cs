using System.ComponentModel.DataAnnotations;

namespace NET.Api.Domain.Entities;

/// <summary>
/// Represents specific data associated with a user account.
/// This allows each account to have its own custom data and settings.
/// </summary>
public class UserAccountData : BaseEntity
{
    /// <summary>
    /// Reference to the UserAccount this data belongs to
    /// </summary>
    [Required]
    public Guid UserAccountId { get; set; }
    
    /// <summary>
    /// Navigation property to the UserAccount
    /// </summary>
    public UserAccount UserAccount { get; set; } = null!;
    
    /// <summary>
    /// Key identifier for the data (e.g., "preferences", "theme", "language")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string DataKey { get; set; } = string.Empty;
    
    /// <summary>
    /// The actual data value stored as JSON or string
    /// </summary>
    [Required]
    public string DataValue { get; set; } = string.Empty;
    
    /// <summary>
    /// Data type for proper deserialization (e.g., "string", "json", "number", "boolean")
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string DataType { get; set; } = "string";
    
    /// <summary>
    /// Category or group for organizing data
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }
    
    /// <summary>
    /// Description of what this data represents
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Indicates if this data is sensitive and should be encrypted
    /// </summary>
    public bool IsSensitive { get; set; } = false;
    
    /// <summary>
    /// Indicates if this data is required for the account to function
    /// </summary>
    public bool IsRequired { get; set; } = false;
    
    /// <summary>
    /// Version of the data for tracking changes
    /// </summary>
    public int Version { get; set; } = 1;
    
    /// <summary>
    /// Updates the data value and increments version
    /// </summary>
    /// <param name="newValue">New data value</param>
    public void UpdateValue(string newValue)
    {
        DataValue = newValue;
        Version++;
        SetUpdatedAt();
    }
    
    /// <summary>
    /// Sets the data type and updates timestamp
    /// </summary>
    /// <param name="dataType">Type of data being stored</param>
    public void SetDataType(string dataType)
    {
        DataType = dataType;
        SetUpdatedAt();
    }
}