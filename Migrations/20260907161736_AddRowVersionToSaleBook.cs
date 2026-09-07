using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KutubxonaAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionToSaleBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SaleBooks",
                type: "rowversion",
                rowVersion: true,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SaleBooks");
        }
    }
}
