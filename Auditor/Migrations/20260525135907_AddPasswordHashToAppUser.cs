using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auditor.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordHashToAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "airport",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__airport__3213E83F178E14CD", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "app_user",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    azure_ad_object_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    display_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__app_user__3213E83F1159EBF7", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "department",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__departme__3213E83FADB797C3", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "question_category",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__question__3213E83F12F8191F", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "question_type",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__question__3213E83F70F1D122", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_template",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: false),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__audit_te__3213E83FB8A2B4CB", x => x.id);
                    table.ForeignKey(
                        name: "FK__audit_tem__creat__6C190EBB",
                        column: x => x.created_by,
                        principalTable: "app_user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    recipient_id = table.Column<long>(type: "bigint", nullable: false),
                    subject = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    entity_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    entity_id = table.Column<long>(type: "bigint", nullable: true),
                    sent_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__notifica__3213E83FAD279980", x => x.id);
                    table.ForeignKey(
                        name: "FK__notificat__recip__1AD3FDA4",
                        column: x => x.recipient_id,
                        principalTable: "app_user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "audit_site",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    airport_id = table.Column<long>(type: "bigint", nullable: false),
                    department_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: false),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__audit_si__3213E83F2E69B5A2", x => x.id);
                    table.ForeignKey(
                        name: "FK__audit_sit__airpo__74AE54BC",
                        column: x => x.airport_id,
                        principalTable: "airport",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__audit_sit__creat__76969D2E",
                        column: x => x.created_by,
                        principalTable: "app_user",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__audit_sit__depar__75A278F5",
                        column: x => x.department_id,
                        principalTable: "department",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "question_bank",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    question_text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    question_type_id = table.Column<long>(type: "bigint", nullable: false),
                    service_standard_recommendation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    responsible_contractor = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_by = table.Column<long>(type: "bigint", nullable: false),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__question__3213E83FD16E9283", x => x.id);
                    table.ForeignKey(
                        name: "FK__question___categ__5FB337D6",
                        column: x => x.category_id,
                        principalTable: "question_category",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__question___creat__619B8048",
                        column: x => x.created_by,
                        principalTable: "app_user",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__question___quest__60A75C0F",
                        column: x => x.question_type_id,
                        principalTable: "question_type",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "audit_schedule",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    template_id = table.Column<long>(type: "bigint", nullable: false),
                    site_id = table.Column<long>(type: "bigint", nullable: false),
                    scheduler_id = table.Column<long>(type: "bigint", nullable: false),
                    scheduled_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    due_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    modification_reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    cancellation_reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__audit_sc__3213E83F48C5834A", x => x.id);
                    table.ForeignKey(
                        name: "FK__audit_sch__sched__01142BA1",
                        column: x => x.scheduler_id,
                        principalTable: "app_user",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__audit_sch__site___00200768",
                        column: x => x.site_id,
                        principalTable: "audit_site",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__audit_sch__templ__7F2BE32F",
                        column: x => x.template_id,
                        principalTable: "audit_template",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "audit_site_template",
                columns: table => new
                {
                    site_id = table.Column<long>(type: "bigint", nullable: false),
                    template_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__audit_si__39CB95CD4E5145E5", x => new { x.site_id, x.template_id });
                    table.ForeignKey(
                        name: "FK__audit_sit__site___797309D9",
                        column: x => x.site_id,
                        principalTable: "audit_site",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__audit_sit__templ__7A672E12",
                        column: x => x.template_id,
                        principalTable: "audit_template",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "audit_template_item",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    template_id = table.Column<long>(type: "bigint", nullable: false),
                    question_bank_id = table.Column<long>(type: "bigint", nullable: false),
                    sequence = table.Column<int>(type: "int", nullable: false),
                    mandatory = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__audit_te__3213E83F30483F4F", x => x.id);
                    table.ForeignKey(
                        name: "FK__audit_tem__quest__6FE99F9F",
                        column: x => x.question_bank_id,
                        principalTable: "question_bank",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__audit_tem__templ__6EF57B66",
                        column: x => x.template_id,
                        principalTable: "audit_template",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "question_bank_option",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    question_bank_id = table.Column<long>(type: "bigint", nullable: false),
                    option_text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    option_value = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    requires_reason = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__question__3213E83F9030CCB8", x => x.id);
                    table.ForeignKey(
                        name: "FK__question___quest__6477ECF3",
                        column: x => x.question_bank_id,
                        principalTable: "question_bank",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "question_bank_reason",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    question_bank_id = table.Column<long>(type: "bigint", nullable: false),
                    reason_text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__question__3213E83F6ED53A41", x => x.id);
                    table.ForeignKey(
                        name: "FK__question___quest__6754599E",
                        column: x => x.question_bank_id,
                        principalTable: "question_bank",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "audit_execution",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    schedule_id = table.Column<long>(type: "bigint", nullable: false),
                    auditor_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    acceptance_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    rejection_reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    submission_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    original_audit_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__audit_ex__3213E83FA1316016", x => x.id);
                    table.ForeignKey(
                        name: "FK__audit_exe__audit__09A971A2",
                        column: x => x.auditor_id,
                        principalTable: "app_user",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__audit_exe__sched__08B54D69",
                        column: x => x.schedule_id,
                        principalTable: "audit_schedule",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "audit_schedule_auditor",
                columns: table => new
                {
                    schedule_id = table.Column<long>(type: "bigint", nullable: false),
                    auditor_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__audit_sc__BD9E29F05A8F4534", x => new { x.schedule_id, x.auditor_id });
                    table.ForeignKey(
                        name: "FK__audit_sch__audit__04E4BC85",
                        column: x => x.auditor_id,
                        principalTable: "app_user",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__audit_sch__sched__03F0984C",
                        column: x => x.schedule_id,
                        principalTable: "audit_schedule",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "audit_attachment",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    execution_id = table.Column<long>(type: "bigint", nullable: false),
                    file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    file_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    content_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    file_size = table.Column<long>(type: "bigint", nullable: true),
                    uploaded_date = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__audit_at__3213E83F3A4458CE", x => x.id);
                    table.ForeignKey(
                        name: "FK__audit_att__execu__160F4887",
                        column: x => x.execution_id,
                        principalTable: "audit_execution",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "audit_execution_result",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    execution_id = table.Column<long>(type: "bigint", nullable: false),
                    total_questions = table.Column<int>(type: "int", nullable: false),
                    compliant_count = table.Column<int>(type: "int", nullable: false),
                    non_compliant_count = table.Column<int>(type: "int", nullable: false),
                    not_applicable_count = table.Column<int>(type: "int", nullable: true),
                    score_percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    calculated_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_execution_result", x => x.id);
                    table.ForeignKey(
                        name: "FK_execution_result_execution",
                        column: x => x.execution_id,
                        principalTable: "audit_execution",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "audit_response",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    execution_id = table.Column<long>(type: "bigint", nullable: false),
                    template_item_id = table.Column<long>(type: "bigint", nullable: false),
                    selected_option_id = table.Column<long>(type: "bigint", nullable: true),
                    comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    compliant = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__audit_re__3213E83F55E5FE9F", x => x.id);
                    table.ForeignKey(
                        name: "FK__audit_res__execu__0C85DE4D",
                        column: x => x.execution_id,
                        principalTable: "audit_execution",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__audit_res__selec__0E6E26BF",
                        column: x => x.selected_option_id,
                        principalTable: "question_bank_option",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__audit_res__templ__0D7A0286",
                        column: x => x.template_item_id,
                        principalTable: "audit_template_item",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "audit_response_attachment",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    response_id = table.Column<long>(type: "bigint", nullable: false),
                    file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    file_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    content_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    file_size = table.Column<long>(type: "bigint", nullable: true),
                    uploaded_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_response_attachment", x => x.id);
                    table.ForeignKey(
                        name: "FK_response_attachment_response",
                        column: x => x.response_id,
                        principalTable: "audit_response",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "audit_response_reason",
                columns: table => new
                {
                    response_id = table.Column<long>(type: "bigint", nullable: false),
                    reason_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__audit_re__B3AA63C32753E0CD", x => new { x.response_id, x.reason_id });
                    table.ForeignKey(
                        name: "FK__audit_res__reaso__123EB7A3",
                        column: x => x.reason_id,
                        principalTable: "question_bank_reason",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__audit_res__respo__114A936A",
                        column: x => x.response_id,
                        principalTable: "audit_response",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "UQ__airport__357D4CF90A3E0150",
                table: "airport",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__airport__72E12F1B1E2BB23C",
                table: "airport",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__app_user__0E42BF72C6C8469F",
                table: "app_user",
                column: "azure_ad_object_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_attachment_execution_id",
                table: "audit_attachment",
                column: "execution_id");

            migrationBuilder.CreateIndex(
                name: "idx_execution_status",
                table: "audit_execution",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_audit_execution_auditor_id",
                table: "audit_execution",
                column: "auditor_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_execution_schedule_id",
                table: "audit_execution",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "UQ_execution_result_execution",
                table: "audit_execution_result",
                column: "execution_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_response_execution",
                table: "audit_response",
                column: "execution_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_response_selected_option_id",
                table: "audit_response",
                column: "selected_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_response_template_item_id",
                table: "audit_response",
                column: "template_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_response_attachment_response_id",
                table: "audit_response_attachment",
                column: "response_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_response_reason_reason_id",
                table: "audit_response_reason",
                column: "reason_id");

            migrationBuilder.CreateIndex(
                name: "idx_schedule_status",
                table: "audit_schedule",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_audit_schedule_scheduler_id",
                table: "audit_schedule",
                column: "scheduler_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_schedule_site_id",
                table: "audit_schedule",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_schedule_template_id",
                table: "audit_schedule",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_schedule_auditor_auditor_id",
                table: "audit_schedule_auditor",
                column: "auditor_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_site_airport_id",
                table: "audit_site",
                column: "airport_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_site_created_by",
                table: "audit_site",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_audit_site_department_id",
                table: "audit_site",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_site_template_template_id",
                table: "audit_site_template",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_template_created_by",
                table: "audit_template",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "UQ__audit_te__72E12F1B84BFD71C",
                table: "audit_template",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_template_item_template",
                table: "audit_template_item",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_template_item_question_bank_id",
                table: "audit_template_item",
                column: "question_bank_id");

            migrationBuilder.CreateIndex(
                name: "idx_notification_recipient",
                table: "notification",
                column: "recipient_id");

            migrationBuilder.CreateIndex(
                name: "idx_question_bank_category",
                table: "question_bank",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_question_bank_created_by",
                table: "question_bank",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_question_bank_question_type_id",
                table: "question_bank",
                column: "question_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_question_bank_option_question_bank_id",
                table: "question_bank_option",
                column: "question_bank_id");

            migrationBuilder.CreateIndex(
                name: "IX_question_bank_reason_question_bank_id",
                table: "question_bank_reason",
                column: "question_bank_id");

            migrationBuilder.CreateIndex(
                name: "UQ__question__72E12F1B59B99650",
                table: "question_category",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__question__72E12F1B2F93D81A",
                table: "question_type",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_attachment");

            migrationBuilder.DropTable(
                name: "audit_execution_result");

            migrationBuilder.DropTable(
                name: "audit_response_attachment");

            migrationBuilder.DropTable(
                name: "audit_response_reason");

            migrationBuilder.DropTable(
                name: "audit_schedule_auditor");

            migrationBuilder.DropTable(
                name: "audit_site_template");

            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropTable(
                name: "question_bank_reason");

            migrationBuilder.DropTable(
                name: "audit_response");

            migrationBuilder.DropTable(
                name: "audit_execution");

            migrationBuilder.DropTable(
                name: "question_bank_option");

            migrationBuilder.DropTable(
                name: "audit_template_item");

            migrationBuilder.DropTable(
                name: "audit_schedule");

            migrationBuilder.DropTable(
                name: "question_bank");

            migrationBuilder.DropTable(
                name: "audit_site");

            migrationBuilder.DropTable(
                name: "audit_template");

            migrationBuilder.DropTable(
                name: "question_category");

            migrationBuilder.DropTable(
                name: "question_type");

            migrationBuilder.DropTable(
                name: "airport");

            migrationBuilder.DropTable(
                name: "department");

            migrationBuilder.DropTable(
                name: "app_user");
        }
    }
}
