using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DummyApp.StorageService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletedOrdersTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompletedOrdersTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySql.EntityFrameworkCore.Metadata.MySQLValueGenerationStrategy.IdentityColumn),
                    Token = table.Column<Guid>(type: "char(36)", nullable: false),
                    Email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedOrdersTokens", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedOrdersTokens_Token",
                table: "CompletedOrdersTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompletedOrdersTokens_Email",
                table: "CompletedOrdersTokens",
                column: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompletedOrdersTokens");
        }
    }
}
