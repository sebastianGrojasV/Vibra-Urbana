using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibraUrbana.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAnulacionVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAnulacion",
                table: "Ventas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoAnulacion",
                table: "Ventas",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UsuarioAnulacionId",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_UsuarioAnulacionId",
                table: "Ventas",
                column: "UsuarioAnulacionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Usuarios_UsuarioAnulacionId",
                table: "Ventas",
                column: "UsuarioAnulacionId",
                principalTable: "Usuarios",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Usuarios_UsuarioAnulacionId",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_UsuarioAnulacionId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "FechaAnulacion",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "MotivoAnulacion",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "UsuarioAnulacionId",
                table: "Ventas");
        }
    }
}
