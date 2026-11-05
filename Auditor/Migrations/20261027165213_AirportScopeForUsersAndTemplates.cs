using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auditor.Migrations
{
    /// <inheritdoc />
    public partial class AirportScopeForUsersAndTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "airport_id",
                table: "audit_template",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "airport_id",
                table: "app_user",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_template_airport_id",
                table: "audit_template",
                column: "airport_id");

            migrationBuilder.CreateIndex(
                name: "IX_app_user_airport_id",
                table: "app_user",
                column: "airport_id");

            migrationBuilder.AddForeignKey(
                name: "FK_app_user_airport",
                table: "app_user",
                column: "airport_id",
                principalTable: "airport",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_audit_template_airport",
                table: "audit_template",
                column: "airport_id",
                principalTable: "airport",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            // Templates imported for one airport ("JNB Terminal A ...", "CPT Landside ...") are only offered there.
            migrationBuilder.Sql(@"
UPDATE t SET airport_id = a.id
FROM audit_template t
JOIN airport a ON t.name LIKE a.code + N' %'
WHERE t.airport_id IS NULL;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_app_user_airport",
                table: "app_user");

            migrationBuilder.DropForeignKey(
                name: "FK_audit_template_airport",
                table: "audit_template");

            migrationBuilder.DropIndex(
                name: "IX_audit_template_airport_id",
                table: "audit_template");

            migrationBuilder.DropIndex(
                name: "IX_app_user_airport_id",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "airport_id",
                table: "audit_template");

            migrationBuilder.DropColumn(
                name: "airport_id",
                table: "app_user");
        }
    }
}
