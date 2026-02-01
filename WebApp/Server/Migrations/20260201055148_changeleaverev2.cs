using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class changeleaverev2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9854d8da-0fcf-489a-9a81-5674817b5a3c", "AQAAAAIAAYagAAAAELVNH5qyJvyZqeQKyXPNSKhUNVzHXXExfayYtTcvYLZN8AkQnSynk01WUwQnF/3o1w==", "1d9080f0-c256-4f06-bba7-86ab40beeddc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "89b3435c-8807-4ab3-8edd-46cf0810276e", "AQAAAAIAAYagAAAAEPcCNCaLjnjyiLSJP9IcxivQE5c/9ZV3H1UzqtOqxus+jAoaDkn4c+462GDQaGeaHg==", "31657232-c7d2-404f-b7b8-fc5cd03d4def" });
        }
    }
}
