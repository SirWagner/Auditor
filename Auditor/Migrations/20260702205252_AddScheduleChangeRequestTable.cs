using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auditor.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleChangeRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "schedule_change_request",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    schedule_id = table.Column<long>(type: "bigint", nullable: false),
                    requested_by = table.Column<long>(type: "bigint", nullable: false),
                    authorizer_id = table.Column<long>(type: "bigint", nullable: true),
                    reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    request_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    decision_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    escalation_level = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedule_change_request", x => x.id);
                    table.ForeignKey(
                        name: "FK__sched_chg__auth__req03",
                        column: x => x.authorizer_id,
                        principalTable: "app_user",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__sched_chg__reqby__req02",
                        column: x => x.requested_by,
                        principalTable: "app_user",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__sched_chg__sched__req01",
                        column: x => x.schedule_id,
                        principalTable: "audit_schedule",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_schedule_change_request_authorizer_id",
                table: "schedule_change_request",
                column: "authorizer_id");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_change_request_requested_by",
                table: "schedule_change_request",
                column: "requested_by");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_change_request_schedule_id",
                table: "schedule_change_request",
                column: "schedule_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "schedule_change_request");
        }
    }
}
