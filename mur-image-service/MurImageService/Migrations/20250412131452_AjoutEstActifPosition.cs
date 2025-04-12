using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MurImageService.Migrations
{
    /// <inheritdoc />
    public partial class AjoutEstActifPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "est_actif",
                table: "position",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "est_actif",
                table: "position");
        }
    }
}
