using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using TravelBuddy.Favorites;
using TravelBuddy.Destinations;
using TravelBuddy.Experiencias;
using TravelBuddy.Favorites;

namespace TravelBuddy.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ConnectionStringName("Default")]
public class TravelBuddyDbContext :
    AbpDbContext<TravelBuddyDbContext>,
    IIdentityDbContext
{
    public DbSet<Destination> Destinations { get; set; }
    public DbSet<Experiencia> Experiencias { get; set; }
    /* Entidades de tus módulos de dominio */
    public DbSet<Destination> Destinations { get; set; }
    public DbSet<Experiencia> Experiencias { get; set; }
    public DbSet<Calificaciones.Calificacion> Calificaciones { get; set; }
    public DbSet<Favorite> Favoritos { get; set; }
    public DbSet<Notificaciones.Notification> Notificaciones { get; set; }

    #region Entities from the modules
    // Identity
    /* Entidades requeridas por IIdentityDbContext */
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Agrega estas 4 lineas que reclama la interfaz IIdentityDbContext:
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    #endregion
    public TravelBuddyDbContext(DbContextOptions<TravelBuddyDbContext> options)
        : base(options)
    {

    public TravelBuddyDbContext(DbContextOptions<TravelBuddyDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */
        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureBlobStoring();
    }

        /* Configure your own tables/entities here */
        builder.Entity<Destination>(b =>
        {
            b.ToTable(TravelBuddyConsts.DbTablePrefix + "Destinations", TravelBuddyConsts.DbSchema);
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Configuración de los módulos predeterminados de ABP */
        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureBlobStoring();

        /* Configuración de tus entidades de dominio */

        // 1. Destinations
        builder.Entity<Destination>(b =>
        {
            b.ToTable(TravelBuddyConsts.DbTablePrefix + "Destinations", TravelBuddyConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.Region).HasMaxLength(500);
            b.Property(x => x.Country).IsRequired().HasMaxLength(100);
            b.Property(x => x.Price).HasColumnType("decimal(18,2)");
            b.Property(x => x.ImageUrl).HasMaxLength(1000);
            b.Property(x => x.IsAvailable).IsRequired();

            b.HasIndex(x => x.GeoDbCityId);
        });

        // 2. Experiencias
        builder.Entity<Experiencia>(b =>
        {
            b.ToTable(TravelBuddyConsts.DbTablePrefix + "Experiencias", TravelBuddyConsts.DbSchema);
            b.ConfigureByConvention();
        });

        // 3. Calificaciones
        builder.Entity<Calificaciones.Calificacion>(b =>
        {
            b.ToTable(TravelBuddyConsts.DbTablePrefix + "Calificaciones", TravelBuddyConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Comentario).IsRequired().HasMaxLength(2000);
            b.Property(x => x.Estrellas).IsRequired();
            b.HasIndex(x => new { x.DestinoId, x.UsuarioId }).IsUnique();
        });

        // 4. Favoritos
        builder.Entity<Favorite>(b =>
        {
            b.ToTable(TravelBuddyConsts.DbTablePrefix + "Favoritos", TravelBuddyConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasKey(x => new { x.UsuarioId, x.DestinoId });
            b.HasIndex(x => new { x.UsuarioId, x.DestinoId }).IsUnique();
        });

        builder.Entity<Favorite>(b =>
        // 5. Notificaciones
        builder.Entity<Notificaciones.Notification>(b =>
        {
            b.ToTable(TravelBuddyConsts.DbTablePrefix + "Notificaciones", TravelBuddyConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasKey(x => new { x.UsuarioId, x.DestinoId });
            b.HasIndex(x => new { x.UsuarioId, x.DestinoId }).IsUnique();

            b.Property(x => x.Title).IsRequired().HasMaxLength(255);
            b.Property(x => x.Message).IsRequired().HasMaxLength(1000);
            b.HasIndex(x => x.UserId);
        });
    }
}
