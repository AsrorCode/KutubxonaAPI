using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KutubxonaAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddGalleryAndVotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookVotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SaleBookId = table.Column<int>(type: "int", nullable: false),
                    VoteType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookVotes_SaleBooks_SaleBookId",
                        column: x => x.SaleBookId,
                        principalTable: "SaleBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookVotes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaleBookImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SaleBookId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleBookImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleBookImages_SaleBooks_SaleBookId",
                        column: x => x.SaleBookId,
                        principalTable: "SaleBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookVotes_SaleBookId",
                table: "BookVotes",
                column: "SaleBookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookVotes_UserId_SaleBookId",
                table: "BookVotes",
                columns: new[] { "UserId", "SaleBookId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleBookImages_SaleBookId",
                table: "SaleBookImages",
                column: "SaleBookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookVotes");

            migrationBuilder.DropTable(
                name: "SaleBookImages");
        }
    }
}
