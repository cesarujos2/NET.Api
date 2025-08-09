using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NET.Api.Domain.Entities;

namespace NET.Api.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<EmailTemplate> EmailTemplates { get; set; }
    public DbSet<UserAccount> UserAccounts { get; set; }
    public DbSet<UserAccountData> UserAccountData { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsRequired();
                
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .IsRequired();
                
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");
        });
        
        // Configure ApplicationRole
        builder.Entity<ApplicationRole>(entity =>
        {
            entity.Property(e => e.Description)
                .HasMaxLength(500);
                
            entity.Property(e => e.HierarchyLevel)
                .IsRequired();
                
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
                
            entity.Property(e => e.IsSystemRole)
                .IsRequired()
                .HasDefaultValue(false);
                
            entity.Property(e => e.CreatedAt)
                .IsRequired();
                
            entity.Property(e => e.UpdatedAt)
                .IsRequired(false);
        });
        
        // Configure Identity tables with Oracle naming conventions
        builder.Entity<ApplicationUser>().ToTable("USERS");
        builder.Entity<ApplicationRole>().ToTable("ROLES");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("USER_ROLES");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>().ToTable("USER_CLAIMS");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>().ToTable("USER_LOGINS");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>().ToTable("USER_TOKENS");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>().ToTable("ROLE_CLAIMS");
        
        // Configure RefreshToken
        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("REFRESH_TOKENS");
            
            entity.Property(e => e.Token)
                .HasMaxLength(500)
                .IsRequired();
                
            entity.Property(e => e.UserId)
                .HasMaxLength(450)
                .IsRequired();
                
            entity.HasIndex(e => e.Token)
                .IsUnique()
                .HasDatabaseName("IX_RefreshTokens_Token");
                
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_RefreshTokens_UserId");
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Configure EmailTemplate
        builder.Entity<EmailTemplate>(entity =>
        {
            entity.ToTable("EMAIL_TEMPLATES");
            
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();
                
            entity.Property(e => e.Subject)
                .HasMaxLength(200)
                .IsRequired();
                
            entity.Property(e => e.HtmlContent)
                .HasMaxLength(4000)
                .IsRequired();
                
            entity.Property(e => e.TextContent)
                .HasMaxLength(4000)
                .IsRequired();
                
            entity.Property(e => e.TemplateType)
                .HasMaxLength(50)
                .IsRequired();
                
            entity.Property(e => e.Description)
                .HasMaxLength(500);
                
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(450);
                
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(450);
                
            entity.HasIndex(e => e.TemplateType)
                .IsUnique()
                .HasDatabaseName("IX_EmailTemplates_TemplateType");
        });
        
        // Configure UserAccount
        builder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("USER_ACCOUNTS");
            
            entity.Property(e => e.Id)
                .HasConversion(
                    v => v.ToString(),
                    v => Guid.Parse(v))
                .HasMaxLength(36);
            
            entity.Property(e => e.ApplicationUserId)
                .HasMaxLength(450)
                .IsRequired();
                
            entity.Property(e => e.AccountName)
                .HasMaxLength(100)
                .IsRequired();
                
            entity.Property(e => e.Description)
                .HasMaxLength(500);
                
            entity.Property(e => e.ProfilePictureUrl)
                .HasMaxLength(500);
                
            entity.Property(e => e.Settings)
                .HasColumnType("TEXT");
                
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(450);
                
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(450);
            
            entity.HasIndex(e => e.ApplicationUserId)
                .HasDatabaseName("IX_UserAccounts_ApplicationUserId");
                
            entity.HasIndex(e => new { e.ApplicationUserId, e.AccountName })
                .IsUnique()
                .HasDatabaseName("IX_UserAccounts_ApplicationUserId_AccountName");
                
            entity.HasIndex(e => new { e.ApplicationUserId, e.IsDefault })
                .HasDatabaseName("IX_UserAccounts_ApplicationUserId_IsDefault")
                .HasFilter("IsDefault = 1");
            
            entity.HasOne(e => e.ApplicationUser)
                .WithMany(u => u.UserAccounts)
                .HasForeignKey(e => e.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Configure UserAccountData
        builder.Entity<UserAccountData>(entity =>
        {
            entity.ToTable("USER_ACCOUNT_DATA");
            
            entity.Property(e => e.Id)
                .HasConversion(
                    v => v.ToString(),
                    v => Guid.Parse(v))
                .HasMaxLength(36);
            
            entity.Property(e => e.UserAccountId)
                .HasConversion(
                    v => v.ToString(),
                    v => Guid.Parse(v))
                .HasMaxLength(36)
                .IsRequired();
                
            entity.Property(e => e.DataKey)
                .HasMaxLength(100)
                .IsRequired();
                
            entity.Property(e => e.DataValue)
                .HasColumnType("TEXT")
                .IsRequired();
                
            entity.Property(e => e.DataType)
                .HasMaxLength(50)
                .IsRequired()
                .HasDefaultValue("string");
                
            entity.Property(e => e.Category)
                .HasMaxLength(100);
                
            entity.Property(e => e.Description)
                .HasMaxLength(500);
                
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(450);
                
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(450);
            
            entity.HasIndex(e => e.UserAccountId)
                .HasDatabaseName("IX_UserAccountData_UserAccountId");
                
            entity.HasIndex(e => new { e.UserAccountId, e.DataKey })
                .IsUnique()
                .HasDatabaseName("IX_UserAccountData_UserAccountId_DataKey");
                
            entity.HasIndex(e => new { e.UserAccountId, e.Category })
                .HasDatabaseName("IX_UserAccountData_UserAccountId_Category");
            
            entity.HasOne(e => e.UserAccount)
                .WithMany(ua => ua.AccountData)
                .HasForeignKey(e => e.UserAccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}