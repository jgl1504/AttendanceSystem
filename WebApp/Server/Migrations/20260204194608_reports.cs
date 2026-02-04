using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class reports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ed513dc3-a05a-41d3-8493-e6c18af55d38", "AQAAAAIAAYagAAAAELF5Fr5djG+94ssBKz/fj2DSqOY7Z8rkc7m/4Yb0ngcC56lzuJ3hjCcHfMT/teLhDQ==", "e82816d5-6114-41c7-9b6b-70b3fe2d35ea" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ae6fd075-fa25-47bb-b60c-e97795dc95b2", "AQAAAAIAAYagAAAAEF+WkjMiKSUe0Niap5wgthF9bzwbzLhIBxweH4OrCncTPbVGw1uBRxll7Sk2eWd6qQ==", "933cfcbe-bfab-4767-869f-2545a3e30ed4" });
        }
    }
}
