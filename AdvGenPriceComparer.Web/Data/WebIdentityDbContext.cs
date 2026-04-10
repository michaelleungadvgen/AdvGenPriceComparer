using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdvGenPriceComparer.Web.Data;

/// <summary>
/// Database context for ASP.NET Core Identity user store.
/// Uses SQLite for user authentication and role management.
/// </summary>
public class WebIdentityDbContext : IdentityDbContext<IdentityUser>
{
    public WebIdentityDbContext(DbContextOptions<WebIdentityDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Identity table names to avoid conflicts with existing tables
        builder.Entity<IdentityUser>().ToTable("WebUsers");
        builder.Entity<IdentityRole>().ToTable("WebRoles");
        builder.Entity<IdentityUserRole<string>>().ToTable("WebUserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("WebUserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("WebUserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("WebUserTokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("WebRoleClaims");
    }
}
