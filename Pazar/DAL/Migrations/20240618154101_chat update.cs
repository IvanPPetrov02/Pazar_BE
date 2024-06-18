using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class chatupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Users_SellerUUID",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_SellerUUID",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "SellerUUID",
                table: "Chats");

            migrationBuilder.AddColumn<int>(
                name: "ItemSoldId",
                table: "Chats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Chats_ItemSoldId",
                table: "Chats",
                column: "ItemSoldId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Items_ItemSoldId",
                table: "Chats",
                column: "ItemSoldId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Items_ItemSoldId",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_ItemSoldId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "ItemSoldId",
                table: "Chats");

            migrationBuilder.AddColumn<Guid>(
                name: "SellerUUID",
                table: "Chats",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_SellerUUID",
                table: "Chats",
                column: "SellerUUID");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Users_SellerUUID",
                table: "Chats",
                column: "SellerUUID",
                principalTable: "Users",
                principalColumn: "UUID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
