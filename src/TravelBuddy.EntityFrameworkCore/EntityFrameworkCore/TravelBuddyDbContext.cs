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
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using TravelBuddy.Favorites; 

namespace TravelBuddy.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ConnectionStringName("Default")]
public class TravelBuddyDbContext :
    AbpDbContext<TravelBuddyDbContext>,
    IIdentityDbContext
{

    public DbSet<Destinations.Destination> Destinations { get; set; }
    public DbSet<Experiencias.Experiencia> Experiencias { get; set; }
    public DbSet<Favorite> Favoritos { get; set; }


    #region Entities from the modules
    // Identity
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

        /* Incluye módulos en el contexto de base de datos de la migración */
        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureBlobStoring();

        /* Configuracion tablas/entidades aquí */
        builder.Entity<Destinations.Destination>(b =>
        {
            b.ToTable(TravelBuddyConsts.DbTablePrefix + "Destinations", TravelBuddyConsts.DbSchema);
            b.ConfigureByConvention();//configuración automática para las propiedades de la clase base
            b.Property(x => x.Nombre).IsRequired().HasMaxLength(200);
            b.Property(x => x.Descripcion).HasMaxLength(1000);
            b.Property(x => x.Ubicacion).HasMaxLength(500);
            b.Property(x => x.Precio).HasColumnType("decimal(18,2)");
            b.Property(x => x.ImagenUrl).HasMaxLength(1000);
            b.Property(x => x.Disponible).IsRequired();
            b.Property(x => x.FechaCreacion).IsRequired();
            b.Property(x => x.FechaActualizacion).IsRequired();
        });

        builder.Entity<Experiencias.Experiencia>(b =>
        {
            b.ToTable(TravelBuddyConsts.DbTablePrefix + "Experiencias", TravelBuddyConsts.DbSchema);
            b.ConfigureByConvention(); //configuración automática para las propiedades de la clase base
            b.Property(x => x.Titulo).IsRequired().HasMaxLength(128);
            b.Property(x => x.Descripcion).IsRequired().HasMaxLength(1024);
        });

        // Configuración de la estructura de la tabla Favoritos[cite: 2, 4]
        builder.Entity<Favorite>(b =>
        {
            b.ToTable(TravelBuddyConsts.DbTablePrefix + "Favoritos", TravelBuddyConsts.DbSchema);
            b.ConfigureByConvention();

            // Definición explícita de clave primaria compuesta[cite: 2, 4]
            b.HasKey(x => new { x.UsuarioId, x.DestinoId });

            // Índices lógicos y restricciones relacionales explícitas[cite: 2, 4]
            b.HasIndex(x => new { x.UsuarioId, x.DestinoId }).IsUnique();
        });
    }
}