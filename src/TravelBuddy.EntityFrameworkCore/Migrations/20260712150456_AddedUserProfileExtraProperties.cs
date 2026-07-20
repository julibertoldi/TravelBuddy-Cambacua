using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelBuddy.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserProfileExtraProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Preferences",
                table: "AbpUsers",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "AbpUsers",
                type: "nvarchar(1000)",
                maxLength: 1000,
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppExperiencias");

            migrationBuilder.DropColumn(
                name: "Preferences",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "AbpUsers");
        }
    }
}
