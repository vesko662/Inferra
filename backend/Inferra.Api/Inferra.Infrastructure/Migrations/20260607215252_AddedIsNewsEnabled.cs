using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inferra.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsNewsEnabled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNewsEnabled",
                table: "Assets",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNewsEnabled",
                table: "Assets");
        }
    }
}
