using Microsoft.EntityFrameworkCore;
using TravelBuddy.Experiencias;
using TravelBuddy.Destinations;
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
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;

namespace TravelBuddy.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ConnectionStringName("Default")]
public class TravelBuddyDbContext :
    AbpDbContext<TravelBuddyDbContext>,
    IIdentityDbContext
{
    public DbSet<Destination> Destinations { get; set; }
    public DbSet<Experiencia> Experiencias { get; set; }

    public DbSet<Destinations.Destination> Destinations { get; set; }
    public DbSet<Experiencias.Experiencia> Experiencias { get; set; }
    public DbSet<Calificaciones.Calificacion> Calificaciones { get; set; }


    #region Entities from the modules
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    #endregion

    public TravelBuddyDbContext(DbContextOptions<TravelBuddyDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureBlobStoring();

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
            b.Property(x => x.GeoDbCityId);

            b.Property(x => x.Population);
            b.Property(x => x.Latitude);
            b.Property(x => x.Longitude);
            b.Property(x => x.LastUpdated);

            b.HasIndex(x => x.GeoDbCityId);
        });

        builder.Entity<Experiencia>(b =>
        {
            b.ToTable(TravelBuddyConsts.DbTablePrefix + "Experiencias", TravelBuddyConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Titulo).IsRequired().HasMaxLength(128);
            b.Property(x => x.Descripcion).IsRequired().HasMaxLength(1024);
        });

        builder.Entity<Calificaciones.Calificacion>(b =>
        {
            b.ToTable(TravelBuddyConsts.DbTablePrefix + "Calificaciones", TravelBuddyConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Comentario).IsRequired().HasMaxLength(2000);
            b.Property(x => x.Estrellas).IsRequired();
            b.HasIndex(x => new { x.DestinoId, x.UsuarioId }).IsUnique();
        });

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(TravelBuddyConsts.DbTablePrefix + "YourEntities", TravelBuddyConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});
    }
}