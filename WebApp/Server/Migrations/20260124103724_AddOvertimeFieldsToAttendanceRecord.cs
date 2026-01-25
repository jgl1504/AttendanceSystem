using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddOvertimeFieldsToAttendanceRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OvertimeApprovedByEmployeeId",
                table: "AttendanceRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OvertimeDecisionTime",
                table: "AttendanceRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OvertimeLocation",
                table: "AttendanceRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8326d973-b8b1-4b2c-9af6-5ee60746129b", "AQAAAAIAAYagAAAAELuyJFMKCVzgebtzOLoTodfogXM+V7ves8fO014y+xuWF4TCS9T/z3Fzle70/Fuc0Q==", "bb8551a0-10c5-4094-91d8-9feb91701286" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_OvertimeApprovedByEmployeeId",
                table: "AttendanceRecords",
                column: "OvertimeApprovedByEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Employees_OvertimeApprovedByEmployeeId",
                table: "AttendanceRecords",
                column: "OvertimeApprovedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Employees_OvertimeApprovedByEmployeeId",
                table: "AttendanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_OvertimeApprovedByEmployeeId",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "OvertimeApprovedByEmployeeId",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "OvertimeDecisionTime",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "OvertimeLocation",
                table: "AttendanceRecords");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f102acb6-e310-45b7-a520-431af1149266", "AQAAAAIAAYagAAAAEMYYgJ8jASPAWDFl+G89tInluPr+VCA2pm5/fGv27/9lEf96tSB4Pc95pfVbseOqtQ==", "c22f2a00-9f2f-4645-b45e-5b82366aedd7" });
        }
    }
}
