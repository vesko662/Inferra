using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inferra.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAssetSupplyPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TotalSupply",
                table: "Assets",
                type: "decimal(38,8)",
                precision: 38,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxSupply",
                table: "Assets",
                type: "decimal(38,8)",
                precision: 38,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CirculatingSupply",
                table: "Assets",
                type: "decimal(38,8)",
                precision: 38,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TotalSupply",
                table: "Assets",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(38,8)",
                oldPrecision: 38,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxSupply",
                table: "Assets",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(38,8)",
                oldPrecision: 38,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CirculatingSupply",
                table: "Assets",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(38,8)",
                oldPrecision: 38,
                oldScale: 8,
                oldNullable: true);
        }
    }
}
