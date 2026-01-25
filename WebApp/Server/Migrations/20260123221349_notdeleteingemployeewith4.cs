using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class notdeleteingemployeewith4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "509f41b3-8f01-4657-9219-599cdc1a6544", "AQAAAAIAAYagAAAAENRkBgSNH2o/y5gGIDyhMXn2ux6WtPuw6NwfOSw+9+lklbvM2n5FqgN7xhiupCbqdQ==", "18645157-917c-4ca1-bf0a-d2c2802d0473" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8ac176f0-40d6-43c7-b5b5-14113f65afa3", "AQAAAAIAAYagAAAAEP+W0NWWsLKavM78lF9vcsMQMoExhugWYvwvK1a9xvt7SyDu+p95M9pCz2rVTFVe2w==", "8f2e4247-76a3-4f73-9a68-dde0b15707c7" });
        }
    }
}
