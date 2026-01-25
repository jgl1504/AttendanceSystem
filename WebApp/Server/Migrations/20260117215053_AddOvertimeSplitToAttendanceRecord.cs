using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddOvertimeSplitToAttendanceRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "HoursWorked",
                table: "AttendanceRecords",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "OvertimeHours",
                table: "AttendanceRecords",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SundayPublicOvertimeHours",
                table: "AttendanceRecords",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "WeekdayOvertimeHours",
                table: "AttendanceRecords",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HoursWorked",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "OvertimeHours",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "SundayPublicOvertimeHours",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "WeekdayOvertimeHours",
                table: "AttendanceRecords");
        }
    }
}
