using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class LeaveAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentFileName",
                table: "LeaveRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f3c34710-34ad-44d9-8b58-9de86f35e920", "AQAAAAIAAYagAAAAEJedFo3eRaKWBsfpsMvWc7/yqYoaSgEtgS0H99Or5Dkqg7saQqemYF3LbCciCJAKEg==", "88a9f537-6571-4620-8a4f-86f55602fe1d" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentFileName",
                table: "LeaveRecords");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "116300a7-d78f-4559-b19d-c29ee13685bb", "AQAAAAIAAYagAAAAEBubIlL9Whk/p45q0oTifmsA8tPSv4O9cTOsvQ2pozBSkRdoGUVDYLwKWy7JuSBvUA==", "ecbc546e-fc4f-482c-b8a4-dc5afe753adf" });
        }
    }
}
