using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelBuddy.Migrations
{
    /// <inheritdoc />
    public partial class InicialModulo3ConGeoDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoriaId",
                table: "AppDestinations");

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "AppDestinations");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "AppDestinations");

            migrationBuilder.RenameColumn(
                name: "Ubicacion",
                table: "AppDestinations",
                newName: "Region");

            migrationBuilder.RenameColumn(
                name: "Precio",
                table: "AppDestinations",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "AppDestinations",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ImagenUrl",
                table: "AppDestinations",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "Disponible",
                table: "AppDestinations",
                newName: "IsAvailable");

            migrationBuilder.RenameColumn(
                name: "Descripcion",
                table: "AppDestinations",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "AppDestinations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "GeoDbCityId",
                table: "AppDestinations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppExperiencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Valoracion = table.Column<int>(type: "int", nullable: false),
                    PalabrasClave = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppExperiencias", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppDestinations_GeoDbCityId",
                table: "AppDestinations",
                column: "GeoDbCityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppExperiencias");

            migrationBuilder.DropIndex(
                name: "IX_AppDestinations_GeoDbCityId",
                table: "AppDestinations");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "AppDestinations");

            migrationBuilder.DropColumn(
                name: "GeoDbCityId",
                table: "AppDestinations");

            migrationBuilder.RenameColumn(
                name: "Region",
                table: "AppDestinations",
                newName: "Ubicacion");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "AppDestinations",
                newName: "Precio");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AppDestinations",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "IsAvailable",
                table: "AppDestinations",
                newName: "Disponible");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "AppDestinations",
                newName: "ImagenUrl");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "AppDestinations",
                newName: "Descripcion");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoriaId",
                table: "AppDestinations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacion",
                table: "AppDestinations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "AppDestinations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
