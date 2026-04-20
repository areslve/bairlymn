using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BairlyMN.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImagePathToChatMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "ChatMessages");
        }
    }
}
