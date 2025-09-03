using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindShelf_DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddingUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "ShoppingCarts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserName",
                table: "ShoppingCarts");
        }
    }
}
