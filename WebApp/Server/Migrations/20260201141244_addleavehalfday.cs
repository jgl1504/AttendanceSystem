using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class addleavehalfday : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowsHalfDays",
                table: "LeaveTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentBalance",
                table: "LeaveBalances",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5406cc53-58c4-4862-ad0a-5d2adc1d5398", "AQAAAAIAAYagAAAAEBQK+lJSqSvhJouBOmYgJW4yS9/Tncr7a0Y/LNoGuZ+1OwVeMjKaKoQL2ur6ClW7Qg==", "82e43710-2782-4fec-999a-31f4a0336308" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowsHalfDays",
                table: "LeaveTypes");

            migrationBuilder.DropColumn(
                name: "CurrentBalance",
                table: "LeaveBalances");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "26359aaf-92f6-4f14-9355-9327249429c9", "AQAAAAIAAYagAAAAEIfKVCxm/V9ZGpWEVgeyRC2OmoRZJGt9349m6oGh7ds3RkJ8PgOjGVtPuTtQnaUiGQ==", "b369c4f9-e306-48ab-a8ec-260f3ab6df22" });
        }
    }
}
