using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibraUrbana.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTrazabilidadEntregaPedidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEntrega",
                table: "Pedidos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEnvio",
                table: "Pedidos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaTransito",
                table: "Pedidos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenEntrega",
                table: "Pedidos",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE Pedidos SET TokenEntrega = LOWER(REPLACE(CONVERT(nvarchar(36), NEWID()), '-', '')) WHERE TokenEntrega = '' OR TokenEntrega IS NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_TokenEntrega",
                table: "Pedidos",
                column: "TokenEntrega",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pedidos_TokenEntrega",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "FechaEntrega",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "FechaEnvio",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "FechaTransito",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "TokenEntrega",
                table: "Pedidos");
        }
    }
}
