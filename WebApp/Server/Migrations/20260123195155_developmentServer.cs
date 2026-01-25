using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class developmentServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "1792e246-20a5-46ae-8f93-d38749742138", "AQAAAAIAAYagAAAAEBsa+KMR0Jom/bZEXtgZqbS2TDoJxlVOmh2T/7iHo4y4Xyynvlvrq4kinlIqdwrdTg==", "7ae5ada2-7d53-42e6-a4bd-e978c75a538f" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a3b7cf3c-6e3a-4ddf-9b6c-1642eb8d68ef", "AQAAAAIAAYagAAAAEIS8Qk6DH3+DgpqbTTJwcr/Nsu4ORUVlQKQ0FB/w11WQGi/sbB8JmkOnGBmQJl4Ohw==", "cfd89291-3918-4139-82d4-b7674708e1a9" });
        }
    }
}
