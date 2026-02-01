using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class changeleavebalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f3b2652d-840b-43fa-acc8-b82a09046b4f", "AQAAAAIAAYagAAAAEE145z1QzgbF6UfWuRzhUX3JUvBweK6abNH6Ns6C46OtVsQ4B3g1Gp1HLsayIFsBnw==", "22e5b83f-eb71-4a1f-a6b6-193715d013dc" });


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
                values: new object[] { "d6cf6e15-e4e9-4308-85a7-1ad185a4c549", "AQAAAAIAAYagAAAAEEDciVU/9nK8m4AX640rH8W+tlzwHDKYrNrYlg0Yfve4Ex1/2q/RhtdR99giVTx51w==", "837df326-179a-4931-a490-a1b9c5a28d80" });
        }
    }
}
