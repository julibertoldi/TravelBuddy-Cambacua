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
    public DbSet<Calificaciones.Calificacion> Calificaciones { get; set; }
    public DbSet<Favorite> Favoritos { get; set; }


    #region Entities from the modules
    // Identity
    /* Notice: We only implemented IIdentityProDbContext 
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext .
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    /* Notice: We only implemented IIdentityProDbContext 
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext .
     *
        /* Incluye módulos en el contexto de base de datos de la migración */
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }

        builder.Entity<Destination>(b =>

        /* Configuracion tablas/entidades aquí */
        builder.Entity<Destinations.Destination>(b =>
        /* Include modules to your migration db context */

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
            b.ConfigureByConvention();//configuración automática para las propiedades de la clase base
            b.Property(x => x.Nombre).IsRequired().HasMaxLength(200);
            b.Property(x => x.Descripcion).HasMaxLength(1000);
            b.Property(x => x.Ubicacion).HasMaxLength(500);
            b.Property(x => x.Precio).HasColumnType("decimal(18,2)");
            b.Property(x => x.ImagenUrl).HasMaxLength(1000);
            b.Property(x => x.Disponible).IsRequired();
            b.Property(x => x.FechaCreacion).IsRequired();
            b.Property(x => x.FechaActualizacion).IsRequired();
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.Nombre).IsRequired().HasMaxLength(200);
            b.Property(x => x.Descripcion).HasMaxLength(1000);
            b.Property(x => x.Ubicacion).HasMaxLength(500);
            b.Property(x => x.Precio).HasColumnType("decimal(18,2)");
            b.ConfigureByConvention();
            b.ConfigureByConvention(); //configuración automática para las propiedades de la clase base
            b.Property(x => x.Disponible).IsRequired();
            b.Property(x => x.FechaCreacion).IsRequired();
            b.Property(x => x.FechaActualizacion).IsRequired();
            /*b.HasMany(x => x.Reservas).WithOne().HasForeignKey("DestinationId").OnDelete(DeleteBehavior.Cascade);*/
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
            b.ToTable(TravelBuddyConsts.DbTablePrefix + "Destinations", TravelBuddyConsts.DbSchema);
            b.ConfigureByConvention();//configuración automática para las propiedades de la clase base
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.Descripcion).HasMaxLength(1000);
            b.Property(x => x.Ubicacion).HasMaxLength(500);
            b.Property(x => x.Precio).HasColumnType("decimal(18,2)");
            b.Property(x => x.ImagenUrl).HasMaxLength(1000);
        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(TravelBuddyConsts.DbTablePrefix + "YourEntities", TravelBuddyConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});

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