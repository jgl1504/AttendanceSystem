using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddOvertimewithDenyAndApprove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OvertimeApproved",
                table: "AttendanceRecords");

            migrationBuilder.AddColumn<int>(
                name: "OvertimeStatus",
                table: "AttendanceRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OvertimeStatus",
                table: "AttendanceRecords");

            migrationBuilder.AddColumn<bool>(
                name: "OvertimeApproved",
                table: "AttendanceRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
