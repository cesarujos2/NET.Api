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
    }
}