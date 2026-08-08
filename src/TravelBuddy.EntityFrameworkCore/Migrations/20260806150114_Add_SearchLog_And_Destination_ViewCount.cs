using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelBuddy.Migrations
{
    /// <inheritdoc />
    public partial class Add_SearchLog_And_Destination_ViewCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Experiencias",
                table: "Experiencias");

            migrationBuilder.RenameTable(
                name: "Experiencias",
                newName: "AppExperiencias");

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "AppDestinations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppExperiencias",
                table: "AppExperiencias",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AppExperiencias",
                table: "AppExperiencias");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "AppDestinations");

            migrationBuilder.RenameTable(
                name: "AppExperiencias",
                newName: "Experiencias");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Experiencias",
                table: "Experiencias",
                column: "Id");
        }
    }
}
