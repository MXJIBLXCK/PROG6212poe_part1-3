using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay_EventMS.Migrations
{
    /// <inheritdoc />
    public partial class RenameTitleToEventName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Events",
                newName: "EventName");

            migrationBuilder.AddColumn<string>(
                name: "BannerUrl",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannerUrl",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "EventName",
                table: "Events",
                newName: "Title");
        }
    }
}