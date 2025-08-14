using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NET.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "USERS",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "USERS",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityDocument",
                table: "USERS",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsProfileComplete",
                table: "USERS",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "USERS");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "USERS");

            migrationBuilder.DropColumn(
                name: "IdentityDocument",
                table: "USERS");

            migrationBuilder.DropColumn(
                name: "IsProfileComplete",
                table: "USERS");
        }
    }
}
