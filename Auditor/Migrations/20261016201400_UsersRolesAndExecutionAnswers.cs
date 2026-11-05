using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auditor.Migrations
{
    /// <inheritdoc />
    public partial class UsersRolesAndExecutionAnswers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "section",
                table: "audit_template_item",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "answer_value",
                table: "audit_response",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "employee_number",
                table: "app_user",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_login_date",
                table: "app_user",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UQ_app_user_email",
                table: "app_user",
                column: "email",
                unique: true,
                filter: "[email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_app_user_employee_number",
                table: "app_user",
                column: "employee_number",
                unique: true,
                filter: "[employee_number] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_app_user_email",
                table: "app_user");

            migrationBuilder.DropIndex(
                name: "UQ_app_user_employee_number",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "section",
                table: "audit_template_item");

            migrationBuilder.DropColumn(
                name: "answer_value",
                table: "audit_response");

            migrationBuilder.DropColumn(
                name: "employee_number",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "last_login_date",
                table: "app_user");
        }
    }
}
