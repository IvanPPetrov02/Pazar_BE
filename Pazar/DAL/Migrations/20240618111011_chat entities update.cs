using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class chatentitiesupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Chats_ChatId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_ReceiverUUID",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ReceiverUUID",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ImageSent",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ReceiverUUID",
                table: "Messages");

            migrationBuilder.AlterColumn<int>(
                name: "ChatId",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BuyerUUID",
                table: "Chats",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "SellerUUID",
                table: "Chats",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_BuyerUUID",
                table: "Chats",
                column: "BuyerUUID");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_SellerUUID",
                table: "Chats",
                column: "SellerUUID");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Users_BuyerUUID",
                table: "Chats",
                column: "BuyerUUID",
                principalTable: "Users",
                principalColumn: "UUID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Users_SellerUUID",
                table: "Chats",
                column: "SellerUUID",
                principalTable: "Users",
                principalColumn: "UUID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Chats_ChatId",
                table: "Messages",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Users_BuyerUUID",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Users_SellerUUID",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Chats_ChatId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Chats_BuyerUUID",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_SellerUUID",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "BuyerUUID",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "SellerUUID",
                table: "Chats");

            migrationBuilder.AlterColumn<int>(
                name: "ChatId",
                table: "Messages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageSent",
                table: "Messages",
                type: "longblob",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReceiverUUID",
                table: "Messages",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceiverUUID",
                table: "Messages",
                column: "ReceiverUUID");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Chats_ChatId",
                table: "Messages",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_ReceiverUUID",
                table: "Messages",
                column: "ReceiverUUID",
                principalTable: "Users",
                principalColumn: "UUID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
