using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MurImageService.Migrations
{
    /// <inheritdoc />
    public partial class codeAjout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "nom_camera",
                table: "position",
                newName: "code_camera");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "code_camera",
                table: "position",
                newName: "nom_camera");
        }
    }
}
