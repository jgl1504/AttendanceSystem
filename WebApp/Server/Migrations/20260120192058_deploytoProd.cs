using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class deploytoProd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "DaysTaken",
                table: "LeaveRecords",
                type: "decimal(7,2)",
                precision: 7,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DaysBalance",
                table: "EmployeeLeaves",
                type: "decimal(7,2)",
                precision: 7,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "AccrualRatePerMonth",
                table: "EmployeeLeaves",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "seed-admin-user-id", 0, "970cabbe-27e4-4da1-ad59-27cbcb7ee6c8", "Projects@aics.co.za", true, false, null, "PROJECTS@AICS.CO.ZA", "PROJECTS@AICS.CO.ZA", "AQAAAAIAAYagAAAAEKu3Vd1r4dI82qlutpg00CinkH0hlA/E9X3qOvgB76n+ZkPqGg4BlpF3eFhAvXlMcg==", null, false, "ecc7349b-ce21-4114-9281-63fbba12b49f", false, "Projects@aics.co.za" });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DepartmentId", "Email", "IdentityUserId", "Name", "PasswordHash", "PasswordSalt", "Phone" },
                values: new object[] { 3, "Projects@aics.co.za", "seed-admin-user-id", "Zuleicke Visser", new byte[] { 115, 101, 101, 100, 45, 104, 97, 115, 104 }, new byte[] { 115, 101, 101, 100, 45, 115, 97, 108, 116 }, "" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id");

            migrationBuilder.AlterColumn<decimal>(
                name: "DaysTaken",
                table: "LeaveRecords",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(7,2)",
                oldPrecision: 7,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "DaysBalance",
                table: "EmployeeLeaves",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(7,2)",
                oldPrecision: 7,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "AccrualRatePerMonth",
                table: "EmployeeLeaves",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DepartmentId", "Email", "IdentityUserId", "Name", "PasswordHash", "PasswordSalt", "Phone" },
                values: new object[] { 1, "test@company.local", null, "Test Employee", new byte[0], new byte[0], "000-000-0000" });
        }
    }
}
