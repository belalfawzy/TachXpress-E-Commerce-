using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechXpress_DepiGraduation.Migrations
{
    /// <inheritdoc />
    public partial class RenameShoppingCartItemTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_shoppingCardItems_AspNetUsers_UserId",
                table: "shoppingCardItems");

            migrationBuilder.DropForeignKey(
                name: "FK_shoppingCardItems_Products_ProductId",
                table: "shoppingCardItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_shoppingCardItems",
                table: "shoppingCardItems");

            migrationBuilder.RenameTable(
                name: "shoppingCardItems",
                newName: "ShoppingCartItems");

            migrationBuilder.RenameColumn(
                name: "ShoppingCardId",
                table: "ShoppingCartItems",
                newName: "ShoppingCartId");

            migrationBuilder.RenameIndex(
                name: "IX_shoppingCardItems_UserId",
                table: "ShoppingCartItems",
                newName: "IX_ShoppingCartItems_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_shoppingCardItems_ProductId",
                table: "ShoppingCartItems",
                newName: "IX_ShoppingCartItems_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShoppingCartItems",
                table: "ShoppingCartItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCartItems_AspNetUsers_UserId",
                table: "ShoppingCartItems",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCartItems_Products_ProductId",
                table: "ShoppingCartItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCartItems_AspNetUsers_UserId",
                table: "ShoppingCartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCartItems_Products_ProductId",
                table: "ShoppingCartItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShoppingCartItems",
                table: "ShoppingCartItems");

            migrationBuilder.RenameTable(
                name: "ShoppingCartItems",
                newName: "shoppingCardItems");

            migrationBuilder.RenameColumn(
                name: "ShoppingCartId",
                table: "shoppingCardItems",
                newName: "ShoppingCardId");

            migrationBuilder.RenameIndex(
                name: "IX_ShoppingCartItems_UserId",
                table: "shoppingCardItems",
                newName: "IX_shoppingCardItems_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ShoppingCartItems_ProductId",
                table: "shoppingCardItems",
                newName: "IX_shoppingCardItems_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_shoppingCardItems",
                table: "shoppingCardItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_shoppingCardItems_AspNetUsers_UserId",
                table: "shoppingCardItems",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_shoppingCardItems_Products_ProductId",
                table: "shoppingCardItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
