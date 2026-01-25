using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class HalfDayLeave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Portion",
                table: "LeaveRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "116300a7-d78f-4559-b19d-c29ee13685bb", "AQAAAAIAAYagAAAAEBubIlL9Whk/p45q0oTifmsA8tPSv4O9cTOsvQ2pozBSkRdoGUVDYLwKWy7JuSBvUA==", "ecbc546e-fc4f-482c-b8a4-dc5afe753adf" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Portion",
                table: "LeaveRecords");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "eb5b7881-eada-4e24-bcb2-8f1d68a98202", "AQAAAAIAAYagAAAAENII6OXfnBM41+mFV0HGKc/R/jGxTuCOmGlQPy5paUzr7nh+aWhRZmiC60eeDpF4SA==", "5135f2e1-e02b-4c33-852d-88f5add3fcee" });
        }
    }
}
