using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace DummyApp.StorageService.Data.Migrations
{
    public partial class AddMessageType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "MessageTypes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Default" });

            migrationBuilder.AddColumn<int>(
                name: "MessageTypeId",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_MessageTypeId",
                table: "Messages",
                column: "MessageTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_MessageTypes_MessageTypeId",
                table: "Messages",
                column: "MessageTypeId",
                principalTable: "MessageTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_MessageTypes_MessageTypeId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "MessageTypes");

            migrationBuilder.DropIndex(
                name: "IX_Messages_MessageTypeId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "MessageTypeId",
                table: "Messages");
        }
    }
}
