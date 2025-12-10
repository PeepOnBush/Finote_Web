using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finote_Web.Migrations
{
    /// <inheritdoc />
    public partial class apikeyconstraintfix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApiKeys_KeyName",
                table: "ApiKeys");

            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "ApiKeys",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "KeyName",
                table: "ApiKeys",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_KeyName",
                table: "ApiKeys",
                column: "KeyName",
                unique: true);
        }
    }
}
