using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DummyApp.StorageService.Data.Migrations
{
    public partial class AddOrderItemPrintSizeAndPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrintSizeId",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriceId",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceValue",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_PrintSizeId",
                table: "OrderItems",
                column: "PrintSizeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_PriceId",
                table: "OrderItems",
                column: "PriceId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_PrintSizes_PrintSizeId",
                table: "OrderItems",
                column: "PrintSizeId",
                principalTable: "PrintSizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Prices_PriceId",
                table: "OrderItems",
                column: "PriceId",
                principalTable: "Prices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_PrintSizes_PrintSizeId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Prices_PriceId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_PrintSizeId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_PriceId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "PrintSizeId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "PriceId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "PriceValue",
                table: "OrderItems");
        }
    }
}
