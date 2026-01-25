using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkCategoryToAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkCategory",
                table: "AttendanceRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "eb5b7881-eada-4e24-bcb2-8f1d68a98202", "AQAAAAIAAYagAAAAENII6OXfnBM41+mFV0HGKc/R/jGxTuCOmGlQPy5paUzr7nh+aWhRZmiC60eeDpF4SA==", "5135f2e1-e02b-4c33-852d-88f5add3fcee" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkCategory",
                table: "AttendanceRecords");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8326d973-b8b1-4b2c-9af6-5ee60746129b", "AQAAAAIAAYagAAAAELuyJFMKCVzgebtzOLoTodfogXM+V7ves8fO014y+xuWF4TCS9T/z3Fzle70/Fuc0Q==", "bb8551a0-10c5-4094-91d8-9feb91701286" });
        }
    }
}
