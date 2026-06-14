using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KutubxonaAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookPages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PageNumber = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookPages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookPages_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Author", "Category", "CreatedAt", "IsAvailable", "Title", "UpdatedAt", "Year" },
                values: new object[,]
                {
                    { 1, "Abdulla Qodiriy", "Roman", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "O'tgan kunlar", null, 1922 },
                    { 2, "Abdulla Qodiriy", "Roman", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Mehrobdan chayon", null, 1929 },
                    { 3, "Tohir Malik", "Roman", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Shaytanat", null, 1996 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookPages_BookId",
                table: "BookPages",
                column: "BookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookPages");

            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
