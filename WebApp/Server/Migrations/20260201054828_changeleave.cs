using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class changeleave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeaveType",
                table: "LeaveRecords");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentPath",
                table: "LeaveRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LeaveTypeId",
                table: "LeaveRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "LeaveTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColorCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PoolType = table.Column<int>(type: "int", nullable: false),
                    PrimaryPoolLeaveTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FallbackPoolLeaveTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AccrualType = table.Column<int>(type: "int", nullable: false),
                    DaysPerYear = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AccrualCycleDurationMonths = table.Column<int>(type: "int", nullable: true),
                    DaysPerCycle = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AllowsCarryover = table.Column<bool>(type: "bit", nullable: false),
                    MaxCarryoverDays = table.Column<int>(type: "int", nullable: false),
                    RequiresSupportingDocument = table.Column<bool>(type: "bit", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false),
                    MinNoticeDays = table.Column<int>(type: "int", nullable: false),
                    MaxConsecutiveDays = table.Column<int>(type: "int", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    PaymentPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsGenderSpecific = table.Column<bool>(type: "bit", nullable: false),
                    RequiredGender = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveTypes_LeaveTypes_FallbackPoolLeaveTypeId",
                        column: x => x.FallbackPoolLeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveTypes_LeaveTypes_PrimaryPoolLeaveTypeId",
                        column: x => x.PrimaryPoolLeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BalanceStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentCycleStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentCycleEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HasBeenUsed = table.Column<bool>(type: "bit", nullable: false),
                    UsedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveBalances_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaveBalances_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "89b3435c-8807-4ab3-8edd-46cf0810276e", "AQAAAAIAAYagAAAAEPcCNCaLjnjyiLSJP9IcxivQE5c/9ZV3H1UzqtOqxus+jAoaDkn4c+462GDQaGeaHg==", "31657232-c7d2-404f-b7b8-fc5cd03d4def" });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRecords_LeaveTypeId",
                table: "LeaveRecords",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_EmployeeId_LeaveTypeId",
                table: "LeaveBalances",
                columns: new[] { "EmployeeId", "LeaveTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_LeaveTypeId",
                table: "LeaveBalances",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTypes_FallbackPoolLeaveTypeId",
                table: "LeaveTypes",
                column: "FallbackPoolLeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTypes_PrimaryPoolLeaveTypeId",
                table: "LeaveTypes",
                column: "PrimaryPoolLeaveTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRecords_LeaveTypes_LeaveTypeId",
                table: "LeaveRecords",
                column: "LeaveTypeId",
                principalTable: "LeaveTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRecords_LeaveTypes_LeaveTypeId",
                table: "LeaveRecords");

            migrationBuilder.DropTable(
                name: "LeaveBalances");

            migrationBuilder.DropTable(
                name: "LeaveTypes");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRecords_LeaveTypeId",
                table: "LeaveRecords");

            migrationBuilder.DropColumn(
                name: "AttachmentPath",
                table: "LeaveRecords");

            migrationBuilder.DropColumn(
                name: "LeaveTypeId",
                table: "LeaveRecords");

            migrationBuilder.AddColumn<int>(
                name: "LeaveType",
                table: "LeaveRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "seed-admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "141749b9-c441-46ba-a8ae-ff3ea64c24a9", "AQAAAAIAAYagAAAAEDVglRXg1O4hpM3pgDZlRTl69K5JJDHJee697806reRWtk/ber858NnwYQPw1gRwgA==", "a78bc67f-f87d-4705-bf9b-ed1ab1acdefc" });
        }
    }
}
