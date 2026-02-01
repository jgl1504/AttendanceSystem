using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class addleavehalfday3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Change precision on LeaveBalances
            migrationBuilder.AlterColumn<decimal>(
                name: "OpeningBalance",
                table: "LeaveBalances",
                type: "decimal(9,2)",
                precision: 9,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(7,2)",
                oldPrecision: 7,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentBalance",
                table: "LeaveBalances",
                type: "decimal(9,2)",
                precision: 9,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            // New flag on LeaveTypes
            migrationBuilder.AddColumn<bool>(
                name: "AllowsHalfDays",
                table: "LeaveTypes",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[]
                {
                    "ae6fd075-fa25-47bb-b60c-e97795dc95b2",
                    "AQAAAAIAAYagAAAAEF+WkjMiKSUe0Niap5wgthF9bzwbzLhIBxweH4OrCncTPbVGw1uBRxll7Sk2eWd6qQ==",
                    "933cfcbe-bfab-4767-869f-2545a3e30ed4"
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove flag
            migrationBuilder.DropColumn(
                name: "AllowsHalfDays",
                table: "LeaveTypes");

            // Revert precision on LeaveBalances
            migrationBuilder.AlterColumn<decimal>(
                name: "OpeningBalance",
                table: "LeaveBalances",
                type: "decimal(7,2)",
                precision: 7,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,2)",
                oldPrecision: 9,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentBalance",
                table: "LeaveBalances",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,2)",
                oldPrecision: 9,
                oldScale: 2);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[]
                {
                    "5406cc53-58c4-4862-ad0a-5d2adc1d5398",
                    "AQAAAAIAAYagAAAAEBQK+lJSqSvhJouBOmYgJW4yS9/Tncr7a0Y/LNoGuZ+1OwVeMjKaKoQL2ur6ClW7Qg==",
                    "82e43710-2782-4fec-999a-31f4a0336308"
                });
        }
    }
}
