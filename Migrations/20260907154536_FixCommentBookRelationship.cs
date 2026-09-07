using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KutubxonaAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixCommentBookRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Books_BookId1",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_BookId1",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "BookId1",
                table: "Comments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookId1",
                table: "Comments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_BookId1",
                table: "Comments",
                column: "BookId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Books_BookId1",
                table: "Comments",
                column: "BookId1",
                principalTable: "Books",
                principalColumn: "Id");
        }
    }
}
