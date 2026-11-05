using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auditor.Migrations
{
    /// <summary>
    /// Airport and Audit Site entity specs from the QA report:
    /// new airport fields, per-airport multi-valued areas (airport_location), and audit site classification/contact fields.
    /// Existing AirSide/TerminalSection values are carried over into airport_location before the old columns are dropped.
    /// </summary>
    public partial class AirportAndAuditSiteSpec : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---------- airport: new columns ----------
            migrationBuilder.AddColumn<string>(
                name: "airport_type",
                table: "airport",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "region",
                table: "airport",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "country",
                table: "airport",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "airport_manager",
                table: "airport",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_address",
                table: "airport",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tel_number",
                table: "airport",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "airport",
                type: "bit",
                nullable: false,
                defaultValue: true);

            // ---------- airport_location ----------
            migrationBuilder.CreateTable(
                name: "airport_location",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    airport_id = table.Column<long>(type: "bigint", nullable: false),
                    area_type = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_airport_location", x => x.id);
                    table.ForeignKey(
                        name: "FK_airport_location_airport",
                        column: x => x.airport_id,
                        principalTable: "airport",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_airport_location_airport_area_name",
                table: "airport_location",
                columns: new[] { "airport_id", "area_type", "name" },
                unique: true);

            // Carry the old single-value areas over before their columns go.
            migrationBuilder.Sql(@"
INSERT INTO airport_location (airport_id, area_type, name)
SELECT DISTINCT id, 'Airside', LEFT(LTRIM(RTRIM(AirSide)), 150)
FROM airport
WHERE NULLIF(LTRIM(RTRIM(AirSide)), '') IS NOT NULL;

INSERT INTO airport_location (airport_id, area_type, name)
SELECT DISTINCT id, 'TerminalLocation', LEFT(LTRIM(RTRIM(TerminalSection)), 150)
FROM airport
WHERE NULLIF(LTRIM(RTRIM(TerminalSection)), '') IS NOT NULL;
");

            migrationBuilder.DropColumn(name: "AirSide", table: "airport");
            migrationBuilder.DropColumn(name: "ComercialArea", table: "airport");
            migrationBuilder.DropColumn(name: "TerminalSection", table: "airport");
            migrationBuilder.DropColumn(name: "location", table: "airport");

            // ---------- audit_site ----------
            migrationBuilder.AddColumn<long>(
                name: "terminal_id",
                table: "audit_site",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "site_category",
                table: "audit_site",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "site_sub_category",
                table: "audit_site",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "site_type",
                table: "audit_site",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contact_name",
                table: "audit_site",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contact_email",
                table: "audit_site",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contact_number",
                table: "audit_site",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_site_terminal_id",
                table: "audit_site",
                column: "terminal_id");

            migrationBuilder.AddForeignKey(
                name: "FK_audit_site_terminal",
                table: "audit_site",
                column: "terminal_id",
                principalTable: "airport_location",
                principalColumn: "id");

            // Status is now Active / Inactive only ('ACTIVE' came from the seed script).
            migrationBuilder.Sql(@"
UPDATE audit_site SET status = 'Active'   WHERE UPPER(status) = 'ACTIVE';
UPDATE audit_site SET status = 'Inactive' WHERE status <> 'Active';
");

            // Audit sites can't be created without a department; make sure there is at least one to pick.
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM department)
BEGIN
    INSERT INTO department (name, description) VALUES
        (N'Operations', N'Airport operations'),
        (N'Commercial', N'Food & beverage, retail and banking concessions');
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_audit_site_terminal", table: "audit_site");
            migrationBuilder.DropIndex(name: "IX_audit_site_terminal_id", table: "audit_site");

            migrationBuilder.DropColumn(name: "terminal_id", table: "audit_site");
            migrationBuilder.DropColumn(name: "site_category", table: "audit_site");
            migrationBuilder.DropColumn(name: "site_sub_category", table: "audit_site");
            migrationBuilder.DropColumn(name: "site_type", table: "audit_site");
            migrationBuilder.DropColumn(name: "contact_name", table: "audit_site");
            migrationBuilder.DropColumn(name: "contact_email", table: "audit_site");
            migrationBuilder.DropColumn(name: "contact_number", table: "audit_site");

            migrationBuilder.AddColumn<string>(
                name: "location",
                table: "airport",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AirSide",
                table: "airport",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComercialArea",
                table: "airport",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TerminalSection",
                table: "airport",
                type: "nvarchar(max)",
                nullable: true);

            // Best effort: put one area of each kind back into the single-value columns.
            migrationBuilder.Sql(@"
UPDATE a SET
    AirSide = (SELECT TOP 1 name FROM airport_location l WHERE l.airport_id = a.id AND l.area_type = 'Airside' ORDER BY l.id),
    TerminalSection = (SELECT TOP 1 name FROM airport_location l WHERE l.airport_id = a.id AND l.area_type = 'TerminalLocation' ORDER BY l.id)
FROM airport a;
");

            migrationBuilder.DropTable(name: "airport_location");

            migrationBuilder.DropColumn(name: "airport_type", table: "airport");
            migrationBuilder.DropColumn(name: "region", table: "airport");
            migrationBuilder.DropColumn(name: "country", table: "airport");
            migrationBuilder.DropColumn(name: "airport_manager", table: "airport");
            migrationBuilder.DropColumn(name: "email_address", table: "airport");
            migrationBuilder.DropColumn(name: "tel_number", table: "airport");
            migrationBuilder.DropColumn(name: "is_active", table: "airport");
        }
    }
}
