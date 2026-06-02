using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inferra.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddForecastEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ForecastRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    ModelType = table.Column<int>(type: "int", nullable: false),
                    ModelVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ForecastStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ForecastEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    HorizonDays = table.Column<int>(type: "int", nullable: false),
                    IsLatest = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForecastRuns_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ForecastPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ForecastRunId = table.Column<int>(type: "int", nullable: false),
                    DayOffset = table.Column<int>(type: "int", nullable: false),
                    TargetDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PredictedPrice = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    LowerBound = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: true),
                    UpperBound = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: true),
                    Confidence = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForecastPoints_ForecastRuns_ForecastRunId",
                        column: x => x.ForecastRunId,
                        principalTable: "ForecastRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ForecastPoints_ForecastRunId_DayOffset",
                table: "ForecastPoints",
                columns: new[] { "ForecastRunId", "DayOffset" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForecastPoints_ForecastRunId_TargetDate",
                table: "ForecastPoints",
                columns: new[] { "ForecastRunId", "TargetDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForecastRuns_AssetId_ModelType_GeneratedAt",
                table: "ForecastRuns",
                columns: new[] { "AssetId", "ModelType", "GeneratedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ForecastRuns_AssetId_ModelType_IsLatest",
                table: "ForecastRuns",
                columns: new[] { "AssetId", "ModelType", "IsLatest" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForecastPoints");

            migrationBuilder.DropTable(
                name: "ForecastRuns");
        }
    }
}
