using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class overtimeNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OvertimeNote",
                table: "AttendanceRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f102acb6-e310-45b7-a520-431af1149266", "AQAAAAIAAYagAAAAEMYYgJ8jASPAWDFl+G89tInluPr+VCA2pm5/fGv27/9lEf96tSB4Pc95pfVbseOqtQ==", "c22f2a00-9f2f-4645-b45e-5b82366aedd7" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OvertimeNote",
                table: "AttendanceRecords");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "bd77bc6b-8869-4709-ab21-45ccf6fc5cf7", "AQAAAAIAAYagAAAAEEUBhpG9NFekNt9zfTNUDtHanD4d+1HGW8NSKfccRGv07Id10PB8EMNhjkksNea9HQ==", "343f3bf1-991a-42b8-a072-12b7ff1c0d15" });
        }
    }
}
