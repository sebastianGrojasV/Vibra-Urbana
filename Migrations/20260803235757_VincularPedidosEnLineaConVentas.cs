using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibraUrbana.Migrations
{
    /// <inheritdoc />
    public partial class VincularPedidosEnLineaConVentas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VentaId",
                table: "Pedidos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_VentaId",
                table: "Pedidos",
                column: "VentaId",
                unique: true,
                filter: "[VentaId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Ventas_VentaId",
                table: "Pedidos",
                column: "VentaId",
                principalTable: "Ventas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Ventas_VentaId",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_VentaId",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "VentaId",
                table: "Pedidos");
        }
    }
}
