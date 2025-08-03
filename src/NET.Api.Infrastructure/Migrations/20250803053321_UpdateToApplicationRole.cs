using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NET.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateToApplicationRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ROLES",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ROLES",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "HierarchyLevel",
                table: "ROLES",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ROLES",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemRole",
                table: "ROLES",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ROLES",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ROLES");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ROLES");

            migrationBuilder.DropColumn(
                name: "HierarchyLevel",
                table: "ROLES");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ROLES");

            migrationBuilder.DropColumn(
                name: "IsSystemRole",
                table: "ROLES");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ROLES");
        }
    }
}
