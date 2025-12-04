using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finote_Web.Migrations
{
    /// <inheritdoc />
    public partial class updateApiKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ApiKeys",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ApiKeys",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhoCreatedId",
                table: "ApiKeys",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WhoDeletedId",
                table: "ApiKeys",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_WhoCreatedId",
                table: "ApiKeys",
                column: "WhoCreatedId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_WhoDeletedId",
                table: "ApiKeys",
                column: "WhoDeletedId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiKeys_Users_WhoCreatedId",
                table: "ApiKeys",
                column: "WhoCreatedId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApiKeys_Users_WhoDeletedId",
                table: "ApiKeys",
                column: "WhoDeletedId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiKeys_Users_WhoCreatedId",
                table: "ApiKeys");

            migrationBuilder.DropForeignKey(
                name: "FK_ApiKeys_Users_WhoDeletedId",
                table: "ApiKeys");

            migrationBuilder.DropIndex(
                name: "IX_ApiKeys_WhoCreatedId",
                table: "ApiKeys");

            migrationBuilder.DropIndex(
                name: "IX_ApiKeys_WhoDeletedId",
                table: "ApiKeys");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ApiKeys");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ApiKeys");

            migrationBuilder.DropColumn(
                name: "WhoCreatedId",
                table: "ApiKeys");

            migrationBuilder.DropColumn(
                name: "WhoDeletedId",
                table: "ApiKeys");
        }
    }
}
