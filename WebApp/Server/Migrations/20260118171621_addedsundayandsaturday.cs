using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class addedsundayandsaturday : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SaturdayHours",
                table: "Departments",
                type: "decimal(4,2)",
                precision: 4,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SundayHours",
                table: "Departments",
                type: "decimal(4,2)",
                precision: 4,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "WorksSunday",
                table: "Departments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "SaturdayHours", "SundayHours", "WorksSunday" },
                values: new object[] { 5m, 0m, false });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "SaturdayHours", "SundayHours", "WorksSunday" },
                values: new object[] { 5m, 0m, false });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "SaturdayHours", "SundayHours", "WorksSunday" },
                values: new object[] { 0m, 0m, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SaturdayHours",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "SundayHours",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "WorksSunday",
                table: "Departments");
        }
    }
}
