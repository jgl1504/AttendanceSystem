using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class overtimemod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ApprovedOvertimeHours",
                table: "AttendanceRecords",
                type: "float",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "dec88280-5449-47c7-aa07-dfb037477295", "AQAAAAIAAYagAAAAEFfK0eciurXMDXTaLEuLIp6Q37KLuqjzKuFToPakHMU0LcpbD2nZWXGrp3wIqL8/fg==", "e7e21533-8695-4aa7-82d1-645a3df807ba" });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 51, 21, 153, DateTimeKind.Utc).AddTicks(7017));

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 51, 21, 153, DateTimeKind.Utc).AddTicks(7022));

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 51, 21, 153, DateTimeKind.Utc).AddTicks(7025));

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 51, 21, 153, DateTimeKind.Utc).AddTicks(7027));

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 51, 21, 153, DateTimeKind.Utc).AddTicks(7029));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: new Guid("28606668-b6e2-431d-9785-1fb3ab3dafe6"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 51, 21, 153, DateTimeKind.Utc).AddTicks(7188));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: new Guid("34deebab-cea1-42d3-a537-b45bfb594aaa"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 51, 21, 153, DateTimeKind.Utc).AddTicks(7237));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: new Guid("6f90f1bf-a17f-48b9-8dc5-5b63374d205f"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 51, 21, 153, DateTimeKind.Utc).AddTicks(7215));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: new Guid("7f80a868-b953-46d7-8a95-6dd2319ae491"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 51, 21, 153, DateTimeKind.Utc).AddTicks(7230));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: new Guid("badd5389-b6d5-4032-8a42-fbc9939c7ab4"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 51, 21, 153, DateTimeKind.Utc).AddTicks(7245));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: new Guid("f929540c-4b73-4e0c-b5d0-845c6a2fc4cf"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 51, 21, 153, DateTimeKind.Utc).AddTicks(7233));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: new Guid("fff0bddb-42ba-4cab-8cc7-d02a6ee5b1c1"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 51, 21, 153, DateTimeKind.Utc).AddTicks(7241));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedOvertimeHours",
                table: "AttendanceRecords");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7193464c-d060-4921-bcd1-ccf0ad37e506", "AQAAAAIAAYagAAAAECSHYHmuWeTFLLXSHI0Hi4Vqdi4GimyV0Qnt47qqcPntXKZWb65UOD8lK4C8E3HIEw==", "a7f33eca-6b6e-49fb-a3c5-fcd1232c7430" });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 16, 20, 40, 3, 851, DateTimeKind.Utc).AddTicks(7624));

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 16, 20, 40, 3, 851, DateTimeKind.Utc).AddTicks(7628));

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 16, 20, 40, 3, 851, DateTimeKind.Utc).AddTicks(7630));

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 16, 20, 40, 3, 851, DateTimeKind.Utc).AddTicks(7632));

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 16, 20, 40, 3, 851, DateTimeKind.Utc).AddTicks(7634));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: new Guid("28606668-b6e2-431d-9785-1fb3ab3dafe6"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 16, 20, 40, 3, 851, DateTimeKind.Utc).AddTicks(7779));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: new Guid("34deebab-cea1-42d3-a537-b45bfb594aaa"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 16, 20, 40, 3, 851, DateTimeKind.Utc).AddTicks(7819));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: new Guid("6f90f1bf-a17f-48b9-8dc5-5b63374d205f"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 16, 20, 40, 3, 851, DateTimeKind.Utc).AddTicks(7800));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: new Guid("7f80a868-b953-46d7-8a95-6dd2319ae491"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 16, 20, 40, 3, 851, DateTimeKind.Utc).AddTicks(7812));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: new Guid("badd5389-b6d5-4032-8a42-fbc9939c7ab4"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 16, 20, 40, 3, 851, DateTimeKind.Utc).AddTicks(7825));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: new Guid("f929540c-4b73-4e0c-b5d0-845c6a2fc4cf"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 16, 20, 40, 3, 851, DateTimeKind.Utc).AddTicks(7815));

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: new Guid("fff0bddb-42ba-4cab-8cc7-d02a6ee5b1c1"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 16, 20, 40, 3, 851, DateTimeKind.Utc).AddTicks(7822));
        }
    }
}
