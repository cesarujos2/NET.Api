using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NET.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeleteIdentificationNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_IdentityDocument",
                table: "USERS");

            migrationBuilder.DropColumn(
                name: "IdentityDocument",
                table: "USERS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityDocument",
                table: "USERS",
                type: "NVARCHAR2(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IdentityDocument",
                table: "USERS",
                column: "IdentityDocument",
                unique: true);
        }
    }
}
