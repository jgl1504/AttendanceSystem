using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class changeleaverev3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Employees_OvertimeApprovedByEmployeeId",
                table: "AttendanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Sites_SiteId",
                table: "AttendanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRecords_LeaveTypes_LeaveTypeId",
                table: "LeaveRecords");

            migrationBuilder.AlterColumn<decimal>(
                name: "PaymentPercentage",
                table: "LeaveTypes",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DaysPerYear",
                table: "LeaveTypes",
                type: "decimal(7,2)",
                precision: 7,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DaysPerCycle",
                table: "LeaveTypes",
                type: "decimal(7,2)",
                precision: 7,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "OpeningBalance",
                table: "LeaveBalances",
                type: "decimal(7,2)",
                precision: 7,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d6cf6e15-e4e9-4308-85a7-1ad185a4c549", "AQAAAAIAAYagAAAAEEDciVU/9nK8m4AX640rH8W+tlzwHDKYrNrYlg0Yfve4Ex1/2q/RhtdR99giVTx51w==", "837df326-179a-4931-a490-a1b9c5a28d80" });

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Employees_OvertimeApprovedByEmployeeId",
                table: "AttendanceRecords",
                column: "OvertimeApprovedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Sites_SiteId",
                table: "AttendanceRecords",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRecords_LeaveTypes_LeaveTypeId",
                table: "LeaveRecords",
                column: "LeaveTypeId",
                principalTable: "LeaveTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Employees_OvertimeApprovedByEmployeeId",
                table: "AttendanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Sites_SiteId",
                table: "AttendanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRecords_LeaveTypes_LeaveTypeId",
                table: "LeaveRecords");

            migrationBuilder.AlterColumn<decimal>(
                name: "PaymentPercentage",
                table: "LeaveTypes",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "DaysPerYear",
                table: "LeaveTypes",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(7,2)",
                oldPrecision: 7,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "DaysPerCycle",
                table: "LeaveTypes",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(7,2)",
                oldPrecision: 7,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "OpeningBalance",
                table: "LeaveBalances",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(7,2)",
                oldPrecision: 7,
                oldScale: 2);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9854d8da-0fcf-489a-9a81-5674817b5a3c", "AQAAAAIAAYagAAAAELVNH5qyJvyZqeQKyXPNSKhUNVzHXXExfayYtTcvYLZN8AkQnSynk01WUwQnF/3o1w==", "1d9080f0-c256-4f06-bba7-86ab40beeddc" });

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Employees_OvertimeApprovedByEmployeeId",
                table: "AttendanceRecords",
                column: "OvertimeApprovedByEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Sites_SiteId",
                table: "AttendanceRecords",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRecords_LeaveTypes_LeaveTypeId",
                table: "LeaveRecords",
                column: "LeaveTypeId",
                principalTable: "LeaveTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
