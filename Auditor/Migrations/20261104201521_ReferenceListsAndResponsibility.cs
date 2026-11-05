using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auditor.Migrations
{
    /// <inheritdoc />
    public partial class ReferenceListsAndResponsibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "reference_list_id",
                table: "question_bank",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "responsibility",
                table: "audit_template_item",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "service_provider",
                table: "audit_template_item",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "service_provider",
                table: "audit_site",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "responsibility",
                table: "audit_response",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "service_provider",
                table: "audit_response",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "reference_list",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_list", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reference_list_item",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    list_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    group_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    airport_id = table.Column<long>(type: "bigint", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    display_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_list_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_reference_list_item_airport",
                        column: x => x.airport_id,
                        principalTable: "airport",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reference_list_item_list",
                        column: x => x.list_id,
                        principalTable: "reference_list",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_question_bank_reference_list_id",
                table: "question_bank",
                column: "reference_list_id");

            migrationBuilder.CreateIndex(
                name: "idx_response_responsibility",
                table: "audit_response",
                column: "responsibility");

            migrationBuilder.CreateIndex(
                name: "idx_response_service_provider",
                table: "audit_response",
                column: "service_provider");

            migrationBuilder.CreateIndex(
                name: "UQ_reference_list_code",
                table: "reference_list",
                column: "code",
                unique: true,
                filter: "[code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_reference_list_name",
                table: "reference_list",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reference_list_item_airport_id",
                table: "reference_list_item",
                column: "airport_id");

            migrationBuilder.CreateIndex(
                name: "UQ_reference_list_item",
                table: "reference_list_item",
                columns: new[] { "list_id", "airport_id", "name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_question_bank_reference_list",
                table: "question_bank",
                column: "reference_list_id",
                principalTable: "reference_list",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_question_bank_reference_list",
                table: "question_bank");

            migrationBuilder.DropTable(
                name: "reference_list_item");

            migrationBuilder.DropTable(
                name: "reference_list");

            migrationBuilder.DropIndex(
                name: "IX_question_bank_reference_list_id",
                table: "question_bank");

            migrationBuilder.DropIndex(
                name: "idx_response_responsibility",
                table: "audit_response");

            migrationBuilder.DropIndex(
                name: "idx_response_service_provider",
                table: "audit_response");

            migrationBuilder.DropColumn(
                name: "reference_list_id",
                table: "question_bank");

            migrationBuilder.DropColumn(
                name: "responsibility",
                table: "audit_template_item");

            migrationBuilder.DropColumn(
                name: "service_provider",
                table: "audit_template_item");

            migrationBuilder.DropColumn(
                name: "service_provider",
                table: "audit_site");

            migrationBuilder.DropColumn(
                name: "responsibility",
                table: "audit_response");

            migrationBuilder.DropColumn(
                name: "service_provider",
                table: "audit_response");
        }
    }
}
