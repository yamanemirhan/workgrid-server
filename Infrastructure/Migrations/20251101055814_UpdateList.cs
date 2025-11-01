using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "Lists",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                table: "Lists",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lists_StatusId",
                table: "Lists",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lists_CardStatuses_StatusId",
                table: "Lists",
                column: "StatusId",
                principalTable: "CardStatuses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lists_CardStatuses_StatusId",
                table: "Lists");

            migrationBuilder.DropIndex(
                name: "IX_Lists_StatusId",
                table: "Lists");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "Lists");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Lists");
        }
    }
}
