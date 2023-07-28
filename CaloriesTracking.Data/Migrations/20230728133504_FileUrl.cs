using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaloriesTracking.Data.Migrations
{
    /// <inheritdoc />
    public partial class FileUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Url",
                table: "AspNetUsers",
                newName: "FileUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileUrl",
                table: "AspNetUsers",
                newName: "Url");
        }
    }
}
