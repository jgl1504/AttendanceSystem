using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class changeleavebalance7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "26359aaf-92f6-4f14-9355-9327249429c9", "AQAAAAIAAYagAAAAEIfKVCxm/V9ZGpWEVgeyRC2OmoRZJGt9349m6oGh7ds3RkJ8PgOjGVtPuTtQnaUiGQ==", "b369c4f9-e306-48ab-a8ec-260f3ab6df22" });

            migrationBuilder.AddColumn<decimal>(
              name: "CurrentBalance",
              table: "LeaveBalances",
              type: "decimal(10,2)",
              nullable: false,
              defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f3b2652d-840b-43fa-acc8-b82a09046b4f", "AQAAAAIAAYagAAAAEE145z1QzgbF6UfWuRzhUX3JUvBweK6abNH6Ns6C46OtVsQ4B3g1Gp1HLsayIFsBnw==", "22e5b83f-eb71-4a1f-a6b6-193715d013dc" });
        }
    }
}
