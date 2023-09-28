using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyBoards.Migrations
{
    public partial class TagSeed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Tags",
                column: "Value",
                value: "Web");

            migrationBuilder.InsertData(
                table: "Tags",
                column: "Value",
                value: "UI");

            migrationBuilder.InsertData(
                table: "Tags",
                column: "Value",
                value: "Desktop");

            migrationBuilder.InsertData(
                table: "Tags",
                column: "Value",
                value: "API");

            migrationBuilder.InsertData(
                table: "Tags",
                column: "Value",
                value: "Service");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Value",
                keyValue: "Service");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Value",
                keyValue: "API");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Value",
                keyValue: "Desktop");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Value",
                keyValue: "UI");

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Value",
                keyValue: "Web");
        }
    }
}
