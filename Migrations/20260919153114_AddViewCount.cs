using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KutubxonaAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddViewCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "SaleBooks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "SaleBooks");
        }
    }
}
