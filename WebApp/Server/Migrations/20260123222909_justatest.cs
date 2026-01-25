using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class justatest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "bd77bc6b-8869-4709-ab21-45ccf6fc5cf7", "AQAAAAIAAYagAAAAEEUBhpG9NFekNt9zfTNUDtHanD4d+1HGW8NSKfccRGv07Id10PB8EMNhjkksNea9HQ==", "343f3bf1-991a-42b8-a072-12b7ff1c0d15" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8d07e844-c08f-48ae-830d-99e946b6e3ad", "AQAAAAIAAYagAAAAEGCJnUu75WHsmNJF+XtGfjORsKCAdMLvbBwfcBKhfBizwjfZIgdNVeK+bpFPGdFuHA==", "e29e5b1f-4d89-432c-9adb-b6dad73566b5" });
        }
    }
}
