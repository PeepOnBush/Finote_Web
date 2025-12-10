using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finote_Web.Migrations
{
    /// <inheritdoc />
    public partial class updateApiKey2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ApiKeys",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ApiKeys");
        }
    }
}
