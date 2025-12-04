using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finote_Web.Migrations
{
    /// <inheritdoc />
    public partial class AddApilogAndApikey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UsedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Prompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PersonSend = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KeyName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    KeyValue = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiLogs_UserId",
                table: "AiLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_KeyName",
                table: "ApiKeys",
                column: "KeyName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiLogs");

            migrationBuilder.DropTable(
                name: "ApiKeys");
        }
    }
}
