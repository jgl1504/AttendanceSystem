using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class addsiteclockin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SiteId",
                table: "AttendanceRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "141749b9-c441-46ba-a8ae-ff3ea64c24a9", "AQAAAAIAAYagAAAAEDVglRXg1O4hpM3pgDZlRTl69K5JJDHJee697806reRWtk/ber858NnwYQPw1gRwgA==", "a78bc67f-f87d-4705-bf9b-ed1ab1acdefc" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_SiteId",
                table: "AttendanceRecords",
                column: "SiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Sites_SiteId",
                table: "AttendanceRecords",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Sites_SiteId",
                table: "AttendanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_SiteId",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "SiteId",
                table: "AttendanceRecords");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ee1eee8a-c835-438f-9dfb-9ad25cc6c8e3", "AQAAAAIAAYagAAAAEO/GwaMT+0GfLHvaGZIGp+/KrgqqTI0p5UVNLl9Yzn+/rGCr+aMRpiKvYbMibCgwUw==", "de9fdb9c-5ea3-42cf-9d08-aeea39fb2b0f" });
        }
    }
}
