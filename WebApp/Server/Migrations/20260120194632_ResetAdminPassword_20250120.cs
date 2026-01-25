using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class ResetAdminPassword_20250120 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a3b7cf3c-6e3a-4ddf-9b6c-1642eb8d68ef", "AQAAAAIAAYagAAAAEIS8Qk6DH3+DgpqbTTJwcr/Nsu4ORUVlQKQ0FB/w11WQGi/sbB8JmkOnGBmQJl4Ohw==", "cfd89291-3918-4139-82d4-b7674708e1a9" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "970cabbe-27e4-4da1-ad59-27cbcb7ee6c8", "AQAAAAIAAYagAAAAEKu3Vd1r4dI82qlutpg00CinkH0hlA/E9X3qOvgB76n+ZkPqGg4BlpF3eFhAvXlMcg==", "ecc7349b-ce21-4114-9281-63fbba12b49f" });
        }
    }
}
