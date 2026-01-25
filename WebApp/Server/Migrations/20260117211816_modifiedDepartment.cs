using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class modifiedDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultEndTime",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "DefaultStartTime",
                table: "Departments");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "BreakPerDay",
                table: "Departments",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "DailyEndTime",
                table: "Departments",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "DailyStartTime",
                table: "Departments",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<decimal>(
                name: "RequiredHoursPerWeek",
                table: "Departments",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "RotatingWeekends",
                table: "Departments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SaturdaysPerMonthRequired",
                table: "Departments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "WorksSaturday",
                table: "Departments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BreakPerDay", "DailyEndTime", "DailyStartTime", "RequiredHoursPerWeek", "RotatingWeekends", "SaturdaysPerMonthRequired", "WorksSaturday" },
                values: new object[] { new TimeSpan(0, 1, 0, 0, 0), new TimeSpan(0, 17, 0, 0, 0), new TimeSpan(0, 7, 30, 0, 0), 45m, true, 3, true });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BreakPerDay", "DailyEndTime", "DailyStartTime", "RequiredHoursPerWeek", "RotatingWeekends", "SaturdaysPerMonthRequired", "WorksSaturday" },
                values: new object[] { new TimeSpan(0, 1, 0, 0, 0), new TimeSpan(0, 17, 30, 0, 0), new TimeSpan(0, 8, 0, 0, 0), 45m, true, 3, true });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BreakPerDay", "DailyEndTime", "DailyStartTime", "RequiredHoursPerWeek", "RotatingWeekends", "SaturdaysPerMonthRequired", "WorksSaturday" },
                values: new object[] { new TimeSpan(0, 1, 0, 0, 0), new TimeSpan(0, 16, 30, 0, 0), new TimeSpan(0, 8, 0, 0, 0), 40m, false, 0, false });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                column: "HireDate",
                value: new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreakPerDay",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "DailyEndTime",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "DailyStartTime",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "RequiredHoursPerWeek",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "RotatingWeekends",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "SaturdaysPerMonthRequired",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "WorksSaturday",
                table: "Departments");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "DefaultEndTime",
                table: "Departments",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "DefaultStartTime",
                table: "Departments",
                type: "time",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DefaultEndTime", "DefaultStartTime" },
                values: new object[] { new TimeSpan(0, 16, 30, 0, 0), new TimeSpan(0, 7, 30, 0, 0) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DefaultEndTime", "DefaultStartTime" },
                values: new object[] { new TimeSpan(0, 17, 0, 0, 0), new TimeSpan(0, 8, 0, 0, 0) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DefaultEndTime", "DefaultStartTime" },
                values: new object[] { new TimeSpan(0, 16, 30, 0, 0), new TimeSpan(0, 8, 0, 0, 0) });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                column: "HireDate",
                value: new DateTime(2026, 1, 16, 17, 25, 37, 545, DateTimeKind.Local).AddTicks(6695));
        }
    }
}
