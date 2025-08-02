using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NET.Api.Domain.Entities;

namespace NET.Api.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<RefreshToken> RefreshTokens { get; set; }

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
                
            entity.Property(e => e.IdentityDocument)
                .HasMaxLength(20)
                .IsRequired();
                
            entity.HasIndex(e => e.IdentityDocument)
                .IsUnique()
                .HasDatabaseName("IX_Users_IdentityDocument");
                
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .IsRequired();
                
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");
        });
        
        // Configure Identity tables with Oracle naming conventions
        builder.Entity<ApplicationUser>().ToTable("USERS");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().ToTable("ROLES");
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
    }
}