using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DummyApp.StorageService.Data.Migrations;

public partial class AddOrderStatus : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Status",
            table: "Orders",
            type: "varchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "Active");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Status",
            table: "Orders");
    }
}
