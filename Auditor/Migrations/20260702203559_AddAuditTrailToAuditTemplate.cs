using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auditor.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditTrailToAuditTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "modified_by",
                table: "audit_template_item",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_date",
                table: "audit_template_item",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "modified_by",
                table: "audit_template",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_date",
                table: "audit_template",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_template_modified_by",
                table: "audit_template",
                column: "modified_by");

            migrationBuilder.AddForeignKey(
                name: "FK__audit_tem__modif__6C190EBC",
                table: "audit_template",
                column: "modified_by",
                principalTable: "app_user",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__audit_tem__modif__6C190EBC",
                table: "audit_template");

            migrationBuilder.DropIndex(
                name: "IX_audit_template_modified_by",
                table: "audit_template");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "audit_template_item");

            migrationBuilder.DropColumn(
                name: "modified_date",
                table: "audit_template_item");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "audit_template");

            migrationBuilder.DropColumn(
                name: "modified_date",
                table: "audit_template");
        }
    }
}
