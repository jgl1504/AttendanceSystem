using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class notdeleteingemployeewithagain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8d07e844-c08f-48ae-830d-99e946b6e3ad", "AQAAAAIAAYagAAAAEGCJnUu75WHsmNJF+XtGfjORsKCAdMLvbBwfcBKhfBizwjfZIgdNVeK+bpFPGdFuHA==", "e29e5b1f-4d89-432c-9adb-b6dad73566b5" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "509f41b3-8f01-4657-9219-599cdc1a6544", "AQAAAAIAAYagAAAAENRkBgSNH2o/y5gGIDyhMXn2ux6WtPuw6NwfOSw+9+lklbvM2n5FqgN7xhiupCbqdQ==", "18645157-917c-4ca1-bf0a-d2c2802d0473" });
        }
    }
}
