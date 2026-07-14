using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibraUrbana.Migrations
{
    /// <inheritdoc />
    public partial class AgregarControlConcurrenciaInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "Inventario",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "Inventario");
        }
    }
}
